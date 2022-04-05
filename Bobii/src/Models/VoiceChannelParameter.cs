using Discord.WebSocket;

namespace Bobii.src.Models
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
