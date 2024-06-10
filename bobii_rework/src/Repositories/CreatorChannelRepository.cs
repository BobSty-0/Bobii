using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories

{
    public class CreatorChannelRepository
    {
        public static async Task AddCreatorChannel(ulong guildId, string tempChannelName, ulong creatorChannelId, int channelSize, int delay, int autodelete)
        {
            await using var context = new BobiiContext();

            var creatorChannel = new createtempchannels
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

        public static async Task<List<createtempchannels>> GetCreatorChannels(ulong guildId)
        {
            await using var context = new BobiiContext();
            return await context.CreateTempChannels
                .Where(c => c.guildid == guildId)
                .ToListAsync();
        }

        public static async Task<createtempchannels> GetCreatorChannel(ulong channelId)
        {
            await using var context = new BobiiContext();
            return await context.CreateTempChannels
                .FirstAsync(c => c.createchannelid == channelId);
        }
    }
}
