using bobii_rework.Entities.EntityFramework;

namespace bobii_rework.src.Extensions
{
    public static class EmoteExtensions
    {
        public static string ToDiscordEmoteString(this Emote emote)
        {
            if (emote.Animated)
            {
                return $"<a:{emote.Name}:{emote.EmoteId}";
            }

            return $"<:{emote.Name}:{emote.EmoteId}";
        }
    }
}
