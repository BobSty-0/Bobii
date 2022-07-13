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
        public static async Task AddLinkOption(string name, string link, ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var floption = new filterlinkoptions();
                    floption.bezeichnung = name;
                    floption.linkbody = link;
                    floption.guildid = guildid;

                    context.FilterLinkOptions.Add(floption);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FLiOptions", true, "AddLinkOption", exceptionMessage: ex.Message);
            }
        }

        public static async Task<bool> CheckIfLinkOptionExists(string name, string link, ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var floption = context.FilterLinkOptions.AsQueryable().Where(f => f.bezeichnung == name && f.linkbody == link && f.guildid == guildid).FirstOrDefault();
                    return floption != null;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FLiOptions", true, "AddLinkOption", exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task DeleteLinkOption(string name, string link, ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var floption = context.FilterLinkOptions.AsQueryable().Where(f => f.bezeichnung == name && f.linkbody == link && f.guildid == guildid).FirstOrDefault();
                    context.FilterLinkOptions.Remove(floption);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FLiOptions", true, "AddLinkOption", exceptionMessage: ex.Message);
            }
        }

        // Gets all the filter link options where the guildid = guildid or guildid = null
        public static async Task<List<filterlinkoptions>> GetLinkOptions(List<filterlinksguild> filterLinks, ulong guildId)
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
                        foreach (var option in context.FilterLinkOptions.AsQueryable().Where(fl => fl.bezeichnung == filterLinkEntity.bezeichnung && (fl.guildid == null || fl.guildid == guildId)).ToList())
                            filterLinkOptionsList.Add(option);
                    }
                    return filterLinkOptionsList;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FLiOptions", true, "GetLinkOptions", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<string[]> GetAllOptionsLinksFromGuild(string name, ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var options = context.FilterLinkOptions.AsEnumerable().Where(f => f.guildid == guildid && f.bezeichnung == name).GroupBy(fl => fl.linkbody).ToList();
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
                    var newList = new List<string>();
                    foreach (var link in list)
                    {
                        newList.Add($"https://{link}");
                    }
                    return newList.ToArray();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FLiOptions", true, "GetAllOptionsFromGuild", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<filterlinkoptions>> GetOptionsFromGuild(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.FilterLinkOptions.AsQueryable().Where(f => f.guildid == guildid).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FLiOptions", true, "GetOptionsFromGuild", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<string[]> GetAllOptionsFromGuildOrderByBezeichnung(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var options = context.FilterLinkOptions.AsEnumerable().Where(f => f.guildid == guildid).GroupBy(fl => fl.bezeichnung).ToList();
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
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FLiOptions", true, "GetAllOptionsFromGuildOrderByBezeichnung", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<string[]> GetAllOptionsFuerGuild(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var options = context.FilterLinkOptions.AsEnumerable().Where(f => (f.guildid == null || f.guildid == guildid)).GroupBy(fl => fl.bezeichnung).ToList();
                    var option = "";
                    var list = new List<string>();
                    foreach (var optionEntity in options)
                    {
                        if (optionEntity.Key != option)
                        {
                            option = optionEntity.Key.Trim();
                            option = option.ToLower();
                            list.Add(option);
                        }
                    }
                    return list.ToArray();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FLiOptions", true, "GetAllOpitons", exceptionMessage: ex.Message);
                return null;
            }
        }
        #endregion
    }
}
