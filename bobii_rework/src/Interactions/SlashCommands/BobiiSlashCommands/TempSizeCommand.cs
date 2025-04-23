using bobii_rework.Extensions;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ImageMagick;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TempSizeCommand : BobiiInteractionBase
    {
        public TempSizeCommand(InteractionContext context) : base(context, false)
        {
        }

        public override async Task ExecuteCommand()
        {
            var modal = await ModalHelper.GetTempSizeModal(Context);
            await Context.Interaction!.RespondWithModalAsync(modal.Build());
        }

        public override async  Task<bool> CheckData()
        {
            if (await UserNotInVoice())
            {
                return true;
            }

            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);

            return await UserNotInTempChannel(tempChannel) || 
                   await NotTheChannelOwnerOrMod(tempChannel) || 
                   await CommandIsDisabled(tempChannel);
        }
    }
}
