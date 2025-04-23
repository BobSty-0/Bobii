using Discord.WebSocket;

namespace bobii_rework.Handler.EventHandler
{
    public class UserJoinedEventHandler : EventHandlerBase
    {
        #region Declarations
        private SocketGuildUser _user;
        #endregion

        #region Tasks
        public async Task ExecuteUserJoinedActionAsync(SocketGuildUser user)
        {
            _user = user;
            await Execute();
        }
        #endregion

        #region Overrides
        public override async Task ExecuteEvent()
        {
        }
        #endregion
    }
}
