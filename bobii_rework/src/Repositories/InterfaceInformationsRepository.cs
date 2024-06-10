using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public static class InterfaceInformationsRepository
    {
        public static async Task<ulong> GetInterfaceEmoteIdWithFallback(ulong guildId, string commandName)
        {
            var information = await GetInterfaceInformationMitFallback(guildId, commandName);

            if (information.EmoteId != 0)
            {
                return information.EmoteId;
            }

            var defaultInformation = await GetInterfaceDefaultInformation(commandName);
            return defaultInformation.EmoteId;
        }

        public static async Task<string> GetInterfaceCustomCommandColorWithFallback(ulong guildId, string commandName)
        {
            var information = await GetInterfaceInformationMitFallback(guildId, commandName);

            if (!string.IsNullOrEmpty(information.CustomCommandColorRGBA))
            {
                return information.CustomCommandColorRGBA;
            }

            var defaultInformation = await GetInterfaceDefaultInformation(commandName);
            return defaultInformation.CustomCommandColorRGBA;
        }

        public static async Task<string> GetInterfaceCustomCommandNameWithFallback(ulong guildId, string commandName)
        {
            var information = await GetInterfaceInformationMitFallback(guildId, commandName);

            if (!string.IsNullOrEmpty(information.CustomCommandName))
            {
                return information.CustomCommandName;
            }

            var defaultInformation = await GetInterfaceDefaultInformation(commandName);
            return defaultInformation.CustomCommandName;
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
