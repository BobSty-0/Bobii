using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public static class TempCommandRepository
    {
        public static async Task<List<tempcommands>> GetTempCommands(ulong guildId, ulong creatorChannelId)
        {
            await using var context = new BobiiContext();
            return await context.Commands
                .Where(c => c.createchannelid == creatorChannelId && c.guildguid == guildId)
                .ToListAsync();
        }

        public static async Task<bool> CommandDisabled(ulong guildId, ulong creatorChannelId, string commandName)
        {
            await using var context = new BobiiContext();
            var command = await context.Commands
                .SingleOrDefaultAsync(c =>
                    c.createchannelid == creatorChannelId && 
                    c.guildguid == guildId && 
                    c.commandname == commandName);

            return command != null;
        }
    }
}
