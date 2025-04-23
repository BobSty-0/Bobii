using bobii_rework.Repositories;
using Discord.WebSocket;

namespace bobii_rework.Handler.EventHandler
{
    public class UserLeftEventHandler : EventHandlerBase
    {
        #region Declarations
        private SocketGuild _guild;
        private SocketUser _user;
#endregion

        #region Tasks
        public async Task ExecuteUserLeftActionAsync(SocketGuild guild, SocketUser user)
        {
            _guild = guild;
            _user = user;
            await Execute();
        }
        #endregion

        #region Overrides
        public override async Task ExecuteEvent()
        {
            await UsedFunctionsRepository.RemoveUsedFunctionsIfExisting(_guild.Id, _user.Id);
            await TempChannelUserConfigRepository.RemoveTempChannelUserConfigsIfExisting(_guild.Id, _guild.Id);
        }
        #endregion

    }
}
