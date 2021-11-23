using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel.EntityFramework
{
    class TempChannelsHelper
    {
        #region Tasks
        public static async Task WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TChannels    {message}");
            await Task.CompletedTask;
        }

        public static async Task AddTC(ulong guildId, ulong tempChannelId, ulong createTempChannelId, ulong ownerId)
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
                    context.TempChannels.Add(tempChannel);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Method: AddTC | Guild: {guildId} | {ex.Message}");
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
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Method: RemoveTC | Guild: {guildId} | {ex.Message}");
            }
        }

        public static async Task<List<tempchannels>> GetTempChannelList(ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.TempChannels.AsQueryable().Where(tc => tc.guildid == guildId).ToList();
                }
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Function: GetTempChannelList | Guild: {guildId} | {ex.Message}");
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
                await WriteToConsol($"Error: | Function: GetTempChannelList | Guild: {channelId} | {ex.Message}");
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
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Function: GetTempChannelList | Guild: {channelId} | {ex.Message}");
            }
        } 
        #endregion
    }
}
