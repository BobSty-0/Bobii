using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink.EntityFramework
{
    class FilterlLinksHelper
    {
        #region Tasks
        public static async Task<bool> FilterLinkAktive(ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterLinkRowOfGuild = context.FilterLink.AsQueryable().Where(fl => fl.guildid == guildId).FirstOrDefault();
                    if (filterLinkRowOfGuild != null)
                    {
                        return filterLinkRowOfGuild.filterlinkactive;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FilterLink", true, "IsFilterWordActive", exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task ActivateFilterLink(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterLink = context.FilterLink.AsQueryable().Where(fl => fl.guildid == guildid).FirstOrDefault();

                    //I dont need to create a line if there is already one there
                    if (filterLink != null)
                    {
                        filterLink.filterlinkactive = true;
                        context.FilterLink.Update(filterLink);
                        context.SaveChanges();
                    }
                    else
                    {
                        var filterLinkEntity = new filterlink();
                        filterLinkEntity.guildid = guildid;
                        filterLinkEntity.filterlinkactive = true;

                        context.FilterLink.Add(filterLinkEntity);
                        context.SaveChanges();
                        await Task.CompletedTask;
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FilterLink", true, "ActivateFilterLink", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task DeactivateFilterLink(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterLinkEntity = context.FilterLink.AsQueryable().Where(fl => fl.guildid == guildid).FirstOrDefault();
                    if (filterLinkEntity != null)
                    {
                        filterLinkEntity.filterlinkactive = false;
                        context.SaveChanges();
                    }
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("FilterLink", true, "DeactivateFilterLink", exceptionMessage: ex.Message);
                return;
            }
        }
        #endregion
    }
}
