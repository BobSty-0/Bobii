using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    // TODO Ratelimit beachten wie beim Namen !!
    // TODO Schauen warum ich hier bitte auch die commands ausführen kann auch wenn ich kein Owner bin
    public class TempSizeCommand(InteractionContext context) : BobiiInteractionBase(context, InteractionReactionType.None)
    {
        public override async Task ExecuteCommand()
        {
            var modal = await ModalHelper.GetTempSizeModal(Context);
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
