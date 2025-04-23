using bobii_rework.Entities.Interactions.Modals;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.Modals.BobiiModals;
using Discord.Interactions;

namespace bobii_rework.Interactions.Modals
{
    public class TempChannelModals : InteractionModuleBase<InteractionContext>
    {
        [ModalInteraction(ModalCustomIds.TempChannelName)]
        public async Task TempName(TempNameModalEntity modal)
        {
            await new TempNameModal(Context, modal).Execute();
        }

        [ModalInteraction(ModalCustomIds.TempChannelSize)]
        public async Task TempSize(TempSizeModalEntity modal)
        {
            await new TempSizeModal(Context, modal.Size).Execute();
        }
    }
}
