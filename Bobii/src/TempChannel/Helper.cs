using Bobii.src.EntityFramework.Entities;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class Helper
    {
        #region Tasks
        public static async Task CreateAndConnectToVoiceChannel(SocketUser user, SocketVoiceState newVoice, string name)
        {
            var category = newVoice.VoiceChannel.Category;
            string channelName = name.Trim();
            if (channelName.Contains("User"))
            {
                channelName = channelName.Replace("User", user.Username);
            }

            var tempChannel = TempChannel.Helper.CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), channelName, newVoice).Result;
            await EntityFramework.TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id);
            await TempChannel.Helper.ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            if (voiceChannel == null)
            {
                await Handler.TempChannelHandler.WriteToConsol($"Error: {user.Guild.Name} | Task: ConnectToVoice | Guild: {user.Guild.Id} | {user} ({user.Id}) could not be connected");
                return;
            }
            await user.ModifyAsync(x => x.Channel = voiceChannel);
            await Handler.TempChannelHandler.WriteToConsol($"Information: {user.Guild.Name} | Task: ConnectToVoice | Guild: {user.Guild.Id} | Channel: {voiceChannel.Id} | {user} ({user.Id}) was successfully connected to {voiceChannel}");
        }

        public static async Task CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client, SocketGuild guild, List<tempchannels> tempchannelIDs, SocketUser user)
        {
            var voiceChannelName = "";
            try
            {
                foreach (var tempChannel in tempchannelIDs)
                {
                    var voiceChannel = client.Guilds
                        .SelectMany(g => g.Channels)
                        .SingleOrDefault(c => c.Id == tempChannel.channelid);

                    if (voiceChannel == null)
                    {
                        await EntityFramework.TempChannelsHelper.RemoveTC(guild.Id, tempChannel.channelid);
                        continue;
                    }
                    voiceChannelName = voiceChannel.Name;

                    if (voiceChannel.Users.Count == 0)
                    {
                        await voiceChannel.DeleteAsync();
                        await EntityFramework.TempChannelsHelper.RemoveTC(guild.Id, tempChannel.channelid);
                        await Handler.TempChannelHandler.WriteToConsol($"Information: {guild.Name} | Task: CheckAndDeleteEmptyVoiceChannels | Guild: {guild.Id} | Channel: {tempChannel.channelid} | Channel successfully deleted");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Access"))
                {
                    await user.SendMessageAsync($"Hey {user.Username}, I'm **missing accsess** to delete the temp-channel which you have left a second ago({voiceChannelName})!\n" +
                        $"It is hard to tell which permission I need to be able to delete the private temp-channel, thats why I suggest giving me the `Administrator` permission in case you want to work " +
                        $"with advanced permissions for temp-channels.\n" +
                        $"My permissions from the invite link are enough to create and delete temp-channels without any additional permissions.");
                }
                await Handler.TempChannelHandler.WriteToConsol($"Error: {guild.Name} | Voicechannel could not be deleted | {ex.Message} | {user} has got a DM because of missing access");
            }
        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketGuildUser user, string catergoryId, string name, SocketVoiceState newVoice)
        {
            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                //Permissions for each role
                foreach (var role in user.Guild.Roles)
                {
                    var permissionOverride = newVoice.VoiceChannel.GetPermissionOverwrite(role);
                    if (permissionOverride != null)
                    {
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value));
                    }
                }

                SocketRole bobiiRole = null;
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    bobiiRole = user.Guild.Roles.Where(role => role.Name == "BobiiDev").First();
                }
                else
                {
                    bobiiRole = user.Guild.Roles.Where(role => role.Name == "Bobii").First();
                }
                    
                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));

                //Create channel with permissions in the target category
                var channel = user.Guild.CreateVoiceChannelAsync(name, prop =>
                {
                    prop.CategoryId = ulong.Parse(catergoryId);
                    prop.PermissionOverwrites = permissions;
                });


                await Handler.TempChannelHandler.WriteToConsol($"Information: {user.Guild.Name} | Task: CreateVoiceChannel | Guild: {user.Guild.Id} | Channel: {channel.Result.Id} | {user} created new voice channel {channel.Result}");
                return channel.Result;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permission"))
                {
                    await user.SendMessageAsync($"Hey {user.Username}, I'm **missing permissions** to create the temp-channel of the create-temp-channel which you just joined!\n" +
                        $"It is hard to tell which permission I need to be able to create the private temp-channel, thats why I suggest giving me the `Administrator` permission in case you want to work " +
                        $"with advanced permissions for temp-channels.\n" +
                        $"My permissions from the invite link are enough to create temp-channels without any additional permissions.");
                }
                await Handler.TempChannelHandler.WriteToConsol($"Error: {user.Guild.Name} | Voicechannel could not be created | {ex.Message} | {user} has got a DM because of missing permissions");
                return null;
            }
        }

        public static Embed CreateVoiceChatInfoEmbed(SocketGuild guild, DiscordSocketClient client, SocketInteraction interaction)
        {
            var config = Program.GetConfig();
            StringBuilder sb = new StringBuilder();
            var createTempChannelList = EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild).Result;
            string header = null;
            if (createTempChannelList.Count == 0)
            {
                header = "No create temp channels yet!";
                sb.AppendLine("You dont have any create-temp-channels yet!\nYou can add some with:\n`/tcadd`");
            }
            else
            {
                header = "Here a list of all create temp channels:";
            }

            foreach (var createTempChannel in createTempChannelList)
            {
                var channelId = createTempChannel.createchannelid;
                var voiceChannel = client.Guilds
                                   .SelectMany(g => g.Channels)
                                   .SingleOrDefault(c => c.Id == channelId);
                if (voiceChannel == null)
                {
                    continue;
                }

                sb.AppendLine("");
                sb.AppendLine($"<#{channelId}>");
                sb.AppendLine($"Id: **{channelId}**");
                sb.AppendLine($"TempChannelName: **{createTempChannel.tempchannelname}**");
            }

            return Bobii.Helper.CreateEmbed(interaction, sb.ToString(), header).Result;
        }

        //Double Code -> Find solution one day!
        public static async Task<string> HelpTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You can create temporary voice channels which are created and deleted automatically.\nTo get a instructions on how to use certain commands use the command: `/bobiiguides`!");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("tc"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append("**/" + command.Name);
                        foreach (var option in command.Options)
                        {
                            sb.Append(" <" + option.Name + ">");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }

        public static async Task<string> HelpEditTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("temp"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append("**/" + command.Name);
                        foreach (var option in command.Options)
                        {
                            sb.Append(" <" + option.Name + ">");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }

        public static async Task TansferOwnerShip(SocketVoiceChannel channel)
        {
            if (channel.Users.Count == 0)
            {
                return;
            }
            var luckyNewOwner = channel.Users.First();
            await EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, luckyNewOwner.Id);
        }
        #endregion
    }
}