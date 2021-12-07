using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterWord.EntityFramework
{
    class FilterWordsHelper
    {
        #region Tasks
        public static async Task AddFilterWord(ulong guildid, string filterWord, string replaceWord)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterWordEntity = new filterwords();
                    filterWordEntity.guildid = guildid;
                    filterWordEntity.filterword = filterWord;
                    filterWordEntity.replaceword = replaceWord;
                    context.FilterWords.Add(filterWordEntity);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FilterWord", true, "AddFilterWord", exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveFilterWord(string filterWord, ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterWordEntity = context.FilterWords.AsQueryable().Where(fw => fw.filterword == filterWord && fw.guildid == guildid).FirstOrDefault();
                    context.FilterWords.Remove(filterWordEntity);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FilterWord", true, "RemoveFilterWord", exceptionMessage: ex.Message);
            }
        }

        public static async Task UpdateFilterWord(string filterWord, string newReplaceWord, ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterWordEntity = context.FilterWords.AsQueryable().Where(fw => fw.filterword == filterWord && fw.guildid == guildid).FirstOrDefault();
                    filterWordEntity.replaceword = newReplaceWord;
                    context.FilterWords.Update(filterWordEntity);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FilterWord", true, "UpdateFilterWord", exceptionMessage: ex.Message);
            }
        }

        public static async Task<bool> CheckIfFilterWordExists(ulong guildId, string filterWord)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterWordEntity = context.FilterWords.AsQueryable().Where(fw => fw.guildid == guildId && fw.filterword == filterWord).FirstOrDefault();
                    if (filterWordEntity != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FilterWord", true, "CheckIfFilterExists", exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task<List<filterwords>> GetFilterWordsFromGuildAsList(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.FilterWords.AsQueryable().Where(fw => fw.guildid == guildid).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FilterWord", true, "GetCreateFilterWordListFromGuild", exceptionMessage: ex.Message);
                return null;
            }
        }
        #endregion
    }
}
