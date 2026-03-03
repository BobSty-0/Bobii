using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using Discord.Interactions;

namespace bobii_rework.Interactions.Buttons
{
    public class TempChannelButtons : InteractionModuleBase<InteractionContext>
    {
        [ComponentInteraction(ButtonCustomIds.TempChannelName)]
        public async Task TempName()
        {
            await new TempNameCommand(Context).Execute();
        }

        [ComponentInteraction(ButtonCustomIds.TempChannelSize)]
        public async Task TempSize()
        {
            await new TempSizeCommand(Context).Execute();
        }

        [ComponentInteraction(ButtonCustomIds.TempChannelClaimOwner)]
        public async Task TempClaimOwner()
        {
            await new TempClaimOwnerCommand(Context).Execute();
        }

        [ComponentInteraction(ButtonCustomIds.TempChannelGiveOwner)]
        public async Task TempGiveOwner()
        {
            await new TempGiveOwnerCommand(Context).Execute();
        }

        [ComponentInteraction(ButtonCustomIds.TempChannelPrivacy)]
        public async Task TempPrivacy()
        {
            await new TempPrivacyCommand(Context).Execute();
        }
    }
}
