using Bobii.src.Commands;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Build.Tasks;
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
        private static List<ulong> _createTempChannelID = new List<ulong>();
        #endregion


        #region Methods
        public static void VoiceChannelActions(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            //TODO 13.05.2021 Hardcodierte Channel-ID ändern
            ulong createTempChannelID = 855888636700000287;

            if (oldVoice.VoiceChannel != null)
            {
                if (_tempchannelIDs.Count > 0)
                {
                    CheckAndDeleteEmptyVoiceChannels(client);
                    if(newVoice.VoiceChannel == null)
                    {
                       return; 
                    }
                }
            }

            if (newVoice.VoiceChannel != null)
            {
                if (newVoice.VoiceChannel.Id == createTempChannelID)
                {
                    CreateAndConnectToVoiceChannel(user, newVoice);
                }
            }
            else
            {
                return;
            }
        }

        public static void CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client)
        {
            _tempchannelIDs = GetTemplateChannelIDsListe();

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
                    voiceChannel.DeleteAsync();
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
                        config["TempChannels"].Value<JObject>(config["TempChannels"].First).Remove(id.ToString());
                        _tempchannelIDs.Remove(id);
                    }
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   Channel: {id} was successfully deleted");
                }
            }
        }

        private static void CreateAndConnectToVoiceChannel(SocketUser user, SocketVoiceState newVoice)
        {
            var category = newVoice.VoiceChannel.Category;
            var userName = user.ToString().Split("#");
            var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), userName[0] + " is sus...");
            _tempchannelIDs.Add(tempChannel.Id);
            CommandHelper.EditConfig("TempChannels", tempChannel.Id.ToString(), tempChannel.Name);
            ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static void ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            user.ModifyAsync(x => x.Channel = voiceChannel);
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

        public static List<ulong> GetTemplateChannelIDsListe()
        {
            List<ulong> tempchannelIDs = new List<ulong>();
            var config = BobiiHelper.GetConfig();
            foreach (JToken token in config["TempChannels"])
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