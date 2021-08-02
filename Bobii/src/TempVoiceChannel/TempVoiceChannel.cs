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

        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   {message}");
            await Task.CompletedTask;
        }
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
                    WriteToConsol($"Information: | Task: CheckAndDeleteEmptyVoiceChannels | Guild: {guildid} | Channel: {row.Field<string>("channelid")} | Channel successfully deleted");
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
            WriteToConsol($"Information: | Task: CreateAndConnectToVoiceChannel | Guild: {user.Guild.Id} | Channel: {voiceChannel.Id} | {user} was successfully connected to {voiceChannel}");
        }
        #endregion

        #region Functions
        public static string StepByStepTcadd()
        {
            return "**Step 1:**\n" +
                                "_Make sure you are in developer mode._\n" +
                                "To do this, go into your settings, select the 'Advanced' setting in the 'App Settings' category and make sure that developer mode is turned on.\n" +
                                "\n" +
                                "**Step 2:**\n" +
                                "_Choose the voice channel you want to add as create-temp-channel._\n" +
                                "Keep in mind you need an already existing voice channel.\n" +
                                "\n" +
                                "**Step 3:**\n" +
                                "_Get the ID of the choosen voice channel._\n" +
                                "To get the ID, right click on the choosen voice channel and then click on 'Copy ID'.\n" +
                                "\n" +
                                "**Step 4:**\n" +
                                "_Choose the name of the temp-channel_\n" +
                                "This will be the name of the created temp-channel. One little thing I implemented is that the word 'User' is replaced with the username.\n" +
                                "Example:\n" +
                                "I call the temp-channel name 'User's channel' (My username = BobSty)\n" +
                                "The output would be:\n" +
                                "BobSty's channel\n" +
                                "\n" +
                                "**Step 5:**\n" +
                                "_Use the given command `/tcadd`_\n" +
                                "First write `/tcadd` in any given text channel. Then use the `Tab` key to select the `voicechannelID` parameter, here you have to insert the ealier copied voice channel ID." +
                                "\nNext use the `Tab` key again to switch to the next parameter which is the `tempchannelname`, here you have put in the earlier choosen temp-channel name.\n" +
                                "After that you only have to use the `Enter` key and the create-temp-channel will be created.\n" +
                                "\n" +
                                "**Step 6:**\n" +
                                "_Test your create-temp-channel_\n" +
                                "To test your channel you can now join the voice channel whose ID you used in the command `/tcadd`. This should then create a temporary voice channel with the temp-channel name.\n" +
                                "\n" +
                                "If you have any issues with this command/guid feel free to msg me on Discord: `BobSty#0815`";
        }
        public static RestVoiceChannel CreateVoiceChannel(SocketGuildUser user, string catergoryId, string name)
        {
            var channel = user.Guild.CreateVoiceChannelAsync(name, prop => prop.CategoryId = ulong.Parse(catergoryId));
            var parsedChannel = channel.Result;
            var premissionOverrides = new OverwritePermissions()
                .Modify(null, PermValue.Allow);
            parsedChannel.AddPermissionOverwriteAsync(user, premissionOverrides);

            WriteToConsol($"Information: | Task: CreateVoiceChannel | Guild: {user.Guild.Id} | Channel: {channel.Result.Id} | {user} created new voice channel {channel.Result}");
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
                header = "No create temp channels yet!";
                sb.AppendLine("You dont have any _create temp channels_ yet!\nYou can add some with:\n **/tcadd <CreateTempChannelID> <TempChannelName>**");
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
                sb.AppendLine($"Name: **{voiceChannel.Name}**");
                sb.AppendLine($"Id: **{channelId}**");
                sb.AppendLine($"TempChannelName: **{row.Field<string>("tempchannelname")}**");
            }

            return TextChannel.TextChannel.CreateEmbed(interaction, sb.ToString(), header) ;
        }
        #endregion
    }
}