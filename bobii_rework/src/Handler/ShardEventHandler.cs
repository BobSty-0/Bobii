using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace bobii_rework.Handler
{
    public class ShardEventHandler
    {
        #region Declarations
        private readonly DiscordSocketClient _client;
        private InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;

        private MessageComponentHandler _messageComponentHandler;
        private VoiceHandler _voiceHandler;
        private MessageHandler _messageHandler;
        private GuildHandler _guildHandler;
        private InteractionHandler _interactionHandler;
        private ChannelHandler _channelHandler;
        private ModalHandler _modalHandler;
        private UserHandler _userHandler;
        #endregion

        #region Constructor
        public ShardEventHandler(DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = client;

            InitHandlerAndServices();
            SubToEvents();
        }
        #endregion

        #region Private Functions
        private void InitHandlerAndServices()
        {
            _interactionService = _serviceProvider.GetRequiredService<InteractionService>();

            _messageComponentHandler = new MessageComponentHandler();
            _voiceHandler = new VoiceHandler();
            _guildHandler = new GuildHandler();
            _channelHandler = new ChannelHandler();
            _modalHandler = new ModalHandler();
            _userHandler = new UserHandler();

            _messageHandler = new MessageHandler(_client);

            _interactionHandler = new InteractionHandler(
                _messageComponentHandler, 
                _interactionService, 
                _client, 
                _serviceProvider);
        }

        private void SubToEvents()
        {
            _client.Ready += _messageHandler.ExecuteClientReadyActionAsync;
            _client.InteractionCreated += _interactionHandler.ExecuteInteractionCreatedActionAsync;
            _client.MessageReceived += _messageHandler.ExecuteMessageReceivedActionAsync;
            _client.MessageDeleted += _messageHandler.ExecuteMessageDeletedActionAsync;
            _client.LeftGuild += _guildHandler.ExecuteLeftGuildActionAsync;
            _client.JoinedGuild += _guildHandler.ExecuteJoinedGuildActionAsync;
            _client.UserVoiceStateUpdated += _voiceHandler.ExecuteVoiceStateUpdatedActionAsync;
            _client.ChannelDestroyed += _channelHandler.ExecuteChannelDestroyedActionAsync;
            _client.ModalSubmitted += _modalHandler.ExecuteModalSubmittedActionAsync;
            _client.UserJoined += _userHandler.ExecuteUserJoinedActionAsync;
            _client.UserLeft += _userHandler.ExecuteUserLeftActionAsync;
        }
        #endregion
    }
}
