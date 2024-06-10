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
        private const string ColumnNameEN = "en";
        private const string ColumnNameDE = "de";
        private const string ColumnNameRU = "ru";
        #endregion

        #region Methods
        /// <summary>
        /// Der Default ist Englisch
        /// </summary>
        public static async Task<Language> GetLanguage(ulong guildId)
        {
            await using var lngContext = new BobiiLngContext();
            var language = await lngContext.Languages
                .SingleOrDefaultAsync(g => g.guildid == guildId);

            return language != null ? language!.langugeshort.ToLanguage() : Language.EN;
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
            switch (language)
            {
                case Language.DE:
                    return GetUebersetzung(entity, ColumnNameDE);
                case Language.RU:
                    return GetUebersetzung(entity, ColumnNameRU);
                default:
                    return GetUebersetzung(entity, ColumnNameEN);
            }
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
