using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempVoiceChannel
{
    class TempVoiceChannel
    {
        #region Declarations
        private static List<ulong> _tempchannelIDs = new List<ulong>();
        private static List<ulong> _createTempChannelIDs = new List<ulong>();
        #endregion

        #region Methods
        public static async Task VoiceChannelActions(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            _createTempChannelIDs = GetObjectIDsListe("CreateTempChannels");
            //Das Ganze hier noch umbauen so dass für alle Server und die Namen der erstellten Channels stimmen, des Weiteren zugriff pro
            //Serverconfig ermöglichen!

            if (oldVoice.VoiceChannel != null)
            {
                if (_tempchannelIDs.Count > 0)
                {
                    await CheckAndDeleteEmptyVoiceChannels(client);
                    if (newVoice.VoiceChannel == null)
                    {
                        return;
                    }
                }
            }

            if (newVoice.VoiceChannel != null)
            {
                foreach (var id in _createTempChannelIDs)
                {
                    if (newVoice.VoiceChannel.Id == id)
                    {
                        await CreateAndConnectToVoiceChannel(user, newVoice);
                    }
                }
            }
            else
            {
                return;
            }
        }

        public static async Task CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client)
        {
            _tempchannelIDs = GetObjectIDsListe("TempChannels");

            var config = Program.GetConfig();

            foreach (ulong id in _tempchannelIDs)
            {
                var voiceChannel = client.Guilds
                    .SelectMany(g => g.Channels)
                    .SingleOrDefault(c => c.Id == id);

                if (voiceChannel == null)
                {
                    continue;
                }

                if (voiceChannel.Users.Count == 0)
                {
                    await voiceChannel.DeleteAsync();
                    //If im removing the last Id from the List it will throw an unhandled exception so im
                    //just creating a new list<ulong> instead of deleting the last member of the list
                    if (_tempchannelIDs.Count == 1)
                    {
                        _tempchannelIDs = new List<ulong>();
                    }
                    else
                    {
                        _tempchannelIDs.Remove(id);
                    }
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   Channel: {id} was successfully deleted");
                }
            }
        }

        private static async Task CreateAndConnectToVoiceChannel(SocketUser user, SocketVoiceState newVoice)
        {
            var category = newVoice.VoiceChannel.Category;
            var userName = user.ToString().Split("#");
            var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), userName[0] + " is sus...");
            _tempchannelIDs.Add(tempChannel.Id);
            TextChannel.TextChannel.EditConfig("TempChannels", tempChannel.Id.ToString(), tempChannel.Name);
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

        public static List<ulong> GetObjectIDsListe(string Object)
        {
            List<ulong> tempchannelIDs = new List<ulong>();
            var config = Program.GetConfig();
            foreach (JToken token in config[Object])
            {
                foreach (JToken key in token)
                {
                    string keyText = key.ToString().Replace("\"", "");
                    keyText = keyText.Replace(":", "");
                    var keyValueName = keyText.Split(" ");
                    tempchannelIDs.Add(ulong.Parse(keyValueName[0]));
                }
            }
            return tempchannelIDs;
        }

        public static Embed CreateVoiceChatInfoEmbed()
        {
            var config = Program.GetConfig();
            StringBuilder sb = new StringBuilder();
            if (config["CreateTempChannels"].ToString() == "[\r\n  {}\r\n]")
            {
                sb.AppendLine("**You dont have any create temp voicechannels yet!**\nYou can add some with: \"'cvcadd <id>\"");
            }
            else
            {
                sb.AppendLine("**Here a list of all create temp voice channels:**");
            }

            foreach (JToken token in config["CreateTempChannels"])
            {
                foreach (JToken key in token)
                {
                    string keyText = key.ToString().Replace("\"", "");
                    keyText = keyText.Replace(":", "");
                    var keyValueName = keyText.Split(" ");
                    sb.AppendLine("");

                    var count = keyValueName.Count();
                    if (count > 2)
                    {
                        sb.Append("**Name:**");
                        //In case there are spacebars in the voicechannel name
                        for (int zaehler = 1; zaehler < count; zaehler++)
                        {
                            if (zaehler == count - 1)
                            {
                                sb.AppendLine(" " + keyValueName[zaehler]);
                            }
                            else
                            {
                                sb.Append(" " + keyValueName[zaehler]);
                            }
                        }
                        sb.AppendLine("**Voicechat ID:** " + keyValueName[0]);
                    }
                    else
                    {
                        sb.AppendLine("**Name:** " + keyValueName[1]);
                        sb.AppendLine("**Voicechat ID:** " + keyValueName[0]);
                    }
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
            .WithColor(0, 225, 225)
            .WithDescription(sb.ToString());

            return embed.Build();
        }

        public static Embed CreateEmbed(string message)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            EmbedBuilder embed = new EmbedBuilder()
            .WithColor(0, 225, 225)
            .WithDescription(message);

            return embed.Build();
        }
        #endregion
    }
}