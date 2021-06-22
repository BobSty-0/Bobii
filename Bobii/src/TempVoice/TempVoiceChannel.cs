using Bobii.src.Commands;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.TempVoice
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
            //TODO 13.05.2021 Hardcodierte Channel-ID ändern
            ulong createTempChannelID = 855888636700000287;

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

            var config = BobiiHelper.GetConfig();

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
                        config["TempChannels"][0][id].Remove();
                    }
                    else
                    {
                        //TODO JG 19.06.2021 Check out how to delete a key from the config.json
                        CommandHelper.DeletConfig("TempChannels", id.ToString());
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
            CommandHelper.EditConfig("TempChannels", tempChannel.Id.ToString(), tempChannel.Name);
            await ConnectToVoice (tempChannel, user as IGuildUser);
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
            var config = BobiiHelper.GetConfig();
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
        #endregion
    }
}