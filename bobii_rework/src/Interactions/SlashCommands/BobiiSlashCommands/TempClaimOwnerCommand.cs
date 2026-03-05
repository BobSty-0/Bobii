using bobii_rework.Entities.EntityFramework;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TempClaimOwnerCommand(InteractionContext context, ResponseType responseType) : BobiiInteractionBase(context, responseType)
    {
        #region Tasks
        public override async Task ExecuteCommand()
        {
            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);
            await TempChannelHelper.LoadOwnerSettings(tempChannel!, Context.User);

            var socketVoice = (SocketVoiceChannel)Context.User!.VoiceChannel;
            var permissions = socketVoice.PermissionOverwrites.ToList();

            permissions = await TempChannelHelper.UpdateOwnerPermissions(permissions, tempChannel!, Context.User);
            permissions = await TempChannelHelper.UpdateWhiteListIfActive(permissions, tempChannel!, Context.User);
            permissions = await TempChannelHelper.UpdateBlockedUsers(permissions, tempChannel, Context.User);

            await socketVoice.ModifyAsync(v => v.PermissionOverwrites = permissions);

            await TempChannelRepository.UpdateOwner(Context.User!.VoiceChannel.Id, Context.User!.Id);
            await Context.RespondOrModifyOriginalResponse(
                Captions.Success,
                Contents.OwnerChanged,
                [Context.User!.Id]);
        }

        public override async Task<bool> CheckData()
        {
            if (await UserNotInVoice())
            {
                return true;
            }

            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);

            return await UserNotInTempChannel(tempChannel) ||
                   await CommandIsDisabled(tempChannel) ||
                   await IsOwner(tempChannel) ||
                   await OwnerStillInVoice(tempChannel);
        }



        public async Task<bool> IsOwner(TempChannel tempChannel)
        {
            if (tempChannel.channelownerid != Context.User!.Id)
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.AlreadyOwner);
            return true;
        }

        public async Task<bool> OwnerStillInVoice(TempChannel tempChannel)
        {
            var socketVoiceChannel = (SocketVoiceChannel)Context.User!.VoiceChannel;
            if (socketVoiceChannel.ConnectedUsers.All(u => u.Id != tempChannel.channelownerid))
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.OwnerStillInVoice,
                new object[] { tempChannel.channelownerid! });
            return true;
        }
        #endregion
    }
}
