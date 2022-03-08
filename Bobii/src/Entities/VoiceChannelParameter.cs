using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Entities
{
    public class VoiceUpdatedParameter
    {
        public SocketUser SocketUser;
        public SocketVoiceState OldVoiceState;
        public SocketVoiceState NewVoiceState;
        public DiscordSocketClient Client;
        public SocketVoiceChannel OldSocketVoiceChannel;
        public SocketVoiceChannel NewSocketVoiceChannel;
        public SocketGuild Guild;
        public Bobii.Enums.VoiceUpdated VoiceUpdated;
        public TempChannel.DelayOnDelete DelayOnDelete;
    }
}
