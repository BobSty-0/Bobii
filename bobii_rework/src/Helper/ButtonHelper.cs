using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Discord;
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
                .WithSkuId(null)
                .WithLabel(labelText)
                .WithUrl(url)
                .WithEmote(emote)
                .WithStyle(ButtonStyle.Link);
        }

        public static async Task<ButtonBuilder> GetSetupButton(Language language)
        {
            var emote = await EmoteRepository.GetEmote(EmoteNames.setup);
            return await GetButton(
                ButtonCustomIds.SetupCreatorChannel,
                Captions.SetupCreatorChannel,
                language,
                Emote.Parse(emote.ToDiscordEmoteString()));
        }

        public static async Task<ButtonBuilder> GetSupportServerButton(Language language)
        {
            var emote = await EmoteRepository.GetEmote(EmoteNames.bobii_logo);

            return await GetLinkButton(
                Captions.SupportServer,
                Configuration.GetConfigValue<string>(Configuration.SupportServerInviteLink)!,
                language,
                Emote.Parse(emote.ToDiscordEmoteString()));
        }

        public static async Task<ButtonBuilder> GetDokumentationButton(Language language)
        {
            var emote = await EmoteRepository.GetEmote(EmoteNames.documentation);
            return await GetLinkButton(
                Captions.Dokumentation,
                Configuration.GetConfigValue<string>(Configuration.DokumentationUrl)!,
                language,
                Emote.Parse(emote.ToDiscordEmoteString()));
        }

        public static async Task<ButtonBuilder> GetDashboardButton(Language language)
        {
            var emote = await EmoteRepository.GetEmote(EmoteNames.@interface);

            return await ButtonHelper.GetLinkButton(
                Captions.DashboardOeffnen,
                Configuration.GetConfigValue<string>(Configuration.DashboardUrl)!,
                language,
                Emote.Parse(emote.ToDiscordEmoteString()));
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

        public static async Task<ButtonBuilder> GetInterfaceButton(Entities.EntityFramework.Emote emoteEntity, string commandName)
        {
            var emote = Emote.Parse(emoteEntity.ToDiscordEmoteString());

            return new ButtonBuilder()
                .WithCustomId($"interface-{commandName}-button")
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(emote);
        }
    }
}
