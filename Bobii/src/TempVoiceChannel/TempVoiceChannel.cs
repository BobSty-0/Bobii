using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bobii.src.DBStuff.Tables;

namespace Bobii.src.TempVoiceChannel
{
    class TempVoiceChannel
    {
        #region Declarations
        private static DataTable _createTempChannelIDs = new DataTable();
        private static DataTable _tempchannelIDs = new DataTable();
        #endregion

        #region Tasks
        public static async Task VoiceChannelActions(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            string guildId;
            if (newVoice.VoiceChannel != null)
            {
                guildId = newVoice.VoiceChannel.Guild.Id.ToString();
            }
            else
            {
                guildId = oldVoice.VoiceChannel.Guild.Id.ToString();
            }
            _createTempChannelIDs = createtempchannels.GetCreateTempChannelListFromGuild(guildId);
            _tempchannelIDs = tempchannels.GetTempChannelList(guildId);

            if (oldVoice.VoiceChannel != null)
            {
                if (_tempchannelIDs.Rows.Count > 0)
                {
                    await CheckAndDeleteEmptyVoiceChannels(client, guildId);
                    if (newVoice.VoiceChannel == null)
                    {
                        return;
                    }
                }
            }

            if (newVoice.VoiceChannel != null)
            {
                foreach (DataRow row in _createTempChannelIDs.Rows)
                {
                    if (newVoice.VoiceChannel.Id.ToString() == row.Field<string>("createchannelid"))
                    {
                        await CreateAndConnectToVoiceChannel(user, newVoice, row.Field<string>("tempchannelname"));
                    }
                }
            }
            else
            {
                return;
            }
        }

        public static async Task CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client, string guildid)
        {
            _tempchannelIDs = tempchannels.GetTempChannelList(guildid);

            var config = Program.GetConfig();

            foreach (DataRow row in _tempchannelIDs.Rows)
            {
                var voiceChannel = client.Guilds
                    .SelectMany(g => g.Channels)
                    .SingleOrDefault(c => c.Id == ulong.Parse(row.Field<string>("channelid")));

                if (voiceChannel == null)
                {
                    tempchannels.RemoveTC(guildid, row.Field<string>("channelid"));
                    continue;
                }

                if (voiceChannel.Users.Count == 0)
                {
                    await voiceChannel.DeleteAsync();
                    tempchannels.RemoveTC(guildid, row.Field<string>("channelid"));
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   Channel: {row.Field<string>("channelid")} was successfully deleted");
                }
            }
        }

        private static async Task CreateAndConnectToVoiceChannel(SocketUser user, SocketVoiceState newVoice, string name)
        {
            var category = newVoice.VoiceChannel.Category;
            string channelName = name.Trim();
            if (channelName.Contains("User"))
            {
                channelName = channelName.Replace("User", user.Username);
            }

            var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), channelName);
            tempchannels.AddTC(newVoice.VoiceChannel.Guild.Id.ToString(), tempChannel.Id.ToString());
            await ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            await user.ModifyAsync(x => x.Channel = voiceChannel);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   {user} was conneted to {voiceChannel.Id}");
        }
        #endregion

        #region Functions
        public static RestVoiceChannel CreateVoiceChannel(SocketGuildUser user, string catergoryId, string name)
        {
            var channel = user.Guild.CreateVoiceChannelAsync(name, prop => prop.CategoryId = ulong.Parse(catergoryId));
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   {user} created a new Channel -> ID: {channel.Result.Id}");
            return channel.Result;
        }

        public static Embed CreateVoiceChatInfoEmbed(string guildId, DiscordSocketClient client, SocketInteraction interaction)
        {
            var config = Program.GetConfig();
            StringBuilder sb = new StringBuilder();
            var createTempChannelList = createtempchannels.GetCreateTempChannelListFromGuild(guildId);
            string header = null;
            if (createTempChannelList.Rows.Count == 0)
            {
                header = "No CreateTempChannels yet!";
                sb.AppendLine("You dont have any create temp voicechannels yet!\nYou can add some with:\n **/tempadd <CreateTempChannelID> <TempChannelName>**");
            }
            else
            {
                header = "Here a list of all CreateTempChannels:";
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
                sb.AppendLine($"Name: **{voiceChannel.Name}**");
                sb.AppendLine($"Id: **{channelId}**");
                sb.AppendLine($"TempChannelName: **{row.Field<string>("tempchannelname")}**");
            }

            return TextChannel.TextChannel.CreateEmbed(interaction, sb.ToString(), header) ;
        }
        #endregion
    }
}