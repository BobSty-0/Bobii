using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel.EntityFramework
{
    class TempChannelUserConfig
    {
        public static async Task<tempchanneluserconfig> GetTempChannelConfig(ulong userId, ulong createChannelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.TempChannelUserConfigs.AsQueryable().Where(t => t.userid == userId && t.createchannelid == createChannelId).SingleOrDefault();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannelUserConfig", true, nameof(GetTempChannelConfig), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<tempchanneluserconfig>> GetTempChannelConfigs(ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.TempChannelUserConfigs.AsQueryable().Where(t => t.guildid == guildId).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannelUserConfig", true, nameof(AddConfig), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task AddConfig(ulong guildId, ulong userId, ulong createTempChannelId, string tempChannelName, int channelSize)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannelUserConfig = new tempchanneluserconfig();
                    tempChannelUserConfig.guildid = guildId;
                    tempChannelUserConfig.userid = userId;
                    tempChannelUserConfig.createchannelid = createTempChannelId;
                    tempChannelUserConfig.tempchannelname = tempChannelName;
                    tempChannelUserConfig.channelsize = channelSize;

                    context.Add(tempChannelUserConfig);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannelUserConfig", true, nameof(AddConfig), exceptionMessage: ex.Message);
            }
        }

        public static async Task<bool> TempChannelUserConfigExists(ulong userId, ulong createTempChannelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.TempChannelUserConfigs.AsQueryable().Where(t => t.userid == userId && t.createchannelid == createTempChannelId).SingleOrDefault() != null;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannelUserConfig", true, nameof(TempChannelUserConfigExists), exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task ChangeConfig(ulong guildId, ulong userId, ulong createTempChannelId, string tempChannelName, int channelSize)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannelUserConfig = context.TempChannelUserConfigs.Single(t => t.userid == userId && t.createchannelid == createTempChannelId);
                    tempChannelUserConfig.guildid = guildId;
                    tempChannelUserConfig.userid = userId;
                    tempChannelUserConfig.createchannelid = createTempChannelId;
                    tempChannelUserConfig.tempchannelname = tempChannelName;
                    tempChannelUserConfig.channelsize = channelSize;

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannelUserConfig", true, nameof(ChangeConfig), exceptionMessage: ex.Message);
            }
        }

        public static async Task DeleteConfig(ulong userId, ulong createTempChannelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempChannelUserConfig = context.TempChannelUserConfigs.Single(t => t.userid == userId && t.createchannelid == createTempChannelId);
                    context.TempChannelUserConfigs.Remove(tempChannelUserConfig);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempChannelUserConfig", true, nameof(DeleteConfig), exceptionMessage: ex.Message);
            }
        }
    }
}
