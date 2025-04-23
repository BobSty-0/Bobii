using bobii_rework.Extensions;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Handler.EventHandler
{
    public class InteractionCreatedEventHandler
    {
        #region Declarations
        private readonly InteractionService _interactionService;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region Constructor
        public InteractionCreatedEventHandler(
            InteractionService interactionService,
            DiscordSocketClient client,
            IServiceProvider serviceProvider)
        {
            _interactionService = interactionService;
            _client = client;
            _serviceProvider = serviceProvider;
        }
        #endregion

        #region Tasks
        public async Task ExecuteInteractionCreatedActionAsync(SocketInteraction interaction)
        {
            try
            {
                await _interactionService.ExecuteCommandAsync(new InteractionContext(_client, interaction), _serviceProvider);
            }
            catch (Exception ex)
            {
                this.WriteLineToConsole(ex.Message);
            }
        }
        #endregion
    }
}
