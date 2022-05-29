using Bobii.src.Modals;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.ModalInteractions
{
    class TempChannelModalInteractions : InteractionModuleBase<SocketInteractionContext>
    {
        [ModalInteraction("createtempchannel_update_name_modal")]
        public async Task ModalResponse(ChangeCreateTempChannelNameModal modal)
        {
            var test = modal.NewName;
            await Context.Interaction.DeferAsync();
        }
    }
}
