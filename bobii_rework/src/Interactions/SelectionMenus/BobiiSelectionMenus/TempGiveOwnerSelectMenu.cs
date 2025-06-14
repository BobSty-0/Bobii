using bobii_rework.Repositories;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus
{
    public class TempGiveOwnerSelectMenu(InteractionContext context) : BobiiInteractionBase(context, false)
    {
        #region Tasks
        public override async Task ExecuteCommand()
        {

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
