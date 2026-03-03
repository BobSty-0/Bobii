using bobii_rework.Extensions;
using bobii_rework.Repositories;
using bobii_rework.src.Helper;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TempGiveOwnerCommand(InteractionContext context) : BobiiInteractionBase(context)
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
