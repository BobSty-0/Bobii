using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink.EntityFramework
{
    class FilterLinkLogsHelper
    {
        #region Tasks
        public static async Task<List<filterlinklogs>> GetFilterLinkLogChannels()
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.FilterLinkLogs.ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FilteLiLog", true, "GetFilterLinkLogChannels", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<ulong> GetFilterLinkLogChannelID(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterLinkLogChannel = context.FilterLinkLogs.AsQueryable().Where(channel => channel.guildid == guildid).FirstOrDefault();
                    if (filterLinkLogChannel != null)
                    {
                        return filterLinkLogChannel.channelid;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FilteLiLog", true, "GetFilterLinkLogChannelID", exceptionMessage: ex.Message);
                return 0;
            }
        }

        public static async Task<bool> DoesALogChannelExist(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterLinkLog = context.FilterLinkLogs.AsQueryable().Where(ch => ch.guildid == guildid).FirstOrDefault() ;
                    if (filterLinkLog != null)
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
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FilteLiLog", true, "DoesALogChannelExist", exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task SetFilterLinkLogChannel(ulong guildid, ulong channelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterLinkLog = new filterlinklogs();
                    filterLinkLog.channelid = channelId;
                    filterLinkLog.guildid = guildid;
                    context.FilterLinkLogs.Add(filterLinkLog);
                    context.SaveChanges();
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FilteLiLog", true, "SetFilterLinkLogChannel", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task RemoveFilterLinkLogChannel(ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterLinkLog = context.FilterLinkLogs.AsQueryable().Where(fl => fl.guildid == guildid).FirstOrDefault();
                    if (filterLinkLog != null)
                    {
                        context.FilterLinkLogs.Remove(filterLinkLog);
                        context.SaveChanges();
                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FilteLiLog", true, "RemoveFilterLinkLogChannel", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task UpdateFilterLinkLogChannel(ulong guildid, ulong newChannel)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var filterLinkLog = context.FilterLinkLogs.AsQueryable().Where(fl => fl.guildid == guildid).FirstOrDefault();
                    filterLinkLog.channelid = newChannel;
                    context.FilterLinkLogs.Update(filterLinkLog);
                    context.SaveChanges();
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("FilteLiLog", true, "UpdateFilterLinkLogChannel", exceptionMessage: ex.Message);
                return;
            }
        }
        #endregion
    }
}
