using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using Bobii.src.Handler;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel.EntityFramework
{
    class AutoScaleCategoriesHelper
    {
        #region Tasks
        public static async Task AddAutoScaleCategory(ulong guildid, string channelName, ulong categoryId, int channelSize, int emptyChannelNumber, int? delay, int? autodelete)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var createTempChannel = new autoscalecategory();
                    createTempChannel.guildid = guildid;
                    createTempChannel.categoryid = categoryId;
                    createTempChannel.channelname = channelName;
                    createTempChannel.channelsize = channelSize;
                    createTempChannel.autodelete = autodelete;
                    createTempChannel.emptychannelnumber = emptyChannelNumber;
                    
                    context.AutoScaleCategories.Add(createTempChannel);

                    context.SaveChanges();
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("CreatTChnl", true, nameof(AutoScaleCategoriesHelper), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveAutoScraeCategory(string guildid, ulong categoryId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var createTempChannel = context.AutoScaleCategories.AsQueryable().Where(channel => channel.categoryid == categoryId).First();
                    context.AutoScaleCategories.Remove(createTempChannel);
                    context.SaveChanges();
                    await Task.CompletedTask;
                }

                _ = TempCommandsHelper.RemoveCommands(categoryId);

                HandlingService.Cache.ResetCreateTempChannelsCache();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("CreatTChnl", true, nameof(RemoveAutoScraeCategory), exceptionMessage: ex.Message);
            }
        }

        public static async Task<autoscalecategory> GetAutoScaleCategory(ulong categoryId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.AutoScaleCategories.AsQueryable().SingleOrDefault(c => c.categoryid == categoryId);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("CreatTChnl", true, nameof(GetAutoScaleCategory), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<autoscalecategory>> GetCreateTempChannelListOfGuild(SocketGuild guild)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.AutoScaleCategories.AsQueryable().Where(channel => channel.guildid == guild.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("CreatTChnl", true, nameof(GetCreateTempChannelListOfGuild), exceptionMessage: ex.Message);
                return null;
            }
        }
        #endregion
    }
}
