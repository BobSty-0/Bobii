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
using System.Drawing.Drawing2D;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TwitchLib.Api.Helix.Models.Users.GetUserBlockList;
using ImageMagick;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using Bobii.src.Bobii.EntityFramework;
using static System.Collections.Specialized.BitVector32;
using Bobii.src.Handler;
using TwitchLib.Api.Helix.Models.Moderation.GetModerators;
using src.InteractionModules.Slashcommands;

namespace Bobii.src.Helper
{
    static class TempChannelHelper
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
                    if (tempChannelConfig.tempchannelname != "")
                    {
                        tempChannelName = tempChannelConfig.tempchannelname;
                        tempChannelName = GetVoiceChannelName(createTempChannel, parameter.SocketUser, tempChannelName, parameter.Client).Result;
                    }
                    else
                    {
                        tempChannelName = createTempChannel.tempchannelname;
                        tempChannelName = GetVoiceChannelName(createTempChannel, parameter.SocketUser, tempChannelName, parameter.Client).Result;
                    }

                    await CreateAndConnectToVoiceChannel(parameter.SocketUser, createTempChannel, parameter.NewVoiceState, parameter.Client, (tempChannelConfig.channelsize.Value != 0 ? tempChannelConfig.channelsize : createTempChannel.channelsize), tempChannelName);
                    return;
                }

                tempChannelName = createTempChannel.tempchannelname;
                tempChannelName = GetVoiceChannelName(createTempChannel, parameter.SocketUser, tempChannelName, parameter.Client).Result;
                await CreateAndConnectToVoiceChannel(parameter.SocketUser, createTempChannel, parameter.NewVoiceState, parameter.Client, createTempChannel.channelsize, tempChannelName);
            }
        }

        public static async Task HandleUserLeftChannel(VoiceUpdatedParameter parameter)
        {
            var tempChannel = TempChannelsHelper.GetTempChannel(parameter.OldSocketVoiceChannel.Id).Result;

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

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, tempChannel.channelid).Result != null && parameter.SocketUser.Id != tempChannel.channelownerid)
            {
                if (UsedFunctionsHelper.GetBlockedUserFunction(parameter.Guild.Id, tempChannel.channelownerid.Value, parameter.SocketUser.Id).Result == null)
                {
                    var permissions = parameter.OldSocketVoiceChannel.PermissionOverwrites.ToList();
                    permissions = EditPermissionConnect(PermValue.Inherit, permissions, parameter.SocketUser as SocketGuildUser, parameter.OldSocketVoiceChannel);
                    await parameter.OldSocketVoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                }
            }

            // Removing view rights of the text-channel if the temp-chanenl has a linked text-channel
            createTempChannel = CreateTempChannelsHelper.GetCreateTempChannelList().Result.FirstOrDefault(c => c.createchannelid == tempChannel.createchannelid);
            if (createTempChannel != null && createTempChannel.delay != null && createTempChannel.delay != 0 && parameter.OldSocketVoiceChannel.ConnectedUsers.Count == 0)
            {
                // We just add an delay if the createTempChannel has an delay
                await parameter.DelayOnDelete.StartDelay(tempChannel, createTempChannel, parameter);
                return;
            }

            if (parameter.OldSocketVoiceChannel.ConnectedUsers.Count() == 0)
            {
                await DeleteTempChannel(parameter, tempChannel);
                if (createTempChannel.tempchannelname.Contains("{count}"))
                {
                    _ = SortCountNeu(createTempChannel, parameter.Client);
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
                var tempChannels = TempChannelsHelper.GetTempChannelList().Result.Where(t => t.channelownerid == user.Id).ToList();
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
                var tempChannel = TempChannelsHelper.GetTempChannel(tempChannelId).Result;
                if (tempChannel == null)
                {
                    return;
                }

                if (TempCommandsHelper.DoesCommandExist(voiceChannel.Guild.Id, tempChannel.createchannelid.Value, GlobalStrings.autotransferowner).Result)
                {
                    return;
                }

                var ownerId = TempChannelsHelper.GetOwnerID(tempChannelId).Result;
                if (voiceChannel.ConnectedUsers.FirstOrDefault(u => u.Id == tempChannel.channelownerid) == null)
                {
                    var user = parameter.Guild.GetUser(ownerId);

                    await TempChannelsHelper.ChangeOwner(tempChannelId, parameter.GuildUser.Id);

                    var permissions = voiceChannel.PermissionOverwrites.ToList();
                    permissions = UnblockAllUsersFromPreviousOwner(user, permissions, voiceChannel).Result;
                    permissions = BlockAllUserFromOwner(parameter.GuildUser, parameter.Client, permissions, null, voiceChannel).Result;
                    permissions = UnmuteIfNewOwnerAndMuted(parameter, permissions).Result;

                    if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
                    {
                        permissions = EditWhitelistedUsers(PermValue.Inherit, new SlashCommandParameter() { Guild = parameter.Guild, GuildID = parameter.GuildID, GuildUser = user }, permissions, voiceChannel).Result;
                        _ = KickUsersWhoAreNoLongerOnWhiteList(voiceChannel, parameter.GuildUser.Id);
                        permissions = EditWhitelistedUsers(PermValue.Allow, parameter, permissions, voiceChannel).Result;
                    }

                    permissions = RemoveManageChannelRightsToUserVc(user, permissions, voiceChannel).Result;
                    permissions = GiveManageChannelRightsToUserVc(parameter.GuildUser, parameter.GuildID, permissions, null, parameter.GuildUser.VoiceChannel).Result;

                    await voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                    await SendOwnerUpdatedMessage(voiceChannel, parameter.Guild, parameter.GuildUser.Id, parameter.Language);
                }
            }
            catch (Exception)
            {
                //nothing
            }
        }

        public static async Task<List<Overwrite>> UnmuteIfNewOwnerAndMuted(SlashCommandParameter parameter, List<Overwrite> permissions)
        {
            var voice = parameter.GuildUser.VoiceChannel;
            if (UsedFunctionsHelper.GetMutedUsedFunctions(voice.Id).Result.SingleOrDefault(f => f.affecteduserid == parameter.GuildUser.Id) != null)
            {
                permissions = EditPermissionSpeak(PermValue.Allow, permissions, parameter.GuildUser, voice);

                await voice.ModifyAsync(v => v.PermissionOverwrites = permissions);
                _ = parameter.GuildUser.ModifyAsync(u => u.Channel = voice);
                _ = parameter.GuildUser.ModifyAsync(u => u.Mute = false);
                await UsedFunctionsHelper.RemoveUsedFunction(voice.Id, GlobalStrings.mute, parameter.GuildUser.Id); ;
            }

            return permissions;
        }

        public static async Task<List<Overwrite>> GiveManageChannelRightsToUserVc(SocketUser user, ulong guildId, List<Overwrite> permissions, RestVoiceChannel restVoiceChannel, SocketVoiceChannel socketVoiceChannel)
        {
            if (restVoiceChannel != null)
            {
                permissions = EditPermissionManageMessages(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);
                permissions = EditPermissionSendMessage(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);
                permissions = EditPermissionViewChannel(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);
                permissions = EditPermissionConnect(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);
                permissions = EditPermissionSlashCommands(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);
                permissions = EditPermissionSpeak(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);

                var tempChannelEntity = TempChannelsHelper.GetTempChannel(restVoiceChannel.Id).Result;
                if (!TempCommandsHelper.DoesCommandExist(guildId, tempChannelEntity.createchannelid.Value, "ownerpermissions").Result)
                {
                    permissions = EditPermissionManageChannel(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);
                }
            }
            else
            {
                permissions = EditPermissionManageMessages(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);
                permissions = EditPermissionSendMessage(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);
                permissions = EditPermissionViewChannel(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);
                permissions = EditPermissionConnect(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);
                permissions = EditPermissionSpeak(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);

                var tempChannelEntity = TempChannelsHelper.GetTempChannel(socketVoiceChannel.Id).Result;
                if (!TempCommandsHelper.DoesCommandExist(guildId, tempChannelEntity.createchannelid.Value, "ownerpermissions").Result)
                {
                    permissions = EditPermissionManageChannel(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);
                }
            }
            return permissions;
        }

        public static async Task<List<Overwrite>> RemoveManageChannelRightsToUserVc(SocketUser user, List<Overwrite> permissionsAll, SocketVoiceChannel voiceChannel)
        {
            var permission = permissionsAll.SingleOrDefault(u => u.TargetId == user.Id);
            if (permission.TargetId != user.Id)
            {
                return permissionsAll;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, voiceChannel.Id).Result == null)
            {
                permissionsAll = EditPermissionConnect(PermValue.Inherit, permissionsAll, user as SocketGuildUser, voiceChannel);
            }

            permissionsAll = EditPermissionManageMessages(PermValue.Inherit, permissionsAll, user as SocketGuildUser, voiceChannel);
            permissionsAll = EditPermissionSendMessage(PermValue.Inherit, permissionsAll, user as SocketGuildUser, voiceChannel);
            permissionsAll = EditPermissionViewChannel(PermValue.Inherit, permissionsAll, user as SocketGuildUser, voiceChannel);
            permissionsAll = EditPermissionSlashCommands(PermValue.Inherit, permissionsAll, user as SocketGuildUser, voiceChannel);
            permissionsAll = EditPermissionSpeak(PermValue.Inherit, permissionsAll, user as SocketGuildUser, voiceChannel);

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(voiceChannel.Id).Result;
            if (!TempCommandsHelper.DoesCommandExist(voiceChannel.Guild.Id, tempChannelEntity.createchannelid.Value, "ownerpermissions").Result)
            {
                permissionsAll = EditPermissionManageChannel(PermValue.Inherit, permissionsAll, user as SocketGuildUser, voiceChannel);
            }

            return permissionsAll;
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

            if (newVoice.VoiceChannel.Category == null)
            {
                var dm = user.CreateDMChannelAsync().Result;
                _ = dm.SendMessageAsync(String.Format(GeneralHelper.GetContent("C279", Bobii.EntityFramework.BobiiHelper.GetLanguage(newVoice.VoiceChannel.Guild.Id).Result).Result, user.GlobalName));
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, nameof(CreateVoiceChannel), createChannelID: createTempChannel.createchannelid, exceptionMessage: "Keine Kategorie zugeordnet");
                return;
            }
            var tempChannel = CreateVoiceChannel(user, channelName, newVoice, createTempChannel, channelSize, client).Result;
            await ConnectToVoice(tempChannel, user as IGuildUser);

            var permissions = tempChannel.PermissionOverwrites.ToList();

            permissions = GiveManageChannelRightsToUserVc(user, ((SocketGuildUser)user).Guild.Id, permissions, tempChannel, null).Result;
            permissions = BlockAllUserFromOwner(user as SocketGuildUser, client, permissions, tempChannel, null).Result;

            await tempChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(tempChannel.Id).Result;
            if (!TempCommandsHelper.DoesCommandExist(((SocketGuildUser)user).Guild.Id, tempChannelEntity.createchannelid.Value, "interface").Result)
            {
                await WriteInterfaceInVoiceChannel(tempChannel, client, tempChannelEntity.createchannelid.Value);
            }
        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketUser user, string channelName, SocketVoiceState newVoice, createtempchannels createTempChannel, int? channelSize, DiscordSocketClient client)
        {
            try
            {
                var category = newVoice.VoiceChannel.Category;
                var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), channelName, channelSize, newVoice).Result;
                _ = TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id);

                var guild = ((SocketGuildUser)user).Guild;
                return tempChannel;
            }
            catch (Exception ex)
            {

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, nameof(CreateVoiceChannel), createChannelID: createTempChannel.createchannelid);
                return null;
            }
        }

        public static async Task<List<Overwrite>> UnblockAllUsersFromPreviousOwner(SocketGuildUser user, List<Overwrite> permissions, SocketVoiceChannel voiceChannel)
        {
            try
            {
                var tempChannel = TempChannelsHelper.GetTempChannel(voiceChannel.Id).Result;
                var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(user.Guild.Id, tempChannel.createchannelid.Value).Result;

                if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.block) != null)
                {
                    return permissions;
                }

                var usedFunctions = UsedFunctionsHelper.GetUsedFunctions(user.Id, user.Guild.Id).Result.Where(u => u.function == GlobalStrings.block).ToList();

                foreach (var usedFunction in usedFunctions)
                {
                    var userToBeUnblocked = user.Guild.GetUser(usedFunction.affecteduserid);
                    if (userToBeUnblocked != null)
                    {
                        permissions = EditPermissionConnect(PermValue.Inherit, permissions, userToBeUnblocked, voiceChannel);

                        if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.hidevoicefromblockedusers) == null)
                        {
                            permissions = EditPermissionViewChannel(PermValue.Inherit, permissions, userToBeUnblocked, voiceChannel);
                        }
                    }
                }

                return permissions;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return permissions;
            }
        }

        public static async Task BlockUserFormBannedVoiceAfterJoining(SocketGuildUser user)
        {
            var usedFunctions = UsedFunctionsHelper.GetUsedFrunctionsFromAffectedUser(user.Id, user.Guild.Id)
                .Result
                .Where(u => u.function == GlobalStrings.block || u.function == GlobalStrings.mute)
                .ToList();

            if (usedFunctions.Count() != 0)
            {
                var tempChannels = TempChannelsHelper.GetTempChannelListFromGuild(user.Guild.Id).Result;
                var affectedTempChannels = tempChannels.Where(t => usedFunctions.Select(u => u.userid).ToList().Contains(t.channelownerid.Value)).ToList();

                foreach (var affectedTempChannel in affectedTempChannels)
                {
                    var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(user.Guild.Id, affectedTempChannel.createchannelid.Value).Result;
                    var tempChannel = user.Guild.GetVoiceChannel(affectedTempChannel.channelid);
                    var newPermissionOverride = new OverwritePermissions();
                    if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.block) == null)
                    {
                        if (UsedFunctionsHelper.GetBlockedUserFunction(affectedTempChannel.guildid, affectedTempChannel.channelownerid.Value, user.Id).Result != null)
                        {
                            var hideVoie = disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.hidevoicefromblockedusers) == null;
                            newPermissionOverride = newPermissionOverride.Modify(connect: PermValue.Deny);
                            if (hideVoie)
                            {
                                newPermissionOverride = newPermissionOverride.Modify(viewChannel: PermValue.Deny);
                            }
                        }
                    }

                    if (UsedFunctionsHelper.GetMutedUsedFunctions(affectedTempChannel.channelid).Result.SingleOrDefault(u => u.affecteduserid == user.Id) != null)
                    {
                        newPermissionOverride = newPermissionOverride.Modify(speak: PermValue.Deny);
                    }

                    if (UsedFunctionsHelper.GetChatMutedUserUsedFunctions(affectedTempChannel.channelid).Result.SingleOrDefault(u => u.affecteduserid == user.Id) != null)
                    {
                        newPermissionOverride = newPermissionOverride.Modify(sendMessages: PermValue.Deny);
                    }

                    await tempChannel.AddPermissionOverwriteAsync(user, newPermissionOverride);
                }
            }
        }

        public static async Task<List<Overwrite>> BlockAllUserFromOwner(SocketGuildUser user, DiscordSocketClient client, List<Overwrite> permissions, RestVoiceChannel restVoiceChannel, SocketVoiceChannel socketVoiceChannel)
        {
            try
            {
                var usedFunctions = UsedFunctionsHelper.GetUsedFunctions(user.Id, user.Guild.Id).Result.Where(u => u.function == GlobalStrings.block).ToList();
                tempchannels tempChannel;
                var disabledCommands = new List<tempcommands>();
                var hideVoie = false;

                if (socketVoiceChannel != null)
                {
                    tempChannel = TempChannelsHelper.GetTempChannel(socketVoiceChannel.Id).Result;
                }
                else
                {
                    tempChannel = TempChannelsHelper.GetTempChannel(restVoiceChannel.Id).Result;
                }

                disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(user.Guild.Id, tempChannel.createchannelid.Value).Result;
                hideVoie = disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.hidevoicefromblockedusers) == null;

                if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.block) != null)
                {
                    return permissions;
                }

                foreach (var usedFunction in usedFunctions)
                {
                    var userToBeBlocked = user.Guild.GetUser(usedFunction.affecteduserid);
                    if (userToBeBlocked == null)
                    {
                        continue;
                    }

                    var newPermissionOverride = new OverwritePermissions().Modify(connect: PermValue.Deny);

                    if (hideVoie)
                    {
                        newPermissionOverride = newPermissionOverride.Modify(viewChannel: PermValue.Deny);
                    }

                    if (restVoiceChannel != null)
                    {

                        EditPermissionConnect(PermValue.Deny, permissions, userToBeBlocked, restVoiceChannel);

                        if (hideVoie)
                        {
                            EditPermissionViewChannel(PermValue.Deny, permissions, userToBeBlocked, restVoiceChannel);
                        }
                    }
                    else
                    {
                        EditPermissionConnect(PermValue.Deny, permissions, userToBeBlocked, socketVoiceChannel);

                        if (hideVoie)
                        {
                            EditPermissionViewChannel(PermValue.Deny, permissions, userToBeBlocked, socketVoiceChannel);
                        }

                        if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.kickblockedusersonownerchange) == null)
                        {
                            if (socketVoiceChannel.ConnectedUsers.Contains(userToBeBlocked))
                            {
                                await userToBeBlocked.ModifyAsync(channel => channel.Channel = null);
                            }
                        }
                    }
                }
                return permissions;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return permissions;
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

        public static async Task TempMuteVoice(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            parameter.Interaction.DeferAsync();
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempMuteVoice), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempMuteVoice), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempMuteVoice), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.chat, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mutevoice, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C333", parameter.Language).Result, GeneralHelper.GetCaption("C262", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMuteVoice), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Muten vom Voice ist fehlgeschlagen", exceptionMessage: "Bereits gemuted");
                return;
            }


            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));
                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        continue;
                    }

                    var modifiedPerm = perm.Permissions.Modify(speak: PermValue.Deny);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(speak: PermValue.Deny)));
                }

                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                _ = UsedFunctionsHelper.AddUsedFunction(tempChannelEntity.channelownerid.Value, 0, GlobalStrings.mutevoice, voiceChannel.Id, parameter.GuildID);

                foreach (var user in parameter.GuildUser.VoiceChannel.ConnectedUsers)
                {
                    _ = user.ModifyAsync(user => user.Channel = voiceChannel);
                }

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C334", parameter.Language).Result, GeneralHelper.GetCaption("C262", parameter.Language).Result),
                    GeneralHelper.GetCaption("C236", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempMuteVoice), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp voice mute successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMuteVoice), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to mute temp-channel voice", exceptionMessage: ex.Message);
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task TempMuteChat(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempMuteChat), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempMuteChat), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempMuteChat), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.chat, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mutechat, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C322", parameter.Language).Result, GeneralHelper.GetCaption("C262", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMuteChat), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Muten vom Chat ist fehlgeschlagen", exceptionMessage: "Bereits gemuted");
                return;
            }


            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));
                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        continue;
                    }

                    var modifiedPerm = perm.Permissions.Modify(sendMessages: PermValue.Deny);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(sendMessages: PermValue.Deny)));
                }

                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                _ = UsedFunctionsHelper.AddUsedFunction(tempChannelEntity.channelownerid.Value, 0, GlobalStrings.mutechat, voiceChannel.Id, parameter.GuildID);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C323", parameter.Language).Result, GeneralHelper.GetCaption("C262", parameter.Language).Result),
                    GeneralHelper.GetCaption("C236", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempMuteChat), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp chat mute successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMuteChat), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to mute temp-channel chat", exceptionMessage: ex.Message);
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task TempUnMuteVoice(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            parameter.Interaction.DeferAsync();
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnMuteVoice), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnMuteVoice), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnMuteVoice), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.privacy, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mutevoice, parameter.GuildUser.VoiceChannel.Id).Result == null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                   String.Format(GeneralHelper.GetContent("C333", parameter.Language).Result, GeneralHelper.GetCaption("C264", parameter.Language).Result),
                   GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnMuteVoice), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Voice konnte nicht entstummt werden", exceptionMessage: "Voice bereits gestummt");
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
                var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        permissions.Add(perm);
                        continue;
                    }
                    var value = createTempChannel.GetPermissionOverwrite(parameter.Guild.GetRole(perm.TargetId)).GetValueOrDefault();
                    var modifiedPerm = perm.Permissions.Modify(speak: PermValue.Inherit);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    var value = createTempChannel.GetPermissionOverwrite(everyoneRole).GetValueOrDefault();
                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(speak: PermValue.Inherit)));
                }

                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.VoiceChannel.Id, GlobalStrings.mutevoice);

                foreach (var user in parameter.GuildUser.VoiceChannel.ConnectedUsers)
                {
                    await user.ModifyAsync(u =>
                    {
                        u.Channel = voiceChannel;
                    });
                }

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C334", parameter.Language).Result, GeneralHelper.GetCaption("C264", parameter.Language).Result),
                    GeneralHelper.GetCaption("C236", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnMuteVoice), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp voice unmute successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnMuteVoice), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Voice konnte nicht entstummt werden", exceptionMessage: ex.Message);
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task TempUnMuteChat(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnMuteChat), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnMuteChat), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnMuteChat), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.privacy, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mutechat, parameter.GuildUser.VoiceChannel.Id).Result == null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                   String.Format(GeneralHelper.GetContent("C322", parameter.Language).Result, GeneralHelper.GetCaption("C264", parameter.Language).Result),
                   GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnMuteChat), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Chat konnte nicht entstummt werden", exceptionMessage: "Chat bereits gestummt");
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
                var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        permissions.Add(perm);
                        continue;
                    }
                    var value = createTempChannel.GetPermissionOverwrite(parameter.Guild.GetRole(perm.TargetId)).GetValueOrDefault();
                    var modifiedPerm = perm.Permissions.Modify(sendMessages: PermValue.Inherit);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    var value = createTempChannel.GetPermissionOverwrite(everyoneRole).GetValueOrDefault();
                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(sendMessages: PermValue.Inherit)));
                }

                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.VoiceChannel.Id, GlobalStrings.mutechat);


                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C323", parameter.Language).Result, GeneralHelper.GetCaption("C264", parameter.Language).Result),
                    GeneralHelper.GetCaption("C236", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnMuteChat), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp chat unmute successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnMuteChat), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Chat konnte nicht entstummt werden", exceptionMessage: ex.Message);
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task TempLock(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.privacy, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                msg.Embeds = new Embed[] {
                    GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C291", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result
                    ).Result
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Cant use lock while whitelist is active", exceptionMessage: "No lock on whitelist");
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C266", parameter.Language).Result, GeneralHelper.GetCaption("C249", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to lock temp-channel", exceptionMessage: "Already locked");
                return;
            }


            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));
                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        continue;
                    }

                    var modifiedPerm = perm.Permissions.Modify(connect: PermValue.Deny);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }
                var voiceChannel = parameter.GuildUser.VoiceChannel;
                foreach (var user in voiceChannel.ConnectedUsers)
                {
                    var permissionOverride = voiceChannel.GetPermissionOverwrite(user);
                    if (!permissionOverride.HasValue)
                    {
                        permissionOverride = new OverwritePermissions().Modify(connect: PermValue.Allow);
                    }
                    else
                    {
                        permissionOverride = permissionOverride.Value.Modify(connect: PermValue.Allow);
                    }

                    permissions.Add(new Overwrite(user.Id, PermissionTarget.User, permissionOverride.Value));
                }

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Deny)));
                }

                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                _ = UsedFunctionsHelper.AddUsedFunction(tempChannelEntity.channelownerid.Value, 0, GlobalStrings.LockKlein, voiceChannel.Id, parameter.GuildID);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C130", parameter.Language).Result,
                    GeneralHelper.GetCaption("C130", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/templock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to lock temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C131", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task TempUnLock(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnLock), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnLock), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnLock), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.privacy, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                msg.Embeds = new Embed[] {
                    GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C291", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result
                    ).Result
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Cant use lock while whitelist is active", exceptionMessage: "No lock on whitelist");
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, parameter.GuildUser.VoiceChannel.Id).Result == null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                   String.Format(GeneralHelper.GetContent("C266", parameter.Language).Result, GeneralHelper.GetCaption("C250", parameter.Language).Result),
                   GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to lock temp-channel", exceptionMessage: "Already unlocked locked");
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
                var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        permissions.Add(perm);
                        continue;
                    }
                    var value = createTempChannel.GetPermissionOverwrite(parameter.Guild.GetRole(perm.TargetId)).GetValueOrDefault();
                    var modifiedPerm = perm.Permissions.Modify(connect: value.Connect);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }
                var voiceChannel = parameter.GuildUser.VoiceChannel;
                foreach (var user in voiceChannel.ConnectedUsers)
                {
                    var permissionOverride = voiceChannel.GetPermissionOverwrite(user);
                    if (!permissionOverride.HasValue)
                    {
                        continue;
                    }

                    permissionOverride = permissionOverride.Value.Modify(connect: PermValue.Inherit);

                    permissions.Add(new Overwrite(user.Id, PermissionTarget.User, permissionOverride.Value));
                }

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    var value = createTempChannel.GetPermissionOverwrite(everyoneRole).GetValueOrDefault();
                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: value.Connect)));
                }

                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.VoiceChannel.Id, GlobalStrings.LockKlein);


                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C132", parameter.Language).Result,
                    GeneralHelper.GetCaption("C132", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunlock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unlock temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C133", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task TempHide(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempHide), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempHide), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempHide), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.privacy, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.hide, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C266", parameter.Language).Result, GeneralHelper.GetCaption("C251", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to hide temp-channel", exceptionMessage: "Already hidden");
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));
                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        continue;
                    }

                    var modifiedPerm = perm.Permissions.Modify(viewChannel: PermValue.Deny);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
                }

                var voiceChannel = parameter.GuildUser.VoiceChannel;
                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                _ = UsedFunctionsHelper.AddUsedFunction(tempChannelEntity.channelownerid.Value, 0, GlobalStrings.hide, parameter.GuildUser.VoiceChannel.Id, parameter.GuildID);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C164", parameter.Language).Result,
                    GeneralHelper.GetCaption("C158", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temphide successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to hide temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C165", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task TempChannelSetup(SlashCommandParameter parameter) 
        {
            parameter.Interaction.DeferAsync();
            if (CheckDatas.CheckUserPermission(parameter, nameof(TempChannelSetup)).Result)
            {
                return;
            }

            try
            {
                var category = parameter.Guild.CreateCategoryChannelAsync("TEMPORARY VOICE CATEGORY").Result;
                var textChannel = parameter.Guild.CreateTextChannelAsync("Interface",
                    prop =>
                    {
                        prop.CategoryId = category.Id;
                    })
                    .Result;

                var voiceChannel = parameter.Guild.CreateVoiceChannelAsync("Join To Create Channel",
                    prop =>
                    {
                        prop.CategoryId = category.Id;
                    })
                    .Result;

                await CreateTempChannelsHelper.AddCC(parameter.GuildID, "{username}'s voice", voiceChannel.Id, 0, null, null);

                await WriteInterfaceInVoiceChannel(textChannel, parameter.Client, voiceChannel.Id);


                await parameter.Interaction.FollowupAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C336", parameter.Language).Result, voiceChannel.Id),
                    GeneralHelper.GetCaption("C236", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempClaimOwner), parameter,
                    message: "/temp setup - successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.FollowupAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.FollowupAsync(embeds: new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result });
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempClaimOwner), parameter,
                    message: "/temp setup - not successfully used");
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
                var user = parameter.Guild.GetUser(ownerId);
                await TempChannelsHelper.ChangeOwner(parameter.GuildUser.VoiceChannel.Id, parameter.GuildUser.Id);

                var permissions = voiceChannel.PermissionOverwrites.ToList();

                permissions = RemoveManageChannelRightsToUserVc(user, permissions, voiceChannel).Result;
                permissions = GiveManageChannelRightsToUserVc(parameter.GuildUser, parameter.GuildID, permissions, null, voiceChannel).Result;

                if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
                {
                    permissions = EditWhitelistedUsers(PermValue.Inherit, new SlashCommandParameter() { Guild = parameter.Guild, GuildID = parameter.GuildID, GuildUser = user }, permissions, voiceChannel).Result;
                    _ = KickUsersWhoAreNoLongerOnWhiteList(voiceChannel, parameter.GuildUser.Id);
                    permissions = EditWhitelistedUsers(PermValue.Allow, parameter, permissions, voiceChannel).Result;
                }

                permissions = UnblockAllUsersFromPreviousOwner(user, permissions, voiceChannel).Result;
                permissions = BlockAllUserFromOwner(parameter.GuildUser, parameter.Client, permissions, null, voiceChannel).Result;
                permissions = UnmuteIfNewOwnerAndMuted(parameter, permissions).Result;

                await voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

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

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnHide), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnHide), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnHide), true).Result)
            {
                return;
            }


            var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.privacy, tempChannel.createchannelid.Value, true).Result)
            {
                return;
            }

            var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.hide, parameter.GuildUser.VoiceChannel.Id).Result == null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C266", parameter.Language).Result, GeneralHelper.GetCaption("C252", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unhide temp-channel", exceptionMessage: "Already unhidden");
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    var value = createTempChannel.GetPermissionOverwrite(parameter.Guild.GetRole(perm.TargetId)).GetValueOrDefault();
                    var modifiedPerm = perm.Permissions.Modify(viewChannel: value.ViewChannel);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    var value = createTempChannel.GetPermissionOverwrite(everyoneRole).GetValueOrDefault();

                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: value.ViewChannel)));
                }

                var voiceChannel = parameter.GuildUser.VoiceChannel;
                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.VoiceChannel.Id, GlobalStrings.hide);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C166", parameter.Language).Result,
                    GeneralHelper.GetCaption("C166", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunhide successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unhide temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C167", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result };
                    msg.Components = null;
                });
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

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempSaveConfig), true).Result ||
            CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempSaveConfig), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.settings, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            var currentVC = parameter.GuildUser.VoiceState.Value.VoiceChannel;
            var tempChannel = TempChannelsHelper.GetTempChannel(currentVC.Id).Result;
            var createTempChannel = CreateTempChannelsHelper.GetCreateTempChannel(tempChannel.createchannelid.Value).Result;

            if (CheckDatas.UserTempChannelConfigExists(parameter).Result)
            {
                await TempChannelUserConfig.ChangeConfig(parameter.GuildID, parameter.GuildUser.Id, tempChannel.createchannelid.Value, currentVC.Name, currentVC.UserLimit.GetValueOrDefault(), createTempChannel.autodelete.GetValueOrDefault());
            }
            else
            {
                await TempChannelUserConfig.AddConfig(parameter.GuildID, parameter.GuildUser.Id, tempChannel.createchannelid.Value, currentVC.Name, currentVC.UserLimit.GetValueOrDefault(), createTempChannel.autodelete.GetValueOrDefault());
            }

            await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C178", parameter.Language).Result,
                             GeneralHelper.GetCaption("C178", parameter.Language).Result).Result };
                msg.Components = null;
            });

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempSaveConfig), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                message: "/temp saveconfig successfully used");
        }

        public static async Task TempDeleteConfig(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempDeleteConfig), true).Result ||
            CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempDeleteConfig), true).Result ||
            CheckDatas.CheckIfUserTempChannelConfigExists(parameter, nameof(TempDeleteConfig)).Result)
            {
                return;
            }

            var currentVC = parameter.GuildUser.VoiceState.Value.VoiceChannel;
            var tempChannel = TempChannelsHelper.GetTempChannel(currentVC.Id).Result;

            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.settings, tempChannel.createchannelid.Value, true).Result)
            {
                return;
            }

            await TempChannelUserConfig.DeleteConfig(parameter.GuildUser.Id, tempChannel.createchannelid.Value);

            await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C180", parameter.Language).Result,
                             GeneralHelper.GetCaption("C180", parameter.Language).Result).Result };
                msg.Components = null;
            });

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempDeleteConfig), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                 message: "/temp deleteconfig successfully used");
        }

        public static async Task TempModAdd(SlashCommandParameter parameter, List<string> userIds)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempModAdd), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempModAdd), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempModAdd), true, false).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "moderator", tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            var successfulAddedUsers = new List<SocketGuildUser>();
            var notSuccessfulAddedUsers = new Dictionary<SocketGuildUser, string>();

            foreach (var userId in userIds)
            {
                var usedGuild = parameter.Client.GetGuild(parameter.Guild.Id);

                var toBeAddedUser = usedGuild.GetUser(userId.ToUlong());
                var checkString = CheckDatas.CheckIfAlreadyModString(parameter, ulong.Parse(userId), 5).Result;
                if (checkString != "")
                {
                    notSuccessfulAddedUsers.Add(toBeAddedUser, checkString);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempModAdd), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to add user as moderator", exceptionMessage: checkString);
                    continue;
                }

                try
                {
                    _ = UsedFunctionsHelper.AddUsedFunction(parameter.GuildUser.Id, toBeAddedUser.Id, GlobalStrings.moderator, 0, parameter.GuildID);
                    successfulAddedUsers.Add(toBeAddedUser);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempModAdd), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "moderator successfully added");
                }
                catch (Exception ex)
                {
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempModAdd), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "failed to add moderator", exceptionMessage: ex.Message);

                    notSuccessfulAddedUsers.Add(toBeAddedUser, GeneralHelper.GetContent("C253", parameter.Language).Result);
                }
            }

            var stringBuilder = new StringBuilder();
            if (successfulAddedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C305", parameter.Language).Result}**");

                foreach (var user in successfulAddedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Id}>");
                }
            }

            if (notSuccessfulAddedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C306", parameter.Language).Result}**");

                foreach (var user in notSuccessfulAddedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Key.Id}>");
                    stringBuilder.AppendLine(user.Value);
                }
            }

            var caption = string.Empty;
            if (successfulAddedUsers.Count() > 0 && notSuccessfulAddedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulAddedUsers.Count() > 0 && notSuccessfulAddedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulAddedUsers.Count() == 0 && notSuccessfulAddedUsers.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }

            var parsedArg = (SocketMessageComponent)parameter.Interaction;
            await parsedArg.UpdateAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                msg.Components = null;
            });
        }

        public static async Task TempModRemove(SlashCommandParameter parameter, List<string> userIds)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempModRemove), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempModRemove), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempModRemove), true, false).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "moderator", tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            var successfulRemovedUsers = new List<SocketGuildUser>();
            var notSuccessfulRemovedUsers = new Dictionary<SocketGuildUser, string>();

            foreach (var userId in userIds)
            {
                var usedGuild = parameter.Client.GetGuild(parameter.Guild.Id);

                var toBeRemovedUser = usedGuild.GetUser(userId.ToUlong());
                var checkString = CheckDatas.CheckIfEvenModString(parameter, ulong.Parse(userId)).Result;
                if (checkString != "")
                {
                    notSuccessfulRemovedUsers.Add(toBeRemovedUser, checkString);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempModRemove), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to remove user as moderator", exceptionMessage: checkString);
                    continue;
                }

                try
                {
                    _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.Id, toBeRemovedUser.Id, GlobalStrings.moderator, parameter.GuildID);
                    successfulRemovedUsers.Add(toBeRemovedUser);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempModRemove), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "moderator successfully removed");
                }
                catch (Exception ex)
                {
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempModRemove), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "failed to remove moderator", exceptionMessage: ex.Message);

                    notSuccessfulRemovedUsers.Add(toBeRemovedUser, GeneralHelper.GetContent("C253", parameter.Language).Result);
                }
            }

            var stringBuilder = new StringBuilder();
            if (successfulRemovedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C308", parameter.Language).Result}**");

                foreach (var user in successfulRemovedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Id}>");
                }
            }

            if (notSuccessfulRemovedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C309", parameter.Language).Result}**");

                foreach (var user in notSuccessfulRemovedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Key.Id}>");
                    stringBuilder.AppendLine(user.Value);
                }
            }

            var caption = string.Empty;
            if (successfulRemovedUsers.Count() > 0 && notSuccessfulRemovedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulRemovedUsers.Count() > 0 && notSuccessfulRemovedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulRemovedUsers.Count() == 0 && notSuccessfulRemovedUsers.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }

            var parsedArg = (SocketMessageComponent)parameter.Interaction;
            await parsedArg.UpdateAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                msg.Components = null;
            });
        }

        public static async Task TempKick(SlashCommandParameter parameter, List<string> userIds, bool epherialMessage = false)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempKick), epherialMessage).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempKick), epherialMessage).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempKick), epherialMessage).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "kick", tempChannelEntity.createchannelid.Value, epherialMessage).Result)
            {
                return;
            }

            var successfulKickedUsers = new List<SocketGuildUser>();
            var notSuccessfulKickedUsers = new Dictionary<SocketGuildUser, string>();

            foreach (var userId in userIds)
            {
                var usedGuild = parameter.Client.GetGuild(parameter.Guild.Id);

                var toBeKickedUser = usedGuild.GetUser(userId.ToUlong());
                var checkString = CheckDatas.CheckIfUserInSameTempVoiceString(parameter, ulong.Parse(userId), "C244").Result;
                if (checkString != "")
                {
                    notSuccessfulKickedUsers.Add(toBeKickedUser, checkString);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempKick), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to kick temp-channel user", exceptionMessage: checkString);
                    continue;
                }


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

                    notSuccessfulKickedUsers.Add(toBeKickedUser, GeneralHelper.GetContent("C253", parameter.Language).Result);
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
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C235", parameter.Language).Result}**");

                foreach (var user in notSuccessfulKickedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Key.Id}>");
                    stringBuilder.AppendLine(user.Value);
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

        public static async Task TempChatMuteUser(SlashCommandParameter parameter, List<string> users)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempChatMuteUser), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempChatMuteUser), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempChatMuteUser), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.chat, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            var successfulMutedUsers = new List<ulong>();
            var notSuccessfulMutedUsers = new Dictionary<ulong, string>();

            var permissions = parameter.GuildUser.VoiceChannel.PermissionOverwrites.ToList();

            foreach (var userId in users)
            {
                try
                {
                    var voiceChannel = parameter.GuildUser.VoiceChannel;
                    var user = parameter.Guild.GetUser(ulong.Parse(userId));
                    var checkPermissionString = CheckDatas.CheckIfUserInSameTempVoiceString(parameter, ulong.Parse(userId), "C296").Result;
                    if (checkPermissionString != "")
                    {
                        notSuccessfulMutedUsers.Add(user.Id, checkPermissionString);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempChatMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to chat mute temp-channel user", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    if (checkPermissionString == "")
                    {
                        if (UserIsChatMuted(voiceChannel.Id, tempChannelEntity.channelownerid.Value, user.Id))
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C262", parameter.Language).Result);
                        }
                    }

                    if (checkPermissionString != "")
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), checkPermissionString);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempChatMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to chat mute user from temp-channel chat", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    permissions = EditPermissionSendMessage(PermValue.Deny, permissions, user, voiceChannel);

                    successfulMutedUsers.Add(ulong.Parse(userId));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Missing Permissions"))
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C274", parameter.Language).Result);
                    }
                    else
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C253", parameter.Language).Result);
                    }

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempChatMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to chat mute user from temp-channel chat", exceptionMessage: ex.Message);
                }
            }

            try
            {
                await parameter.GuildUser.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                var voiceChannel = parameter.GuildUser.VoiceChannel;
                foreach (var user in successfulMutedUsers)
                {
                    var guildUser = parameter.Guild.GetUser(user);
                    _ = guildUser.ModifyAsync(u => u.Channel = voiceChannel);
                    await UsedFunctionsHelper.AddUsedFunction(tempChannelEntity.channelownerid.Value, user, GlobalStrings.mutechatuser, voiceChannel.Id, parameter.GuildID);
                }
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempChatMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp chat mute user successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempChatMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to mute user from temp-channel", exceptionMessage: ex.Message);

                return;
            }

            var stringBuilder = new StringBuilder();
            if (successfulMutedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C324", parameter.Language).Result}**");

                foreach (var user in successfulMutedUsers)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            if (notSuccessfulMutedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C325", parameter.Language).Result}**");

                foreach (var user in notSuccessfulMutedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Key}>");
                    stringBuilder.AppendLine($"{user.Value}");
                }
            }

            var caption = string.Empty;
            if (successfulMutedUsers.Count() > 0 && notSuccessfulMutedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulMutedUsers.Count() > 0 && notSuccessfulMutedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulMutedUsers.Count() == 0 && notSuccessfulMutedUsers.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }

            var parsedArg = (SocketMessageComponent)parameter.Interaction;
            await parsedArg.UpdateAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                msg.Components = null;
            });
        }

        public static async Task TempChatUnMuteUser(SlashCommandParameter parameter, List<string> users)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempChatUnMuteUser), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempChatUnMuteUser), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempChatUnMuteUser), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.chat, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            var successfulMutedUsers = new List<ulong>();
            var notSuccessfulMutedUsers = new Dictionary<ulong, string>();

            var permissions = parameter.GuildUser.VoiceChannel.PermissionOverwrites.ToList();

            foreach (var userId in users)
            {
                try
                {
                    var voiceChannel = parameter.GuildUser.VoiceChannel;
                    var user = parameter.Guild.GetUser(ulong.Parse(userId));
                    var checkPermissionString = CheckDatas.CheckIfUserInSameTempVoiceString(parameter, ulong.Parse(userId), "C297").Result;
                    if (checkPermissionString != "")
                    {
                        notSuccessfulMutedUsers.Add(user.Id, checkPermissionString);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempChatUnMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to unmute temp-channel user in chat", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    if (checkPermissionString == "")
                    {
                        if (!UserIsChatMuted(voiceChannel.Id, tempChannelEntity.channelownerid.Value, user.Id))
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C264", parameter.Language).Result);
                        }
                    }

                    if (checkPermissionString != "")
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), checkPermissionString);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempChatUnMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to unmute user from temp-channel chat", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    permissions = EditPermissionSendMessage(PermValue.Inherit, permissions, user, voiceChannel);

                    successfulMutedUsers.Add(ulong.Parse(userId));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Missing Permissions"))
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C274", parameter.Language).Result);
                    }
                    else
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C253", parameter.Language).Result);
                    }

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempChatUnMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed toun mute user from temp-channel", exceptionMessage: ex.Message);
                }
            }

            try
            {
                await parameter.GuildUser.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                var voiceChannel = parameter.GuildUser.VoiceChannel;
                foreach (var user in successfulMutedUsers)
                {
                    var guildUser = parameter.Guild.GetUser(user);
                    _ = guildUser.ModifyAsync(u => u.Channel = voiceChannel);
                    _ = guildUser.ModifyAsync(u => u.Mute = false);

                    await UsedFunctionsHelper.RemoveUsedFunction(voiceChannel.Id, GlobalStrings.mutechatuser, user);
                }
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempChatUnMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp chat unmute user successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempChatUnMuteUser), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to mute user from temp-channel", exceptionMessage: ex.Message);
                return;
            }

            var stringBuilder = new StringBuilder();
            if (successfulMutedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C326", parameter.Language).Result}**");

                foreach (var user in successfulMutedUsers)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            if (notSuccessfulMutedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C327", parameter.Language).Result}**");

                foreach (var user in notSuccessfulMutedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Key}>");
                    stringBuilder.AppendLine($"{user.Value}");
                }
            }

            var caption = string.Empty;
            if (successfulMutedUsers.Count() > 0 && notSuccessfulMutedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulMutedUsers.Count() > 0 && notSuccessfulMutedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulMutedUsers.Count() == 0 && notSuccessfulMutedUsers.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }

            var parsedArg = (SocketMessageComponent)parameter.Interaction;
            await parsedArg.UpdateAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                msg.Components = null;
            });
        }

        public static async Task TempMute(SlashCommandParameter parameter, List<string> users, bool epherialMessage = false)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempMute), epherialMessage).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempMute), epherialMessage).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempMute), epherialMessage).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "mute", tempChannelEntity.createchannelid.Value, epherialMessage).Result)
            {
                return;
            }

            var successfulMutedUsers = new List<ulong>();
            var notSuccessfulMutedUsers = new Dictionary<ulong, string>();

            var permissions = parameter.GuildUser.VoiceChannel.PermissionOverwrites.ToList();

            foreach (var userId in users)
            {
                try
                {
                    var voiceChannel = parameter.GuildUser.VoiceChannel;
                    var user = parameter.Guild.GetUser(ulong.Parse(userId));
                    var checkPermissionString = CheckDatas.CheckIfUserInSameTempVoiceString(parameter, ulong.Parse(userId), "C261").Result;
                    if (checkPermissionString != "")
                    {
                        notSuccessfulMutedUsers.Add(user.Id, checkPermissionString);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to mute temp-channel user", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    if (checkPermissionString == "")
                    {
                        if (UserIsMuted(voiceChannel.Id, tempChannelEntity.channelownerid.Value, user.Id))
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C262", parameter.Language).Result);
                        }
                    }

                    if (checkPermissionString != "")
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), checkPermissionString);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to mute user from temp-channel", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    permissions = EditPermissionSpeak(PermValue.Deny, permissions, user, voiceChannel);

                    successfulMutedUsers.Add(ulong.Parse(userId));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Missing Permissions"))
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C274", parameter.Language).Result);
                    }
                    else
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C253", parameter.Language).Result);
                    }

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to mute user from temp-channel", exceptionMessage: ex.Message);
                }
            }

            try
            {
                await parameter.GuildUser.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                var voiceChannel = parameter.GuildUser.VoiceChannel;
                foreach (var user in successfulMutedUsers)
                {
                    var guildUser = parameter.Guild.GetUser(user);
                    _ = guildUser.ModifyAsync(u => u.Channel = voiceChannel);
                    await UsedFunctionsHelper.AddUsedFunction(tempChannelEntity.channelownerid.Value, user, GlobalStrings.mute, voiceChannel.Id, parameter.GuildID);
                }
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp mute successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to mute user from temp-channel", exceptionMessage: ex.Message);

                return;
            }

            var stringBuilder = new StringBuilder();
            if (successfulMutedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C272", parameter.Language).Result}**");

                foreach (var user in successfulMutedUsers)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            if (notSuccessfulMutedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C273", parameter.Language).Result}**");

                foreach (var user in notSuccessfulMutedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Key}>");
                    stringBuilder.AppendLine($"{user.Value}");
                }
            }

            var caption = string.Empty;
            if (successfulMutedUsers.Count() > 0 && notSuccessfulMutedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulMutedUsers.Count() > 0 && notSuccessfulMutedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulMutedUsers.Count() == 0 && notSuccessfulMutedUsers.Count > 0)
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

        public static async Task TempUnMute(SlashCommandParameter parameter, List<string> users, bool epherialMessage = false)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnMute), epherialMessage).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnMute), epherialMessage).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnMute), epherialMessage).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "mute", tempChannelEntity.createchannelid.Value, epherialMessage).Result)
            {
                return;
            }

            var successfulMutedUsers = new List<ulong>();
            var notSuccessfulMutedUsers = new Dictionary<ulong, string>();

            var permissions = parameter.GuildUser.VoiceChannel.PermissionOverwrites.ToList();

            foreach (var userId in users)
            {
                try
                {
                    var voiceChannel = parameter.GuildUser.VoiceChannel;
                    var user = parameter.Guild.GetUser(ulong.Parse(userId));
                    var checkPermissionString = CheckDatas.CheckIfUserInSameTempVoiceString(parameter, ulong.Parse(userId), "C263").Result;
                    if (checkPermissionString != "")
                    {
                        notSuccessfulMutedUsers.Add(user.Id, checkPermissionString);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to unmute temp-channel user", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    if (checkPermissionString == "")
                    {
                        if (!UserIsMuted(voiceChannel.Id, tempChannelEntity.channelownerid.Value, user.Id))
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C264", parameter.Language).Result);
                        }
                    }

                    if (checkPermissionString != "")
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), checkPermissionString);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to unmute user from temp-channel", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    permissions = EditPermissionSpeak(PermValue.Inherit, permissions, user, voiceChannel);

                    successfulMutedUsers.Add(ulong.Parse(userId));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Missing Permissions"))
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C274", parameter.Language).Result);
                    }
                    else
                    {
                        notSuccessfulMutedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C253", parameter.Language).Result);
                    }

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed toun mute user from temp-channel", exceptionMessage: ex.Message);
                }
            }

            try
            {
                await parameter.GuildUser.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                var voiceChannel = parameter.GuildUser.VoiceChannel;
                foreach (var user in successfulMutedUsers)
                {
                    var guildUser = parameter.Guild.GetUser(user);
                    _ = guildUser.ModifyAsync(u =>
                    {
                        u.Channel = voiceChannel;
                    });

                    await UsedFunctionsHelper.RemoveUsedFunction(voiceChannel.Id, GlobalStrings.mute, user);
                }
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp unmute successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempMute), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to mute user from temp-channel", exceptionMessage: ex.Message);
                return;
            }

            var stringBuilder = new StringBuilder();
            if (successfulMutedUsers.Count() > 0)
            {
                stringBuilder.AppendLine($"**{String.Format(GeneralHelper.GetContent("C276", parameter.Language).Result, GeneralHelper.GetCaption("C264", parameter.Language).Result)}**");

                foreach (var user in successfulMutedUsers)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            if (notSuccessfulMutedUsers.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{String.Format(GeneralHelper.GetContent("C275", parameter.Language).Result, GeneralHelper.GetCaption("C264", parameter.Language).Result)}**");

                foreach (var user in notSuccessfulMutedUsers)
                {
                    stringBuilder.AppendLine($"<@{user.Key}>");
                    stringBuilder.AppendLine($"{user.Value}");
                }
            }

            var caption = string.Empty;
            if (successfulMutedUsers.Count() > 0 && notSuccessfulMutedUsers.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulMutedUsers.Count() > 0 && notSuccessfulMutedUsers.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulMutedUsers.Count() == 0 && notSuccessfulMutedUsers.Count > 0)
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

        public static bool UserIsChatMuted(ulong voiceChannelId, ulong userId, ulong affectedUserId)
        {
            return UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mutechatuser, voiceChannelId, userId, affectedUserId).Result != null;
        }

        public static bool UserIsMuted(ulong voiceChannelId, ulong userId, ulong affectedUserId)
        {
            return UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mute, voiceChannelId, userId, affectedUserId).Result != null;
        }

        public static List<Overwrite> EditPermissionManageMessages(PermValue connect, List<Overwrite> overwrites, SocketGuildUser user, SocketVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(manageMessages: connect)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(manageMessages: connect));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionManageMessages(PermValue connect, List<Overwrite> overwrites, SocketGuildUser user, RestVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(manageMessages: connect)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(manageMessages: connect));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionSendMessage(PermValue connect, List<Overwrite> overwrites, SocketGuildUser user, RestVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(sendMessages: connect)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(sendMessages: connect));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionSendMessage(PermValue connect, List<Overwrite> overwrites, SocketGuildUser user, SocketVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(sendMessages: connect)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(sendMessages: connect));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionConnect(PermValue connect, List<Overwrite> overwrites, SocketGuildUser user, SocketVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(connect: connect)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(connect: connect));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionConnect(PermValue connect, List<Overwrite> overwrites, SocketRole role, SocketVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == role.Id);

            if (overwrite.TargetId != role.Id)
            {
                overwrites.Add(new Overwrite(role.Id, PermissionTarget.Role, new OverwritePermissions().Modify(connect: connect)));
            }
            else
            {
                overwrite = new Overwrite(role.Id, PermissionTarget.Role, overwrite.Permissions.Modify(connect: connect));
                int index = overwrites.FindIndex(o => o.TargetId == role.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionManageChannel(PermValue manageChannel, List<Overwrite> overwrites, SocketGuildUser user, SocketVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(manageChannel: manageChannel)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(manageChannel: manageChannel));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionManageChannel(PermValue manageChannel, List<Overwrite> overwrites, SocketGuildUser user, RestVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(manageChannel: manageChannel)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(manageChannel: manageChannel));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionSpeak(PermValue useApplicationCommands, List<Overwrite> overwrites, SocketGuildUser user, IVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(speak: useApplicationCommands)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(speak: useApplicationCommands));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionSlashCommands(PermValue useApplicationCommands, List<Overwrite> overwrites, SocketGuildUser user, IVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(useApplicationCommands: useApplicationCommands)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(useApplicationCommands: useApplicationCommands));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionConnect(PermValue connect, List<Overwrite> overwrites, SocketGuildUser user, RestVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(connect: connect)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(connect: connect));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionViewChannel(PermValue viewChannel, List<Overwrite> overwrites, SocketGuildUser user, SocketVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(viewChannel: viewChannel)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(viewChannel: viewChannel));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionViewChannel(PermValue viewChannel, List<Overwrite> overwrites, SocketGuildUser user, RestVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(viewChannel: viewChannel)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(viewChannel: viewChannel));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static List<Overwrite> EditPermissionSpeak(PermValue speak, List<Overwrite> overwrites, SocketGuildUser user, SocketVoiceChannel voiceChannel)
        {
            var overwrite = overwrites.SingleOrDefault(p => p.TargetId == user.Id);

            if (overwrite.TargetId != user.Id)
            {
                overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions().Modify(speak: speak)));
            }
            else
            {
                overwrite = new Overwrite(user.Id, PermissionTarget.User, overwrite.Permissions.Modify(speak: speak));
                int index = overwrites.FindIndex(o => o.TargetId == user.Id);

                if (index != -1)
                    overwrites[index] = overwrite;
            }

            return overwrites;
        }

        public static async Task TempUnBlock(SlashCommandParameter parameter, List<string> users, bool epherialMessage = false)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

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

            var isModerator = CheckDatas.CheckIfUserIsModerator(parameter).Result;

            if (isModerator && CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.moderator, tempChannelEntity.createchannelid.Value, epherialMessage).Result)
            {
                return;
            }


            var successfulBlockedUsers = new List<ulong>();
            var notSuccessfulBlockedUsers = new Dictionary<ulong, string>();
            var permissions = parameter.GuildUser.VoiceChannel.PermissionOverwrites.ToList();
            var voiceChannel = parameter.GuildUser.VoiceChannel;

            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(parameter.Guild.Id, tempChannelEntity.createchannelid.Value).Result;
            foreach (var userId in users)
            {
                try
                {
                    var user = parameter.Guild.GetUser(ulong.Parse(userId));
                    var checkPermissionString = CheckDatas.CheckPermissionsString(parameter, ulong.Parse(userId), "C246", false).Result;
                    if (checkPermissionString == "")
                    {
                        if (UsedFunctionsHelper.GetUsedFunction(tempChannelEntity.channelownerid.Value, ulong.Parse(userId), GlobalStrings.block, parameter.GuildID).Result == null)
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C247", parameter.Language).Result);
                        }
                    }
                    if (checkPermissionString == "" &&
                            isModerator &&
                            UsedFunctionsHelper.GetBlockedUserFunction(parameter.GuildID, tempChannelEntity.channelownerid.Value, user.Id).Result.channelid == 0)
                    {
                        checkPermissionString = GeneralHelper.GetContent("C331", parameter.Language).Result;
                    }


                    if (checkPermissionString != "")
                    {
                        notSuccessfulBlockedUsers.Add(ulong.Parse(userId), checkPermissionString);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to unblock user from temp-channel", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    permissions = EditPermissionConnect(PermValue.Inherit, permissions, user, voiceChannel);

                    if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.hidevoicefromblockedusers) == null)
                    {
                        permissions = EditPermissionViewChannel(PermValue.Inherit, permissions, user, voiceChannel);
                    }

                    successfulBlockedUsers.Add(ulong.Parse(userId));
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempunblock successfully used");
                }
                catch (Exception ex)
                {
                    notSuccessfulBlockedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C253", parameter.Language).Result);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to unblock user from temp-channel", exceptionMessage: ex.Message);
                }
            }

            try
            {
                await parameter.GuildUser.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                foreach (var user in successfulBlockedUsers)
                {
                    _ = UsedFunctionsHelper.RemoveUsedFunction(tempChannelEntity.channelownerid.Value, user, GlobalStrings.block, parameter.GuildID);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunblock successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unblock user from temp-channel", exceptionMessage: ex.Message);
                return;
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
                    stringBuilder.AppendLine($"<@{user.Key}>");
                    stringBuilder.AppendLine($"{user.Value}");
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

            var isModerator = CheckDatas.CheckIfUserIsModerator(parameter).Result;

            if (isModerator && CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.moderator, tempChannelEntity.createchannelid.Value, epherialMessage).Result)
            {
                return;
            }

            var successfulBlockedUsers = new List<ulong>();
            var notSuccessfulBlockedUsers = new Dictionary<ulong, string>();
            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(parameter.Guild.Id, tempChannelEntity.createchannelid.Value).Result;

            var hideVoie = disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.hidevoicefromblockedusers) == null;
            var permissions = parameter.GuildUser.VoiceChannel.PermissionOverwrites.ToList();
            var voiceChannel = parameter.GuildUser.VoiceChannel;
            foreach (var userId in userIds)
            {
                try
                {
                    var userToBeBlocked = parameter.Guild.GetUser(ulong.Parse(userId));
                    var checkPermissionString = CheckDatas.CheckPermissionsString(parameter, ulong.Parse(userId), "C245").Result;

                    if (checkPermissionString == "")
                    {
                        if (UsedFunctionsHelper.GetUsedFunction(tempChannelEntity.channelownerid.Value, userToBeBlocked.Id, GlobalStrings.block, parameter.GuildID).Result != null)
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C248", parameter.Language).Result);
                            if (voiceChannel.ConnectedUsers.Contains(userToBeBlocked))
                            {
                                await userToBeBlocked.ModifyAsync(channel => channel.Channel = null);
                            }
                        }
                    }

                    if (checkPermissionString != "")
                    {
                        notSuccessfulBlockedUsers.Add(ulong.Parse(userId), checkPermissionString);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to block user from temp-channel", exceptionMessage: checkPermissionString);
                        continue;
                    }

                    permissions = EditPermissionConnect(PermValue.Deny, permissions, userToBeBlocked, voiceChannel);

                    if (hideVoie)
                    {
                        permissions = EditPermissionViewChannel(PermValue.Deny, permissions, userToBeBlocked, voiceChannel);
                    }

                    if (voiceChannel.ConnectedUsers.Contains(userToBeBlocked))
                    {
                        await userToBeBlocked.ModifyAsync(channel => channel.Channel = null);
                    }


                    successfulBlockedUsers.Add(ulong.Parse(userId));
                }
                catch (Exception ex)
                {
                    notSuccessfulBlockedUsers.Add(ulong.Parse(userId), GeneralHelper.GetContent("C253", parameter.Language).Result);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to block user from temp-channel", exceptionMessage: ex.Message);
                }
            }

            try
            {
                await parameter.GuildUser.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                foreach (var user in successfulBlockedUsers)
                {
                    if (UsedFunctionsHelper.GetWhitelistUsedFunction(parameter.GuildUser.Id, user, parameter.GuildID).Result != null)
                    {
                        _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.Id, user, GlobalStrings.whitelist, parameter.GuildID);
                    }

                    if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, user, GlobalStrings.moderator, parameter.GuildID).Result != null)
                    {
                        _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.Id, user, GlobalStrings.moderator, parameter.GuildID);
                    }

                    ulong channelId = 0;
                    if (isModerator)
                    {
                        channelId = voiceChannel.Id;
                    }
                    _ = UsedFunctionsHelper.AddUsedFunction(tempChannelEntity.channelownerid.Value, user, GlobalStrings.block, channelId, parameter.GuildID);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempblock successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to block user from temp-channel", exceptionMessage: ex.Message);
                return;
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
                    stringBuilder.AppendLine($"<@{user.Key}>");
                    stringBuilder.AppendLine($"{user.Value}");
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

        public static async Task TempWhiteListAdd(SlashCommandParameter parameter, List<string> userIds)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempWhiteListAdd), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempWhiteListAdd), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempWhiteListAdd), true, false).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "whitelist", tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            var successfulAddedMentions = new List<string>();
            var notSuccessfulAddedMentions = new Dictionary<string, string>();

            var permissions = parameter.GuildUser.VoiceChannel.PermissionOverwrites.ToList();
            var voiceChannel = parameter.GuildUser.VoiceChannel;
            foreach (var id in userIds)
            {
                var userToBeAdded = parameter.Guild.GetUser(ulong.Parse(id));
                try
                {
                    var checkPermissionString = String.Empty;
                    if (userToBeAdded == null)
                    {
                        var roleToBeAdded = parameter.Guild.GetRole(ulong.Parse(id));

                        if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, roleToBeAdded.Id, GlobalStrings.whitelist, parameter.GuildID).Result != null)
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C270", parameter.Language).Result);
                        }

                        if (checkPermissionString != "")
                        {
                            notSuccessfulAddedMentions.Add($"&{id}", checkPermissionString);
                            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempWhiteListAdd), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                                message: "Failed to add role to whiteliste of temp-channel", exceptionMessage: checkPermissionString);
                            continue;
                        }

                        if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
                        {
                            permissions = EditPermissionConnect(PermValue.Allow, permissions, roleToBeAdded, voiceChannel);
                        }
                        successfulAddedMentions.Add($"&{id}");
                    }
                    else
                    {
                        if (userToBeAdded.Id == parameter.GuildUser.Id)
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C256", parameter.Language).Result, GeneralHelper.GetCaption("C271", parameter.Language).Result);
                        }

                        if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, userToBeAdded.Id, GlobalStrings.block, parameter.GuildID).Result != null)
                        {
                            checkPermissionString = GeneralHelper.GetContent("C292", parameter.Language).Result;
                        }

                        if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, userToBeAdded.Id, GlobalStrings.whitelist, parameter.GuildID).Result != null)
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C270", parameter.Language).Result);
                        }

                        if (checkPermissionString != "")
                        {
                            notSuccessfulAddedMentions.Add(id, checkPermissionString);
                            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempWhiteListAdd), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                                message: "Failed to add role to whiteliste of temp-channel", exceptionMessage: checkPermissionString);
                            continue;
                        }

                        if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
                        {
                            permissions = EditPermissionConnect(PermValue.Allow, permissions, userToBeAdded, voiceChannel);
                        }
                        successfulAddedMentions.Add(id);
                    }
                }
                catch (Exception ex)
                {
                    var idMitUnd = id;
                    if (userToBeAdded == null)
                    {
                        idMitUnd = $"&{id}";
                    }
                    notSuccessfulAddedMentions.Add(idMitUnd, GeneralHelper.GetContent("C253", parameter.Language).Result);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to add mentionable to whitelist of temp-channel", exceptionMessage: ex.Message);
                }
            }

            try
            {
                if (voiceChannel.PermissionOverwrites != permissions)
                {
                    await parameter.GuildUser.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                }

                foreach (var user in successfulAddedMentions)
                {
                    _ = UsedFunctionsHelper.AddUsedFunction(parameter.GuildUser.Id, ulong.Parse(user.Replace("&", "")), GlobalStrings.whitelist, 0, parameter.GuildID, !user.Contains("&"));
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempWhiteListAdd), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp whitelist add successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempWhiteListAdd), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to add mentionable to whitelist of temp-channel", exceptionMessage: ex.Message);
                return;
            }

            var stringBuilder = new StringBuilder();
            if (successfulAddedMentions.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C281", parameter.Language).Result}**");

                foreach (var user in successfulAddedMentions)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            if (notSuccessfulAddedMentions.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C282", parameter.Language).Result}**");

                foreach (var user in notSuccessfulAddedMentions)
                {
                    stringBuilder.AppendLine($"<@{user.Key}>");
                    stringBuilder.AppendLine($"{user.Value}");
                }
            }

            var caption = string.Empty;
            if (successfulAddedMentions.Count() > 0 && notSuccessfulAddedMentions.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulAddedMentions.Count() > 0 && notSuccessfulAddedMentions.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulAddedMentions.Count() == 0 && notSuccessfulAddedMentions.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }
            var parsedArg = (SocketMessageComponent)parameter.Interaction;
            await parsedArg.UpdateAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                msg.Components = null;
            });
        }


        public static async Task TempWhiteListRemove(SlashCommandParameter parameter, List<string> userIds)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempWhiteListRemove), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempWhiteListRemove), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempWhiteListRemove), true, false).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "whitelist", tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            var successfulRemovedMentions = new List<string>();
            var notSuccessfulRemovedMentions = new Dictionary<string, string>();

            var permissions = parameter.GuildUser.VoiceChannel.PermissionOverwrites.ToList();
            var voiceChannel = parameter.GuildUser.VoiceChannel;
            foreach (var id in userIds)
            {
                var userToBeAdded = parameter.Guild.GetUser(ulong.Parse(id));
                try
                {
                    var checkPermissionString = String.Empty;
                    if (userToBeAdded == null)
                    {
                        var roleToBeAdded = parameter.Guild.GetRole(ulong.Parse(id));

                        if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, roleToBeAdded.Id, GlobalStrings.whitelist, parameter.GuildID).Result == null)
                        {
                            checkPermissionString = GeneralHelper.GetContent("C284", parameter.Language).Result;
                        }

                        if (checkPermissionString != "")
                        {
                            notSuccessfulRemovedMentions.Add($"&{id}", checkPermissionString);
                            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempWhiteListRemove), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                                message: "Failed to remove role to whiteliste of temp-channel", exceptionMessage: checkPermissionString);
                            continue;
                        }

                        if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
                        {
                            permissions = EditPermissionConnect(PermValue.Inherit, permissions, roleToBeAdded, voiceChannel);
                        }
                        successfulRemovedMentions.Add($"&{id}");
                    }
                    else
                    {
                        if (userToBeAdded.Id == parameter.GuildUser.Id)
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C256", parameter.Language).Result, GeneralHelper.GetCaption("C271", parameter.Language).Result);
                        }

                        if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, userToBeAdded.Id, GlobalStrings.whitelist, parameter.GuildID).Result == null)
                        {
                            checkPermissionString = GeneralHelper.GetContent("C284", parameter.Language).Result;
                        }

                        if (checkPermissionString != "")
                        {
                            notSuccessfulRemovedMentions.Add(id, checkPermissionString);
                            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempWhiteListRemove), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                                message: "Failed to add role to whiteliste of temp-channel", exceptionMessage: checkPermissionString);
                            continue;
                        }

                        if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
                        {
                            permissions = EditPermissionConnect(PermValue.Inherit, permissions, userToBeAdded, voiceChannel);
                        }
                        successfulRemovedMentions.Add(id);
                    }
                }
                catch (Exception ex)
                {
                    var idMitUnd = id;
                    if (userToBeAdded == null)
                    {
                        idMitUnd = $"&{id}";
                    }
                    notSuccessfulRemovedMentions.Add(idMitUnd, GeneralHelper.GetContent("C253", parameter.Language).Result);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempWhiteListRemove), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to add mentionable to whitelist of temp-channel", exceptionMessage: ex.Message);
                }
            }

            try
            {
                if (voiceChannel.PermissionOverwrites != permissions)
                {
                    await parameter.GuildUser.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                }

                foreach (var user in successfulRemovedMentions)
                {
                    _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.Id, ulong.Parse(user.Replace("&", "")), GlobalStrings.whitelist, parameter.GuildID);
                }

                if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
                {
                    _ = KickUsersWhoAreNoLongerOnWhiteList(voiceChannel, tempChannelEntity.channelownerid.Value);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempWhiteListRemove), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp whitelist remove successfully used");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result },
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempWhiteListRemove), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to remove mentionable from whitelist of temp-channel", exceptionMessage: ex.Message);
                return;
            }

            var stringBuilder = new StringBuilder();
            if (successfulRemovedMentions.Count() > 0)
            {
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C285", parameter.Language).Result}**");

                foreach (var user in successfulRemovedMentions)
                {
                    stringBuilder.AppendLine($"<@{user}>");
                }
            }

            if (notSuccessfulRemovedMentions.Count() > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"**{GeneralHelper.GetContent("C286", parameter.Language).Result}**");

                foreach (var user in notSuccessfulRemovedMentions)
                {
                    stringBuilder.AppendLine($"<@{user.Key}>");
                    stringBuilder.AppendLine($"{user.Value}");
                }
            }

            var caption = string.Empty;
            if (successfulRemovedMentions.Count() > 0 && notSuccessfulRemovedMentions.Count() > 0)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (successfulRemovedMentions.Count() > 0 && notSuccessfulRemovedMentions.Count == 0)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (successfulRemovedMentions.Count() == 0 && notSuccessfulRemovedMentions.Count > 0)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }
            var parsedArg = (SocketMessageComponent)parameter.Interaction;
            await parsedArg.UpdateAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            stringBuilder.ToString(),
                            caption).Result  };
                msg.Components = null;
            });
        }

        public static async Task ActivateWhiteList(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(ActivateWhiteList), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(ActivateWhiteList), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(ActivateWhiteList), true, false).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "whitelist", tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C288", parameter.Language).Result,
                            GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(ActivateWhiteList), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Whiteliste ist bereits aktiv", exceptionMessage: "Already active");
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));
                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                var voiceChannel = parameter.GuildUser.VoiceChannel;
                var whiteListedUsers = KickUsersWhoAreNoLongerOnWhiteList(voiceChannel, parameter.GuildUser.Id).Result;
                if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, parameter.GuildUser.VoiceChannel.Id).Result == null)
                {
                    foreach (var perm in perms)
                    {
                        // geblockte User werden nicht angefasst
                        if (perm.TargetType == PermissionTarget.User)
                        {
                            permissions.Add(perm);
                            continue;
                        }

                        if (perm.TargetId == bobiiRole.Id)
                        {
                            continue;
                        }

                        var modifiedPerm = perm.Permissions.Modify(connect: PermValue.Deny);
                        permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                    }

                    foreach (var user in whiteListedUsers)
                    {
                        var permissionOverride = voiceChannel.GetPermissionOverwrite(user);
                        if (!permissionOverride.HasValue)
                        {
                            permissionOverride = new OverwritePermissions().Modify(connect: PermValue.Allow);
                        }
                        else
                        {
                            permissionOverride = permissionOverride.Value.Modify(connect: PermValue.Allow);
                        }

                        if (permissions.Select(p => p.TargetId).Contains(user.Id))
                        {
                            int index = permissions.FindIndex(o => o.TargetId == user.Id);

                            if (index != -1)
                                permissions[index] = new Overwrite(user.Id, PermissionTarget.User, permissionOverride.Value);
                        }
                        else
                        {
                            permissions.Add(new Overwrite(user.Id, PermissionTarget.User, permissionOverride.Value));
                        }
                    }

                    if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                    {
                        permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Deny)));
                    }
                }
                else
                {
                    permissions = perms.Select(p => p).Distinct().ToList();
                    _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.VoiceChannel.Id, GlobalStrings.LockKlein);
                }

                permissions = EditWhitelistedUsers(PermValue.Allow, parameter, permissions, voiceChannel).Result;

                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                _ = UsedFunctionsHelper.AddUsedFunction(parameter.GuildUser.Id, 0, GlobalStrings.whitelistactive, voiceChannel.Id, parameter.GuildID);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C289", parameter.Language).Result,
                            GeneralHelper.GetCaption("C236", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(ActivateWhiteList), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp whitelist successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(ActivateWhiteList), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to whitelist temp-channel", exceptionMessage: ex.Message);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C290", parameter.Language).Result,
                            GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task ReplyToTempToggleFunction(SlashCommandParameter parameter, string contentLangKey, string extraCaptionLangKey, string catpionLangKey)
        {
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent(contentLangKey, parameter.Language).Result,GeneralHelper.GetCaption(extraCaptionLangKey, parameter.Language).Result),
                             GeneralHelper.GetCaption(catpionLangKey, parameter.Language).Result).Result }, ephemeral: true);
        }

        public static async Task<List<Overwrite>> EditWhitelistedUsers(PermValue permValue, SlashCommandParameter parameter, List<Overwrite> permissions, SocketVoiceChannel voiceChannel)
        {
            var whiteListedMentions = UsedFunctionsHelper.GetWhitelistUsedFunctions(parameter.GuildUser.Id, parameter.GuildID).Result;
            whiteListedMentions.Add(new usedfunctions() { affecteduserid = parameter.GuildUser.Id });

            foreach (var mention in whiteListedMentions)
            {
                var user = parameter.Guild.GetUser(mention.affecteduserid);
                var role = parameter.Guild.GetRole(mention.affecteduserid);
                if (user != null)
                {
                    permissions = EditPermissionConnect(permValue, permissions, user, voiceChannel);
                }
                else if (role != null)
                {
                    permissions = EditPermissionConnect(permValue, permissions, role, voiceChannel);
                }
            }
            return permissions;
        }

        public static async Task DeactivateWhiteList(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(DeactivateWhiteList), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(DeactivateWhiteList), true).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(DeactivateWhiteList), true, false).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "whitelist", tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result == null)
            {
                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C293", parameter.Language).Result,
                            GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(DeactivateWhiteList), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Whiteliste ist bereits deaktiviert", exceptionMessage: "Already deactivated");
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationName)).First();
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
                var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

                var everyoneRole = parameter.Guild.GetRole(parameter.GuildID);
                var perms = parameter.GuildUser.VoiceChannel.PermissionOverwrites;
                foreach (var perm in perms)
                {
                    // geblockte User werden nicht angefasst
                    if (perm.TargetType == PermissionTarget.User)
                    {
                        permissions.Add(perm);
                        continue;
                    }

                    if (perm.TargetId == bobiiRole.Id)
                    {
                        permissions.Add(perm);
                        continue;
                    }
                    var value = createTempChannel.GetPermissionOverwrite(parameter.Guild.GetRole(perm.TargetId)).GetValueOrDefault();
                    var modifiedPerm = perm.Permissions.Modify(connect: value.Connect);
                    permissions.Add(new Overwrite(perm.TargetId, PermissionTarget.Role, modifiedPerm));
                }
                var voiceChannel = parameter.GuildUser.VoiceChannel;
                foreach (var user in voiceChannel.ConnectedUsers)
                {
                    var permissionOverride = voiceChannel.GetPermissionOverwrite(user);
                    if (!permissionOverride.HasValue)
                    {
                        continue;
                    }

                    permissionOverride = permissionOverride.Value.Modify(connect: PermValue.Inherit);

                    if (permissions.Select(p => p.TargetId).Contains(user.Id))
                    {
                        int index = permissions.FindIndex(o => o.TargetId == user.Id);

                        if (index != -1)
                            permissions[index] = new Overwrite(user.Id, PermissionTarget.User, permissionOverride.Value);
                    }
                    else
                    {
                        permissions.Add(new Overwrite(user.Id, PermissionTarget.User, permissionOverride.Value));
                    }
                }

                if (!perms.Select(p => p.TargetId).Contains(everyoneRole.Id))
                {
                    var value = createTempChannel.GetPermissionOverwrite(everyoneRole).GetValueOrDefault();
                    permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: value.Connect)));
                }

                permissions = EditWhitelistedUsers(PermValue.Inherit, parameter, permissions, voiceChannel).Result;

                _ = voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions.Distinct().ToList());

                _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.VoiceChannel.Id, GlobalStrings.whitelistactive);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C294", parameter.Language).Result,
                            GeneralHelper.GetCaption("C236", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(DeactivateWhiteList), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp whitelist deactivate successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(DeactivateWhiteList), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to dactivate whitelist temp-channel", exceptionMessage: ex.Message);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C295", parameter.Language).Result,
                            GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
        }

        public static async Task<List<SocketGuildUser>> KickUsersWhoAreNoLongerOnWhiteList(SocketVoiceChannel voice, ulong ownerId)
        {
            var whiteListedMentions = UsedFunctionsHelper.GetWhitelistUsedFunctions(ownerId, voice.Guild.Id).Result;
            var connectedUsers = voice.ConnectedUsers;
            var whiteListedUsers = new List<SocketGuildUser>();
            foreach (var user in connectedUsers)
            {
                if (user.Id == ownerId || whiteListedMentions.Select(w => w.affecteduserid).Contains(user.Id))
                {
                    whiteListedUsers.Add(user);
                    continue;
                }

                var whiteListedRoles = whiteListedMentions.Where(w => !w.isuser).ToList();
                var flag = false;
                foreach (var role in user.Roles)
                {
                    if (whiteListedRoles.Select(w => w.affecteduserid).Contains(role.Id))
                    {
                        flag = true;
                        whiteListedUsers.Add(user);
                        continue;
                    }
                }

                if (flag)
                {
                    continue;
                }

                if (user.VoiceChannel == voice)
                {
                    _ = user.ModifyAsync(v => v.Channel = null);
                }
            }

            return whiteListedUsers;
        }

        public static async Task TempInfo(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempInfo)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempInfo)).Result)
            {
                return;
            }

            await parameter.Interaction.DeferAsync();

            var infoString = GetTempInfoString(parameter);
            await parameter.Interaction.FollowupAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            infoString,
                            parameter.GuildUser.VoiceChannel.Name).Result }, ephemeral: true);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempInfo), new SlashCommandParameter() { Guild = parameter.Guild, GuildUser = parameter.GuildUser },
                message: "/temp info successfully used");
        }

        public static int GetAutoDeleteTime(SlashCommandParameter parameter, tempchannels tempChannel)
        {
            var userConfig = HandlingService.Cache.TempChannelUserConfigs.SingleOrDefault(c => c.userid == tempChannel.channelownerid.Value && c.createchannelid == tempChannel.createchannelid);
            if (userConfig != null && userConfig.autodelete.HasValue && userConfig.autodelete > 0)
            {
                return userConfig.autodelete.Value;
            }

            var createTempChannel = CreateTempChannelsHelper.GetCreateTempChannel(tempChannel.createchannelid.Value).Result;
            if (createTempChannel.autodelete.HasValue && createTempChannel.autodelete > 0)
            {
                return createTempChannel.autodelete.Value;
            }

            return 0;
        }

        public static string GetTempInfoString(SlashCommandParameter parameter)
        {
            var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            var sb = new StringBuilder();
            sb.AppendLine(String.Format(GeneralHelper.GetContent("C278", parameter.Language).Result, tempChannel.unixtimestamp));
            sb.AppendLine();
            var appendLine = false;

            var whitelistAktiv = UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null;

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C267", parameter.Language).Result, GeneralHelper.GetCaption("C249", parameter.Language).Result));
                appendLine = true;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.hide, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C267", parameter.Language).Result, GeneralHelper.GetCaption("C251", parameter.Language).Result));
                appendLine = true;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mutechat, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C332", parameter.Language).Result));
                appendLine = true;
            }

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mutevoice, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C335", parameter.Language).Result));
                appendLine = true;
            }

            var autodeleteTime = GetAutoDeleteTime(parameter, tempChannel);
            if (HandlingService.Cache.TempCommands.FirstOrDefault(c => c.createchannelid == tempChannel.createchannelid && c.commandname == GlobalStrings.chat) == null &&
                autodeleteTime > 0)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C315", parameter.Language).Result, autodeleteTime));
                appendLine = true;
            }

            if (whitelistAktiv)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C267", parameter.Language).Result, GeneralHelper.GetCaption("C272", parameter.Language).Result));
                appendLine = true;
            }

            if (appendLine)
            {
                sb.AppendLine();
            }

            sb.AppendLine(String.Format(GeneralHelper.GetContent("C263", parameter.Language).Result, tempChannel.channelownerid.Value));

            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(parameter.Guild.Id, tempChannel.createchannelid.Value).Result;

            if (disabledCommands.FirstOrDefault(c => c.commandname == GlobalStrings.moderator) == null)
            {
                var moderators = UsedFunctionsHelper.GetAllModeratorsFromUser(tempChannel.channelownerid.Value, parameter.GuildID).Result;

                if (moderators.Count() > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine(GeneralHelper.GetContent("C310", parameter.Language).Result);

                    var countt = 0;
                    foreach (var mod in moderators)
                    {
                        countt++;
                        if (parameter.Guild.GetRole(mod.affecteduserid) != null)
                        {
                            sb.Append($"<@&{mod.affecteduserid}>");
                        }
                        else
                        {
                            sb.Append($"<@{mod.affecteduserid}>");
                        }


                        if (countt < moderators.Count)
                        {
                            sb.Append(", ");
                        }
                    }
                    sb.AppendLine();
                }
            }


            var whiteListedMentions = UsedFunctionsHelper.GetWhitelistUsedFunctions(tempChannel.channelownerid.Value, parameter.GuildID).Result;
            if (whiteListedMentions.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(GeneralHelper.GetContent("C283", parameter.Language).Result);
            }

            var count = 0;

            foreach (var mention in whiteListedMentions)
            {
                count++;
                if (parameter.Guild.GetRole(mention.affecteduserid) != null)
                {
                    sb.Append($"<@&{mention.affecteduserid}>");
                }
                else
                {
                    sb.Append($"<@{mention.affecteduserid}>");
                }


                if (count < whiteListedMentions.Count)
                {
                    sb.Append(", ");
                }
            }

            if (whiteListedMentions.Count > 0)
            {
                sb.AppendLine();
            }

            if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.block) == null)
            {
                var blockedUsers = UsedFunctionsHelper.GetUsedFunctions(tempChannel.channelownerid.Value, tempChannel.guildid).Result
                    .Where(u => u.function == GlobalStrings.block)
                    .ToList();

                if (blockedUsers.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine(GeneralHelper.GetContent("C264", parameter.Language).Result);
                }

                count = 0;

                foreach (var blockedUser in blockedUsers)
                {
                    count++;
                    sb.Append($"<@{blockedUser.affecteduserid}>");

                    if (count < blockedUsers.Count)
                    {
                        sb.Append(", ");
                    }
                }
            }

            if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.mute) == null)
            {
                var mutedUsers = UsedFunctionsHelper.GetMutedUsedFunctions(tempChannel.channelid).Result;

                if (mutedUsers.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(GeneralHelper.GetContent("C277", parameter.Language).Result);
                }

                var mutedCount = 0;
                foreach (var user in mutedUsers)
                {
                    mutedCount++;
                    sb.Append($"<@{user.affecteduserid}>");

                    if (mutedCount < mutedUsers.Count)
                    {
                        sb.Append(", ");
                    }
                }
            }

            if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.chat) == null)
            {
                var chatMutedUsers = UsedFunctionsHelper.GetChatMutedUserUsedFunctions(tempChannel.channelid).Result;

                if (chatMutedUsers.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(GeneralHelper.GetContent("C328", parameter.Language).Result);
                }

                var chatMutedUsersCount = 0;
                foreach (var user in chatMutedUsers)
                {
                    chatMutedUsersCount++;
                    sb.Append($"<@{user.affecteduserid}>");

                    if (chatMutedUsersCount < chatMutedUsers.Count)
                    {
                        sb.Append(", ");
                    }
                }
            }

            return sb.ToString();
        }

        public static async Task TempOwner(SlashCommandParameter parameter, string userId, bool epherialMessage = false)
        {
            await GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckUserID(parameter, userId, nameof(TempOwner)).Result)
            {
                return;
            }

            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempOwner), epherialMessage).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempOwner), epherialMessage).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempOwner), epherialMessage, false).Result ||
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

                var currentOwner = parameter.Guild.GetUser(tempChannel.channelownerid.Value);
                var newOwner = parameter.Guild.GetUser(userId.ToUlong());

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

                var voiceChannel = parameter.Guild.GetVoiceChannel(tempChannel.channelid);
                var guildUser = parameter.Guild.GetUser(userId.ToUlong());

                var permissions = voiceChannel.PermissionOverwrites.ToList();

                permissions = RemoveManageChannelRightsToUserVc(currentOwner, permissions, voiceChannel).Result;
                permissions = GiveManageChannelRightsToUserVc(newOwner, parameter.GuildID, permissions, null, voiceChannel).Result;
                if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.whitelistactive, parameter.GuildUser.VoiceChannel.Id).Result != null)
                {
                    permissions = EditWhitelistedUsers(PermValue.Inherit, parameter, permissions, voiceChannel).Result;
                    _ = KickUsersWhoAreNoLongerOnWhiteList(voiceChannel, newOwner.Id);
                    permissions = EditWhitelistedUsers(PermValue.Allow, new SlashCommandParameter() { Guild = parameter.Guild, GuildID = parameter.GuildID, GuildUser = newOwner }, permissions, voiceChannel).Result;
                }

                permissions = UnblockAllUsersFromPreviousOwner(parameter.GuildUser, permissions, parameter.GuildUser.VoiceChannel).Result;
                permissions = BlockAllUserFromOwner(guildUser, parameter.Client, permissions, null, voiceChannel).Result;
                permissions = UnmuteIfNewOwnerAndMuted(parameter, permissions).Result;

                await voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

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

                await SendOwnerUpdatedMessage(parameter.GuildUser.VoiceChannel, parameter.Guild, guildUser.Id, parameter.Language);
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

        public static ComponentBuilder GetButtonsComponentBuilder(Dictionary<ButtonBuilder, MagickImage> dict)
        {
            var count = 0;
            var componentBuilder = new ComponentBuilder();
            var rowBuilder = new ActionRowBuilder();
            var countAll = 0;
            foreach (var button in dict)
            {
                count++;
                countAll++;
                rowBuilder.WithButton(button.Key);
                if (count == 4)
                {
                    count = 0;
                    componentBuilder.AddRow(rowBuilder);
                    rowBuilder = new ActionRowBuilder();
                    continue;
                }

                if (countAll == dict.Count())
                {
                    componentBuilder.AddRow(rowBuilder);
                }
            }

            return componentBuilder;
        }

        public static MagickImage ComebineBitmap(MagickImage main, MagickImage Overlay, int x, int y)
        {
            main.Composite(Overlay, x, y, CompositeOperator.Over);

            return main;
        }

        public static MagickImage GetButtonsBitmap(Dictionary<ButtonBuilder, MagickImage> dict)
        {
            var magickGrundBild = GetRightSizedBitmap(dict.Count());

            var x = 0;
            var y = 0;
            var count = 0;
            foreach (var image in dict.Values)
            {
                count++;
                magickGrundBild = ComebineBitmap(magickGrundBild, image, x, y);
                image.Dispose();
                x += 240;
                if (count == 4)
                {
                    count = 0;
                    y += 90;
                    x = 0;
                }
            }

            return magickGrundBild;
        }

        public static MagickImage GetRightSizedBitmap(int anzahlImages)
        {
            switch (anzahlImages)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    return new MagickImage(MagickColors.Transparent, 920, 60);
                case 5:
                case 6:
                case 7:
                case 8:
                    return new MagickImage(MagickColors.Transparent, 920, 150);
                case 9:
                case 10:
                case 11:
                case 12:
                    return new MagickImage(MagickColors.Transparent, 920, 240);
                case 13:
                case 14:
                case 15:
                case 16:
                    return new MagickImage(MagickColors.Transparent, 920, 330);
                default:
                    return new MagickImage(MagickColors.Transparent, 920, 330);

            }
        }


        public static async Task SaveNewInterfaceButtonPicture(DiscordSocketClient client, List<tempcommands> disabledCommands, ulong createTempChannelId)
        {
            var buttonsMitBildern = GetInterfaceButtonsMitBild(client, disabledCommands).Result;
            var buttonComponentBuilder = GetButtonsComponentBuilder(buttonsMitBildern);
            var img = GetButtonsBitmap(buttonsMitBildern);
            img.Write($"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons_5.png", MagickFormat.Png);
            img.Dispose();
        }

        public static string GetOrSaveAndGetButtonsImageName(DiscordSocketClient client, List<tempcommands> disabledCommands, ulong createTempChannelId)
        {
            var filePath = $"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons_5.png";
            if (File.Exists(filePath))
            {
                return Path.GetFileName(filePath);
            }
            else
            {
                _ = SaveNewInterfaceButtonPicture(client, disabledCommands, createTempChannelId);
                var oldFilePaths = new string[] {
                    $"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons.png",
                    $"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons_neu.png",
                    $"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons_1.png",
                    $"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons_2.png",
                    $"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons_3.png",
                    $"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons_4.png"};

                foreach (var path in oldFilePaths)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }

                return Path.GetFileName(filePath);
            }
        }

        public static async Task WriteInterfaceInVoiceChannel(ITextChannel tempChannel, DiscordSocketClient client, ulong createTempChannelId)
        {
            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(tempChannel.GuildId, createTempChannelId).Result;

            var imgFileNameAttachement = "";
            var fileName = "";
            try
            {
                fileName = GetOrSaveAndGetButtonsImageName(client, disabledCommands, createTempChannelId);
                imgFileNameAttachement = $"attachment://{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                imgFileNameAttachement = "https://cdn.discordapp.com/attachments/910868343030960129/1152272115039481887/964126199603400705_buttons.png";
            }

            var buttonsMitBildern = GetInterfaceButtonsMitBild(client, disabledCommands).Result;
            var buttonComponentBuilder = GetButtonsComponentBuilder(buttonsMitBildern);

            var voiceChannel = (IRestMessageChannel)tempChannel;
            //var componentBuilder = new ComponentBuilder();
            //await AddInterfaceButtons(componentBuilder, disabledCommands);

            var lang = Bobii.EntityFramework.BobiiHelper.GetLanguage(tempChannel.GuildId).Result;

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(GeneralHelper.GetCaption("C211", lang).Result)
                .WithColor(74, 171, 189)
                .WithImageUrl(imgFileNameAttachement)
                .WithDescription(GeneralHelper.GetContent("C208", lang).Result)
                .WithFooter(DateTime.Now.ToString("dd/MM/yyyy"));

            if (imgFileNameAttachement == "https://cdn.discordapp.com/attachments/910868343030960129/1152272115039481887/964126199603400705_buttons.png")
            {
                await voiceChannel.SendMessageAsync(embeds: new Embed[] { embed.Build() }, components: buttonComponentBuilder.Build());
            }
            else
            {
                await voiceChannel.SendFileAsync(fileName, embeds: new Embed[] { embed.Build() }, components: buttonComponentBuilder.Build());
            }
        }

        public static async Task SendSuccessMessageContext(SocketGuildUser triggerUser, SocketGuildUser affectedUser, string functionLangKey)
        {
            var lang = BobiiHelper.GetLanguage(triggerUser.Guild.Id).Result;
            await triggerUser.SendMessageAsync(
                String.Format(
                    GeneralHelper.GetContent("C306", lang).Result,
                    affectedUser.DisplayName,
                    GeneralHelper.GetCaption(functionLangKey, lang).Result
                ));
        }

        public static MagickImage ToMagickImage(this Bitmap bmp)
        {
            IMagickImage img = null;
            MagickFactory f = new MagickFactory();
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                img = new MagickImage(f.Image.Create(ms));
            }
            return (MagickImage)img;
        }

        public static async Task<Dictionary<ButtonBuilder, MagickImage>> GetInterfaceButtonsMitBild(DiscordSocketClient client, List<tempcommands> disabledCommands)
        {
            var commands = client.GetGlobalApplicationCommandsAsync()
                .Result
                .Single(c => c.Name == GlobalStrings.temp)
                .Options.Select(c => c.Name)
                .ToList();

            var dict = new Dictionary<ButtonBuilder, MagickImage>();
            foreach (var command in commands)
            {
                if (CommandDisabled(disabledCommands, command) || command == "interface")
                {
                    continue;
                }

                var button = GetButton($"temp-interface-{command}", Emojis()[command], command, CommandDisabled(disabledCommands, command));
                MagickImage image;
                try
                {
                    image = new MagickImage($"{Directory.GetCurrentDirectory()}/buttons/{command}button.png");
                }
                catch (Exception ex)
                {
                    image = null;
                    Console.WriteLine(ex.Message);
                }


                dict.Add(button, image);
            }

            return dict;
        }

        public static SelectMenuBuilder SettingsSelectionMenu(SlashCommandParameter parameter)
        {
            var saveSettingsEmote = Emote.Parse(Emojis()["saveconfig"]);
            var deleteSettingsEmote = Emote.Parse(Emojis()["deleteconfig"]);
            return new SelectMenuBuilder()
                .WithPlaceholder(GeneralHelper.GetCaption("C254", parameter.Language).Result)
                .WithCustomId("temp-interface-settings")
                .WithType(ComponentType.SelectMenu)
                .WithOptions(new List<SelectMenuOptionBuilder>
                    {
                new SelectMenuOptionBuilder()
                    //Save Config
                    .WithLabel(GeneralHelper.GetCaption("C282", parameter.Language).Result)
                    .WithValue("temp-channel-settings-save")
                    .WithEmote(saveSettingsEmote),
                new SelectMenuOptionBuilder()
                    //Delete Config
                    .WithLabel(GeneralHelper.GetCaption("C283", parameter.Language).Result)
                    .WithValue("temp-channel-settings-delete")
                    .WithEmote(deleteSettingsEmote)
             });
        }

        public static SelectMenuBuilder PrivacySelectionMenu(SlashCommandParameter parameter)
        {
            var lockEmote = Emote.Parse(Emojis()["lock"]);
            var unlockEmote = Emote.Parse(Emojis()["unlock"]);
            var hideEmote = Emote.Parse(Emojis()["hide"]);
            var unhideEmote = Emote.Parse(Emojis()["unhide"]);
            return new SelectMenuBuilder()
                .WithPlaceholder(GeneralHelper.GetCaption("C254", parameter.Language).Result)
                .WithCustomId("temp-interface-privacy")
                .WithType(ComponentType.SelectMenu)
                .WithOptions(new List<SelectMenuOptionBuilder>
                    {
                new SelectMenuOptionBuilder()
                    //Lock Channel
                    .WithLabel(String.Format(GeneralHelper.GetContent("C300", parameter.Language).Result, GeneralHelper.GetCaption("C278", parameter.Language).Result))
                    .WithValue("temp-channel-privacy-lock")
                    .WithEmote(lockEmote),
                new SelectMenuOptionBuilder()
                    //Unlock Channel
                    .WithLabel(String.Format(GeneralHelper.GetContent("C300", parameter.Language).Result, GeneralHelper.GetCaption("C279", parameter.Language).Result))
                    .WithValue("temp-channel-privacy-unlock")
                    .WithEmote(unlockEmote),
                new SelectMenuOptionBuilder()
                    //Hide Channel
                    .WithLabel(String.Format(GeneralHelper.GetContent("C300", parameter.Language).Result, GeneralHelper.GetCaption("C280", parameter.Language).Result))
                    .WithValue("temp-channel-privacy-hide")
                    .WithEmote(hideEmote),
                new SelectMenuOptionBuilder()
                    //Unhide Channe
                    .WithLabel(String.Format(GeneralHelper.GetContent("C300", parameter.Language).Result, GeneralHelper.GetCaption("C281", parameter.Language).Result))
                    .WithValue("temp-channel-privacy-unhide")
                    .WithEmote(unhideEmote)
             });
        }

        public static SelectMenuBuilder WhiteListSelectionMenu(SlashCommandParameter parameter)
        {
            var whiteListEmote = Emote.Parse(Emojis()["whitelist"]);
            var whitelistInactive = Emote.Parse(Emojis()["whitelistinactive"]);
            var whiteListaddEmote = Emote.Parse(Emojis()["whitelistadd"]);
            var whiteListRemoveEmote = Emote.Parse(Emojis()["whitelistremove"]);
            return new SelectMenuBuilder()
                .WithPlaceholder(GeneralHelper.GetCaption("C254", parameter.Language).Result)
                .WithCustomId("temp-interface-whitelist")
                .WithType(ComponentType.SelectMenu)
                .WithOptions(new List<SelectMenuOptionBuilder>
                    {
                new SelectMenuOptionBuilder()
                    //Mute users
                    .WithLabel(GeneralHelper.GetCaption("C266", parameter.Language).Result)
                    .WithValue("temp-channel-whitelist-activate")
                    .WithEmote(whiteListEmote),
                new SelectMenuOptionBuilder()
                    //Unmute users
                    .WithLabel(GeneralHelper.GetCaption("C267", parameter.Language).Result)
                    .WithValue("temp-channel-whitelist-deactivate")
                    .WithEmote(whitelistInactive),
                new SelectMenuOptionBuilder()
                    //Mute all users
                    .WithLabel(GeneralHelper.GetCaption("C268", parameter.Language).Result)
                    .WithValue("temp-channel-whitelist-add")
                    .WithEmote(whiteListaddEmote),
                new SelectMenuOptionBuilder()
                    //Unmute all users
                    .WithLabel(GeneralHelper.GetCaption("C269", parameter.Language).Result)
                    .WithValue("temp-channel-whitelist-remove")
                    .WithEmote(whiteListRemoveEmote)
             });
        }

        public static SelectMenuBuilder ModeratorSelectionMenu(SlashCommandParameter parameter)
        {
            var addModEmoji = Emote.Parse(Emojis()["moderator"]);
            var removeModEmoji = Emote.Parse(Emojis()["removemoderator"]);
            return new SelectMenuBuilder()
                .WithPlaceholder(GeneralHelper.GetCaption("C254", parameter.Language).Result)
                .WithCustomId("temp-interface-moderator")
                .WithType(ComponentType.SelectMenu)
                .WithOptions(new List<SelectMenuOptionBuilder>
                    {
                new SelectMenuOptionBuilder()
                    //Add Mod
                    .WithLabel(GeneralHelper.GetCaption("C285", parameter.Language).Result)
                    .WithValue("temp-channel-moderator-add")
                    .WithEmote(addModEmoji),
                new SelectMenuOptionBuilder()
                    //Remove Mod
                    .WithLabel(GeneralHelper.GetCaption("C286", parameter.Language).Result)
                    .WithValue("temp-channel-moderator-remove")
                    .WithEmote(removeModEmoji),
             });
        }

        public static SelectMenuBuilder MessagesSelectionMenu(SlashCommandParameter parameter)
        {
            var messageEmoji = Emote.Parse(Emojis()["chat"]);
            var autodeleteEmoji = Emote.Parse(Emojis()["autodelete"]);
            var deleteMessageEmoji = Emote.Parse(Emojis()["deletemessage"]);
            var muteChatEmoji = Emote.Parse(Emojis()["mutechat"]);

            return new SelectMenuBuilder()
                .WithPlaceholder(GeneralHelper.GetCaption("C254", parameter.Language).Result)
                .WithCustomId("temp-interface-messages")
                .WithType(ComponentType.SelectMenu)
                .WithOptions(new List<SelectMenuOptionBuilder>
                    {
                new SelectMenuOptionBuilder()
                    //Autodelete
                    .WithLabel(GeneralHelper.GetCaption("C287", parameter.Language).Result)
                    .WithValue("temp-channel-messages-autodelete")
                    .WithEmote(autodeleteEmoji),
                new SelectMenuOptionBuilder()
                    //Delete Message from user
                    .WithLabel(GeneralHelper.GetCaption("C288", parameter.Language).Result)
                    .WithValue("temp-channel-messages-deletemessages-user")
                    .WithEmote(deleteMessageEmoji),
                new SelectMenuOptionBuilder()
                    //Delte all messages
                    .WithLabel(GeneralHelper.GetCaption("C289", parameter.Language).Result)
                    .WithValue("temp-channel-messages-deletemessages")
                    .WithEmote(deleteMessageEmoji),
                new SelectMenuOptionBuilder()
                    //Mute chat
                    .WithLabel(GeneralHelper.GetCaption("C290", parameter.Language).Result)
                    .WithValue("temp-channel-messages-mute-chat")
                    .WithEmote(muteChatEmoji),
                new SelectMenuOptionBuilder()
                    //unmute chat
                    .WithLabel(GeneralHelper.GetCaption("C291", parameter.Language).Result)
                    .WithValue("temp-channel-messages-unmute-chat")
                    .WithEmote(messageEmoji),
                new SelectMenuOptionBuilder()
                    //Mute chat
                    .WithLabel(GeneralHelper.GetCaption("C292", parameter.Language).Result)
                    .WithValue("temp-channel-messages-mute-user")
                    .WithEmote(muteChatEmoji),
                                new SelectMenuOptionBuilder()
                    //unmute chat
                    .WithLabel(GeneralHelper.GetCaption("C293", parameter.Language).Result)
                    .WithValue("temp-channel-messages-unmute-user")
                    .WithEmote(messageEmoji)
             });
        }

        public static SelectMenuBuilder MuteSelectionMenu(SlashCommandParameter parameter)
        {
            var muteEmoji = Emote.Parse(TempChannelHelper.Emojis()["muteemote"]);
            var unmuteEmoji = Emote.Parse(TempChannelHelper.Emojis()["mute"]);
            return new SelectMenuBuilder()
                .WithPlaceholder(GeneralHelper.GetCaption("C254", parameter.Language).Result)
                .WithCustomId("temp-interface-mute")
                .WithType(ComponentType.SelectMenu)
                .WithOptions(new List<SelectMenuOptionBuilder>
                    {
                new SelectMenuOptionBuilder()
                    //Mute users
                    .WithLabel(GeneralHelper.GetCaption("C255", parameter.Language).Result)
                    .WithValue("temp-channel-mute-users")
                    .WithEmote(muteEmoji),
                new SelectMenuOptionBuilder()
                    //Unmute users
                    .WithLabel(GeneralHelper.GetCaption("C256", parameter.Language).Result)
                    .WithValue("temp-channel-unmute-users")
                    .WithEmote(unmuteEmoji),
                new SelectMenuOptionBuilder()
                    //Mute all users
                    .WithLabel(GeneralHelper.GetCaption("C257", parameter.Language).Result)
                    .WithValue("temp-channel-mute-all")
                    .WithEmote(muteEmoji),
                new SelectMenuOptionBuilder()
                    //Unmute all users
                    .WithLabel(GeneralHelper.GetCaption("C258", parameter.Language).Result)
                    .WithValue("temp-channel-unmute-all")
                    .WithEmote(unmuteEmoji)
             });
        }

        public static Dictionary<string, string> Emojis()
        {
            return new Dictionary<string, string>()
            {
                { "name", "<:edit:1138160331122802818>" },
                { "unlock", "<:lockopen:1138164700434149477>"},
                { "lock", "<:lockclosed:1138164855820525702>"},
                { "privacy", "<:lockclosed:1138164855820525702>"},
                { "hide", "<:hidenew:1149745796057669775>"},
                { "unhide", "<:unhidenew:1149745799136280606>"},
                { "kick", "<:userkickednew:1149730990680461426>" },
                { "block", "<:blockednew:1153698716117639189>"},
                { "unblock", "<:userunblockednew:1149731419195707592>"},
                { "saveconfig", "<:config:1138181363338588351>"},
                { "settings", "<:config:1138181363338588351>"},
                { "deleteconfig", "<:noconfig:1138181406799966209>"},
                { "size", "<:userlimitnew:1151507242651238421>"},
                { "giveowner", "<:ownergive:1149325094045356072>"},
                { "claimowner", "<:ownerclaim:1149325095488204810>" },
                { "info", "<:infoneu:1152542500989435924>"},
                // nicht wirklich das mute emote, das ist das unmute aber wegen dem Slashcommand wird das hier unter mute benutzt
                { "mute", "<:unmute:1151506858750775326>"},
                { "muteemote", "<:mute:1151506855659585626>"},
                { "whitelist", "<:whitelist:1153724523724668998>"},
                { "whitelistinactive", "<:whitelistinactive:1153727440376570016>"},
                { "whitelistadd", "<:whitelistadd:1154035536961482793>"},
                { "whitelistremove", "<:whitelistremove:1154035534923055134>"},
                { "moderator", "<:addmod:1156212851732664430>" },
                { "removemoderator", "<:removemod:1156212854307950592>" },
                { "chat", "<:messages:1157995749645230140>"},
                { "autodelete", "<:autodelete:1157998122618867754>"},
                { "deletemessage", "<:deletemessage:1157999927276875837>" },
                { "mutechat", "<:mutechat:1158000700844953671>" }
            };
        }

        public static ButtonBuilder GetButton(string customId, string emojiString, string commandName, bool disabled = false)
        {
            return new ButtonBuilder()
                .WithCustomId(customId)
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(Emote.Parse(emojiString))
                .WithDisabled(disabled);
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
                        permissionOverride = permissionOverride.Value.Modify(sendMessages: PermValue.Inherit);
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

                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow, sendMessages: PermValue.Allow)));

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
                    message: $"Voicechannel could not be created, {user} has got a DM if it was missing permissions or null ref", exceptionMessage: ex.Message + ex.StackTrace);
                return null;
            }
        }

        public static async Task TempDeleteUserMessages(SlashCommandParameter parameter, List<ulong> userIds)
        {
            await parameter.Interaction.DeferAsync();
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempDeleteUserMessages), true).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempDeleteUserMessages), true).Result)
            {
                return;
            }
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

            if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempDeleteUserMessages), true).Result)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.chat, tempChannelEntity.createchannelid.Value, true).Result)
            {
                return;
            }

            var sb = new StringBuilder();
            var textChannel = (SocketTextChannel)parameter.GuildUser.VoiceChannel;
            var messages = textChannel.GetMessagesAsync(1000).ToListAsync().Result.FirstOrDefault().Where(m => m.Flags != MessageFlags.Ephemeral);
            var success = false;
            var error = false;

            foreach (var userId in userIds)
            {
                var userMessages = messages.Where(m => m.Author.Id == userId).ToList();
                if (userMessages.Count() == 0)
                {
                    sb.AppendLine(String.Format(GeneralHelper.GetContent("C319", parameter.Language).Result, userId));
                    error = true;
                    await HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempDeleteUserMessages), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Keine Nachrichten gefunden");
                    continue;
                }

                try
                {
                    var channel = (ITextChannel)userMessages.First().Channel;
                    await channel.DeleteMessagesAsync(userMessages);

                    sb.AppendLine(String.Format(GeneralHelper.GetContent("C320", parameter.Language).Result, userMessages.Count(), userId));

                    await HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempDeleteUserMessages), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: $"Alle {userMessages.Count()} Nachrichten van {userId} gelöscht");

                    success = true;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Missing Permissions"))
                    {
                        await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                        {
                            msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                            msg.Components = null;
                        });
                    }
                    else
                    {
                        await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                        {
                            msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                            msg.Components = null;
                        });
                    }

                    await HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempDeleteAllMessages), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Failed to delete all messages", exceptionMessage: ex.Message);
                    return;
                }
            }

            var caption = string.Empty;
            if (success && error)
            {
                caption = GeneralHelper.GetCaption("C237", parameter.Language).Result;
            }
            if (success && !error)
            {
                caption = GeneralHelper.GetCaption("C236", parameter.Language).Result;
            }
            if (!success && error)
            {
                caption = GeneralHelper.GetCaption("C238", parameter.Language).Result;
            }

            await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        sb.ToString(),
                        caption).Result };
                msg.Components = null;
            });
        }

        public static async Task TempDeleteAllMessages(SlashCommandParameter parameter)
        {
            try
            {
                var textChannel = (SocketTextChannel)parameter.GuildUser.VoiceChannel;
                var messages = textChannel.GetMessagesAsync(1000).ToListAsync().Result.FirstOrDefault().Where(m => m.Flags != MessageFlags.Ephemeral);
                var count = messages.Count();

                if (count == 0)
                {
                    await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C318", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                        msg.Components = null;
                    });

                    await HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempDeleteAllMessages), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "Keine Nachrichten gefunden");
                    return;
                }

                var channel = (ITextChannel)messages.First().Channel;
                await channel.DeleteMessagesAsync(messages);

                await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        String.Format(GeneralHelper.GetContent("C317", parameter.Language).Result, count),
                        GeneralHelper.GetCaption("C236", parameter.Language).Result).Result };
                    msg.Components = null;
                });

                await HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempDeleteAllMessages), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: $"Alle {count} Nachrichten gelöscht");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C316", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                        msg.Components = null;
                    });
                }
                else
                {
                    await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetCaption("C038", parameter.Language).Result,
                        GeneralHelper.GetCaption("C238", parameter.Language).Result).Result };
                        msg.Components = null;
                    });
                }

                await HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempDeleteAllMessages), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to delete all messages", exceptionMessage: ex.Message);
                return;
            }
        }

        public static Embed CreateCreateTempChannelInformation(SlashCommandParameter parameter, ulong createTempChannelId)
        {
            var createTempChannel = CreateTempChannelsHelper.GetCreateTempChannel(createTempChannelId).Result;
            if (createTempChannel == null)
            {
                return GeneralHelper.CreateEmbed(parameter.Interaction, "", "").Result;
            }

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


            if (HandlingService.Cache.TempCommands.FirstOrDefault(c => c.createchannelid == createTempChannel.createchannelid && c.commandname == GlobalStrings.chat) == null &&
                createTempChannel.autodelete.HasValue &&
                createTempChannel.autodelete > 0)
            {
                sb.AppendLine(String.Format(GeneralHelper.GetContent("C315", parameter.Language).Result, createTempChannel.autodelete.Value));
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
            sb.AppendLine(
                GetCommandsTable(
                    parameter,
                    disabledCommands,
                    new List<string>()
                        {
                            GlobalStrings.InterfaceKlein,
                            GlobalStrings.ownerpermissions,
                            GlobalStrings.kickblockedusersonownerchange,
                            GlobalStrings.hidevoicefromblockedusers,
                            GlobalStrings.autotransferowner
                        },
                    "C243",
                    false)
                );

            return GeneralHelper.CreateEmbed(parameter.Interaction, sb.ToString(), header).Result;
        }

        public static string GetCommandsTable(SlashCommandParameter parameter, List<tempcommands> disabledCommands, List<string> commands, string spc, bool tempCommands = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine("╔═════════════════════════════════════════════╦═════════════╗");
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
                    if (!tempCommands)
                    {
                        var commandUebersetzt = GetCommandFunctionTranslation(parameter.Language, command);
                        AddRow(sb, commandUebersetzt, (disabledCommands.SingleOrDefault(c => c.commandname == command) == null).ToString(), true, temp);
                    }
                    else
                    {
                        AddRow(sb, command, (disabledCommands.SingleOrDefault(c => c.commandname == command) == null).ToString(), true, temp);
                    }
                }
                else
                {
                    if (!tempCommands)
                    {
                        var commandUebersetzt = GetCommandFunctionTranslation(parameter.Language, command);
                        AddRow(sb, commandUebersetzt, (disabledCommands.SingleOrDefault(c => c.commandname == command) == null).ToString(), false, temp);
                    }
                    else
                    {
                        AddRow(sb, command, (disabledCommands.SingleOrDefault(c => c.commandname == command) == null).ToString(), false, temp);
                    }
                }
            }

            sb.AppendLine("╚═════════════════════════════════════════════╩═════════════╝");
            sb.AppendLine("```");

            return sb.ToString();
        }

        public static string GetCommandFunctionTranslation(string lang, string commandName)
        {
            return GeneralHelper.GetCaption(GetCommandFunctionLangKeys()[commandName], lang).Result;
        }

        public static Dictionary<string, string> GetCommandFunctionLangKeys()
        {
            return new Dictionary<string, string>()
            {
                { GlobalStrings.InterfaceKlein, "C276"},
                { GlobalStrings.ownerpermissions, "C277"},
                {  GlobalStrings.kickblockedusersonownerchange, "C275"},
                { GlobalStrings.hidevoicefromblockedusers, "C274" },
                { GlobalStrings.autotransferowner, "C273"}
            };
        }

        public static void AddRow(StringBuilder sb, string command, string active, bool lastRow = false, string temp = "/temp ")
        {
            var str = $"║ {temp}{command}";
            str = Auffuellen(str, 47, "║");

            str += $" {active}";
            str = Auffuellen(str, 61, "║");

            sb.AppendLine(str);
            if (!lastRow)
            {
                sb.AppendLine("╠═════════════════════════════════════════════╬═════════════╣");
            }

        }

        public static string Auffuellen(string str, int pos, string zeichen)
        {
            var sb = new StringBuilder();
            sb.Append(str);
            while (sb.Length < pos - 1)
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

        public static async Task<string> HelpEditTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId, bool withoutHint = false, string createVoiceChannelId = "")
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
            return GeneralHelper.CreateInfoPart(commandList, language, sb.ToString(), "temp", guildId, !withoutHint, createVoiceChannelId).Result;
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