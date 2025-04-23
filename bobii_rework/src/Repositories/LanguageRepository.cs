using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using bobii_rework.Extensions;
using Microsoft.EntityFrameworkCore;
using Language = bobii_rework.Enums.Language;

namespace bobii_rework.Repositories
{
    public static class LanguageRepository
    {
        #region Declarations
        private const string MissingText = "[Missing Text]";
        private const string ErrorText = "[Error]";
        #endregion

        #region Methods
        public static async Task ChangeLanguage(ulong guildId, Language language)
        {
            await using var context = new BobiiLngContext();
            var languageEntity = await context.Languages.SingleOrDefaultAsync(l => l.guildid == guildId);
            if (languageEntity != null)
            {
                languageEntity.langugeshort = language.ToString();
            }
            else
            {
                languageEntity = new language()
                {
                    guildid = guildId,
                    langugeshort = language.ToString()
                };
                context.Languages.Add(languageEntity);
            }

            await context.SaveChangesAsync();
        }
        public static async Task RemoveLanguageIfExisting(ulong guildId)
        {
            await using var context = new BobiiLngContext();
            var guildLanguage = await context.Languages.SingleOrDefaultAsync(l => l.guildid == guildId);
            if (guildLanguage == null)
            {
                return;
            }

            context.Languages.Remove(guildLanguage);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Der Default ist Englisch
        /// </summary>
        public static async Task<Language> GetLanguage(ulong guildId)
        {
            await using var lngContext = new BobiiLngContext();
            var language = await lngContext.Languages
                .SingleOrDefaultAsync(g => g.guildid == guildId);

            return language != null ? language!.langugeshort.ToLanguage() : Language.en;
        }

        public static async Task<string> GetCaption(string spc, Language language)
        {
            await using var lngContext = new BobiiLngContext();
            var caption = await lngContext.Captions
                .SingleOrDefaultAsync(c => c.msgid == spc);

            if (caption == null)
            {
                return MissingText;
            }

            return GetUebersetzung(caption, language);
        }

        public static async Task<string> GetContent(string spc, Language language)
        {
            await using var lngContext = new BobiiLngContext();
            var content = await lngContext.Contents
                .SingleOrDefaultAsync(c => c.msgid == spc);

            if (content == null)
            {
                return MissingText;
            }

            return GetUebersetzung(content, language);
        }
        #endregion

        #region Private Functions
        private static string GetUebersetzung(object entity, Language language)
        {
            return language switch
            {
                Language.de => GetUebersetzung(entity, Language.de.ToString()),
                Language.ru => GetUebersetzung(entity, Language.ru.ToString()),
                _ => GetUebersetzung(entity, Language.en.ToString())
            };
        }

        private static string GetUebersetzung(object entity, string columnName)
        {
            var value = entity.GetType()
                .GetProperty(columnName)?
                .GetValue(entity)?
                .ToString();

            return value ?? ErrorText;
        }
        #endregion
    }
}
