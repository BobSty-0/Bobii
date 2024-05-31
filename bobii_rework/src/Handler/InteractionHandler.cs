using bobii_rework.Extensions;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Handler
{
    public class InteractionHandler
    {
        #region Declarations
        private readonly MessageComponentHandler _messageComponentHandler;
        private readonly InteractionService _interactionService;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region Constructor
        public InteractionHandler(
            MessageComponentHandler messageComponentHandler,
            InteractionService interactionService,
            DiscordSocketClient client,
            IServiceProvider serviceProvider)
        {
            _messageComponentHandler = messageComponentHandler;
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
                var context = new InteractionContext(_client, interaction);

                await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
                //switch (interaction.Type)
                //{
                //    case InteractionType.MessageComponent:
                //        await _messageComponentHandler.ExecuteMessageComponentAction(context);
                //        break;
                //    case InteractionType.ApplicationCommand:
                //        await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
                //        break;
                //    default:
                //        throw new NotSupportedException($"{interaction.Type} wird nicht unterstützt");
                //}

            }
            catch (Exception ex)
            {
                this.WriteLineToConsole(ex.Message);
            }
        }
        #endregion
    }
}
