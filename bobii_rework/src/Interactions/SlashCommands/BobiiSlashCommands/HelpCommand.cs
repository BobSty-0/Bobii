using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.src.Enums;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using bobii_rework.src.Helper;
using Discord.Interactions;
using System.Text;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    internal class HelpCommand(InteractionContext context) : BobiiInteractionBase(context, InteractionReactionType.Defer)
    {
        public override async Task ExecuteCommand()
        {
            var text = await GetText(Context.Language);
            var components = await MessageComponentHelper.GetHelpCommandComponents(Context.Language);

            await Context.ModifyOriginalResponse(
                "",
                text,
                messageComponent: components);
        }

        private async Task<string> GetText(Language language)
        {
            var caption = await Context.GetCaptionAsync(Captions.AppInformationsCaption);
            var appEgoText = await Context.GetContentAsync(Contents.AppEgoText);
            var tempChannelCaption = await Context.GetCaptionAsync(Captions.TempChannelCaption);
            var tempChannelIntro = await Context.GetContentAsync(Contents.TempChannelIntro);
            var dashboardCaption = await Context.GetCaptionAsync(Captions.DashboardCaption);
            var dashboardHinweis = await Context.GetContentAsync(Contents.DashboardHinweis);
            var dokumentationCaption = await Context.GetCaptionAsync(Captions.DokumentationCaption);
            var dokumentationHinweis = await Context.GetContentAsync(Contents.DokumentationHinweis);
            var supportHinweisCaption = await Context.GetCaptionAsync(Captions.IrgendwelcheFragen);
            var supportServerHinweis = await Context.GetContentAsync(Contents.SupportServerHinweis);

            var sb = new StringBuilder();
            sb.AppendLine(caption);
            sb.AppendLine(appEgoText);
            sb.AppendLine(tempChannelCaption);
            sb.AppendLine(tempChannelIntro);
            sb.AppendLine(dashboardCaption);
            sb.AppendLine(string.Format(dashboardHinweis, Configuration.GetConfigValue<string>(Configuration.DashboardUrl)));
            sb.AppendLine(dokumentationCaption);
            sb.AppendLine(dokumentationHinweis);
            sb.AppendLine(supportHinweisCaption);
            sb.AppendLine(supportServerHinweis);

            return sb.ToString();
        }
    }
}
