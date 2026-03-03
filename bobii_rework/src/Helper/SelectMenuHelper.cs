using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Discord;
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
        public static SelectMenuBuilder GetLanguageSelectMenu(Language currentLanguage)
        {
            var options = GetLanguageOptions(currentLanguage);
            return GetSelectMenu(SelectMenuCustomIds.GuildJoinedLanguage, options);
        }

        public static async Task<SelectMenuBuilder> GetPrivacySelectMenu(Language language)
        {
            var placeholder = await LanguageRepository.GetCaption(Captions.ChooseAction, language);
            var options = await GetTempPrivacyOptions(language);
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
        private static async Task<List<SelectMenuOptionBuilder>> GetTempPrivacyOptions(Language language)
        {
            var options = new List<SelectMenuOptionBuilder>();

            var lockCaption = await LanguageRepository.GetCaption(Captions.LockYourVoiceChannel, language);
            var lockEmoteEntity = await EmoteRepository.GetEmote(EmoteNames.interface_lock);
            var lockEmote = Emote.Parse($"<:{lockEmoteEntity.Name}:{lockEmoteEntity.EmoteId}>");
            var option = new SelectMenuOptionBuilder()
                .WithLabel(lockCaption)
                .WithValue(SelectMenuValues.TempChannelLock)
                .WithEmote(lockEmote);

            options.Add(option);

            return options;
        }

        private static List<SelectMenuOptionBuilder> GetLanguageOptions(Language currentLanguage)
        {
            var enumValues = Enum.GetValues(typeof(Language)).Cast<Language>();
            var options = new List<SelectMenuOptionBuilder>();
            foreach (var language in enumValues)
            {
                var emoteString = Configuration.GetConfigValue<string>($"{language.ToString().ToUpper()}_EmoteString");
                var emote = Emote.Parse(emoteString);
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
