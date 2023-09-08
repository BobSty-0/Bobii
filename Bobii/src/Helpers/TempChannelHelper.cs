using Bobii.src.Bobii;
using Bobii.src.EntityFramework.Entities;
using Bobii.src.Enums;
using Bobii.src.Helper;
using Bobii.src.Models;
using Bobii.src.TempChannel;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Core.Extensions.System;
using TwitchLib.Communication.Interfaces;
using System.Drawing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.IO;
using Npgsql;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;

namespace Bobii.src.Helper
{
    class TempChannelHelper
    {
        #region Tasks
        public static async Task HandleUserJoinedChannel(VoiceUpdatedParameter parameter)
        {
            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(parameter.NewSocketVoiceChannel.Id).Result;
            var createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(parameter.Guild).Result
                .SingleOrDefault(channel => channel.createchannelid == parameter.NewSocketVoiceChannel.Id);

            if (createTempChannel != null)
            {
                var existingTempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result.OrderByDescending(ch => ch.id).FirstOrDefault(c => c.channelownerid == parameter.SocketUser.Id && c.createchannelid == createTempChannel.createchannelid);
                if (existingTempChannel != null)
                {
                    var guildUser = parameter.Guild.GetUser(parameter.SocketUser.Id);
                    var tempVoice = (SocketVoiceChannel)parameter.Client.GetChannel(existingTempChannel.channelid);
                    if (tempVoice.ConnectedUsers.Count == 0)
                    {
                        await guildUser.ModifyAsync(c => c.Channel = tempVoice);
                        return;
                    }
                }
            }

            // Giving view rights in case that the temp-channel has a linked text-channel
            if (tempChannel != null)
            {
                if (tempChannel.deletedate != null)
                {
                    // Stopping the delay if another user joins the voice channel which has an delay
                    await parameter.DelayOnDelete.StopDelay(tempChannel);
                    return;
                }
            }

            // Checking if the joined channel is a create-temp-channel
            if (createTempChannel != null)
            {
                string channelName = string.Empty;
                var tempChannelName = string.Empty;

                if (TempChannel.EntityFramework.TempChannelUserConfig.TempChannelUserConfigExists(parameter.SocketUser.Id, createTempChannel.createchannelid).Result)
                {
                    var tempChannelConfig = TempChannel.EntityFramework.TempChannelUserConfig.GetTempChannelConfig(parameter.SocketUser.Id, createTempChannel.createchannelid).Result;
                    tempChannelName = tempChannelConfig.tempchannelname;
                    tempChannelName = GetVoiceChannelName(createTempChannel, parameter.SocketUser, tempChannelName, parameter.Client).Result;
                    await CreateAndConnectToVoiceChannel(parameter.SocketUser, createTempChannel, parameter.NewVoiceState, parameter.Client, tempChannelConfig.channelsize, tempChannelName);
                    return;
                }

                tempChannelName = createTempChannel.tempchannelname;
                tempChannelName = GetVoiceChannelName(createTempChannel, parameter.SocketUser, tempChannelName, parameter.Client).Result;
                await CreateAndConnectToVoiceChannel(parameter.SocketUser, createTempChannel, parameter.NewVoiceState, parameter.Client, createTempChannel.channelsize, tempChannelName);
            }
        }

        public static async Task HandleUserLeftChannel(VoiceUpdatedParameter parameter)
        {
            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(parameter.OldSocketVoiceChannel.Id).Result;

            createtempchannels createTempChannel;
            if (parameter.VoiceUpdated == VoiceUpdated.UserLeftAndJoinedChannel)
            {
                createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(parameter.Guild).Result
                    .SingleOrDefault(channel => channel.createchannelid == parameter.NewSocketVoiceChannel.Id);

                if (createTempChannel != null)
                {
                    var existingTempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result.OrderByDescending(ch => ch.id).FirstOrDefault(c => c.channelownerid == parameter.SocketUser.Id && c.createchannelid == createTempChannel.createchannelid);
                    if (existingTempChannel != null)
                    {
                        var guildUser = parameter.Guild.GetUser(parameter.SocketUser.Id);
                        var tempVoice = (SocketVoiceChannel)parameter.Client.GetChannel(existingTempChannel.channelid);
                        if (tempVoice != null && tempVoice.Id == parameter.OldSocketVoiceChannel.Id && tempVoice.ConnectedUsers.Count == 0)
                        {
                            return;
                        }
                    }
                }
            }

            if (tempChannel == null)
            {
                return;
            }

            // Removing view rights of the text-channel if the temp-chanenl has a linked text-channel
            createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelList().Result.FirstOrDefault(c => c.createchannelid == tempChannel.createchannelid);
            if (createTempChannel != null && createTempChannel.delay != null && createTempChannel.delay != 0 && parameter.OldSocketVoiceChannel.ConnectedUsers.Count == 0)
            {
                // We just add an delay if the createTempChannel has an delay
                await parameter.DelayOnDelete.StartDelay(tempChannel, createTempChannel, parameter);
                return;
            }

            if (parameter.OldSocketVoiceChannel.ConnectedUsers.Count() == 0)
            {
                await TempChannelHelper.DeleteTempChannel(parameter, tempChannel);
                if (createTempChannel.tempchannelname.Contains("{count}"))
                {
                    _ = TempChannelHelper.SortCountNeu(createTempChannel, parameter.Client);
                }
                return;
            }
        }

        public static async Task<VoiceUpdatedParameter> GetVoiceUpdatedParameter(SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState, SocketUser user, DiscordSocketClient client, DelayOnDelete delayOnDeleteClass)
        {
            var parameter = new VoiceUpdatedParameter();
            parameter.VoiceUpdated = GetVoiceUpdatedEnum(oldVoiceState, newVoiceState).Result;
            parameter.Client = client;
            parameter.SocketUser = user;
            parameter.DelayOnDelete = delayOnDeleteClass;
            if (parameter.VoiceUpdated == VoiceUpdated.UserLeftAndJoinedChannel)
            {
                parameter.NewSocketVoiceChannel = newVoiceState.VoiceChannel;
                parameter.OldSocketVoiceChannel = oldVoiceState.VoiceChannel;
                parameter.OldVoiceState = oldVoiceState;
                parameter.NewVoiceState = newVoiceState;
                parameter.Guild = parameter.NewSocketVoiceChannel.Guild;

            }

            if (parameter.VoiceUpdated == VoiceUpdated.UserJoinedAChannel)
            {
                parameter.NewSocketVoiceChannel = newVoiceState.VoiceChannel;
                parameter.NewVoiceState = newVoiceState;
                parameter.Guild = parameter.NewSocketVoiceChannel.Guild;
            }
            else
            {
                parameter.OldSocketVoiceChannel = oldVoiceState.VoiceChannel;
                parameter.OldVoiceState = oldVoiceState;
                parameter.Guild = parameter.OldSocketVoiceChannel.Guild;
            }
            await Task.CompletedTask;
            return parameter;
        }

        public static async Task<VoiceUpdated> GetVoiceUpdatedEnum(SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {
            if (oldVoiceState.VoiceChannel == null && newVoiceState.VoiceChannel == null)
            {
                return VoiceUpdated.ChannelDestroyed;
            }
            if (oldVoiceState.VoiceChannel != null && newVoiceState.VoiceChannel != null)
            {
                return VoiceUpdated.UserLeftAndJoinedChannel;
            }

            if (oldVoiceState.VoiceChannel != null)
            {
                return VoiceUpdated.UserLeftAChannel;
            }
            if (newVoiceState.VoiceChannel != null)
            {
                return VoiceUpdated.UserJoinedAChannel;
            }
            // Will never happen
            return VoiceUpdated.ChannelDestroyed;
        }

        public static async Task<bool> ConnectBackToDelayedChannel(DiscordSocketClient client, SocketGuildUser user, createtempchannels createTempChannel)
        {
            try
            {
                var tempChannels = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result.Where(t => t.channelownerid == user.Id).ToList();
                if (tempChannels.Count() == 0)
                {
                    return false;
                }

                foreach (var tempChannel in tempChannels)
                {
                    if (tempChannel.createchannelid != createTempChannel.createchannelid)
                    {
                        continue;
                    }
                    var voiceChannel = (SocketVoiceChannel)client.GetChannelAsync(tempChannel.channelid).Result;
                    if (voiceChannel == null)
                    {
                        continue;
                    }

                    await user.ModifyAsync(t => t.Channel = voiceChannel);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, nameof(ConnectBackToDelayedChannel),
                        new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                        message: $"User successfully moved back into hes channel");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, nameof(ConnectBackToDelayedChannel),
                    new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                    message: $"Could not move user back into hes channel", exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task GiveOwnerIfOwnerNotInVoice(SlashCommandParameter parameter)
        {
            try
            {
                var voiceChannel = parameter.GuildUser.VoiceState.Value.VoiceChannel;
                var tempChannelId = voiceChannel.Id;
                var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(tempChannelId).Result;
                if (tempChannel == null)
                {
                    return;
                }

                var ownerId = TempChannel.EntityFramework.TempChannelsHelper.GetOwnerID(tempChannelId).Result;
                if (voiceChannel.ConnectedUsers.FirstOrDefault(u => u.Id == tempChannel.channelownerid) == null)
                {
                    await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(tempChannelId, parameter.GuildUser.Id);
                    await GiveManageChannelRightsToUserVc(parameter.GuildUser, parameter.GuildID, null, parameter.GuildUser.VoiceChannel);
                    await SendOwnerUpdatedMessage(voiceChannel, parameter.Guild, parameter.GuildUser.Id, parameter.Language);
                }
            }
            catch (Exception)
            {
                //nothing
            }
        }

        public static async Task GiveManageChannelRightsToUserVc(SocketUser user, ulong guildId, RestVoiceChannel restVoiceChannel, SocketVoiceChannel socketVoiceChannel)
        {
            if (restVoiceChannel != null)
            {
                var tempChannelEntity = TempChannelsHelper.GetTempChannel(restVoiceChannel.Id).Result;
                if (TempCommandsHelper.DoesCommandExist(guildId, tempChannelEntity.createchannelid.Value, "ownerpermissions").Result)
                {
                    return;
                }

                await restVoiceChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(manageChannel: PermValue.Allow));
            }
            else
            {
                var tempChannelEntity = TempChannelsHelper.GetTempChannel(socketVoiceChannel.Id).Result;
                if (TempCommandsHelper.DoesCommandExist(guildId, tempChannelEntity.createchannelid.Value, "ownerpermissions").Result)
                {
                    return;
                }

                await socketVoiceChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(manageChannel: PermValue.Allow));
            }
        }

        public static async Task RemoveManageChannelRightsToUserVc(SocketUser user, SocketVoiceChannel voiceChannel)
        {
            await voiceChannel.RemovePermissionOverwriteAsync(user);
        }

        public static async Task<string> GetVoiceChannelName(createtempchannels createTempChannel, SocketUser user, string tempChannelName, DiscordSocketClient client)
        {
            switch (tempChannelName)
            {
                case var s when tempChannelName.Contains("{count}"):
                    tempChannelName = tempChannelName.Replace("{count}",
                        (TempChannel.EntityFramework.TempChannelsHelper.GetCount(createTempChannel.createchannelid).Result).ToString());
                    break;
                case var s when tempChannelName.Contains("{username}"):
                    tempChannelName = tempChannelName.Replace("{username}", user.GlobalName);
                    break;
                case var s when tempChannelName.Contains("{nickname}"):
                    var guildUser = client.GetGuild(createTempChannel.guildid)?.GetUser(user.Id);

                    if (guildUser == null || guildUser.Nickname == null)
                    {
                        tempChannelName = tempChannelName.Replace("{nickname}", user.GlobalName);
                    }

                    tempChannelName = tempChannelName.Replace("{nickname}", guildUser.Nickname);
                    break;
            }
            await Task.CompletedTask;
            return tempChannelName;
        }

        public static async Task CreateAndConnectToVoiceChannel(
            SocketUser user,
            createtempchannels createTempChannel,
            SocketVoiceState newVoice,
            DiscordSocketClient client,
            int? channelSize,
            string channelName)
        {
            var tempChannel = CreateVoiceChannel(user, channelName, newVoice, createTempChannel, channelSize).Result;
            await ConnectToVoice(tempChannel, user as IGuildUser);

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(tempChannel.Id).Result;
            if (!TempCommandsHelper.DoesCommandExist(((SocketGuildUser)user).Guild.Id, tempChannelEntity.createchannelid.Value, "interface").Result)
            {
                await WriteInterfaceInVoiceChannel(tempChannel, client);
            }
        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketUser user, string channelName, SocketVoiceState newVoice, createtempchannels createTempChannel, int? channelSize)
        {
            try
            {
                var category = newVoice.VoiceChannel.Category;
                var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), channelName, channelSize, newVoice).Result;
                _ = TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id);

                await GiveManageChannelRightsToUserVc(user, ((SocketGuildUser)user).Guild.Id, tempChannel, null);


                var guild = ((SocketGuildUser)user).Guild;
                return tempChannel;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, nameof(CreateVoiceChannel), createChannelID: createTempChannel.createchannelid);
                return null;
            }
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            if (voiceChannel == null)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, "ConnectToVoice",
                    new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user }, message: $"{user} ({user.Id}) could not be connected");
                return;
            }
            await user.ModifyAsync(x => x.Channel = voiceChannel);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "ConnectToVoice",
                  new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                  message: $"{user} ({user.Id}) was successfully connected to {voiceChannel}", tempChannelID: voiceChannel.Id);
        }

        public static async Task CheckForTempChannelCorps(SlashCommandParameter parameter)
        {
            var tempChannels = TempChannelsHelper.GetTempChannelList().Result;
            try
            {
                foreach (var tempChannel in tempChannels)
                {
                    var channel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.channelid);

                    if (channel == null)
                    {
                        await TempChannelsHelper.RemoveTC(0, tempChannel.channelid);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, nameof(CheckForTempChannelCorps),
                            parameter, message: "Corps detected!", tempChannelID: tempChannel.channelid);
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, nameof(CheckForTempChannelCorps),
                    parameter, message: "Corps detected!", exceptionMessage: ex.Message);
            }
        }

        public static async Task DeleteTempChannel(VoiceUpdatedParameter parameter, tempchannels tempChannel)
        {
            var socketGuildUser = parameter.Guild.GetUser(parameter.SocketUser.Id);
            await parameter.OldSocketVoiceChannel.DeleteAsync();

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "CheckAndDeleteEmptyVoiceChannels",
                  new SlashCommandParameter() { Guild = parameter.Guild, GuildUser = socketGuildUser },
                  message: $"Channel successfully deleted", tempChannelID: tempChannel.channelid);

            if (TempChannelsHelper.GetTempChannel(tempChannel.channelid).Result != null)
            {
                await TempChannelsHelper.RemoveTC(parameter.Guild.Id, tempChannel.channelid);
            }
        }

        public static async Task TempLock(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock)).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "lock", tempChannelEntity.createchannelid.Value).Result)
            {
                return;
            }

            try
            {
                var everyoneRole = parameter.Guild.Roles.Where(role => role.Name == "@everyone").First();
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                var newPermissionOverride = new OverwritePermissions(connect: PermValue.Deny);
                var test = voiceChannel.AddPermissionOverwriteAsync(everyoneRole, newPermissionOverride);


                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C130", parameter.Language).Result,
                    GeneralHelper.GetCaption("C130", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/templock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to lock temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C131", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempUnLock(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnLock)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnLock)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnLock)).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "unlock", tempChannelEntity.createchannelid.Value).Result)
            {
                return;
            }

            try
            {
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
                var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

                var everyoneRole = parameter.Guild.Roles.Where(role => role.Name == "@everyone").First();
                var value = createTempChannel.GetPermissionOverwrite(everyoneRole).GetValueOrDefault();

                var voiceChannel = parameter.GuildUser.VoiceChannel;

                var newPermissionOverride = new OverwritePermissions(connect: value.Connect);
                await voiceChannel.AddPermissionOverwriteAsync(everyoneRole, newPermissionOverride);


                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C132", parameter.Language).Result,
                    GeneralHelper.GetCaption("C132", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunlock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unlock temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C133", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempHide(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempHide)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempHide)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempHide)).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "hide", tempChannelEntity.createchannelid.Value).Result)
            {
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();

                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));

                //Permissions for each role
                foreach (var role in parameter.Guild.Roles)
                {
                    var permissionOverride = parameter.GuildUser.VoiceState.Value.VoiceChannel.GetPermissionOverwrite(role);
                    if (permissionOverride != null)
                    {
                        if (role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName))
                        {
                            continue;
                        }
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value.Modify(viewChannel: PermValue.Deny)));
                    }
                    else if (role.Name == "@everyone" && permissionOverride == null)
                    {
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
                    }
                }
                var tempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id);
                await tempChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C164", parameter.Language).Result,
                    GeneralHelper.GetCaption("C158", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temphide successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to hide temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C165", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempClaimOwner(SlashCommandParameter parameter)
        {
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempClaimOwner)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempClaimOwner)).Result)
            {
                return;
            }
            var voiceChannel = parameter.GuildUser.VoiceChannel;
            var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
            var ownerId = TempChannel.EntityFramework.TempChannelsHelper.GetOwnerID(voiceChannel.Id).Result;
            if (ownerId == parameter.GuildUser.Id)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C246", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempClaimOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp claimowner - already owner");
            }

            if (voiceChannel.ConnectedUsers.FirstOrDefault(u => u.Id == tempChannel.channelownerid) == null)
            {
                await TempChannelsHelper.ChangeOwner(parameter.GuildUser.VoiceChannel.Id, parameter.GuildUser.Id);

                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C244", parameter.Language).Result,
                    GeneralHelper.GetCaption("C236", parameter.Language).Result).Result }, ephemeral: true);

                await SendOwnerUpdatedMessage(parameter.GuildUser.VoiceChannel, parameter.Guild, parameter.GuildUser.Id, parameter.Language);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempClaimOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp claimowner succesfully used");
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C245", parameter.Language).Result, tempChannel.channelownerid),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempClaimOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp claimowner - owner still in voice");
            }
        }

        public static async Task TempUnHide(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnHide)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnHide)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnHide)).Result)
            {
                return;
            }


            var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "unhide", tempChannel.createchannelid.Value).Result)
            {
                return;
            }

            var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();

                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));

                //Permissions for each role
                foreach (var role in parameter.Guild.Roles)
                {

                    var permissionOverride = createTempChannel.GetPermissionOverwrite(role);
                    if (permissionOverride != null)
                    {
                        if (role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName))
                        {
                            continue;
                        }
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value));
                    }
                }

                await parameter.GuildUser.VoiceState.Value.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C166", parameter.Language).Result,
                    GeneralHelper.GetCaption("C166", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temphide successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unhide temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C167", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempSize(SlashCommandParameter parameter, int newsize)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);


            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempSize)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempSize)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempSize)).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "size", tempChannelEntity.createchannelid.Value).Result)
            {
                return;
            }

            try
            {
                if (newsize > 99)
                {
                    _ = parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = null);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C120", parameter.Language).Result,
                        GeneralHelper.GetCaption("C120", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempSize), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempsize successfully used");
                }
                else
                {
                    _ = Task.Run(async () => parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = newsize));
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        string.Format(GeneralHelper.GetContent("C121", parameter.Language).Result, newsize),
                        GeneralHelper.GetCaption("C120", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempSize), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempsize successfully used");
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempSize), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel size", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C122", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                return;
            }
        }

        public static async Task TempSaveConfig(SlashCommandParameter parameter)
        {

            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempSaveConfig)).Result ||
            CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempSaveConfig)).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "saveconfig", tempChannelEntity.createchannelid.Value).Result)
            {
                return;
            }

            var currentVC = parameter.GuildUser.VoiceState.Value.VoiceChannel;
            var tempChannel = TempChannelsHelper.GetTempChannel(currentVC.Id).Result;

            if (CheckDatas.UserTempChannelConfigExists(parameter).Result)
            {
                await TempChannelUserConfig.ChangeConfig(parameter.GuildID, parameter.GuildUser.Id, tempChannel.createchannelid.Value, currentVC.Name, currentVC.UserLimit.GetValueOrDefault());
            }
            else
            {
                await TempChannelUserConfig.AddConfig(parameter.GuildID, parameter.GuildUser.Id, tempChannel.createchannelid.Value, currentVC.Name, currentVC.UserLimit.GetValueOrDefault());
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C178", parameter.Language).Result,
                             GeneralHelper.GetCaption("C178", parameter.Language).Result).Result }, ephemeral: true);

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempSaveConfig), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                message: "/temp saveconfig successfully used");
        }

        public static async Task TempDeleteConfig(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempDeleteConfig)).Result ||
            CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempDeleteConfig)).Result ||
            CheckDatas.CheckIfUserTempChannelConfigExists(parameter, nameof(TempDeleteConfig)).Result)
            {
                return;
            }

            var currentVC = parameter.GuildUser.VoiceState.Value.VoiceChannel;
            var tempChannel = TempChannelsHelper.GetTempChannel(currentVC.Id).Result;

            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "deleteconfig", tempChannel.createchannelid.Value).Result)
            {
                return;
            }

            await TempChannelUserConfig.DeleteConfig(parameter.GuildUser.Id, tempChannel.createchannelid.Value);

            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C180", parameter.Language).Result,
                             GeneralHelper.GetCaption("C180", parameter.Language).Result).Result }, ephemeral: true);

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempDeleteConfig), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                 message: "/temp deleteconfig successfully used");
        }

        public static async Task TempKick(SlashCommandParameter parameter, List<string> userIds, bool epherialMessage = false)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            foreach (var userId in userIds)
            {
                if (CheckDatas.CheckUserID(parameter, userId, nameof(TempKick)).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempKick), epherialMessage).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempKick), epherialMessage).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempKick), epherialMessage).Result ||
                    CheckDatas.CheckIfUserInSameTempVoice(parameter, userId.ToUlong(), nameof(TempKick), epherialMessage).Result)
                {
                    return;
                }

                var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "kick", tempChannelEntity.createchannelid.Value, epherialMessage).Result)
                {
                    return;
                }
            }

            var successfulKickedUsers = new List<SocketGuildUser>();
            var notSuccessfulKickedUsers = new List<SocketGuildUser>();

            foreach (var userId in userIds)
            {
                var usedGuild = parameter.Client.GetGuild(parameter.Guild.Id);

                var toBeKickedUser = usedGuild.GetUser(userId.ToUlong());
                try
                {
                    await toBeKickedUser.ModifyAsync(channel => channel.Channel = null);
                    successfulKickedUsers.Add(toBeKickedUser);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempKick), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempkick successfully used");
                }
                catch (Exception ex)
                {
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempKick), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to kick temp-channel user", exceptionMessage: ex.Message);

                    notSuccessfulKickedUsers.Add(toBeKickedUser);
                }
            }

            var stringBuilder = new StringBuilder();
            if (successfulKickedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C234", parameter.Language).Result}**");

                foreach (var user in successfulKickedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Id}>");
                }
            }

            if (notSuccessfulKickedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C234", parameter.Language).Result}**");

                foreach (var user in successfulKickedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Id}>");
                }
            }

            var caption = string.Empty;
            if (successfulKickedUsers.Count() > 0 && notSuccessfulKickedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulKickedUsers.Count() > 0 && notSuccessfulKickedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulKickedUsers.Count() == 0 && notSuccessfulKickedUsers.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }

            if (epherialMessage)
            {
                var parsedArg = (SocketMessageComponent)parameter.Interaction;
                await parsedArg.UpdateAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                    msg.Components = null;
                });
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(stringBuilder.ToString()),
                    caption).Result }, ephemeral: true);
            }
        }

        public static async Task TempUnBlock(SlashCommandParameter parameter, List<string> users, bool epherialMessage = false)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            foreach (var userId in users)
            {
                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnBlock), epherialMessage).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnBlock), epherialMessage).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnBlock), epherialMessage).Result)
                {
                    return;
                }

                var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "unblock", tempChannelEntity.createchannelid.Value, epherialMessage).Result)
                {
                    return;
                }
            }


            var successfulBlockedUsers = new List<ulong>();
            var notSuccessfulBlockedUsers = new List<ulong>();
            foreach (var userId in users)
            {
                try
                {
                    var voiceChannel = parameter.GuildUser.VoiceChannel;
                    await voiceChannel.RemovePermissionOverwriteAsync(parameter.Client.GetUserAsync(ulong.Parse(userId)).Result);

                    successfulBlockedUsers.Add(ulong.Parse(userId));
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempunblock successfully used");
                }
                catch (Exception ex)
                {
                    notSuccessfulBlockedUsers.Add(ulong.Parse(userId));
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to unblock user from temp-channel", exceptionMessage: ex.Message);
                }
            }

            var stringBuilder = new StringBuilder();
            if (successfulBlockedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C238", parameter.Language).Result}**");

                foreach (var user in successfulBlockedUsers)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            if (notSuccessfulBlockedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C239", parameter.Language).Result}**");

                foreach (var user in notSuccessfulBlockedUsers)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            var caption = string.Empty;
            if (successfulBlockedUsers.Count() > 0 && notSuccessfulBlockedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulBlockedUsers.Count() > 0 && notSuccessfulBlockedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulBlockedUsers.Count() == 0 && notSuccessfulBlockedUsers.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }

            if (epherialMessage)
            {
                var parsedArg = (SocketMessageComponent)parameter.Interaction;
                await parsedArg.UpdateAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                    msg.Components = null;
                });
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(stringBuilder.ToString()),
                    caption).Result }, ephemeral: true);
            }
        }

        public static async Task TempBlock(SlashCommandParameter parameter, List<string> userIds, bool epherialMessage = false)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            foreach (var userId in userIds)
            {
                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempBlock), epherialMessage).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempBlock), epherialMessage).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempBlock), epherialMessage).Result)
                {
                    return;
                }

                var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "block", tempChannelEntity.createchannelid.Value, epherialMessage).Result)
                {
                    return;
                }
            }

            var successfulBlockedUsers = new List<ulong>();
            var notSuccessfulBlockedUsers = new List<ulong>();

            foreach (var userId in userIds)
            {
                try
                {
                    var newPermissionOverride = new OverwritePermissions().Modify(connect: PermValue.Deny, viewChannel: PermValue.Deny);
                    var voiceChannel = parameter.GuildUser.VoiceChannel;

                    _ = voiceChannel.AddPermissionOverwriteAsync(parameter.Client.GetUserAsync(ulong.Parse(userId)).Result, newPermissionOverride);

                    successfulBlockedUsers.Add(ulong.Parse(userId));
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempblock successfully used");
                }
                catch (Exception ex)
                {
                    notSuccessfulBlockedUsers.Add(ulong.Parse(userId));
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to block user from temp-channel", exceptionMessage: ex.Message);
                }

            }

            var stringBuilder = new StringBuilder();
            if (successfulBlockedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C236", parameter.Language).Result}**");

                foreach (var user in successfulBlockedUsers)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            if (notSuccessfulBlockedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C237", parameter.Language).Result}**");

                foreach (var user in notSuccessfulBlockedUsers)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            var caption = string.Empty;
            if (successfulBlockedUsers.Count() > 0 && notSuccessfulBlockedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulBlockedUsers.Count() > 0 && notSuccessfulBlockedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulBlockedUsers.Count() == 0 && notSuccessfulBlockedUsers.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }

            if (epherialMessage)
            {
                var parsedArg = (SocketMessageComponent)parameter.Interaction;
                await parsedArg.UpdateAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                    msg.Components = null;
                });
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(stringBuilder.ToString()),
                    caption).Result }, ephemeral: true);
            }
        }

        public static async Task TempOwner(SlashCommandParameter parameter, string userId, bool epherialMessage = false)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckUserID(parameter, userId, nameof(TempOwner)).Result)
            {
                return;
            }

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempOwner), epherialMessage).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempOwner), epherialMessage).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempOwner), epherialMessage).Result ||
                CheckDatas.CheckIfUserInSameTempVoice(parameter, userId.ToUlong(), nameof(TempOwner), epherialMessage).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "getowner", tempChannelEntity.createchannelid.Value, epherialMessage).Result)
            {
                return;
            }

            try
            {
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                var currentOwner = parameter.Client.GetUser(tempChannel.channelownerid.Value);
                var newOwner = parameter.Client.GetUser(userId.ToUlong());

                if (newOwner.IsBot)
                {
                    if (epherialMessage)
                    {
                        var parsedArg = (SocketMessageComponent)parameter.Interaction;
                        await parsedArg.UpdateAsync(msg =>
                        {
                            msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C124", parameter.Language).Result,
                            GeneralHelper.GetCaption("C124", parameter.Language).Result).Result };
                            msg.Components = null;
                        });
                    }
                    else
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C124", parameter.Language).Result,
                            GeneralHelper.GetCaption("C124", parameter.Language).Result).Result }, ephemeral: true);
                    }

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempOwner), new SlashCommandParameter() { Guild = parameter.Guild, GuildUser = parameter.GuildUser },
                        message: "User is a Bot");
                    return;
                }

                var voiceChannel = parameter.Client.GetChannel(tempChannel.channelid);

                await TempChannelHelper.RemoveManageChannelRightsToUserVc(currentOwner, voiceChannel as SocketVoiceChannel);
                await TempChannelHelper.GiveManageChannelRightsToUserVc(newOwner, parameter.GuildID, null, voiceChannel as SocketVoiceChannel);


                await TempChannelsHelper.ChangeOwner(parameter.GuildUser.VoiceChannel.Id, userId.ToUlong());

                if (epherialMessage)
                {
                    var parsedArg = (SocketMessageComponent)parameter.Interaction;
                    await parsedArg.UpdateAsync(msg =>
                    {
                        msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            string.Format(GeneralHelper.GetContent("C125", parameter.Language).Result, userId),
                            GeneralHelper.GetCaption("C125", parameter.Language).Result).Result };
                        msg.Components = null;
                    });
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C125", parameter.Language).Result, userId),
                    GeneralHelper.GetCaption("C125", parameter.Language).Result).Result }, ephemeral: true);
                }

                await SendOwnerUpdatedMessage(parameter.GuildUser.VoiceChannel, parameter.Guild, parameter.GuildUser.Id, parameter.Language);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempowner successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel owner", exceptionMessage: ex.Message);

                if (epherialMessage)
                {
                    var parsedArg = (SocketMessageComponent)parameter.Interaction;
                    await parsedArg.UpdateAsync(msg =>
                    {
                        msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C126", parameter.Language).Result,
                            GeneralHelper.GetCaption("C038", parameter.Language).Result).Result };
                        msg.Components = null;
                    });
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C126", parameter.Language).Result,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                return;
            }
        }

        public static void AddInterfaceButton(ActionRowBuilder actionRowBuilder, string customId, string emojiString, bool disabled = false)
        {
            actionRowBuilder.WithButton(customId: customId, style: ButtonStyle.Secondary, emote: Emote.Parse(emojiString), disabled: disabled);
        }

        public static ComponentBuilder GetButtonsComponentBuilder(Dictionary<ButtonBuilder, System.Drawing.Image> dict)
        {
            var count = 0;
            var componentBuilder = new ComponentBuilder();
            var rowBuilder = new ActionRowBuilder();
            foreach (var button in dict)
            {
                count++;
                rowBuilder.WithButton(button.Key);
                if (count == 4)
                {
                    componentBuilder.AddRow(rowBuilder);
                    rowBuilder = new ActionRowBuilder();
                }
            }

            return componentBuilder;
        }

        public static Bitmap GetButtonsBitmap(Dictionary<ButtonBuilder, System.Drawing.Image> dict)
        {
            var bitmap = GetRightSizedBitmap(dict.Count());

            using Graphics g = Graphics.FromImage(bitmap);
            g.Clear(System.Drawing.Color.Transparent);

            var x = 0;
            var y = 0;
            var count = 0;
            foreach(var image in dict.Values)
            { 
                count++;
                g.DrawImage(image, x, y, 200, 60);
                x += 240;
                if(count == 4)
                {
                    count = 0;
                    y += 90;
                    x = 0;
                }
            }

            return bitmap;
        }

        public static Bitmap GetRightSizedBitmap(int anzahlImages)
        {
            switch (anzahlImages)
            {
                case 1 | 2 | 3 | 4:
                    return new Bitmap(1000, 60);
                case 5 | 6 | 7 | 8:
                    return new Bitmap(1000, 150);
                case 9 | 10 | 11:
                    return new Bitmap(1000, 240);
                case 12:
                    return new Bitmap(1000, 240);
                case 13 | 14 | 15 | 16:
                    return new Bitmap(1000, 330);
                default: 
                    return new Bitmap(1000, 330);

            }
        }


        public static async Task SaveNewInterfaceButtonPicture(DiscordSocketClient client, List<tempcommands> disabledCommands, ulong createTempChannelId)
        {
            var buttonsMitBildern = GetInterfaceButtonsMitBild(client, disabledCommands).Result;
            var buttonComponentBuilder = GetButtonsComponentBuilder(buttonsMitBildern);
            var img = GetButtonsBitmap(buttonsMitBildern);
            img.Save($"{Directory.GetCurrentDirectory()}buttons\\{createTempChannelId}_buttons.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public static string GetOrSaveAndGetButtonsImageName(DiscordSocketClient client, List<tempcommands> disabledCommands, ulong createTempChannelId)
        {
            var filePath = $"{Directory.GetCurrentDirectory()}buttons\\{createTempChannelId}_buttons.png";
            if (File.Exists(filePath))
            {
                return Path.GetFileName(filePath);
            }
            else
            {
                _ = SaveNewInterfaceButtonPicture(client, disabledCommands, createTempChannelId);
                return Path.GetFileName(filePath);
            }
        }

        public static async Task WriteInterfaceInVoiceChannel(RestVoiceChannel tempChannel, DiscordSocketClient client)
        {
            var tempChannelEntity = TempChannelsHelper.GetTempChannel(tempChannel.Id).Result;
            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(tempChannel.GuildId, tempChannelEntity.createchannelid.Value).Result;

            var imgFileName = GetOrSaveAndGetButtonsImageName(client, disabledCommands, tempChannelEntity.createchannelid.Value);
            var buttonsMitBildern = GetInterfaceButtonsMitBild(client, disabledCommands).Result;
            var buttonComponentBuilder = GetButtonsComponentBuilder(buttonsMitBildern);

            var voiceChannel = (IRestMessageChannel)tempChannel;
            //var componentBuilder = new ComponentBuilder();
            //await AddInterfaceButtons(componentBuilder, disabledCommands);

            var lang = Bobii.EntityFramework.BobiiHelper.GetLanguage(tempChannel.GuildId).Result;

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(GeneralHelper.GetCaption("C211", lang).Result)
                .WithColor(74, 171, 189)
                .WithImageUrl($"attachment://{imgFileName}")
                .WithDescription(GeneralHelper.GetContent("C208", lang).Result)
                .WithFooter(DateTime.Now.ToString("dd/MM/yyyy"));

            await voiceChannel.SendMessageAsync("", embeds: new Embed[] { embed.Build() }, components: buttonComponentBuilder.Build());
        }

        public static async Task<Dictionary<ButtonBuilder, System.Drawing.Image>> GetInterfaceButtonsMitBild(DiscordSocketClient client, List<tempcommands> disabledCommands)
        {
            var commands = client.GetGlobalApplicationCommandsAsync()
                .Result
                .Single(c => c.Name == GlobalStrings.temp)
                .Options.Select(c => c.Name)
                .ToList();

            var dict = new Dictionary<ButtonBuilder, System.Drawing.Image>();
            foreach (var command in commands)
            {
                var button = GetButton($"temp-interface-{command}", Emojis()[command], command, disabledCommands);
                var img = System.Drawing.Image.FromFile($"{Directory.GetCurrentDirectory()}\\images\\{command}button.png");
                dict.Add(button, img);
            }

            return dict;
        }

        public static Dictionary<string, string> Emojis()
        {
            return new Dictionary<string, string>()
            {
                { "name", "<:edit:1138160331122802818>" },
                { "unlock", "<:lockopen:1138164700434149477>"},
                { "lock", "<:lockclosed:1138164855820525702>"},
                { "hide", "<:hidenew:1149745951997710497>"},
                { "unhide", "<:unhidenew:1149745951997710497>"},
                { "kick", "<userkickednew:1149731040689143808>" },
                { "block", "<:userblockednew:1149731292313833552>"},
                { "unblock", "<:userunblockednew:1149731489060245524>"},
                { "saveconfig", "<:config:1138181363338588351>"},
                { "deleteconfig", "<:noconfig:1138181406799966209>"},
                { "size", "<:userlimit:1149730495219896472>"},
                { "giveowner", "<:ownergive:1149728498336944159>"},
                { "claimowner", "<:ownerclaim:1149728391315071037>" }
            };
        }

        public static ButtonBuilder GetButton(string customId, string emojiString, string commandName, List<tempcommands> disabledCommands)
        {
            return new ButtonBuilder()
                .WithCustomId(customId)
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(Emote.Parse(emojiString))
                .WithDisabled(CommandDisabled(disabledCommands, commandName));
        }

        public static async Task AddInterfaceButtons(ComponentBuilder componentBuilder, List<tempcommands> disabledCommands)
        {
            var rowBuilder = new ActionRowBuilder();
            AddInterfaceButton(rowBuilder, "temp-interface-name", "<:edit:1138160331122802818>", CommandDisabled(disabledCommands, "name"));
            AddInterfaceButton(rowBuilder, "temp-interface-openchannel", "<:lockopen:1138164700434149477>", CommandDisabled(disabledCommands, "unlock"));
            AddInterfaceButton(rowBuilder, "temp-interface-closechannel", "<:lockclosed:1138164855820525702>", CommandDisabled(disabledCommands, "lock"));
            AddInterfaceButton(rowBuilder, "temp-interface-hidechannel", "<:ghost:1138173699254665268>", CommandDisabled(disabledCommands, "hide"));
            componentBuilder.AddRow(rowBuilder);

            rowBuilder = new ActionRowBuilder();
            AddInterfaceButton(rowBuilder, "temp-interface-unhidechannel", "<:noghost:1138173749900882101>", CommandDisabled(disabledCommands, "unhide"));
            AddInterfaceButton(rowBuilder, "temp-interface-kick", "<:userkickednew:1149731040689143808>", CommandDisabled(disabledCommands, "kick"));
            AddInterfaceButton(rowBuilder, "temp-interface-block", "<:userblockednew:1149731292313833552>", CommandDisabled(disabledCommands, "block"));
            AddInterfaceButton(rowBuilder, "temp-interface-unblock", "<:userunblockednew:1149731489060245524>", CommandDisabled(disabledCommands, "unblock"));
            componentBuilder.AddRow(rowBuilder);

            rowBuilder = new ActionRowBuilder();
            AddInterfaceButton(rowBuilder, "temp-interface-saveconfig", "<:config:1138181363338588351>", CommandDisabled(disabledCommands, "saveconfig"));
            AddInterfaceButton(rowBuilder, "temp-interface-deleteconfig", "<:noconfig:1138181406799966209>", CommandDisabled(disabledCommands, "deleteconfig"));
            AddInterfaceButton(rowBuilder, "temp-interface-size", "<:userlimit:1149730495219896472>", CommandDisabled(disabledCommands, "size"));
            AddInterfaceButton(rowBuilder, "temp-interface-giveowner", "<:ownergive:1149728498336944159>", CommandDisabled(disabledCommands, "getowner"));
            componentBuilder.AddRow(rowBuilder);

            rowBuilder = new ActionRowBuilder();
            AddInterfaceButton(rowBuilder, "temp-interface-claimowner", "<:ownerclaim:1149728391315071037>", CommandDisabled(disabledCommands, "claimowner"));
            componentBuilder.AddRow(rowBuilder);
        }

        public static bool CommandDisabled(List<tempcommands> disabledCommands, string commandName)
        {
            return disabledCommands.FirstOrDefault(command => command.commandname == commandName) != null;
        }

        public static async Task CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client)
        {
            var voiceChannelName = "";
            var guild = client.GetGuild(GeneralHelper.GetConfigKeyValue(ConfigKeys.MainGuildID).ToUlong());
            var socketGuildUser = guild.GetUser(GeneralHelper.GetConfigKeyValue(ConfigKeys.MainGuildID).ToUlong());
            var tempChannelIDs = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result;
            try
            {
                foreach (var tempChannel in tempChannelIDs)
                {
                    if (tempChannel.deletedate != null)
                    {
                        continue;
                    }

                    var voiceChannel = (SocketVoiceChannel)client.GetChannel(tempChannel.channelid);
                    if (voiceChannel == null)
                    {
                        continue;
                    }

                    voiceChannelName = voiceChannel.Name;

                    if (voiceChannel.ConnectedUsers.Count == 0)
                    {
                        await voiceChannel.DeleteAsync();

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "CheckAndDeleteEmptyVoiceChannels",
                              new SlashCommandParameter() { Guild = guild, GuildUser = socketGuildUser },
                              message: $"Channel successfully deleted", tempChannelID: tempChannel.channelid);

                        var tempChannelEF = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(tempChannel.channelid).Result;

                        await TempChannel.EntityFramework.TempChannelsHelper.RemoveTC(guild.Id, tempChannel.channelid);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Access"))
                {
                    var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guild.Id).Result;
                    await socketGuildUser.SendMessageAsync(string.Format(GeneralHelper.GetContent("C097", language).Result, socketGuildUser.Username, voiceChannelName));
                }
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, "CheckAndDeleteEmptyVoiceChannels",
                    new SlashCommandParameter() { Guild = guild, GuildUser = socketGuildUser },
                    message: $"Voicechannel could not be deleted, {socketGuildUser} has got a DM if it was missing access", exceptionMessage: ex.Message);
            }
        }

        public static async Task<bool> CheckIfChannelWithNameExists(SocketGuildUser user, string catergoryId, string name, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            var guild = user.Guild;
            var categoryChannel = guild.GetCategoryChannel(ulong.Parse(catergoryId));

            foreach (var channel in categoryChannel.Channels)
            {
                if (channel.Name == name)
                {
                    await user.ModifyAsync(x => x.Channel = (SocketVoiceChannel)channel);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "ConnectToVoice",
                          new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                          message: $"{user} ({user.Id}) was successfully connected to {channel.Name}", tempChannelID: channel.Id);
                    return true;
                }
            }

            return false;
        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketGuildUser user, string catergoryId, string name, int? channelSize, SocketVoiceState newVoice)
        {
            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                //Permissions for each role
                foreach (var role in user.Guild.Roles)
                {
                    var permissionOverride = newVoice.VoiceChannel.GetPermissionOverwrite(role);
                    if (permissionOverride != null)
                    {
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value));
                    }
                }

                SocketRole bobiiRole = null;
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    bobiiRole = user.Guild.Roles.Where(role => role.Name == "BobiiDev").First();
                }
                else
                {
                    bobiiRole = user.Guild.Roles.Where(role => role.Name == "Bobii").First();
                }

                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));

                //Create channel with permissions in the target category
                var channel = user.Guild.CreateVoiceChannelAsync(name, prop =>
                {
                    prop.CategoryId = ulong.Parse(catergoryId);
                    prop.PermissionOverwrites = permissions;
                    prop.UserLimit = channelSize;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "CreateVoiceChannel",
                    new SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"{user} created new voice channel {channel.Result}", tempChannelID: channel.Result.Id);
                return channel.Result;
            }
            catch (Exception ex)
            {
                var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(user.Guild.Id).Result;
                if (ex.Message.Contains("Missing Permission"))
                {
                    await user.SendMessageAsync(String.Format(GeneralHelper.GetContent("C098", language).Result, user.Username));
                }

                if (ex.Message.Contains("Object reference not set to an instance of an object"))
                {
                    await user.SendMessageAsync(String.Format(GeneralHelper.GetContent("C099", language).Result, user.Username));
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, "CreateVoiceChannel",
                    new SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"Voicechannel could not be created, {user} has got a DM if it was missing permissions or null ref", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static Embed CreateCreateTempChannelInformation(SlashCommandParameter parameter, ulong createTempChannelId)
        {
            var createTempChannel = CreateTempChannelsHelper.GetCreateTempChannel(createTempChannelId).Result;
            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(parameter.GuildID, createTempChannelId).Result;
            var viceChannel = (IVoiceChannel)parameter.Client.GetChannel(createTempChannelId);
            var header = viceChannel.Name;

            var sb = new StringBuilder();
            sb.AppendLine(String.Format(GeneralHelper.GetContent("C248", parameter.Language).Result, createTempChannel.tempchannelname));
            if (createTempChannel.channelsize.HasValue && createTempChannel.channelsize != 0)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C249", parameter.Language).Result, createTempChannel.channelsize));
            }

            if (createTempChannel.delay.HasValue && createTempChannel.delay != 0)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C250", parameter.Language).Result, createTempChannel.delay));
            }

            sb.AppendLine();

            var commands = parameter.Client.GetGlobalApplicationCommandsAsync()
                .Result
                .Where(c => c.Name == GlobalStrings.temp)
                .First()
                .Options
                .Select(o => o.Name)
                .ToList();

            sb.AppendLine(GetCommandsTable(parameter, disabledCommands, commands, "C241"));

            sb.AppendLine();
            sb.AppendLine(GetCommandsTable(parameter, disabledCommands, new List<string>() { "interface", "ownerpermissions" }, "C243", false));

            return GeneralHelper.CreateEmbed(parameter.Interaction, sb.ToString(), header).Result;
        }

        public static string GetCommandsTable(SlashCommandParameter parameter, List<tempcommands> disabledCommands, List<string> commands, string spc, bool tempCommands = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine("╔══════════════════════╦═══════════╗");
            AddRow(sb, $"*{GeneralHelper.GetCaption(spc, parameter.Language).Result}*", $"*{GeneralHelper.GetCaption("C242", parameter.Language).Result}*", false, "");
            var count = 0;

            var temp = "";
            if (tempCommands)
            {
                temp = "/temp ";
            }
            foreach (var command in commands)
            {
                count++;

                if (count == commands.Count())
                {
                    AddRow(sb, command, (disabledCommands.SingleOrDefault(c => c.commandname == command) == null).ToString(), true, temp);
                }
                else 
                {
                    AddRow(sb, command, (disabledCommands.SingleOrDefault(c => c.commandname == command) == null).ToString(), false, temp);
                }
            }

            sb.AppendLine("╚══════════════════════╩═════════=═╝");
            sb.AppendLine("```");

            return sb.ToString();
        }

        public static void AddRow(StringBuilder sb, string command, string active, bool lastRow = false, string temp = "/temp ")
        {
            var str = $"║ {temp}{command}";
            str = Auffuellen(str, 24, "║");

            str += $" {active}";
            str = Auffuellen(str, 36, "║");

            sb.AppendLine(str);
            if (!lastRow)
            {
                sb.AppendLine("╠══════════════════════╬═══════════╣");
            }

        }

        public static string Auffuellen(string str, int pos, string zeichen)
        {
            var sb = new StringBuilder();
            sb.Append(str);
            while(sb.Length < pos - 1)
            {
                sb.Append(" ");
            }

            sb.Append(zeichen);
            return sb.ToString();
        }

        public static Embed CreateVoiceChatInfoEmbed(SlashCommandParameter parameter)
        {
            StringBuilder sb = new StringBuilder();
            var createTempChannelList = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(parameter.Guild).Result;
            string header = null;
            if (createTempChannelList.Count == 0)
            {
                header = GeneralHelper.GetCaption("C100", parameter.Language).Result;
                sb.AppendLine(GeneralHelper.GetContent("C100", parameter.Language).Result);
            }
            else
            {
                header = GeneralHelper.GetCaption("C101", parameter.Language).Result;
            }

            foreach (var createTempChannel in createTempChannelList)
            {
                var channelId = createTempChannel.createchannelid;
                var voiceChannel = parameter.Client.Guilds
                                   .SelectMany(g => g.Channels)
                                   .SingleOrDefault(c => c.Id == channelId);
                if (voiceChannel == null)
                {
                    continue;
                }

                sb.AppendLine("");
                sb.AppendLine($"<#{channelId}>");
                sb.AppendLine($"Id: **{channelId}**");
                sb.AppendLine($"TempChannelName: **{createTempChannel.tempchannelname}**");

                if (createTempChannel.channelsize != null && createTempChannel.channelsize != 0)
                {
                    sb.AppendLine($"TempChannelSize: **{createTempChannel.channelsize}**");
                }

                if (createTempChannel.delay.HasValue && createTempChannel.delay != 0)
                {
                    sb.AppendLine($"Delay: **{createTempChannel.delay}**");
                }
            }

            return GeneralHelper.CreateEmbed(parameter.Interaction, sb.ToString(), header).Result;
        }

        //Double Code -> Find solution one day!
        public static async Task<string> HelpTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            return GeneralHelper.CreateInfoPart(
                commandList,
                language,
                GeneralHelper.GetContent("C102", language).Result + GeneralHelper.GetContent("C103", language).Result,
                "createtempchannel",
                guildId).Result;
        }

        public static async Task SortCountNeu(createtempchannels createTempChannel, DiscordSocketClient client)
        {
            try
            {
                var tempChannelsFromGuild = TempChannelsHelper.GetTempChannelList().Result
                    .Where(channel => channel.createchannelid == createTempChannel.createchannelid)
                    .OrderBy(channel => channel.count);

                var count = 1;
                foreach (tempchannels channel in tempChannelsFromGuild)
                {
                    // Wenn die nummer nicht stimmt, dann muss der Channel neu numeriert werden.
                    if (channel.count != count)
                    {
                        // Voice channel im Discord ermitteln
                        var discordChannel = (SocketVoiceChannel)client.GetChannel(channel.channelid);
                        if (discordChannel == null)
                        {
                            continue;
                        }

                        // index ermittel an welcher stelle die Zahl stehen sollte
                        var indexOfCountWord = createTempChannel.tempchannelname.IndexOf("{count}");

                        if (discordChannel.Name.Contains(channel.count.ToString()) && discordChannel.Name[indexOfCountWord].ToString() == channel.count.ToString())
                        {
                            var discordChannelName = discordChannel.Name;
                            var nameInChar = discordChannelName.ToCharArray();
                            nameInChar[indexOfCountWord] = char.Parse(count.ToString());
                            _ = Task.Run(async () => discordChannel.ModifyAsync(c => c.Name = new string(nameInChar)));
                            _ = TempChannelsHelper.UpdateCount(channel.id, count);
                        }
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            await Task.CompletedTask;
        }

        public static async Task<string> HelpEditTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId, bool withoutHint = false)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            var sb = new StringBuilder();
            if (!withoutHint)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine(GeneralHelper.GetContent("C104", language).Result);
                sb.AppendLine(GeneralHelper.GetContent("C105", language).Result);
                sb.AppendLine();
                sb.AppendLine(GeneralHelper.GetContent("C187", language).Result);
            }
            return GeneralHelper.CreateInfoPart(commandList, language, sb.ToString(), "temp", guildId, !withoutHint).Result;
        }

        public static async Task<ulong> TansferOwnerShip(SocketVoiceChannel channel, DiscordSocketClient client)
        {
            var lang = Bobii.EntityFramework.BobiiHelper.GetLanguage(channel.Guild.Id).Result;
            if (channel.ConnectedUsers.Where(u => u.IsBot == false).Count() == 0)
            {
                if (channel.ConnectedUsers.Count != 0)
                {
                    await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, 0);
                    await SendOwnerUpdatedBotMessage(channel, channel.Guild, lang);
                }
                return 0;
            }
            var luckyNewOwner = channel.ConnectedUsers.Where(u => u.IsBot == false).First();
            await GiveManageChannelRightsToUserVc(luckyNewOwner, channel.Guild.Id, null, channel);

            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(channel.Id).Result;
            await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, luckyNewOwner.Id);
            await SendOwnerUpdatedMessage(channel, channel.Guild, luckyNewOwner.Id, lang);
            return luckyNewOwner.Id;
        }

        public static async Task SendOwnerUpdatedMessage(SocketVoiceChannel channel, SocketGuild guild, ulong userId, string language)
        {
            await channel.SendMessageAsync("", embeds: new Embed[] {
                GeneralHelper.CreateEmbed(guild, String.Format(GeneralHelper.GetCaption("C233", language).Result, userId), "").Result
            });
        }

        public static async Task SendOwnerUpdatedBotMessage(SocketVoiceChannel channel, SocketGuild guild, string language)
        {
            await channel.SendMessageAsync("", embeds: new Embed[] {
                GeneralHelper.CreateEmbed(guild, GeneralHelper.GetContent("C231", language).Result, "").Result
            });
        }
        #endregion
    }
}