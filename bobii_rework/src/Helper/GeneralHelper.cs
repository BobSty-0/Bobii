using bobii_rework.Enums;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using ImageMagick;
using System.Text;

namespace bobii_rework.Helper
{
    public static class GeneralHelper
    {
        public static string GetFileExtension(MagickFormat format)
        {
            return format switch
            {
                MagickFormat.WebP => ".webp",
                _ => throw new Exception($"[{format}] is not supported as file extension.")
            };
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
