using bobii_rework.Enums;
using Discord;

namespace bobii_rework.Entities.Interactions
{
    public class BobiiInteractionContext
    {
        public IDiscordClient? Client { get; set; }
        public IGuild? Guild { get; set; }
        public IGuildUser? User { get; set; }
        public Language Language { get; set; }
        public IDiscordInteraction? Interaction { get; set; }
        public bool HasResponded { get; set; }
    }
}
