using bobii_rework.Entities.EntityFramework;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Repositories;
using Discord;
using Discord.WebSocket;

namespace bobii_rework.Helper
{
    public static class TempChannelHelper
    {
        #region Constants
        private const string UsernameKeyword = "{username}";
        private const string CountKeyword = "{count}";
        private const string NicknameKeyword = "{nickname}";
        #endregion

        #region Tasks
        public static async Task<List<Overwrite>> UpdateBlockedUsers(List<Overwrite> permissions, TempChannel tempChannel, IGuildUser newOwner)
        {
            var blockIsDisabled = await TempCommandRepository.CommandDisabled(
                newOwner.Guild!.Id,
                tempChannel.createchannelid!.Value,
                SlashCommandNames.Block);

            if (blockIsDisabled)
            {
                return permissions;
            }

            return await ModifyBlockedUsersFromOwner(permissions, tempChannel, PermValue.Inherit, newOwner);
        }

        public static async Task<List<Overwrite>> ModifyBlockedUsersFromOwner(List<Overwrite> permissions, TempChannel tempChannel, PermValue permValue, IGuildUser guildUser)
        {
            var oldOwnerBlockedUsers = await UsedFunctionsRepository.GetUsedUserFunctions(SlashCommandNames.Block, guildUser.Guild!.Id, tempChannel.channelownerid!.Value);
            foreach (var blockedUserFunction in oldOwnerBlockedUsers)
            {
                var blockedUser = await guildUser.Guild.GetUserAsync(blockedUserFunction.affecteduserid);
                if (blockedUser == null)
                {
                    continue;
                }

                permissions = EditPermissions(
                    permissions, blockedUser.Id,
                    PermissionTarget.User,
                    c => c.Modify(connect: permValue));
            }

            var currentOwnerBlockedUsers = await UsedFunctionsRepository.GetUsedUserFunctions(SlashCommandNames.Block, guildUser.Guild!.Id, guildUser!.Id);
            foreach (var blockedUserFunction in currentOwnerBlockedUsers)
            {
                var blockedUser = await guildUser.Guild.GetUserAsync(blockedUserFunction.affecteduserid);
                if (blockedUser == null)
                {
                    continue;
                }

                permissions = EditPermissions(
                    permissions, blockedUser.Id,
                    PermissionTarget.User,
                    c => c.Modify(connect: permValue));
            }

            return permissions;
        }

        public static async Task<List<Overwrite>> UpdateWhiteListIfActive(List<Overwrite> permissions, TempChannel tempChannel, IGuildUser newOwner)
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
                newOwner.Guild!.Id,
                tempChannel.channelownerid!.Value);

            oldWhitelistedUsers.Add(new UsedFunction { affecteduserid = tempChannel.channelownerid!.Value });

            permissions = await UpdateConnectPermissions(permissions, oldWhitelistedUsers, PermValue.Inherit, newOwner);

            var newWhiteListedUsers = await UsedFunctionsRepository.GetUsedUserFunctions(
                SlashCommandNames.Whitelist,
                newOwner.Guild!.Id,
                newOwner!.Id);

            await KickNotWhitelistedUser(newWhiteListedUsers, tempChannel, newOwner);

            permissions = await UpdateConnectPermissions(permissions, newWhiteListedUsers, PermValue.Allow, newOwner);

            return permissions;
        }

        public static async Task KickNotWhitelistedUser(List<UsedFunction> newWhiteListedUsers, TempChannel tempChannel, IGuildUser guildUser)
        {
            var socketVoiceChannel = (SocketVoiceChannel)guildUser.VoiceChannel;
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

        public static async Task<List<Overwrite>> UpdateConnectPermissions(List<Overwrite> permissions, List<UsedFunction> usedFunctions, PermValue permValue, IGuildUser guildUser)
        {
            foreach (var usedFunction in usedFunctions)
            {
                var user = await guildUser.Guild!.GetUserAsync(usedFunction.affecteduserid);
                var role = guildUser.Guild.GetRole(usedFunction.affecteduserid);

                if (user != null)
                {
                    permissions = EditPermissions(
                        permissions, user.Id,
                        PermissionTarget.User,
                        c => c.Modify(connect: permValue));
                    continue;
                }

                permissions = EditPermissions(
                    permissions, role.Id,
                    PermissionTarget.Role,
                    c => c.Modify(connect: permValue));
            }

            return permissions;
        }

        public static async Task<List<Overwrite>> UpdateOwnerPermissions(List<Overwrite> permissions, TempChannel tempChannel, IGuildUser newOwner)
        {
            var currentOwner = await newOwner.Guild!.GetUserAsync(tempChannel.channelownerid!.Value);
            // Manage Channel Permissions vom aktuellen Owner weg nehmen
            if (currentOwner != null)
            {
                permissions = await UpdateManageChannelRights(permissions, currentOwner, tempChannel, PermValue.Inherit);
            }

            permissions = await UpdateManageChannelRights(permissions, newOwner!, tempChannel, PermValue.Allow);

            return permissions;
        }

        public static async Task LoadOwnerSettings(TempChannel tempChannel, IGuildUser newOwner)
        {
            var creatorChannel = await CreatorChannelRepository.GetCreatorChannel(tempChannel.createchannelid!.Value);
            var userConfig = await TempChannelUserConfigRepository.GetTempChannelUserConfig(
                creatorChannel!.createchannelid,
                newOwner.Id);
            var socketGuildUser = (SocketGuildUser)newOwner;
            var channelName = await GetTempChannelName(creatorChannel, userConfig, socketGuildUser);

            await socketGuildUser.VoiceChannel.ModifyAsync(c =>
            {
                c.Name = channelName;
                c.UserLimit = userConfig?.channelsize ?? 0;
            });
        }

        public static async Task<List<Overwrite>> UpdateManageChannelRights(
            List<Overwrite> overwrites,
            IGuildUser user,
            TempChannel tempChannel,
            PermValue permValue)
        {
            var permission = overwrites.SingleOrDefault(u => u.TargetId == user.Id);
            // Wenn es keine Permission gibt, dann müssen auch keine Permissions weggenommen werden
            if (permValue != PermValue.Inherit && permission.TargetId != user.Id)
            {
                return overwrites;
            }

            EditPermissions(
                overwrites,
                user.Id,
                PermissionTarget.User,
               o => o.Modify(
                   connect: permValue,
                   manageMessages: permValue,
                   sendMessages: permValue,
                   viewChannel: permValue,
                   useSlashCommands: permValue,
                   speak: permValue));

            overwrites = await EditManageChannelPermissions(overwrites, user, tempChannel, permValue);

            if (permValue != PermValue.Inherit)
            {
                return overwrites;
            }

            // TODO 

            overwrites = await EditConnectPermissionsIfChannelNotLocked(overwrites, user);

            return overwrites;
        }

        public static async Task<List<Overwrite>> EditConnectPermissionsIfChannelNotLocked(
            List<Overwrite> overwrites,
            IGuildUser user)
        {
            // TODO Hier muss noch dringend geprüft werden, wie das mit 2 Usern aussieht.
            // User.VoiceChannel läuft z.B. bei claim owner in einen Fehler, wenn der Owner raus ist.
            var channelLocked = await UsedFunctionsRepository.GetUsedChannelFunction(
                SlashCommandNames.Lock,
                user.VoiceChannel.Id);

            // Wenn der Channel gelockt ist, dann soll er connect beibehalten
            if (channelLocked == null)
            {
                EditPermissions(
                    overwrites,
                    user.Id,
                    PermissionTarget.User,
                    o => o.Modify(
                        connect: PermValue.Allow));
            }

            return overwrites;
        }

        public static async Task<List<Overwrite>> EditManageChannelPermissions(
            List<Overwrite> overwrites,
            IGuildUser user,
            TempChannel tempChannel,
            PermValue permValue)
        {
            var ownerPermissions = await TempCommandRepository.CommandDisabled(
                user.GuildId,
                tempChannel.createchannelid!.Value,
                UtilityNames.OwnerPermissions);

            if (!ownerPermissions)
            {
                EditPermissions(
                    overwrites,
                    user.Id,
                    PermissionTarget.User,
                    o => o.Modify(
                        manageChannel: permValue));
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissions(
            List<Overwrite> overwrites,
            ulong entityId,
            PermissionTarget permissionTarget,
            Func<OverwritePermissions, OverwritePermissions> modifyPermissions)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == entityId);

            if (overwrite.TargetId != entityId)
            {
                overwrites.Add(new Overwrite(entityId, permissionTarget, modifyPermissions(new OverwritePermissions())));
                return overwrites;
            }

            overwrite = new Overwrite(entityId, permissionTarget, modifyPermissions(overwrite.Permissions));
            var index = overwrites.FindIndex(o => o.TargetId == entityId);

            if (index != -1)
                overwrites[index] = overwrite;

            return overwrites;
        }

        public static async Task<string> GetTempChannelName(CreateTempChannel creatorChannel, TempChannelUserConfig? userConfig, SocketGuildUser user)
        {
            var tempChannelName = !string.IsNullOrEmpty(userConfig?.tempchannelname) ? userConfig.tempchannelname : creatorChannel.tempchannelname;

            if (tempChannelName.Contains(CountKeyword))
            {
                var newChannelCount = await TempChannelRepository.GetMaxTempChannelCount(creatorChannel.createchannelid) + 1;
                tempChannelName = tempChannelName.Replace(CountKeyword, newChannelCount.ToString());
            }

            if (tempChannelName.Contains(UsernameKeyword))
            {
                tempChannelName = tempChannelName.Replace(UsernameKeyword, user.GlobalName);
            }

            if (tempChannelName.Contains(NicknameKeyword))
            {
                var nickName = !string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.GlobalName;
                tempChannelName = tempChannelName.Replace(NicknameKeyword, nickName);
            }

            if (string.IsNullOrEmpty(tempChannelName))
            {
                tempChannelName = "Temp Voice";
            }

            return tempChannelName;
        }
    }
    #endregion
}
