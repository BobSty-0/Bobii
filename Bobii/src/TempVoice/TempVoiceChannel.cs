using Bobii.src.HelpFunctions;
using Bobii.src.HelpMethods;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Bobii.src.TempVoice
{
    class TempVoiceChannel
    {
        #region Declarations
        private static List<ulong> _tempchannelIDs = new List<ulong>();
        #endregion

        public static void VoiceChannelActions(SocketUser user, SocketVoiceState voice1, DiscordSocketClient client)
        {
            //TODO 13.05.2021 Hardcodierte Channel-ID ändern
            ulong createTempChannelID = 853576181898805288;

            if (voice1.VoiceChannel == null)
            {
                return;
            }

            if (voice1.VoiceChannel.Id == createTempChannelID)
            {
                CreateAndConnectToVoiceChannel(user, voice1);
            }

            if (_tempchannelIDs.Count > 0)
            {
                CheckAndDeleteEmptyVoiceChannels(client);
            }
        }

        private static void CreateAndConnectToVoiceChannel(SocketUser user, SocketVoiceState voice1)
        {
            var category = voice1.VoiceChannel.Category;
            var tempChannel = Functions.CreateVoiceChannel(user as SocketGuildUser, "test", category.Id);
            _tempchannelIDs.Add(tempChannel.Id);
            Methods.ConnectToVoice(tempChannel, user as IGuildUser);
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
                    _tempchannelIDs.Remove(id);
                }
            }
        }
    }
}