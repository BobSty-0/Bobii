using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories

{
    public class CreatorChannelRepository
    {
        public static async Task RemoveCreatorChannelIfExisting(ulong channelId)
        {
            await using var context = new BobiiContext();
            var channel = await context.CreateTempChannels.FirstOrDefaultAsync(c => c.createchannelid == channelId);

            if (channel == null)
            {
                return;
            }

            context.CreateTempChannels.Remove(channel);
            await context.SaveChangesAsync();
        }

        public static async Task RemoveCreatorChannelsIfExisting(ulong guildId)
        {
            await using var context = new BobiiContext();
            var guildCreateTempChannels = context.CreateTempChannels.Where(ctc => ctc.guildid == guildId);

            if (!guildCreateTempChannels.Any())
            {
                return;
            }

            context.CreateTempChannels.RemoveRange(guildCreateTempChannels);
            await context.SaveChangesAsync();
        }

        public static async Task AddCreatorChannel(ulong guildId, string tempChannelName, ulong creatorChannelId, int channelSize, int delay, int autodelete)
        {
            await using var context = new BobiiContext();

            var creatorChannel = new CreateTempChannel
            {
                guildid = guildId,
                createchannelid = creatorChannelId,
                tempchannelname = tempChannelName,
                channelsize = channelSize,
                delay = delay,
                autodelete = autodelete
            };

            await context.CreateTempChannels.AddAsync(creatorChannel);
            await context.SaveChangesAsync();
        }

        public static async Task<CreateTempChannel[]> GetCreatorChannels(ulong guildId)
        {
            await using var context = new BobiiContext();
            return await context.CreateTempChannels
                .Where(c => c.guildid == guildId)
                .ToArrayAsync();
        }

        public static async Task<CreateTempChannel?> GetCreatorChannel(ulong channelId)
        {
            await using var context = new BobiiContext();
            return await context.CreateTempChannels
                .FirstOrDefaultAsync(c => c.createchannelid == channelId);
        }
    }
}
