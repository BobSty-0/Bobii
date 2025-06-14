using Discord;

namespace bobii_rework.Extensions
{
    public static class InteractionExtension
    {
        #region Constants
        private const string ResourceFolderName = "Resources";
        private const string LoadingGifFileName = "Loading_Gif.gif";
        #endregion

        public static async Task RespondWithLoadingMessage(this IDiscordInteraction interaction)
        {
            await interaction.RespondWithFileAsync(GetLoadingGifFilePath(), LoadingGifFileName, ephemeral: true);
        }

        public static async Task ModifyOriginalResponseWithLoadingMessage(this IDiscordClient interClient)
        {

        }

        private static string GetLoadingGifFilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ResourceFolderName, LoadingGifFileName);
        }
    }
}
