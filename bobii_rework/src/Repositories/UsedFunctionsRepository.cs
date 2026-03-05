using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public static class UsedFunctionsRepository
    {
        public static async Task<UsedFunction> CreateUsedFunction(
            ulong userId,
            ulong affectedUserId,
            string function,
            ulong guildId,
            ulong channelId,
            bool isUser = true)
        {
            await using var context = new BobiiContext();
            var usedFunction = new UsedFunction()
            {
                UserId = userId,
                AffectedUserId = affectedUserId,
                Function = function,
                ChannelId = channelId,
                GuildId = guildId,
                // TODO nochmal genauer nachschauen wofür das IsUser gebraucht wird...
                IsUser = isUser,
                DoneAt = DateTime.Now,
            };
            await context.UsedFunctions.AddAsync(usedFunction);
            await context.SaveChangesAsync();

            return usedFunction;
        }

        public static async Task<List<UsedFunction>> GetUsedUserFunctions(
            string function,
            ulong guildId,
            ulong userId)
        {
            await using var context = new BobiiContext();
            return await context.UsedFunctions.Where(u =>
                    u.Function == function &&
                    u.GuildId == guildId &&
                    u.UserId == userId)
                .ToListAsync();
        }

        public static async Task<UsedFunction?> GetUsedChannelFunction(string function, ulong channelId)
        {
            await using var context = new BobiiContext();
            return await context.UsedFunctions.SingleOrDefaultAsync(u => u.Function == function && u.ChannelId == channelId && u.AffectedUserId == 0);
        }

        public static async Task<UsedFunction?> GetUsedUserFunction(
            string function,
            ulong guildId,
            ulong affectedUserId)
        {
            await using var context = new BobiiContext();
            return await context.UsedFunctions.SingleOrDefaultAsync(u =>
                u.Function == function &&
                u.GuildId == guildId &&
                u.AffectedUserId == affectedUserId);
        }

        public static async Task<UsedFunction?> GetUsedUserFunction(
            string function,
            ulong guildId,
            ulong userId,
            ulong affectedUserId)
        {
            await using var context = new BobiiContext();
            return await context.UsedFunctions.SingleOrDefaultAsync(u =>
                u.Function == function &&
                u.GuildId == guildId &&
                u.UserId == userId &&
                u.AffectedUserId == affectedUserId);
        }

        public static async Task RemoveUsedChannelFunctionsIfExisting(ulong channelId)
        {
            await using var context = new BobiiContext();
            var channelUsedFunctions = context.UsedFunctions.Where(u => u.ChannelId == channelId);
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
            var guildUsedFunctions = context.UsedFunctions.Where(u => u.GuildId == guildId && u.UserId == userId);
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
            var guildUsedFunctions = context.UsedFunctions.Where(u => u.GuildId == guildId);
            if (!guildUsedFunctions.Any())
            {
                return;
            }
            context.UsedFunctions.RemoveRange(guildUsedFunctions);
            await context.SaveChangesAsync();
        }
    }
}
