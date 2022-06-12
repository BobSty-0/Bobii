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
        [ModalInteraction("createtempchannel_update_name_modal*,*")]
        public async Task ModalUpdateNameResponse(string id, string language, ChangeCreateTempChannelNameModal modal)
        {
            await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeTempChannelName(modal.NewName, ulong.Parse(id));
            await Context.Interaction.RespondAsync(null, new Discord.Embed[] { Bobii.Helper.CreateEmbed(Context.Interaction,
               string.Format(Bobii.Helper.GetContent("C171", language).Result, modal.NewName), Bobii.Helper.GetCaption("C174", language).Result).Result});
        }
    }
}
