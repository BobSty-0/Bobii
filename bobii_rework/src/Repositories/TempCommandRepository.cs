using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public static class TempCommandRepository
    {
        public static async Task RemoveTempCommandsIfExisting(ulong guildId)
        {
            await using var context = new BobiiContext();
            var guildCommands = context.Commands.Where(c => c.GuildId == guildId);

            if (!guildCommands.Any())
            {
                return;
            }

            context.Commands.RemoveRange(guildCommands);
            await context.SaveChangesAsync();
        }

        public static async Task<List<TempCommand>> GetTempCommands(ulong guildId, ulong creatorChannelId)
        {
            await using var context = new BobiiContext();
            return await context.Commands
                .Where(c => c.CreateChannelId == creatorChannelId && c.GuildId == guildId)
                .ToListAsync();
        }

        public static async Task<string[]> GetDisabledTempCommandNames(ulong creatorChannelId)
        {
            await using var context = new BobiiContext();
            return await context.Commands
                .Where(c => c.CreateChannelId == creatorChannelId && !c.Enabled)
                .Select(c => c.CommandName)
                .ToArrayAsync();
        }

        public static async Task<bool> CommandDisabled(ulong guildId, ulong creatorChannelId, string commandName)
        {
            await using var context = new BobiiContext();
            var command = await context.Commands
                .SingleOrDefaultAsync(c =>
                    c.CreateChannelId == creatorChannelId &&
                    c.GuildId == guildId &&
                    c.CommandName == commandName);

            return command != null;
        }
    }
}
