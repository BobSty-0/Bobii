using bobii_rework.Extensions;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using bobii_rework.src.Helper;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    // TODO schauen warum der command nicht mehr funktioniert
    public class TempGiveOwnerCommand(InteractionContext context, InteractionReactionType interactionReactionType) : BobiiInteractionBase(context, interactionReactionType)
    {
        #region Tasks
        public override async Task ExecuteCommand()
        {
            await Context.ModifyOriginalResponse(
                await MessageComponentHelper.GetTempGiveOwnerUserSelectMessageComponent(Context.Language));
        }

        public override async Task<bool> CheckData()
        {
            if (await UserNotInVoice())
            {
                return true;
            }

            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);

            return await UserNotInTempChannel(tempChannel) ||
                   await NotTheChannelOwner(tempChannel, true) ||
                   await CommandIsDisabled(tempChannel);
        }

        #endregion
    }
}
