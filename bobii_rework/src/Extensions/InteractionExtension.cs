using bobii_rework.GlobalConstants.Discord;
using bobii_rework.Repositories;
using Discord;

namespace bobii_rework.Extensions
{
    public static class InteractionExtension
    {
        #region Constants
        private const string ResourceFolderName = "Resources";
        private const string LoadingGifFileName = "loading.gif";
        #endregion

        public static async Task RespondWithLoadingMessage(this IDiscordInteraction interaction)
        {
            var emote = await EmoteRepository.GetEmote(EmoteNames.loading);
            await interaction.RespondAsync($"<a:{emote.Name}:{emote.EmoteId}>", ephemeral: true);
        }
    }
}
