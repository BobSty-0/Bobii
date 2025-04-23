using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public static class UsedFunctionsRepository
    {
        public static async Task<List<usedfunctions>> GetUsedUserFunctions(
            string function,
            ulong guildId,
            ulong userId)
        {
            await using var context = new BobiiContext();
            return await context.UsedFunctions.Where(u =>
                    u.function == function &&
                    u.guildid == guildId &&
                    u.userid == userId)
                .ToListAsync();
        }

        public static async Task<usedfunctions?> GetUsedChannelFunction(string function, ulong channelId)
        {
            await using var context = new BobiiContext();
            return await context.UsedFunctions.SingleOrDefaultAsync(u => u.function == function && u.channelid == channelId && u.affecteduserid == 0);
        }

        public static async Task<usedfunctions?> GetUsedUserFunction(
            string function,
            ulong guildId,
            ulong affectedUserId)
        {
            await using var context = new BobiiContext();
            return await context.UsedFunctions.SingleOrDefaultAsync(u =>
                u.function == function &&
                u.guildid == guildId &&
                u.affecteduserid == affectedUserId);
        }

        public static async Task<usedfunctions?> GetUsedUserFunction(
            string function, 
            ulong guildId, 
            ulong userId, 
            ulong affectedUserId)
        {
            await using var context = new BobiiContext();
            return await context.UsedFunctions.SingleOrDefaultAsync(u =>
                u.function == function && 
                u.guildid == guildId &&
                u.userid == userId &&
                u.affecteduserid == affectedUserId);
        }

        public static async Task RemoveUsedChannelFunctionsIfExisting(ulong channelId)
        {
            await using var context = new BobiiContext();
            var channelUsedFunctions = context.UsedFunctions.Where(u => u.channelid == channelId);
            if (!channelUsedFunctions.Any())
            {
                return;
            }
            context.UsedFunctions.RemoveRange(channelUsedFunctions);
            await context.SaveChangesAsync();
        }

        public static async Task RemoveUsedFunctionsIfExisting(ulong guildId, ulong userId)
        {
            await using var context = new BobiiContext();
            var guildUsedFunctions = context.UsedFunctions.Where(u => u.guildid == guildId && u.userid == userId);
            if (!guildUsedFunctions.Any())
            {
                return;
            }
            context.UsedFunctions.RemoveRange(guildUsedFunctions);
            await context.SaveChangesAsync();
        }

        public static async Task RemoveUsedFunctionsIfExisting(ulong guildId)
        {
            await using var context = new BobiiContext();
            var guildUsedFunctions = context.UsedFunctions.Where(u => u.guildid == guildId);
            if (!guildUsedFunctions.Any())
            {
                return;
            }
            context.UsedFunctions.RemoveRange(guildUsedFunctions);
            await context.SaveChangesAsync();
        }
    }
}
