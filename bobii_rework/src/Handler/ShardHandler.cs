using bobii_rework.Handler.EventHandler;
using bobii_rework.Handler.UtilityHandler;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using UserLeftEventHandler = bobii_rework.Handler.EventHandler.UserLeftEventHandler;

namespace bobii_rework.Handler
{
    public class ShardHandler
    {
        #region Declarations
        private readonly DiscordSocketClient _client;
        private InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;

        private InteractionCreatedEventHandler _interactionHandler;
        private TempChannelDelayHandler _tempChannelDelayHandler;
        #endregion

        #region Constructor
        public ShardHandler(DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = client;

            _interactionService = _serviceProvider.GetRequiredService<InteractionService>();
            _tempChannelDelayHandler = _serviceProvider.GetRequiredService<TempChannelDelayHandler>();
            _interactionHandler = new InteractionCreatedEventHandler(
                _interactionService,
                _client,
                _serviceProvider);

            SubToEvents();
        }
        #endregion

        #region Private Functions
        private void SubToEvents()
        {
            //_client.Ready += _messageHandler.ExecuteClientReadyActionAsync;
            _client.InteractionCreated += _interactionHandler.ExecuteInteractionCreatedActionAsync;
            //_client.MessageReceived += _messageHandler.ExecuteMessageReceivedActionAsync;
            //_client.MessageDeleted += _messageHandler.ExecuteMessageDeletedActionAsync;
             _client.LeftGuild += new LeftGuildEventHandler().ExecuteLeftGuildActionAsync;
            _client.JoinedGuild += new JoinedGuildEventHandler().ExecuteJoinedGuildActionAsync;
            _client.UserVoiceStateUpdated += new UserVoiceStateUpdatedEventHandler(_client, _tempChannelDelayHandler).ExecuteVoiceStateUpdatedActionAsync;
            _client.ChannelDestroyed += new ChannelDestroyedEventHandler().ExecuteChannelDestroyedActionAsync;
            _client.UserJoined += new UserJoinedEventHandler().ExecuteUserJoinedActionAsync;
            _client.UserLeft += new UserLeftEventHandler().ExecuteUserLeftActionAsync;
        }
        #endregion
    }
}
