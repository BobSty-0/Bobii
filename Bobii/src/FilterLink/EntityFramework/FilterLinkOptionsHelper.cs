using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink.EntityFramework
{
    class FilterLinkOptionsHelper
    {
        #region Task
        // §TODO JG/16.11.2021 schauen ob diese Funktion tatsächlich funktioniert
        public static async Task<List<filterlinkoptions>> GetLinkOptions(List<filterlinksguild> filterLinks)
        {
            if (filterLinks.Count == 0)
            {
                return null;
            }

            try
            {
                var filterLinkOptionsList = new List<filterlinkoptions>();
                using (var context = new BobiiEntities())
                {
                    foreach (var filterLinkEntity in filterLinks)
                    {
                        foreach (var option in context.FilterLinkOptions.AsQueryable().Where(fl => fl.bezeichnung == filterLinkEntity.bezeichnung).ToList())
                            filterLinkOptionsList.Add(option);
                    }
                    return filterLinkOptionsList;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FLiOptions", true, "GetLinkOptions", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<string[]> GetAllOptions()
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var options = context.FilterLinkOptions.AsEnumerable().GroupBy(fl => fl.bezeichnung).ToList();
                    var option = "";
                    var list = new List<string>();
                    foreach (var optionEntity in options)
                    {
                        if (optionEntity.Key != option)
                        {
                            option = optionEntity.Key.Trim();
                            list.Add(option);
                        }
                    }
                    return list.ToArray();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FLiOptions", true, "GetAllOpitons", exceptionMessage: ex.Message);
                return null;
            }
        }
        #endregion
    }
}
