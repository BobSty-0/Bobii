using Bobii.src.Bobii.Enums;
using Bobii.src.Entities;
using Bobii.src.EntityFramework.Entities;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    public class VoiceUpdateHandler
    {
        #region Static Tasks
        public static async Task HandleVoiceUpdated(SocketVoiceState oldVoice, SocketVoiceState newVoice, SocketUser user, DiscordSocketClient client, List<SocketUser> cooldownList)
        {
            var parameter = Helper.GetVoiceUpdatedParameter(oldVoice, newVoice, user, client).Result;
            await Handle(parameter, cooldownList);
        }
        #endregion

        #region Tasks
        public static async Task Handle(VoiceUpdatedParameter parameter, List<SocketUser> cooldownList)
        {
            switch (parameter.VoiceUpdated)
            {
                case VoiceUpdated.ChannelDestroyed:
                    //Nothing
                case VoiceUpdated.UserJoinedAChannel:
                    await Helper.HandleUserJoinedChannel(parameter, cooldownList);
                    break;
                case VoiceUpdated.UserLeftAChannel:
                    await Helper.HandleUserLeftChannel(parameter);
                    break;
                case VoiceUpdated.UserLeftAndJoinedChannel:
                    await Helper.HandleUserJoinedChannel(parameter, cooldownList);
                    await Helper.HandleUserLeftChannel(parameter);
                    break;
            }
        }
        #endregion
    }
}
