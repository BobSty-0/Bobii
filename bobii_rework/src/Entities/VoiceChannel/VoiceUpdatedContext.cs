using bobii_rework.Entities.Interactions;
using bobii_rework.Enums;
using Discord.WebSocket;

namespace bobii_rework.src.Entities.VoiceChannel
{
    public class VoiceUpdatedContext : BobiiInteractionContext
    {
        public SocketVoiceChannel OldVoiceChannel { get; set; }
        public SocketVoiceChannel NewVoiceChannel { get; set; }
        public VoiceAction VoiceAction { get; set; }
    }
}
