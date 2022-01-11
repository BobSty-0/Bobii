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
        public static async Task CreateAndConnectToVoiceChannel(SocketUser user, SocketVoiceState newVoice, string name, int? channelSize, bool textChannel, DiscordSocketClient client)
        {
            var category = newVoice.VoiceChannel.Category;
            string channelName = name.Trim();
            var socketGuildUser = (SocketGuildUser)user;
            if (channelName.Contains("{count}"))
            {
                channelName = channelName.Replace("{count}",
                    (EntityFramework.TempChannelsHelper.GetCount(newVoice.VoiceChannel.Id).Result).ToString());
            }

            if (channelName.Contains("{activity}"))
            {
                var activityString = "Chilling";
                foreach (var activity in user.Activities)
                {
                    if (activity.Type == ActivityType.Playing || activity.Type == ActivityType.Listening || activity.Type == ActivityType.Watching)
                    {
                        activityString = activity.Name;
                    }

                    if (activity.Type == ActivityType.Streaming)
                    {
                        activityString = "Streaming";
                    }
                }
                channelName = channelName.Replace("{activity}", activityString);
            }

            if (channelName.Contains("{username}"))
            {
                channelName = channelName.Replace("{username}", user.Username);
            }

            // insert the Spülmaschinö id here
            if (socketGuildUser.Guild.Id == 0)
            {
                if (CheckIfChannelWithNameExists(user as SocketGuildUser, category.Id.ToString(), channelName, newVoice, client).Result)
                {
                    return;
                }
            }

            var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), channelName, channelSize, newVoice).Result;
            if (textChannel)
            {
                var textChannelRestChannel = CreateTextChannel(user as SocketGuildUser, newVoice, channelName, category.Id.ToString()).Result;
                await EntityFramework.TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id, textChannelRestChannel.Id);
            }
            else
            {
                await EntityFramework.TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id, 0);
            }
            
            await TempChannel.Helper.ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            if (voiceChannel == null)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, "ConnectToVoice",
                    new Entities.SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user }, message: $"{ user} ({ user.Id}) could not be connected");
                return;
            }
            await user.ModifyAsync(x => x.Channel = voiceChannel);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "ConnectToVoice",
                  new Entities.SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                  message: $"{user} ({user.Id}) was successfully connected to {voiceChannel}", tempChannelID: voiceChannel.Id);
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
                        
                        await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "CheckAndDeleteEmptyVoiceChannels",
                              new Entities.SlashCommandParameter() { Guild = guild, GuildUser = (SocketGuildUser)user },
                              message: $"Channel successfully deleted", tempChannelID: tempChannel.channelid);

                        var tempChannelEF = EntityFramework.TempChannelsHelper.GetTempChannel(tempChannel.channelid);

                        if (tempChannelEF.Result.textchannelid != 0)
                        {
                            var textChannel = client.Guilds
                                .SelectMany(g => g.Channels)
                                .SingleOrDefault(c => c.Id == tempChannelEF.Result.textchannelid);

                            if (textChannel != null)
                            {
                                await textChannel.DeleteAsync();
                                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "DeleteTextChannel",
                                      new Entities.SlashCommandParameter() { Guild = guild, GuildUser = (SocketGuildUser)user },
                                      message: $"Text channel successfully deleted", tempChannelID: tempChannel.channelid);
                            }
                        }

                        await EntityFramework.TempChannelsHelper.RemoveTC(guild.Id, tempChannel.channelid);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Access"))
                {
                    await user.SendMessageAsync($"Hey {user.Username}, I'm **missing accsess** to delete the temp-channel which you have left a second ago({voiceChannelName})!\n" +
                        $"This can happen because of missing permissions or because someone is spam joining the create-temp-channel");
                }
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, "CheckAndDeleteEmptyVoiceChannels",
                    new Entities.SlashCommandParameter() { Guild = guild, GuildUser = (SocketGuildUser)user },
                    message: $"Voicechannel could not be deleted, {user} has got a DM if it was missing access", exceptionMessage: ex.Message);
            }
        }

        public static async Task<bool> CheckIfChannelWithNameExists(SocketGuildUser user, string catergoryId, string name, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            var guild = user.Guild;
            var categoryChannel = guild.GetCategoryChannel(ulong.Parse(catergoryId));
            
            foreach(var channel in categoryChannel.Channels)
            {
                if(channel.Name == name)
                {
                    await user.ModifyAsync(x => x.Channel = (SocketVoiceChannel)channel);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "ConnectToVoice",
                          new Entities.SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                          message: $"{user} ({user.Id}) was successfully connected to {channel.Name}", tempChannelID: channel.Id);
                    return true;
                }
            }

            return false;
        }

        public static async Task<RestTextChannel> CreateTextChannel(SocketGuildUser user, SocketVoiceState newVoice, string name, string catergoryId)
        {
            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
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
                var channel = user.Guild.CreateTextChannelAsync(name, prop =>
                {
                    prop.CategoryId = ulong.Parse(catergoryId);
                    prop.PermissionOverwrites = permissions;
                });

                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "CreateTextChannel",
                    new Entities.SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"{user} created new text channel {channel.Result}", tempChannelID: channel.Result.Id);
                return channel.Result;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, "CreateTextChannel",
                    new Entities.SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"Text channel could not be created", exceptionMessage: ex.Message);
                return null;
                throw;
            }

        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketGuildUser user, string catergoryId, string name, int? channelSize, SocketVoiceState newVoice)
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
                    prop.UserLimit = channelSize;
                });

                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "CreateVoiceChannel",
                    new Entities.SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"{user} created new voice channel {channel.Result}", tempChannelID: channel.Result.Id);
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
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, "CreateVoiceChannel",
                    new Entities.SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"Voicechannel could not be created, {user} has got a DM if it was missing permissions", exceptionMessage: ex.Message);
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

                if (createTempChannel.channelsize != null)
                {
                    sb.AppendLine($"TempChannelSize: **{createTempChannel.channelsize}**");
                }

                if (createTempChannel.textchannel.Value)
                {
                    sb.AppendLine("Text channel: **On**");
                }
            }

            return Bobii.Helper.CreateEmbed(interaction, sb.ToString(), header).Result;
        }

        //Double Code -> Find solution one day!
        public static async Task<string> HelpTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "You can create temporary voice channels which are created and deleted automatically." +
                "\nTo get a instructions on how to use certain commands use the command: `/bobiiguides`!\n" +
                "\nAlso following things will be replaced in the temp-channel name:" +
                "\n`{username}` -> will be replaced with the username" +
                "\n`{activity}` -> will be replaced with the current game of the users activity status" +
                "\n`{count}` -> will be replaced with a count of all active temp-channels from the create-temp-channel", "tc").Result;
        }

        public static async Task<string> HelpEditTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, bool withoutHint = false)
        {
            await Task.CompletedTask;
            var sb = new StringBuilder();
            if (!withoutHint)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("Here are all my commands to edit the temp-channels:");
                sb.AppendLine("If you want to create an embed which shows all this commands below please use: `/ tccreateinfo`");
            }
            return Bobii.Helper.CreateInfoPart(commandList, sb.ToString(), "temp").Result;
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