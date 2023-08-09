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
using TwitchLib.Communication.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

            // If the user was the owner of the temp-channel which he left, then the owner ship will be transfered to a new random owner
            if (tempChannel.channelownerid == parameter.SocketUser.Id)
            {
                await RemoveManageChannelRightsToUserVc(parameter.SocketUser, parameter.OldSocketVoiceChannel);
                await TansferOwnerShip(parameter.OldSocketVoiceChannel, parameter.Client);
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

        public static async Task GiveOwnerIfOwnerIDZero(SlashCommandParameter parameter)
        {
            try
            {
                var tempChannelId = parameter.GuildUser.VoiceState.Value.VoiceChannel.Id;
                var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(tempChannelId).Result;
                if (tempChannel == null)
                {
                    return;
                }

                var ownerId = TempChannel.EntityFramework.TempChannelsHelper.GetOwnerID(tempChannelId).Result;
                if (ownerId == 0)
                {
                    await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(tempChannelId, parameter.GuildUser.Id);
                    await GiveManageChannelRightsToUserVc(parameter.GuildUser, parameter.GuildID, null, parameter.GuildUser.VoiceChannel);
                }
            }
            catch (Exception)
            {
                //nothing
            }
        }

        public static async Task GiveManageChannelRightsToUserVc(SocketUser user, ulong guildId, RestVoiceChannel restVoiceChannel, SocketVoiceChannel socketVoiceChannel)
        {
            if (TempCommandsHelper.DoesCommandExist(guildId, "ownerpermissions").Result)
            {
                return;
            }

            if (restVoiceChannel != null)
            {
                await restVoiceChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(manageChannel: PermValue.Allow));
            }
            else
            {
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
            await WriteInterfaceInVoiceChannel(tempChannel, client);
        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketUser user, string channelName, SocketVoiceState newVoice, createtempchannels createTempChannel, int? channelSize)
        {
            try
            {
                var category = newVoice.VoiceChannel.Category;
                var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), channelName, channelSize, newVoice).Result;

                _ = TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id);
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
                    new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user }, message: $"{ user} ({ user.Id}) could not be connected");
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
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "lock").Result)
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
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnLock)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnLock)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnLock)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "unlock").Result)
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
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempHide)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempHide)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempHide)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "hide").Result)
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

        public static async Task TempUnHide(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnHide)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnHide)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnHide)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "unhide").Result)
            {
                return;
            }

            var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;

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
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);


            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempSize)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempSize)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempSize)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "size").Result)
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
                        GeneralHelper.GetCaption("C121", parameter.Language).Result).Result }, ephemeral: true);
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

            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempSaveConfig)).Result ||
            CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempSaveConfig)).Result ||
            CheckDatas.CheckIfCommandIsDisabled(parameter, "saveconfig").Result)
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
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempDeleteConfig)).Result ||
            CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempDeleteConfig)).Result ||
            CheckDatas.CheckIfUserTempChannelConfigExists(parameter, nameof(TempDeleteConfig)).Result ||
            CheckDatas.CheckIfCommandIsDisabled(parameter, "deleteconfig").Result)
            {
                return;
            }

            var currentVC = parameter.GuildUser.VoiceState.Value.VoiceChannel;
            var tempChannel = TempChannelsHelper.GetTempChannel(currentVC.Id).Result;

            await TempChannelUserConfig.DeleteConfig(parameter.GuildUser.Id, tempChannel.createchannelid.Value);

            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C180", parameter.Language).Result,
                             GeneralHelper.GetCaption("C180", parameter.Language).Result).Result }, ephemeral: true);

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempDeleteConfig), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                 message: "/temp deleteconfig successfully used");
        }

        public static async Task TempKick(SlashCommandParameter parameter, string userId)
        {
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            if (CheckDatas.CheckUserID(parameter, userId, nameof(TempKick)).Result)
            {
                return;
            }

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempKick)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempKick)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempKick)).Result ||
                CheckDatas.CheckIfUserInSameTempVoice(parameter, userId.ToUlong(), nameof(TempKick)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "kick").Result)
            {
                return;
            }

            var usedGuild = parameter.Client.GetGuild(parameter.Guild.Id);

            var toBeKickedUser = usedGuild.GetUser(userId.ToUlong());
            try
            {
                await toBeKickedUser.ModifyAsync(channel => channel.Channel = null);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C128", parameter.Language).Result, toBeKickedUser.Id),
                    GeneralHelper.GetCaption("C128", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempKick), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempkick successfully used");

            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempKick), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to kick temp-channel user", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C129", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempUnBlock(SlashCommandParameter parameter, IUser user)
        {
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            var userId = user.Id;

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnBlock)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnBlock)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnBlock)).Result ||
                CheckDatas.CheckIfUserInGuild(parameter, userId, nameof(TempUnBlock)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "unblock").Result)
            {
                return;
            }

            try
            {
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                await voiceChannel.RemovePermissionOverwriteAsync(parameter.Client.GetUserAsync(userId).Result);

                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C136", parameter.Language).Result,
                    GeneralHelper.GetCaption("C136", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunblock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unblock user from temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("137", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempBlock(SlashCommandParameter parameter, IUser user)
        {
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);
            var userId = user.Id;

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempBlock)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempBlock)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempBlock)).Result ||
                CheckDatas.CheckIfUserInGuild(parameter, userId, nameof(TempBlock)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "block").Result)
            {
                return;
            }

            try
            {
                var newPermissionOverride = new OverwritePermissions().Modify(connect: PermValue.Deny, viewChannel: PermValue.Deny);
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                _ = voiceChannel.AddPermissionOverwriteAsync(parameter.Client.GetUserAsync(userId).Result, newPermissionOverride);

                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C134", parameter.Language).Result,
                    GeneralHelper.GetCaption("C134", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempblock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to block user from temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C135", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempOwner(SlashCommandParameter parameter, string userId)
        {
            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

            if (CheckDatas.CheckUserID(parameter, userId, nameof(TempOwner)).Result)
            {
                return;
            }

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempOwner)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempOwner)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempOwner)).Result ||
                CheckDatas.CheckIfUserInSameTempVoice(parameter, userId.ToUlong(), nameof(TempOwner)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "onwer").Result)
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
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C124", parameter.Language).Result,
                        GeneralHelper.GetCaption("C124", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempOwner), new SlashCommandParameter() { Guild = parameter.Guild, GuildUser = parameter.GuildUser },
                        message: "User is a Bot");
                    return;
                }

                var voiceChannel = parameter.Client.GetChannel(tempChannel.channelid);

                await TempChannelHelper.RemoveManageChannelRightsToUserVc(currentOwner, voiceChannel as SocketVoiceChannel);
                await TempChannelHelper.GiveManageChannelRightsToUserVc(newOwner, parameter.GuildID, null, voiceChannel as SocketVoiceChannel);


                await TempChannelsHelper.ChangeOwner(parameter.GuildUser.VoiceChannel.Id, userId.ToUlong());
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C125", parameter.Language).Result, userId),
                    GeneralHelper.GetCaption("C125", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempowner successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel owner", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C126", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                return;
            }
        }

        public static void AddInterfaceButton(ComponentBuilder componentBuid, string customId, string emojiString)
        {
            componentBuid.WithButton(customId: customId, style: ButtonStyle.Secondary, emote: Emote.Parse(emojiString));
        }

        public static async Task WriteInterfaceInVoiceChannel(RestVoiceChannel tempChannel, DiscordSocketClient client)
        {
            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(tempChannel.GuildId).Result;
            var voiceChannel = (IRestMessageChannel)tempChannel;
            var componentBuilder = new ComponentBuilder();
            await AddInterfaceButtons(componentBuilder, disabledCommands);

            var lang = Bobii.EntityFramework.BobiiHelper.GetLanguage(tempChannel.GuildId).Result;

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(GeneralHelper.GetCaption("C211", lang).Result)
                .WithColor(74, 171, 189)
                .WithImageUrl("https://cdn.discordapp.com/attachments/910868343030960129/1138542889459253249/Erklarbar.png")
                .WithDescription(GeneralHelper.GetContent("C208", lang).Result)
                .WithFooter(DateTime.Now.ToString("dd/MM/yyyy"));

            await voiceChannel.SendMessageAsync("", embeds: new Embed[] { embed.Build() }, components: componentBuilder.Build());
        }

        public static async Task AddInterfaceButtons(ComponentBuilder componentBuilder, List<tempcommands> disabledCommands)
        {
            if (!CommandDisabled(disabledCommands, "name"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-name", "<:edit:1138160331122802818>");
            }
            if (!CommandDisabled(disabledCommands, "unlock"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-openchannel", "<:lockopen:1138164700434149477>");
            }
            if (!CommandDisabled(disabledCommands, "lock"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-closechannel", "<:lockclosed:1138164855820525702>");
            }
            if (!CommandDisabled(disabledCommands, "hide"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-hidechannel", "<:ghost:1138173699254665268>");
            }
            if (!CommandDisabled(disabledCommands, "unhide"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-unhidechannel", "<:noghost:1138173749900882101>");
            }
            if (!CommandDisabled(disabledCommands, "kick"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-kick", "<:userkicked:1138489936383856750>");
            }
            if (!CommandDisabled(disabledCommands, "block"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-block", "<:userblocked:1138489934710321182>");
            }
            if (!CommandDisabled(disabledCommands, "unblock"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-unblock", "<:userunblocked:1138489942134235166>");
            }
            if (!CommandDisabled(disabledCommands, "saveconfig"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-saveconfig", "<:config:1138181363338588351>");
            }
            if (!CommandDisabled(disabledCommands, "deleteconfig"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-deleteconfig", "<:noconfig:1138181406799966209>");
            }
            if (!CommandDisabled(disabledCommands, "size"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-size", "<:usersize:1138489939080794193>");
            }
            if (!CommandDisabled(disabledCommands, "owner"))
            {
                AddInterfaceButton(componentBuilder, "temp-interface-owner", "<:owner:1138519029254992002>");
            }
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

                await GiveManageChannelRightsToUserVc(user, user.Guild.Id, channel.Result, null);

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

        public static async Task TansferOwnerShip(SocketVoiceChannel channel, DiscordSocketClient client)
        {
            if (channel.ConnectedUsers.Where(u => u.IsBot == false).Count() == 0)
            {
                if (channel.ConnectedUsers.Count != 0)
                {
                    await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, 0);
                }
                return;
            }
            var luckyNewOwner = channel.ConnectedUsers.Where(u => u.IsBot == false).First();
            await GiveManageChannelRightsToUserVc(luckyNewOwner, channel.Guild.Id, null, channel);

            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(channel.Id).Result;
            await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, luckyNewOwner.Id);
        }
        #endregion
    }
}