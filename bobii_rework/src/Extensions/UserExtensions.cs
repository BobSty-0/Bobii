using bobii_rework.Enums;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using Discord;

namespace bobii_rework.Extensions
{
    public static class UserExtensions
    {
        #region Methods
        public static async Task SendDm(this IUser user, string spcBody, Language language, object[]? parameters = null)
        {
            var text = await LanguageRepository.GetContent(spcBody, language);

            if (parameters != null)
            {
                text = string.Format(text, parameters);
            }

            await user.SendMessageAsync(text, components: await GetDeleteDmButton(language));
        }
        #endregion

        #region Private Functions
        private static async Task<MessageComponent> GetDeleteDmButton(Language language)
        {
            var text = await LanguageRepository.GetCaption(Captions.Delete, language);
            return new ComponentBuilder().WithButton(text, ButtonCustomIds.DmDelete, ButtonStyle.Secondary).Build();
        }
        #endregion

    }
}
