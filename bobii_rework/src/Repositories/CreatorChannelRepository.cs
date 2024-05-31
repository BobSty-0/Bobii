using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories

{
    public class CreatorChannelRepository
    {
        public async Task<List<createtempchannels>> GetCreatorChannels(ulong guildId)
        {
            await using var context = new BobiiContext();
            return await context.CreateTempChannels
                .Where(c => c.guildid == guildId)
                .ToListAsync();
        }
    }
}
