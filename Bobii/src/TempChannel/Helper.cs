﻿using Bobii.src.EntityFramework.Entities;
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
            await EntityFramework.TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id);
            await TempChannel.Helper.ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            await user.ModifyAsync(x => x.Channel = voiceChannel);
            await Handler.TempChannelHandler.WriteToConsol($"Information: {user.Guild.Name} | Task: CreateAndConnectToVoiceChannel | Guild: {user.Guild.Id} | Channel: {voiceChannel.Id} | {user} ({user.Id}) was successfully connected to {voiceChannel}");
        }

        public static async Task CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client, SocketGuild guild, List<tempchannels> tempchannelIDs)
        {
            var config = Program.GetConfig();

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

                if (voiceChannel.Users.Count == 0)
                {
                    await voiceChannel.DeleteAsync();
                    await EntityFramework.TempChannelsHelper.RemoveTC(guild.Id, tempChannel.channelid);
                    await Handler.TempChannelHandler.WriteToConsol($"Information: {guild.Name} | Task: CheckAndDeleteEmptyVoiceChannels | Guild: {guild.Id} | Channel: {tempChannel.channelid} | Channel successfully deleted");
                }
            }
        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketGuildUser user, string catergoryId, string name, SocketVoiceState newVoice)
        {
            List<Overwrite> permissions = new List<Overwrite>();
            //Permissions for each role
            foreach (var role in user.Guild.Roles)
            {
                var permissionOverride = newVoice.VoiceChannel.GetPermissionOverwrite(role);
                if (permissionOverride != null)
                {
                    var newPermissionOverride = new OverwritePermissions(
                        permissionOverride.Value.CreateInstantInvite,
                        permissionOverride.Value.ManageChannel,
                        permissionOverride.Value.AddReactions,
                        permissionOverride.Value.ViewChannel,
                        permissionOverride.Value.SendMessages,
                        permissionOverride.Value.SendTTSMessages,
                        permissionOverride.Value.ManageMessages,
                        permissionOverride.Value.EmbedLinks,
                        permissionOverride.Value.AttachFiles,
                        permissionOverride.Value.ReadMessageHistory,
                        permissionOverride.Value.MentionEveryone,
                        permissionOverride.Value.UseExternalEmojis,
                        permissionOverride.Value.Connect,
                        permissionOverride.Value.Speak,
                        permissionOverride.Value.MuteMembers,
                        permissionOverride.Value.DeafenMembers,
                        permissionOverride.Value.MoveMembers,
                        // $TODO 08.09.2021/JG figure out how this is called -> UseVoiceActivision
                        PermValue.Allow,
                        permissionOverride.Value.ManageRoles,
                        permissionOverride.Value.ManageWebhooks,
                        permissionOverride.Value.PrioritySpeaker,
                        permissionOverride.Value.Stream);
                    permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value));
                }
            }

            //Permissions for the creator of the channel
            permissions.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions()
                .Modify(null, PermValue.Allow)));

            //Create channel with permissions in the target category
            var channel = user.Guild.CreateVoiceChannelAsync(name, prop => {
                prop.CategoryId = ulong.Parse(catergoryId);
                prop.PermissionOverwrites = permissions;
            });

            try
            {
                await Handler.TempChannelHandler.WriteToConsol($"Information: {user.Guild.Name} | Task: CreateVoiceChannel | Guild: {user.Guild.Id} | Channel: {channel.Result.Id} | {user} created new voice channel {channel.Result}");
            }
            catch (Exception ex)
            {
                await Handler.TempChannelHandler.WriteToConsol(ex.Message);
            }

            return channel.Result;
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
        #endregion
    }
}