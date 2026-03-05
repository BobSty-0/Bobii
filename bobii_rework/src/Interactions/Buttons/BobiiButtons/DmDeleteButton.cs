using bobii_rework.src.Enums;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Interactions.Buttons.BobiiButtons
{
    public class DmDeleteButton(InteractionContext context) : BobiiInteractionBase(context, ResponseType.None)
    {
        public override async Task ExecuteCommand()
        {
            var socketMessageComponent = (SocketMessageComponent)Context.Interaction!;
            await socketMessageComponent.Message.DeleteAsync();
        }
    }
}
