using bobii_rework.Enums;
using bobii_rework.Extensions;
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
            string placeholder = "")
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
