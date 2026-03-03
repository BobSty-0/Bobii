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
            var permissions = socketVoice.PermissionOverwrites.ToList();

            permissions = await TempChannelHelper.UpdateOwnerPermissions(permissions, tempChannel!, newOwner);
            permissions = await TempChannelHelper.UpdateWhiteListIfActive(permissions, tempChannel!, newOwner);
            permissions = await TempChannelHelper.UpdateBlockedUsers(permissions, tempChannel, newOwner);

            await socketVoice.ModifyAsync(v => v.PermissionOverwrites = permissions);

            await TempChannelRepository.UpdateOwner(Context.User!.VoiceChannel.Id, newOwner.Id);

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
            // TODO Noch die andere Checks genau wie GivenUserNotInVoice einbauen
            return await UserNotInTempChannel(tempChannel) ||
                   await NotTheChannelOwner(tempChannel, true) ||
                   await CommandIsDisabled(tempChannel) ||
                   await GivenUserNotInVoice(newOwner) ||
                   await GivenUserNotInSameChannel(newOwner);
        }
        #endregion
    }
}
