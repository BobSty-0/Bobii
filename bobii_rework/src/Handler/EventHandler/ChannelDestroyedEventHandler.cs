using bobii_rework.Repositories;
using Discord.WebSocket;

namespace bobii_rework.Handler.EventHandler
{
    public class ChannelDestroyedEventHandler : EventHandlerBase
    {
        #region Declarations
        private SocketChannel _channel;
        #endregion

        #region Task
        public async Task ExecuteChannelDestroyedActionAsync(SocketChannel channel)
        {
            _channel = channel;
            await Execute();
        }
        #endregion

        #region Overrides
        public override async Task ExecuteEvent()
        {
            await DeleteCreatorChannelInterfaceIfExisting(_channel);
            await CreatorChannelRepository.RemoveCreatorChannelIfExisting(_channel.Id);
            await TempChannelRepository.RemoveTempChannelIfExisting(_channel.Id);
            await UsedFunctionsRepository.RemoveUsedChannelFunctionsIfExisting(_channel.Id);
            // Count aktualisieren einbauen
        }
        #endregion

        #region Private Tasks
        private async Task DeleteCreatorChannelInterfaceIfExisting(SocketChannel channel)
        {
            var creatorChannel = await CreatorChannelRepository.GetCreatorChannel(channel.Id);
            if (creatorChannel == null)
            {
                return;
            }

            var socketGuildChannel = (SocketGuildChannel)channel;
            var interfacePath = Path.Combine(Directory.GetCurrentDirectory(), socketGuildChannel.Guild.Id.ToString(), channel.Id.ToString());
            if (Directory.Exists(interfacePath))
            {
                Directory.Delete(interfacePath, true);
            }
        }
        #endregion

    }
}
