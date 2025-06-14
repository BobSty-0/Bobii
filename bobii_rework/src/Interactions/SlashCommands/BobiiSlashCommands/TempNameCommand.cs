using bobii_rework.Helper;
using bobii_rework.Repositories;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TempNameCommand(InteractionContext context) : BobiiInteractionBase(context, false)
    {
        public override async Task ExecuteCommand()
        {
            var modal = await ModalHelper.GetTempNameModal(Context);
            await Context.Interaction!.RespondWithModalAsync(modal.Build());
        }

        public override async Task<bool> CheckData()
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
