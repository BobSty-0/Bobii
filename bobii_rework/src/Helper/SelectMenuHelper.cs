using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord;

namespace bobii_rework.Helper
{
    public static class SelectMenuHelper
    {
        #region Methods
        public static async Task<SelectMenuBuilder> GetLanguageSelectMenu(Language currentLanguage)
        {
            var options = await GetLanguageOptions(currentLanguage);
            return GetSelectMenu(SelectMenuCustomIds.GuildJoinedLanguage, options);
        }

        public static async Task<SelectMenuBuilder> GetPrivacySelectMenu(ulong guildId, Language language)
        {
            var placeholder = await LanguageRepository.GetCaption(Captions.ChooseAction, language);
            var options = await GetTempPrivacyOptions(guildId, language);
            return GetSelectMenu(SelectMenuCustomIds.TempChannelPrivacy, options, placeholder: placeholder);
        }

        public static SelectMenuBuilder GetUserSelectMenu(
            string customId,
            string placeholder)
        {
            return GetSelectMenu(customId, componentType: ComponentType.UserSelect, placeholder: placeholder);
        }

        public static SelectMenuBuilder GetSelectMenu(
            string customId,
            List<SelectMenuOptionBuilder> options = null,
            ComponentType componentType = ComponentType.SelectMenu,
            string placeholder = "[TODO Placeholder]")
        {
            var builder = new SelectMenuBuilder()
                .WithCustomId(customId)
                .WithType(componentType)
                .WithPlaceholder(placeholder);

            if (options != null && options.Any())
            {
                builder.WithOptions(options);
            }

            return builder;
        }
        #endregion

        #region PrivateMethdos
        private static async Task<List<SelectMenuOptionBuilder>> GetTempPrivacyOptions(ulong guildId, Language language)
        {
            var options = new List<SelectMenuOptionBuilder>();

            var option = await GetCommandOption(
                Captions.LockYourVoiceChannel,
                SlashCommandNames.Lock,
                SelectMenuValues.TempChannelLock,
                guildId,
                language);

            options.Add(option);

            option = await GetCommandOption(
                Captions.UnlockYourVoiceChannel,
                SlashCommandNames.Unlock,
                SelectMenuValues.TempChannelUnlock,
                guildId,
                language);

            options.Add(option);

            option = await GetCommandOption(
                Captions.HideYourVoiceChannel,
                SlashCommandNames.Hide,
                SelectMenuValues.TempChannelHide,
                guildId,
                language);

            options.Add(option);

            option = await GetCommandOption(
                Captions.UnhideYourVoiceChannel,
                SlashCommandNames.Unhide,
                SelectMenuValues.TempChannelUnhide,
                guildId,
                language);

            options.Add(option);

            return options;
        }

        private static async Task<SelectMenuOptionBuilder> GetCommandOption(
            string captionSpc,
            string commandName,
            string selectionMenuValue,
            ulong guildId, Language language)
        {
            var caption = await LanguageRepository.GetCaption(captionSpc, language);
            var interfaceInformation = await InterfaceInformationsRepository.GetInterfaceInformationMitFallback(guildId, commandName);
            var emoteEntity = await EmoteRepository.GetEmote(interfaceInformation.EmoteId);
            var emote = Emote.Parse(emoteEntity.ToDiscordEmoteString());

            return new SelectMenuOptionBuilder()
                .WithLabel(caption)
                .WithValue(selectionMenuValue)
                .WithEmote(emote);
        }

        private static async Task<List<SelectMenuOptionBuilder>> GetLanguageOptions(Language currentLanguage)
        {
            var enumValues = Enum.GetValues(typeof(Language)).Cast<Language>();
            var options = new List<SelectMenuOptionBuilder>();
            foreach (var language in enumValues)
            {
                var emoteEntity = await EmoteRepository.GetEmote($"{language}_flag");
                var emote = Emote.Parse(emoteEntity.ToDiscordEmoteString());
                var option = new SelectMenuOptionBuilder()
                    .WithLabel(language.GetChoiceDisplay())
                    .WithValue(language.ToString())
                    .WithEmote(emote)
                    .WithDefault(language == currentLanguage);

                options.Add(option);
            }

            return options;
        }
        #endregion
    }
}
