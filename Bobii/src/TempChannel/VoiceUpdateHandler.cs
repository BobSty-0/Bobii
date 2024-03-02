using Bobii.src.Enums;
using Bobii.src.Helper;
using Bobii.src.Models;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    public class VoiceUpdateHandler
    {
        #region Static Tasks
        public static async Task HandleVoiceUpdated(SocketVoiceState oldVoice, SocketVoiceState newVoice, SocketUser user, DiscordShardedClient client, DelayOnDelete delayOnDeleteClass)
        {
            var parameter = TempChannelHelper.GetVoiceUpdatedParameter(oldVoice, newVoice, user, client, delayOnDeleteClass).Result;
            await Handle(parameter);
        }
        #endregion

        #region Tasks
        public static async Task Handle(VoiceUpdatedParameter parameter)
        {
            switch (parameter.VoiceUpdated)
            {
                case VoiceUpdated.ChannelDestroyed:
                    //Nothing
                case VoiceUpdated.UserJoinedAChannel:
                    await TempChannelHelper.HandleUserJoinedChannel(parameter);
                    break;
                case VoiceUpdated.UserLeftAChannel:
                    await TempChannelHelper.HandleUserLeftChannel(parameter);
                    break;
                case VoiceUpdated.UserLeftAndJoinedChannel:
                    await TempChannelHelper.HandleUserJoinedChannel(parameter);
                    await TempChannelHelper.HandleUserLeftChannel(parameter);
                    break;
            }
        }
        #endregion
    }
}
