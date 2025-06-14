using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using Discord;

namespace bobii_rework.Helper
{
    public static class ButtonHelper
    {
        public static async Task<ButtonBuilder> GetLinkButton(
            string spcLabel,
            string url,
            Language language,
            Emote emote = null)
        {
            var labelText = await LanguageRepository.GetCaption(spcLabel, language);

            return new ButtonBuilder()
                .WithCustomId(null)
                .WithLabel(labelText)
                .WithUrl(url)
                .WithEmote(emote)
                .WithStyle(ButtonStyle.Link);
        }

        public static async Task<ButtonBuilder> GetSetupButton(Language language)
        {
            return await ButtonHelper.GetButton(
                ButtonCustomIds.SetupCreatorChannel,
                Captions.SetupCreatorChannel,
                language,
                Emote.Parse(Configuration.GetConfigValue<string>(Configuration.SetupEmoteString)!));
        }

        public static async Task<ButtonBuilder> GetSupportServerButton(Language language)
        {
            return await ButtonHelper.GetLinkButton(
                Captions.SupportServer,
                Configuration.GetConfigValue<string>(Configuration.SupportServerInviteLink)!,
                language,
                Emote.Parse(Configuration.GetConfigValue<string>(Configuration.AppLogoEmoteString)!));
        }

        public static async Task<ButtonBuilder> GetDokumentationButton(Language language)
        {
            return await ButtonHelper.GetLinkButton(
                Captions.Dokumentation,
                Configuration.GetConfigValue<string>(Configuration.DokumentationUrl)!,
                language,
                Emote.Parse(Configuration.GetConfigValue<string>(Configuration.DocumentationEmoteString)));
        }

        public static async Task<ButtonBuilder> GetDashboardButton(Language language)
        {
            return await ButtonHelper.GetLinkButton(
                Captions.DashboardOeffnen,
                Configuration.GetConfigValue<string>(Configuration.DashboardUrl)!,
                language,
                Emote.Parse(Configuration.GetConfigValue<string>(Configuration.DashboardEmoteString)));
        }

        public static async Task<ButtonBuilder> GetButton(
            string customId,
            string spcLabel,
            Language language,
            Emote emote = null,
            ButtonStyle buttonStyle = ButtonStyle.Primary)
        {
            var labelText = await LanguageRepository.GetCaption(spcLabel, language);

            return new ButtonBuilder()
                .WithCustomId(customId)
                .WithLabel(labelText)
                .WithEmote(emote)
                .WithStyle(buttonStyle);
        }

        public static async Task<ButtonBuilder> GetInterfaceButton(string commandName, ulong emoteId)
        {
            Emote emote;
            try
            {
                emote = Emote.Parse($"<:interface_{commandName}:{emoteId}>");
            }
            catch (Exception ex)
            {
                ex.WriteLineToConsole("Emote nicht gefunden");
                var defaultInterfaceInformation = await InterfaceInformationsRepository.GetInterfaceDefaultInformation(commandName);
                emote = Emote.Parse($"<:interface_{commandName}:{defaultInterfaceInformation.EmoteId}");
            }

            return new ButtonBuilder()
                .WithCustomId($"interface-{commandName}-button")
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(emote);
        }
    }
}
