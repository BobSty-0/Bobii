using Bobii.src.Bobii;
using Bobii.src.Helper;
using Bobii.src.Modals;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.ModalInteractions
{
    class TempChannelModalInteractions : InteractionModuleBase<SocketInteractionContext>
    {
        [ModalInteraction("createtempchannel_update_name_modal*,*")]
        public async Task ModalUpdateCreateTempChannelNameResponse(string id, string language, ChangeCreateTempChannelNameModal modal)
        {
            await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeTempChannelName(modal.NewName, ulong.Parse(id));
            await Context.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
               string.Format(GeneralHelper.GetContent("C171", language).Result, modal.NewName), GeneralHelper.GetCaption("C174", language).Result).Result});
            // TODO Write to console somehow
        }

        [ModalInteraction("tempchannel_update_name_modal*,*")]
        public async Task ModalUpdateTempChannelNameResponse(string id, string language, ChangeCreateTempChannelNameModal modal)
        {
            try
            {
                _ = Task.Run(async () => ((SocketVoiceChannel)Context.Client.GetChannel(id.ToUlong())).ModifyAsync(channel => channel.Name = modal.NewName));

                await Context.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                    string.Format(GeneralHelper.GetContent("C118", language).Result, modal.NewName),
                    GeneralHelper.GetCaption("C118", language).Result).Result }, ephemeral: true);
                // TODO Write to console somehow
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                    GeneralHelper.GetContent("C119", language).Result,
                    GeneralHelper.GetCaption("C038", language).Result).Result }, ephemeral: true);
                return;
                // TODO Write to console somehow
            }
        }
    }
}
