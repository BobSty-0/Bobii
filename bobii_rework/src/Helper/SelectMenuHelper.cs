using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json.Linq;

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

        public static SelectMenuBuilder GetSelectMenu(string customId, List<SelectMenuOptionBuilder> options)
        {
            return new SelectMenuBuilder()
                .WithCustomId(customId)
                .WithType(ComponentType.SelectMenu)
                .WithOptions(options);
        }
        #endregion

        #region PrivateMethdos
        private static List<SelectMenuOptionBuilder> GetLanguageOptions(Language currentLanguage)
        {
            var enumValues =  Enum.GetValues(typeof(Language)).Cast<Language>();
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
