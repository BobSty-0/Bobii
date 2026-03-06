using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using bobii_rework.src.GlobalConstants.Interactions;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Interactions.SlashCommands.BobiiSelectionMenuCommands
{
    public class TempLockCommand(InteractionContext context) : BobiiInteractionBase(context, InteractionReactionType.Defer)
    {
        public override async Task ExecuteCommand()
        {
            var permissions = Context.User.VoiceChannel.PermissionOverwrites.ToList();

            var appId = Configuration.GetConfigValue<ulong>(Configuration.ApplicationID);
            // ToArray, da sonst keine Elemente entfernt werden können
            foreach (var permission in permissions.ToArray())
            {
                // geblockte User werden nicht angefasst
                if (permission.TargetType == PermissionTarget.User)
                {
                    continue;
                }

                if (permission.TargetId == appId)
                {
                    permissions.Remove(permission);
                    continue;
                }

                TempChannelHelper.EditPermissions(
                    permissions,
                    permission.TargetId,
                    permission.TargetType,
                    c => c.Modify(connect: PermValue.Deny));
            }

            var appRole = await TempChannelHelper.GetAppRole(Context.Guild);
            permissions.Add(new Overwrite(appRole.Id, PermissionTarget.Role, TempChannelHelper.GetAppOverwritePermission()));

            // Wenn die everyone Rolle noch nicht vorhanden ist, dann muss diese noch hinzugefügt werden um allen anderen Usern das Joinen zu verbieten
            if (permissions.All(p => p.TargetId != Context.Guild.Id))
            {
                var everyoneRole = await Context.Guild.GetRoleAsync(Context.Guild.Id);
                permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Deny)));
            }

            var socketVoiceChannel = (SocketVoiceChannel)Context.User.VoiceChannel;
            socketVoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

            var tempChannel = await TempChannelRepository.GetTempChannel(socketVoiceChannel.Id);
            await UsedFunctionsRepository.CreateUsedFunction(
                tempChannel.channelownerid.Value,
                0,
                CommandNames.locked,
                Context.Guild.Id,
                socketVoiceChannel.Id);

            await Context.RespondOrModifyOriginalResponse(
                Captions.Success,
                Contents.ChannelSuccessfullyLocked);
        }

        public override async Task<bool> CheckData()
        {
            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);

            return await WhitelistActive(tempChannel) ||
                   await ChannelAlreadyLocked(tempChannel);
        }
    }
}
