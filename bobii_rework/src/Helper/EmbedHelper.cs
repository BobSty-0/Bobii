using Discord;

namespace bobii_rework.Helper
{
    public static class EmbedHelper
    {
        #region Methods
        public static Embed GetEmbed(string body, string header = "", string imgUrl = "")
        {
            var embed = new EmbedBuilder()
                .WithTitle(header)
                .WithColor(Configuration.GetBobiiColor())
                .WithImageUrl(imgUrl)
                .WithDescription(body)
                .WithCurrentTimestamp();

            return embed.Build();
        }
        #endregion
    }
}
