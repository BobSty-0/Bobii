using bobii_rework.Enums;
using bobii_rework.Repositories;
using bobii_rework.src.Helper;
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

            await user.SendMessageAsync(text, components: await MessageComponentHelper.GetDeleteDmButton(language));
        }
        #endregion
    }
}
