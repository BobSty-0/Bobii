using Discord;

namespace bobii_rework.Helper
{
    public class EmbedHelper
    {
        #region Methods
        public static Embed GetEmbed(string body, string header)
        {
            var embed = new EmbedBuilder()
                .WithTitle(header)
                .WithColor(Configuration.GetBobiiColor())
                .WithDescription(body)
                .WithCurrentTimestamp();

            return embed.Build();
        }
        #endregion
    }
}
