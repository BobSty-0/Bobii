using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public static class InterfaceInformationsRepository
    {
        public static async Task RemoveInterfaceInformationsIfExisting(ulong guildId)
        {
            await using var context = new BobiiContext();
            var guildInterfaceInformations = context.InterfaceInformations.Where(tc => tc.GuildId == guildId);
            if (!guildInterfaceInformations.Any())
            {
                return;
            }
            context.InterfaceInformations.RemoveRange(guildInterfaceInformations);
            await context.SaveChangesAsync();
        }

        public static async Task<List<InterfaceInformation>> GetCustomGuildInterfaceInformations(ulong guildId)
        {
            await using var context = new BobiiContext();
            var guildInformations = await context.InterfaceInformations
                .Where(i => i.GuildId == guildId)
                .OrderBy(i => i.Sort)
                .ToListAsync();

            return guildInformations;
        }

        public static async Task<InterfaceInformation> GetInterfaceInformationMitFallback(ulong guildId, string commandName)
        {
            await using var context = new BobiiContext();
            var guildInformation = await context.InterfaceInformations
                .SingleOrDefaultAsync(e => e.GuildId == guildId && e.CommandName == commandName);

            if (guildInformation != null)
            {
                return guildInformation;
            }

            return await GetInterfaceDefaultInformation(commandName);
        }

        public static async Task<InterfaceInformation> GetInterfaceDefaultInformation(string commandName)
        {
            // Die Default Emotes sind alle auf der Support Guild
            var supportGuildId = Configuration.GetConfigValue<ulong>(Configuration.SupportGuildID);
            await using var context = new BobiiContext();

            return await context.InterfaceInformations
                .SingleAsync(e => e.GuildId == supportGuildId && e.CommandName == commandName);
        }
    }
}
