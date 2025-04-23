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

        public static async Task<List<Overwrite>> UpdateManageChannelRights(
            List<Overwrite> overwrites,
            IGuildUser user,
            tempchannels tempChannel,
            PermValue permValue)
        {
            var permission = overwrites.SingleOrDefault(u => u.TargetId == user.Id);
            // Wenn es keine Permission gibt, dann müssen auch keine Permissions weggenommen werden
            if (permValue != PermValue.Inherit && permission.TargetId != user.Id)
            {
                return overwrites;
            }

            TempChannelHelper.EditPermissions(
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

            overwrites = await EditConnectPermissionsIfChannelNotLocked(overwrites, user);

            return overwrites;
        }

        public static async Task<List<Overwrite>> EditConnectPermissionsIfChannelNotLocked(
            List<Overwrite> overwrites,
            IGuildUser user)
        {
            var channelLocked = await UsedFunctionsRepository.GetUsedChannelFunction(
                SlashCommandNames.Lock,
                user.VoiceChannel.Id);

            // Wenn der Channel gelockt ist, dann soll er connect beibehalten
            if (channelLocked == null)
            {
                TempChannelHelper.EditPermissions(
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
            tempchannels tempChannel,
            PermValue permValue)
        {
            var ownerPermissions = await TempCommandRepository.CommandDisabled(
                user.GuildId,
                tempChannel.createchannelid!.Value,
                UtilityNames.OwnerPermissions);

            if (!ownerPermissions)
            {
                TempChannelHelper.EditPermissions(
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

        public static async Task<string> GetTempChannelName(createtempchannels creatorChannel, tempchanneluserconfig? userConfig, SocketGuildUser user)
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
