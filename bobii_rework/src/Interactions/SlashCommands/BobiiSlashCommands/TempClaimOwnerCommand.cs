using bobii_rework.Entities.EntityFramework;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TempClaimOwnerCommand(InteractionContext context) : BobiiInteractionBase(context)
    {
        #region Tasks
        public override async Task ExecuteCommand()
        {
            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);
            await LoadOwnerSettings(tempChannel!);

            var socketVoice = (SocketVoiceChannel)Context.User!.VoiceChannel;
            var permissions = socketVoice.PermissionOverwrites.ToList();

            permissions = await UpdateOwnerPermissions(permissions, tempChannel!);
            permissions = await UpdateWhiteListIfActive(permissions, tempChannel!);
            permissions = await UpdateBlockedUsers(permissions, tempChannel);

            await socketVoice.ModifyAsync(v => v.PermissionOverwrites = permissions);

            await TempChannelRepository.UpdateOwner(Context.User!.VoiceChannel.Id, Context.User!.Id);
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

        public async Task<List<Overwrite>> UpdateBlockedUsers(List<Overwrite> permissions, TempChannel tempChannel)
        {
            var blockIsDisabled = await TempCommandRepository.CommandDisabled(
                Context.Guild!.Id,
                tempChannel.createchannelid!.Value,
                SlashCommandNames.Block);

            if (blockIsDisabled)
            {
                return permissions;
            }

            return await ModifyBlockedUsersFromOwner(permissions, tempChannel.channelownerid!.Value, PermValue.Inherit);
        }

        public async Task<List<Overwrite>> ModifyBlockedUsersFromOwner(List<Overwrite> permissions, ulong ownerId, PermValue permValue)
        {
            var currentOwnerBlockedUsers = await UsedFunctionsRepository.GetUsedUserFunctions(SlashCommandNames.Block, Context.Guild!.Id, ownerId);
            foreach (var blockedUserFunction in currentOwnerBlockedUsers)
            {
                var blockedUser = await Context.Guild.GetUserAsync(blockedUserFunction.affecteduserid);
                if (blockedUser == null)
                {
                    continue;
                }

                permissions = TempChannelHelper.EditPermissions(
                    permissions, blockedUser.Id,
                    PermissionTarget.User,
                    c => c.Modify(connect: permValue));
            }

            return permissions;
        }

        public async Task<List<Overwrite>> UpdateWhiteListIfActive(List<Overwrite> permissions, TempChannel tempChannel)
        {
            var whiteListActive = await UsedFunctionsRepository.GetUsedChannelFunction(
                    SlashCommandNames.Whitelist,
                    tempChannel.channelid);
            if (whiteListActive == null)
            {
                return permissions;
            }

            var oldWhitelistedUsers = await UsedFunctionsRepository.GetUsedUserFunctions(
                SlashCommandNames.Whitelist,
                Context.Guild!.Id,
                tempChannel.channelownerid!.Value);

            oldWhitelistedUsers.Add(new UsedFunction { affecteduserid = tempChannel.channelownerid!.Value });

            permissions = await UpdateConnectPermissions(permissions, oldWhitelistedUsers, PermValue.Inherit);

            var newWhiteListedUsers = await UsedFunctionsRepository.GetUsedUserFunctions(
                SlashCommandNames.Whitelist,
                Context.Guild!.Id,
                Context.User!.Id);

            await KickNotWhitelistedUser(newWhiteListedUsers, tempChannel);

            permissions = await UpdateConnectPermissions(permissions, newWhiteListedUsers, PermValue.Allow);

            return permissions;
        }

        public async Task KickNotWhitelistedUser(List<UsedFunction> newWhiteListedUsers, TempChannel tempChannel)
        {
            var socketVoiceChannel = (SocketVoiceChannel)Context.User!.VoiceChannel;
            foreach (var user in socketVoiceChannel.ConnectedUsers)
            {
                var userInWhitelist = user.Id == tempChannel.channelownerid || newWhiteListedUsers.Any(u => u.affecteduserid == user.Id);
                if (userInWhitelist)
                {
                    continue;
                }

                var userHasWhitelistedRole = false;
                foreach (var role in user.Roles)
                {
                    if (userHasWhitelistedRole)
                    {
                        break;
                    }
                    userHasWhitelistedRole = newWhiteListedUsers.Any(u => u.affecteduserid == role.Id);
                }

                if (userHasWhitelistedRole)
                {
                    continue;
                }

                await user.ModifyAsync(v => v.Channel = null);
            }
        }

        public async Task<List<Overwrite>> UpdateConnectPermissions(List<Overwrite> permissions, List<UsedFunction> usedFunctions, PermValue permValue)
        {
            foreach (var usedFunction in usedFunctions)
            {
                var user = await Context.Guild!.GetUserAsync(usedFunction.affecteduserid);
                var role = Context.Guild.GetRole(usedFunction.affecteduserid);

                if (user != null)
                {
                    permissions = TempChannelHelper.EditPermissions(
                        permissions, user.Id,
                        PermissionTarget.User,
                        c => c.Modify(connect: permValue));
                    continue;
                }

                permissions = TempChannelHelper.EditPermissions(
                    permissions, role.Id,
                    PermissionTarget.Role,
                    c => c.Modify(connect: permValue));
            }

            return permissions;
        }

        public async Task<List<Overwrite>> UpdateOwnerPermissions(List<Overwrite> permissions, TempChannel tempChannel)
        {
            var currentOwner = await Context.Guild!.GetUserAsync(tempChannel.channelownerid!.Value);
            // Manage Channel Permissions vom aktuellen Owner weg nehmen
            if (currentOwner != null)
            {
                permissions = await TempChannelHelper.UpdateManageChannelRights(permissions, currentOwner, tempChannel, PermValue.Inherit);
            }

            permissions = await TempChannelHelper.UpdateManageChannelRights(permissions, Context.User!, tempChannel, PermValue.Allow);

            return permissions;
        }

        public async Task LoadOwnerSettings(TempChannel tempChannel)
        {
            var creatorChannel = await CreatorChannelRepository.GetCreatorChannel(tempChannel.createchannelid!.Value);
            var userConfig = await TempChannelUserConfigRepository.GetTempChannelUserConfig(
                    creatorChannel!.createchannelid,
                    Context.User!.Id);
            var socketGuildUser = (SocketGuildUser)Context.User;
            var channelName = await TempChannelHelper.GetTempChannelName(creatorChannel, userConfig, socketGuildUser);

            await socketGuildUser.VoiceChannel.ModifyAsync(c =>
            {
                c.Name = channelName;
                c.UserLimit = userConfig?.channelsize ?? 0;
            });
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
