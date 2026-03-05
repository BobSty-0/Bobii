using bobii_rework.Enums;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord;

namespace bobii_rework.src.Helper
{
    public static class MessageComponentHelper
    {
        #region Tasks
        public static async Task<MessageComponent> GetDeleteDmButton(Language language)
        {
            var text = await LanguageRepository.GetCaption(Captions.Delete, language);
            return new ComponentBuilder().WithButton(text, ButtonCustomIds.DmDelete, ButtonStyle.Secondary).Build();
        }

        public static async Task<MessageComponent> GetTempPrivacyMessageComponent(ulong guildId, Language language)
        {
            var privacySelectionMenu = await SelectMenuHelper.GetPrivacySelectMenu(guildId, language);
            var componentBuilder = new ComponentBuilder()
                .WithSelectMenu(privacySelectionMenu);

            return componentBuilder.Build();
        }

        public static async Task<MessageComponent> GetTempGiveOwnerUserSelectMessageComponent(Language language)
        {
            var placeholder = await LanguageRepository.GetCaption(Captions.ChooseOwner, language);

            var languageSelectMenu = SelectMenuHelper.GetUserSelectMenu(SelectMenuCustomIds.TempChannelGiveOwner, placeholder);
            var componentBuilder = new ComponentBuilder()
                .WithSelectMenu(languageSelectMenu);

            return componentBuilder.Build();
        }

        public static async Task<MessageComponent> GetJoinedGuildMessageComponent(Language language)
        {
            var languageSelectMenu = await SelectMenuHelper.GetLanguageSelectMenu(language);

            var componentBuilder = new ComponentBuilder()
                .WithButton(await ButtonHelper.GetSetupButton(language))
                .WithButton(await ButtonHelper.GetDashboardButton(language))
                .WithButton(await ButtonHelper.GetSupportServerButton(language))
                .WithSelectMenu(languageSelectMenu);

            return componentBuilder.Build();
        }

        public static async Task<MessageComponent> GetHelpCommandComponents(Language language)
        {
            var componentBuilder = new ComponentBuilder()
                .WithButton(await ButtonHelper.GetSetupButton(language))
                .WithButton(await ButtonHelper.GetDashboardButton(language))
                .WithButton(await ButtonHelper.GetDokumentationButton(language))
                .WithButton(await ButtonHelper.GetSupportServerButton(language));

            return componentBuilder.Build();
        }

        public static async Task<MessageComponent> GetDashboardButtonMessageComponent(Language language, string url)
        {
            return new ComponentBuilder()
                .WithButton(await ButtonHelper.GetDashboardButton(language))
                .Build();
        }
        #endregion
    }
}
