using Bobii.src.EntityFramework.Entities;
using Bobii.src.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel.EntityFramework
{
    public class LanguageHelper
    {
        public static async Task<bool> LanguageExistiert(ulong guildId)
        {
            return GetLanguage(guildId).Result != null;
        }

        public static async Task<language> GetLanguage(ulong guildId)
        {
            try
            {
                using (var context = new BobiiLngCodes())
                {
                    return context.Languages.SingleOrDefault(c => c.guildid == guildId);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetLanguage), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task UpdateLanguage(ulong guildId, string language)
        {
            try
            {
                using (var context = new BobiiLngCodes())
                {
                    var lang = context.Languages.Single(c => c.guildid == guildId);
                    lang.langugeshort = language;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(UpdateLanguage), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveLanguage(ulong guildId)
        {
            try
            {
                using (var context = new BobiiLngCodes())
                {
                    var lang = context.Languages.Single(c => c.guildid == guildId);

                    context.Languages.Remove(lang);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveLanguage), exceptionMessage: ex.Message);
            }
        }

        public static async Task AddLanguage(ulong guildId, string language)
        {
            try
            {
                using (var context = new BobiiLngCodes())
                {
                    var lang = new language();
                    lang.guildid = guildId;
                    lang.langugeshort = language;

                    context.Languages.Add(lang);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(AddLanguage), exceptionMessage: ex.Message);
            }
        }
    }
}
