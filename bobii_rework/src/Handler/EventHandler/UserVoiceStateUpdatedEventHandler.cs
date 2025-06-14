using bobii_rework.Entities.EntityFramework;
using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Handler.UtilityHandler;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.Entities.VoiceChannel;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace bobii_rework.Handler.EventHandler
{
    public class UserVoiceStateUpdatedEventHandler : EventHandlerBase
    {
        #region Declarations
        private readonly DiscordSocketClient _client;
        private readonly TempChannelDelayHandler _tempChannelDelayHandler;

        private SocketVoiceState _oldVoiceState;
        private SocketVoiceState _newVoiceState;
        private SocketUser _user;
        #endregion

        #region Consturctors

        public UserVoiceStateUpdatedEventHandler(DiscordSocketClient client, TempChannelDelayHandler tempChannelDelayHandler)
        {
            _client = client;
            _tempChannelDelayHandler = tempChannelDelayHandler;
        }
        #endregion

        #region Tasks
        public async Task ExecuteVoiceStateUpdatedActionAsync(SocketUser user, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {
            _user = user;
            _oldVoiceState = oldVoiceState;
            _newVoiceState = newVoiceState;
            await Execute();
        }

        #endregion

        #region Overrieds
        public override async Task ExecuteEvent()
        {
            var voiceAction = GetVoiceAction(_oldVoiceState, _newVoiceState);
            var voiceUpdatedContext = GetVoiceUpdatedContext(_user, _oldVoiceState, _newVoiceState, voiceAction);

            switch (voiceAction)
            {
                case VoiceAction.UserJoinedAChannel:
                    await HandleUserJoinedChannel(voiceUpdatedContext);
                    break;
                case VoiceAction.UserLeftAChannel:
                    await HandleUserLeftChannel(voiceUpdatedContext);
                    break;
                case VoiceAction.UserLeftAndJoinedChannel:
                    await HandleUserJoinedChannel(voiceUpdatedContext);
                    await HandleUserLeftChannel(voiceUpdatedContext);
                    break;
                case VoiceAction.NoAction:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(voiceAction.ToString());
            }
        }
        #endregion

        #region Private Tasks
        private async Task HandleUserLeftChannel(VoiceUpdatedContext voiceUpdatedContext)
        {
            var tempChannel = await TempChannelRepository.GetTempChannel(voiceUpdatedContext.OldVoiceChannel.Id);
            if (tempChannel == null || await NutzerIstChannelOwner(voiceUpdatedContext))
            {
                return;
            }

            await RemoveConnectRightsIfChannelLocked(voiceUpdatedContext, tempChannel);

            if (voiceUpdatedContext.OldVoiceChannel.ConnectedUsers.Count == 0)
            {
                await voiceUpdatedContext.OldVoiceChannel.DeleteAsync();
            }
        }

        private async Task RemoveConnectRightsIfChannelLocked(VoiceUpdatedContext voiceUpdatedContext, TempChannel tempChannel)
        {
            var channelLocked = await UsedFunctionsRepository.GetUsedChannelFunction(SlashCommandNames.Lock, tempChannel.channelid) != null;
            var userBlocked = await UsedFunctionsRepository.GetUsedUserFunction(
                SlashCommandNames.Block,
                voiceUpdatedContext.Guild!.Id,
                tempChannel.channelownerid.GetValueOrDefault(),
                voiceUpdatedContext.User!.Id) != null;

            if (channelLocked || userBlocked)
            {
                var permissions = TempChannelHelper.EditPermissions(
                    voiceUpdatedContext.OldVoiceChannel.PermissionOverwrites.ToList(),
                    voiceUpdatedContext.User.Id,
                    PermissionTarget.User,
                    o => o.Modify(connect: PermValue.Inherit));

                await voiceUpdatedContext.OldVoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
            }
        }

        private async Task<bool> NutzerIstChannelOwner(VoiceUpdatedContext voiceUpdatedContext)
        {
            if (voiceUpdatedContext.VoiceAction != VoiceAction.UserLeftAndJoinedChannel)
            {
                return false;
            }

            var creatorChannel = await CreatorChannelRepository.GetCreatorChannel(voiceUpdatedContext.NewVoiceChannel.Id);
            var existingTempChannel = await TempChannelRepository.GetTempChannel(creatorChannel?.createchannelid, voiceUpdatedContext.User!.Id);
            if (existingTempChannel == null)
            {
                return false;
            }

            var tempVoice = (SocketVoiceChannel)await voiceUpdatedContext.Client!.GetChannelAsync(existingTempChannel.channelid);
            return tempVoice?.Id == voiceUpdatedContext.OldVoiceChannel.Id &&
                                                tempVoice.ConnectedUsers.Count == 0;
        }

        private async Task HandleUserJoinedChannel(VoiceUpdatedContext voiceUpdatedContext)
        {
            var creatorChannel = await CreatorChannelRepository.GetCreatorChannel(voiceUpdatedContext.NewVoiceChannel.Id);
            var tempChannelFromUser = await TempChannelRepository.GetTempChannel(creatorChannel?.createchannelid, voiceUpdatedContext.User!.Id);

            if (tempChannelFromUser != null)
            {
                await MoveUserBackToHisTempChannel(voiceUpdatedContext, tempChannelFromUser);
                return;
            }

            var tempChannel = await TempChannelRepository.GetTempChannel(voiceUpdatedContext.NewVoiceChannel.Id);

            // Giving view rights in case that the temp-channel has a linked text-channel
            if (tempChannel?.deletedate != null)
            {
                // Stopping the delay if another user joins the voice channel which has an delay
                await _tempChannelDelayHandler.StopDelayTask(tempChannel);
                return;
            }

            if (creatorChannel == null)
            {
                return;
            }

            if (voiceUpdatedContext.NewVoiceChannel.Category == null)
            {
                await voiceUpdatedContext.User!.SendDm(Contents.KeineCategory, voiceUpdatedContext.Language, new object[] { voiceUpdatedContext.User.GlobalName });
                return;
            }

            var channel = await CreateTempChannel(creatorChannel, (SocketGuildUser)voiceUpdatedContext.User!, voiceUpdatedContext.NewVoiceChannel);
            await voiceUpdatedContext.User.ModifyAsync(u => u.ChannelId = channel.Id);

            if (!await TempCommandRepository.CommandDisabled(voiceUpdatedContext.Guild!.Id, creatorChannel.createchannelid, SlashCommandNames.Interface))
            {
                await channel.SendInterface(voiceUpdatedContext, creatorChannel.createchannelid);
            }
        }

        private async Task<RestVoiceChannel> CreateTempChannel(CreateTempChannel creatorChannel, SocketGuildUser guildUser, SocketVoiceChannel creatorVoiceChannel)
        {
            var userConfig = await TempChannelUserConfigRepository.GetTempChannelUserConfig(creatorChannel.createchannelid, guildUser.Id);
            var tempChannelName = await TempChannelHelper.GetTempChannelName(creatorChannel, userConfig, guildUser);
            var channelSize = userConfig?.channelsize.GetValueOrDefault() != 0 ? userConfig?.channelsize : creatorChannel.channelsize;

            var voiceChannel = await CreateVoiceChannel(creatorChannel, tempChannelName, creatorVoiceChannel, channelSize);
            await TempChannelRepository.AddTempChannel(guildUser.Guild.Id, voiceChannel.Id, guildUser.Id, creatorVoiceChannel.Id);
            return voiceChannel;
        }

        private async Task<RestVoiceChannel> CreateVoiceChannel(
            CreateTempChannel creatorChannel,
            string tempChannelName,
            SocketVoiceChannel newVoiceChannel,
            int? channelSize)
        {
            var permissions = GetPermissionsFromCreatorChannel(newVoiceChannel.Guild.Roles, newVoiceChannel);

            return await newVoiceChannel.Guild.CreateVoiceChannelAsync(tempChannelName, prop =>
            {
                prop.CategoryId = newVoiceChannel.CategoryId;
                prop.PermissionOverwrites = permissions;
                prop.UserLimit = channelSize;
            });
        }

        private List<Overwrite> GetPermissionsFromCreatorChannel(IReadOnlyCollection<SocketRole> roles, SocketVoiceChannel creatorChannel)
        {
            var permissions = new List<Overwrite>();
            //Permissions for each role
            foreach (var role in roles)
            {
                var permissionOverride = creatorChannel.GetPermissionOverwrite(role);
                if (permissionOverride == null)
                {
                    continue;
                }
                permissionOverride = permissionOverride.Value.Modify(sendMessages: PermValue.Inherit);
                permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value));
            }

            var applicationName = Configuration.GetConfigValue<string>(Configuration.ApplicationName);
            var botRole = roles.Single(role => role.Name == applicationName);

            var botOverridePermissions = new OverwritePermissions(
                connect: PermValue.Allow,
                manageChannel: PermValue.Allow,
                viewChannel: PermValue.Allow,
                moveMembers: PermValue.Allow,
                sendMessages: PermValue.Allow);

            var botOverride = new Overwrite(
                botRole.Id,
                PermissionTarget.Role,
                botOverridePermissions);

            permissions.Add(botOverride);

            return permissions;
        }

        private async Task MoveUserBackToHisTempChannel(VoiceUpdatedContext voiceUpdatedContext, TempChannel tempChannelFromUser)
        {
            var guildUser = (SocketGuildUser)voiceUpdatedContext.User!;
            var tempVoice = (SocketVoiceChannel)await voiceUpdatedContext.Client!.GetChannelAsync(tempChannelFromUser.channelid);
            if (tempVoice?.ConnectedUsers.Count == 0)
            {
                await guildUser.ModifyAsync(c => c.Channel = tempVoice);
            }
        }

        private VoiceUpdatedContext GetVoiceUpdatedContext(
            SocketUser user,
            SocketVoiceState oldVoiceState,
            SocketVoiceState newVoiceState,
            VoiceAction voiceAction)
        {
            var guildUser = (SocketGuildUser)user;
            return new VoiceUpdatedContext()
            {
                Client = _client,
                Guild = guildUser.Guild,
                User = (IGuildUser)user,
                Interaction = null,
                Language = LanguageRepository.GetLanguage(guildUser.Guild.Id).Result,
                OldVoiceChannel = oldVoiceState.VoiceChannel,
                NewVoiceChannel = newVoiceState.VoiceChannel,
                VoiceAction = voiceAction
            };
        }
        private VoiceAction GetVoiceAction(SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {
            if (oldVoiceState.VoiceChannel == null && newVoiceState.VoiceChannel == null)
            {
                return VoiceAction.NoAction;
            }

            if (oldVoiceState.VoiceChannel != null && newVoiceState.VoiceChannel != null)
            {
                return VoiceAction.UserLeftAndJoinedChannel;
            }

            if (oldVoiceState.VoiceChannel != null)
            {
                return VoiceAction.UserLeftAChannel;
            }

            if (newVoiceState.VoiceChannel != null)
            {
                return VoiceAction.UserJoinedAChannel;
            }

            return VoiceAction.NoAction;
        }
        #endregion
    }
}
