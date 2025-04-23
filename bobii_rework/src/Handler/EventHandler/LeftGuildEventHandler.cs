using bobii_rework.Extensions;
using bobii_rework.Repositories;
using Discord.WebSocket;

namespace bobii_rework.Handler.EventHandler
{
    public class LeftGuildEventHandler : EventHandlerBase
    {
        #region Declarations
        private SocketGuild _guild;
        #endregion

        #region Tasks
        public async Task ExecuteLeftGuildActionAsync(SocketGuild guild)
        {
            _guild = guild;
            await Execute();
        }
        #endregion

        #region Overrieds
        public override async Task ExecuteEvent()
        {
            RemoveGuildFolder(_guild.Id);
            await RemoveGuildData(_guild.Id);
            this.WriteLineToConsole($"Guild verlassen: {_guild.Name} | {_guild.Id}");
        }
        #endregion

        #region Private Tasks
        private async Task RemoveGuildData(ulong guildId)
        {
            await LanguageRepository.RemoveLanguageIfExisting(guildId);
            await CreatorChannelRepository.RemoveCreatorChannelsIfExisting(guildId);
            await TempChannelRepository.RemoveTempChannelsIfExisting(guildId);
            await InterfaceInformationsRepository.RemoveInterfaceInformationsIfExisting(guildId);
            await TempCommandRepository.RemoveTempCommandsIfExisting(guildId);
            await UsedFunctionsRepository.RemoveUsedFunctionsIfExisting(guildId);
            await TempChannelUserConfigRepository.RemoveTempChannelUserConfigsIfExisting(guildId);
        }

        public static void RemoveGuildFolder(ulong guildId)
        {
            var guildDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), guildId.ToString());

            if (!Directory.Exists(guildDirectoryPath))
            {
                return;
            }

            var guildDirectory = new DirectoryInfo(guildDirectoryPath);
            guildDirectory.Delete(true);
        }
        #endregion
    }
}
