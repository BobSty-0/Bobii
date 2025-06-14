using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public static class TempChannelUserConfigRepository
    {
        public static async Task<TempChannelUserConfig?> GetTempChannelUserConfig(ulong creatorChannelId, ulong userId)
        {
            await using var context = new BobiiContext();
            return await context.TempChannelUserConfigs.FirstOrDefaultAsync(t => t.createchannelid == creatorChannelId && t.userid == userId);
        }

        public static async Task RemoveTempChannelUserConfigsIfExisting(ulong guildId, ulong userId)
        {
            await using var context = new BobiiContext();
            var guildUserConfigs = context.TempChannelUserConfigs.Where(c => c.guildid == guildId && c.userid == userId);

            if (!guildUserConfigs.Any())
            {
                return;
            }

            context.TempChannelUserConfigs.RemoveRange(guildUserConfigs);
            await context.SaveChangesAsync();
        }

        public static async Task RemoveTempChannelUserConfigsIfExisting(ulong guildId)
        {
            await using var context = new BobiiContext();
            var guildUserConfigs = context.TempChannelUserConfigs.Where(c => c.guildid == guildId);

            if (!guildUserConfigs.Any())
            {
                return;
            }

            context.TempChannelUserConfigs.RemoveRange(guildUserConfigs);
            await context.SaveChangesAsync();
        }
    }
}
