using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Interactions.Buttons.BobiiButtons
{
    internal class DmDeleteButton : BobiiInteractionBase
    {
        public DmDeleteButton(InteractionContext context) : base(context)
        {
        }

        public override async Task ExecuteCommand()
        {
            var socketMessageComponent = (SocketMessageComponent)Context.Interaction!;
            await socketMessageComponent.Message.DeleteAsync();
        }
    }
}
