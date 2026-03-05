using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus
{
    public class TempGiveOwnerSelectMenu(InteractionContext context, IGuildUser newOwner) : BobiiInteractionBase(context, false)
    {
        #region Tasks
        public override async Task ExecuteCommand()
        {
            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);
            await TempChannelHelper.LoadOwnerSettings(tempChannel!, newOwner);

            var socketVoice = (SocketVoiceChannel)Context.User!.VoiceChannel;
            await TempChannelHelper.TransferOwner(tempChannel, socketVoice, newOwner);

            await Context.ModifyOriginalResponse(
    Captions.Success,
    Contents.OwnerChanged,
    [newOwner.Id]);
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
                   await GivenUserNotInVoice(newOwner) ||
                   await GivenUserNotInSameChannel(newOwner);
        }
        #endregion
    }
}
