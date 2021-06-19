using Bobii.src.Commands;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
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
            ulong createTempChannelID = 853576181898805288;

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
            foreach (ulong id in _tempchannelIDs)
            {
                var voiceChannel = client.Guilds
                    .SelectMany(g => g.Channels)
                    .SingleOrDefault(c => c.Id == id);

                if (voiceChannel == null)
                {
                    return;
                }

                if (voiceChannel.Users.Count == 0)
                {
                    voiceChannel.DeleteAsync();
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

        private static void CreateAndConnectToVoiceChannel(SocketUser user, SocketVoiceState newVoice)
        {
            var category = newVoice.VoiceChannel.Category;
            var userName = user.ToString().Split("#");
            var tempChannel = CreateVoiceChannel(user as SocketGuildUser, userName[0] + " is sus...", category.Id);
            _tempchannelIDs.Add(tempChannel.Id);
            CommandHelper.EditConfig("TempChannels", tempChannel.Name, tempChannel.Id.ToString());
            ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static void ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            user.ModifyAsync(x => x.Channel = voiceChannel);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   {user} was conneted to {voiceChannel.Id}");
        }
        #endregion

        #region Functions
        public static RestVoiceChannel CreateVoiceChannel(SocketGuildUser user, string name, ulong catergoryId)
        {
            var channel = user.Guild.CreateVoiceChannelAsync(name, prop => prop.CategoryId = catergoryId);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   {user} created a new Channel -> ID: {channel.Result.Id}");
            return channel.Result;
        }
        #endregion
    }
}