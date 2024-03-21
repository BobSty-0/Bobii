using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using Bobii.src.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api.Core.RateLimiter;

namespace Bobii.src.TempChannel.EntityFramework
{
    class TempChannelsHelper
    {
        #region Tasks
        public static async Task AddTC(ulong guildId, ulong tempChannelId, ulong createTempChannelId, ulong ownerId, bool autoscale = false, ulong autoscalecategory = 0)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannel = new tempchannels();
                    tempChannel.guildid = guildId;
                    tempChannel.channelid = tempChannelId;
                    tempChannel.channelownerid = ownerId;
                    tempChannel.createchannelid = createTempChannelId;
                    tempChannel.unixtimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    tempChannel.autoscale = autoscale;
                    tempChannel.autoscalercategoryid = autoscalecategory;
                    var count = new int();
                    if (autoscale)
                    {
                        Console.WriteLine("ja");
                        if (context.TempChannels.AsQueryable().Where(t => t.autoscalercategoryid == autoscalecategory)?.Count() == 0)
                        {
                            Console.WriteLine("Er macht count 1");
                            count = 1;
                        }
                        else
                        {
                            Console.WriteLine("Er versucht den Rest zu ermitteln");
                            count = (context.TempChannels.AsQueryable().Where(t => t.autoscalercategoryid == autoscalecategory)?.Max(channel => channel.count)).GetValueOrDefault() + 1;
                        }

                    }
                    else
                    {
                        if (context.TempChannels.AsQueryable().Where(t => t.createchannelid == createTempChannelId)?.Count() == 0)
                        {
                            count = 1;
                        }
                        else
                        {
                            count = (context.TempChannels.AsQueryable().Where(t => t.createchannelid == createTempChannelId)?.Max(channel => channel.count)).GetValueOrDefault() + 1;
                        }
                    }

                    tempChannel.count = count;
                    context.TempChannels.Add(tempChannel);
                    context.SaveChanges();
                }

                HandlingService.Cache.ResetTempChannelsCache();
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "AddTC", exceptionMessage: ex.Message);
            }
        }

        public static async Task<tempchannels> GetTempChannel(ulong tempChannelID)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.TempChannels.AsQueryable().FirstOrDefault(t => t.channelid == tempChannelID);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "GetTempChannel", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<int> GetCountAutoScale(ulong categoryId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    if (context.TempChannels.AsQueryable().Where(c => c.autoscalercategoryid.Value == categoryId).Count() == 0)
                    {
                        return 1;
                    }
                    return context.TempChannels.AsQueryable().Where(c => c.autoscalercategoryid.Value == categoryId).Max(channel => channel.count) + 1;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, nameof(GetCountAutoScale), exceptionMessage: ex.Message);
                return 0;
            }
        }

        public static async Task<int> GetCount(ulong createChannelID)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    if (context.TempChannels.AsQueryable().Where(c => c.createchannelid == createChannelID).Count() == 0)
                    {
                        return 1;
                    }
                    return context.TempChannels.AsQueryable().Where(c => c.createchannelid == createChannelID).Max(channel => channel.count) + 1;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "RemoveTC", exceptionMessage: ex.Message);
                return 0;
            }
        }

        public static async Task RemoveTC(ulong guildId, ulong tempChannelID)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannel = context.TempChannels.AsQueryable().Where(tc => tc.channelid == tempChannelID).FirstOrDefault();
                    context.TempChannels.Remove(tempChannel);
                    context.SaveChanges();
                }

                _ = UsedFunctionsHelper.RemoveUsedFunction(tempChannelID);

                HandlingService.Cache.ResetTempChannelsCache();
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "RemoveTC", exceptionMessage: ex.Message);
            }
        }

        public static async Task UpdateCount(long tempChannelId, int count)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannel = context.TempChannels.AsQueryable().FirstOrDefault(t => t.id == tempChannelId);
                    tempChannel.count = count;
                    context.SaveChanges();
                }

                HandlingService.Cache.ResetTempChannelsCache();
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, nameof(UpdateCount), exceptionMessage: ex.Message);
            }
        }

        public static async Task UpdateDeleteDelay(long tempChannelID, DateTime? deleteDate)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannel = context.TempChannels.AsQueryable().FirstOrDefault(t => t.id == tempChannelID);
                    tempChannel.deletedate = deleteDate;
                    context.SaveChanges();
                }

                HandlingService.Cache.ResetTempChannelsCache();
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, nameof(UpdateDeleteDelay), exceptionMessage: ex.Message);
            }
        }

        public static async Task<List<tempchannels>> GetTempChannelList(bool autoscale = false)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.TempChannels.AsEnumerable().Where(c => c.autoscale == autoscale).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "GetTempChannelList", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<tempchannels>> GetTempChannelListFromGuild(ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.TempChannels.AsQueryable().Where(tc => tc.guildid == guildId && !tc.autoscale).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "GetTempChannelList", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<ulong> GetOwnerID(ulong channelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.TempChannels.AsQueryable().Where(tc => tc.channelid == channelId).Select(t => t.channelownerid.Value).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "GetOwnerID", exceptionMessage: ex.Message);
                return 0;
            }
        }

        public static async Task ChangeOwner(ulong channelId, ulong newOwnerId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannel = context.TempChannels.AsQueryable().Where(channel => channel.channelid == channelId).First();
                    tempChannel.channelownerid = newOwnerId;
                    context.TempChannels.Update(tempChannel);
                    context.SaveChanges();
                }

                HandlingService.Cache.ResetTempChannelsCache();
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "ChangeOwner", exceptionMessage: ex.Message);
            }
        }

        public static async Task<bool> DoesOwnerExist(ulong ownerId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannel = context.TempChannels.AsQueryable().Where(channel => channel.channelownerid == ownerId).FirstOrDefault();
                    return tempChannel != null;
                }

            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "DoesOwnerExist", exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task<bool> DoesTempChannelExist(ulong channelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannel = context.TempChannels.AsQueryable().Where(channel => channel.channelid == channelId).FirstOrDefault();
                    return tempChannel != null;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannl", true, "DoesTempChannelExist", exceptionMessage: ex.Message);
                return false;
            }
        }
        #endregion
    }
}
