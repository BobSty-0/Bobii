using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public static class TempChannelRepository
    {
        public static async Task<int> GetMaxTempChannelCount(ulong creatorChannelId)
        {
            await using var context = new BobiiContext();
            var tempChannels = context.TempChannels.Where(t => t.createchannelid == creatorChannelId);
            if (await tempChannels.AnyAsync())
            {
                return await tempChannels.MaxAsync(t => t.count);
            }

            return 0;
        }

        public static async Task<tempchannels[]> GetTempChannels()
        {
            await using var context = new BobiiContext();
            return await context.TempChannels.ToArrayAsync();
        }

        public static async Task<tempchannels[]> GetTempChannels(ulong guildId)
        {
            await using var context = new BobiiContext();
            return await context.TempChannels.Where(t => t.guildid == guildId).ToArrayAsync();
        }

        public static async Task<tempchannels[]> GetTempChannelsMitDelay()
        {
            await using var context = new BobiiContext();
            return await context.TempChannels.Where(t => t.deletedate != null).ToArrayAsync();
        }

        public static async Task<tempchannels?> GetTempChannel(ulong channelId)
        {
            await using var context = new BobiiContext();
            return await context.TempChannels.SingleOrDefaultAsync(t => t.channelid == channelId);
        }

        public static async Task<tempchannels?> GetTempChannel(ulong? creatorChannelId, ulong userId)
        {
            await using var context = new BobiiContext();
            return await context.TempChannels.SingleOrDefaultAsync(t => t.createchannelid == creatorChannelId.GetValueOrDefault() && t.channelownerid == userId);
        }

        public static async Task UpdateOwner(ulong channelId, ulong ownerId)
        {
            await using var context = new BobiiContext();
            var tempChannel = await context.TempChannels.SingleAsync(t => t.channelid == channelId);
            tempChannel.channelownerid = ownerId;
            await context.SaveChangesAsync();
        }

        public static async Task UpdateDelay(ulong channelId, DateTime? newDeleteDateTime)
        {
            await using var context = new BobiiContext();
            var tempChannel = await context.TempChannels.SingleAsync(t => t.channelid == channelId);
            tempChannel.deletedate = newDeleteDateTime;
            await context.SaveChangesAsync();
        }

        public static async Task AddTempChannel(
            ulong guildId,
            ulong channelId,
            ulong channelOwnerId,
            ulong creatorChannelId,
            bool autoScale = false,
            ulong autoScaleCategoryId = 0)
        {
            await using var context = new BobiiContext();
            var tempChannel = new tempchannels
            {
                guildid = guildId,
                channelid = channelId,
                channelownerid = channelOwnerId,
                createchannelid = creatorChannelId,
                autoscale = autoScale,
                autoscalercategoryid = autoScaleCategoryId,
                unixtimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                count = (context.TempChannels.Count(t => t.createchannelid == creatorChannelId) + 1)
            };

            context.TempChannels.Add(tempChannel);
            await context.SaveChangesAsync();
        }

        public static async Task RemoveTempChannelIfExisting(ulong channelId)
        {
            await using var context = new BobiiContext();
            var channel = await context.TempChannels.FirstOrDefaultAsync(c => c.channelid == channelId);

            if (channel == null)
            {
                return;
            }

            context.TempChannels.Remove(channel);
            await context.SaveChangesAsync();
        }

        public static async Task RemoveTempChannelsIfExisting(ulong guildId)
        {
            await using var context = new BobiiContext();
            var guildTempChannels = context.TempChannels.Where(tc => tc.guildid == guildId);

            if (!guildTempChannels.Any())
            {
                return;
            }

            context.TempChannels.RemoveRange(guildTempChannels);
            await context.SaveChangesAsync();
        }
    }
}
