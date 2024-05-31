using Discord;
using Discord.WebSocket;

namespace bobii_rework.Handler
{
    public class MessageHandler
    {
        #region Declarations
        private readonly DiscordSocketClient _client;
        private IChannel _dmSupportChannel;
        #endregion

        #region Constructor
        public MessageHandler(DiscordSocketClient client)
        {
            _client = client;
        }
        #endregion

        #region Tasks
        public async Task ExecuteClientReadyActionAsync()
        {
            _client.Ready -= ExecuteClientReadyActionAsync;

            var supportGuidId = Configuration.GetConfigValue<ulong>(Configuration.SupportGuildID);
            var supportGuild = await _client.Rest.GetGuildAsync(supportGuidId);
        }

        public async Task ExecuteMessageReceivedActionAsync(IMessage message)
        {

        }

        public async Task ExecuteMessageDeletedActionAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {

        }
        #endregion
    }
}
