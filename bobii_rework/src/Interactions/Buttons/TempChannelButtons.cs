using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using bobii_rework.src.Enums;
using Discord.Interactions;

namespace bobii_rework.Interactions.Buttons
{
    public class TempChannelButtons : InteractionModuleBase<InteractionContext>
    {
        [ComponentInteraction(ButtonCustomIds.TempChannelName)]
        public async Task TempName()
        {
            await new TempNameCommand(Context, ResponseType.Respond).Execute();
        }

        [ComponentInteraction(ButtonCustomIds.TempChannelSize)]
        public async Task TempSize()
        {
            await new TempSizeCommand(Context, ResponseType.Respond).Execute();
        }

        [ComponentInteraction(ButtonCustomIds.TempChannelClaimOwner)]
        public async Task TempClaimOwner()
        {
            await new TempClaimOwnerCommand(Context, ResponseType.Respond).Execute();
        }

        [ComponentInteraction(ButtonCustomIds.TempChannelGiveOwner)]
        public async Task TempGiveOwner()
        {
            await new TempGiveOwnerCommand(Context, ResponseType.Respond).Execute();
        }

        [ComponentInteraction(ButtonCustomIds.TempChannelPrivacy)]
        public async Task TempPrivacy()
        {
            await new TempPrivacyCommand(Context, ResponseType.Respond).Execute();
        }
    }
}
