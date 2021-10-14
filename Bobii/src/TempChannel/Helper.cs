using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bobii.src.DBStuff.Tables;

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
            tempchannels.AddTC(newVoice.VoiceChannel.Guild.Id.ToString(), tempChannel.Id.ToString());
            await TempChannel.Helper.ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            await user.ModifyAsync(x => x.Channel = voiceChannel);
            await Handler.TempChannelHandler.WriteToConsol($"Information: {user.Guild.Name} | Task: CreateAndConnectToVoiceChannel | Guild: {user.Guild.Id} | Channel: {voiceChannel.Id} | {user} ({user.Id}) was successfully connected to {voiceChannel}");
        }

        public static async Task CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client, SocketGuild guild, DataTable tempchannelIDs)
        {
            var config = Program.GetConfig();

            foreach (DataRow row in tempchannelIDs.Rows)
            {
                var voiceChannel = client.Guilds
                    .SelectMany(g => g.Channels)
                    .SingleOrDefault(c => c.Id == ulong.Parse(row.Field<string>("channelid")));

                if (voiceChannel == null)
                {
                    tempchannels.RemoveTC(guild.Id.ToString(), row.Field<string>("channelid"));
                    continue;
                }

                if (voiceChannel.Users.Count == 0)
                {
                    await voiceChannel.DeleteAsync();
                    tempchannels.RemoveTC(guild.Id.ToString(), row.Field<string>("channelid"));
                    await Handler.TempChannelHandler.WriteToConsol($"Information: {guild.Name} | Task: CheckAndDeleteEmptyVoiceChannels | Guild: {guild.Id} | Channel: {row.Field<string>("channelid")} | Channel successfully deleted");
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
            var createTempChannelList = createtempchannels.GetCreateTempChannelListFromGuild(guild);
            string header = null;
            if (createTempChannelList.Rows.Count == 0)
            {
                header = "No create temp channels yet!";
                sb.AppendLine("You dont have any create-temp-channels yet!\nYou can add some with:\n`/tcadd`");
            }
            else
            {
                header = "Here a list of all create temp channels:";
            }

            foreach (DataRow row in createTempChannelList.Rows)
            {
                var channelId = row.Field<string>("createchannelid");
                var voiceChannel = client.Guilds
                                   .SelectMany(g => g.Channels)
                                   .SingleOrDefault(c => c.Id == ulong.Parse(channelId));
                if (voiceChannel == null)
                {
                    continue;
                }

                sb.AppendLine("");
                sb.AppendLine($"<#{channelId}>");
                sb.AppendLine($"Id: **{channelId}**");
                sb.AppendLine($"TempChannelName: **{row.Field<string>("tempchannelname")}**");
            }

            return TextChannel.TextChannel.CreateEmbed(interaction, sb.ToString(), header);
        }
        #endregion
    }
}