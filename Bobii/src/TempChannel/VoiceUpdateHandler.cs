using Bobii.src.Bobii.Enums;
using Bobii.src.Models;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    public class VoiceUpdateHandler
    {
        #region Static Tasks
        public static async Task HandleVoiceUpdated(SocketVoiceState oldVoice, SocketVoiceState newVoice, SocketUser user, DiscordSocketClient client, DelayOnDelete delayOnDeleteClass)
        {
            var parameter = Helper.GetVoiceUpdatedParameter(oldVoice, newVoice, user, client, delayOnDeleteClass).Result;
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
                    await Helper.HandleUserJoinedChannel(parameter);
                    break;
                case VoiceUpdated.UserLeftAChannel:
                    await Helper.HandleUserLeftChannel(parameter);
                    break;
                case VoiceUpdated.UserLeftAndJoinedChannel:
                    await Helper.HandleUserJoinedChannel(parameter);
                    await Helper.HandleUserLeftChannel(parameter);
                    break;
            }
        }
        #endregion
    }
}
