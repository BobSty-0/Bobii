using System.Drawing;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using System.Text;
using bobii_rework.Enums;
using bobii_rework.GlobalConstants.Interactions;
using Discord;

namespace bobii_rework.Helper
{
    public static class GeneralHelper
    {
        public static async Task<MessageComponent> GetJoinedGuildMessageComponent(Language language)
        {
            var languageSelectMenu = SelectMenuHelper.GetLanguageSelectMenu(language);

            var componentBuilder = new ComponentBuilder()
                .WithButton(await ButtonHelper.GetSetupButton(language))
                .WithButton(await ButtonHelper.GetDashboardButton(language))
                .WithButton(await ButtonHelper.GetSupportServerButton(language))
                .WithSelectMenu(languageSelectMenu);

            return componentBuilder.Build();
        }

        public static async Task<string> GetJoinedGuildText(Language language)
        {
            var caption = await LanguageRepository.GetCaption(Captions.JoinMessageCaption, language);
            var appEgoText = await LanguageRepository.GetContent(Contents.AppEgoText, language);
            var tempChannelCaption = await LanguageRepository.GetCaption(Captions.TempChannelCaption, language);
            var tempChannelIntro = await LanguageRepository.GetContent(Contents.TempChannelIntro, language);
            var dashboardCaption = await LanguageRepository.GetCaption(Captions.DashboardCaption, language);
            var dashboardHinweis = await LanguageRepository.GetContent(Contents.DashboardHinweis, language);
            var supportHinweisCaption = await LanguageRepository.GetCaption(Captions.IrgendwelcheFragen, language);
            var supportServerHinweis = await LanguageRepository.GetContent(Contents.SupportServerHinweis, language);

            var sb = new StringBuilder();
            sb.AppendLine(caption);
            sb.AppendLine(appEgoText);
            sb.AppendLine(tempChannelCaption);
            sb.AppendLine(tempChannelIntro);
            sb.AppendLine(dashboardCaption);
            sb.AppendLine(string.Format(dashboardHinweis, Configuration.GetConfigValue<string>(Configuration.DashboardUrl)));
            sb.AppendLine(supportHinweisCaption);
            sb.AppendLine(supportServerHinweis);

            return sb.ToString();
        }
    }
}
