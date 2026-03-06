using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SlashCommands.BobiiSelectionMenuCommands;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus
{
    public class TempPrivacySelectionMenu(InteractionContext context, string value) : BobiiInteractionBase(context, InteractionReactionType.None)
    {
        public override async Task ExecuteCommand()
        {
            switch (value)
            {
                case SelectMenuValues.TempChannelLock:
                    await new TempLockCommand(context).Execute();
                    break;
                case SelectMenuValues.TempChannelUnlock:
                    break;
                case SelectMenuValues.TempChannelHide:
                    break;
                case SelectMenuValues.TempChannelUnhide:
                    break;
            }
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
    }
}
