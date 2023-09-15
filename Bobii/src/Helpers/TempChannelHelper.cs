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

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, tempChannel.channelid).Result != null && parameter.SocketUser.Id != tempChannel.channelownerid)
            {
                var permissions = parameter.OldSocketVoiceChannel.GetPermissionOverwrite(parameter.SocketUser);
                var perms = parameter.OldSocketVoiceChannel.PermissionOverwrites;
                if (permissions.HasValue)
                {
                    permissions = permissions.Value.Modify(connect: PermValue.Inherit);
                    await parameter.OldSocketVoiceChannel.AddPermissionOverwriteAsync(parameter.SocketUser, permissions.Value);
                }
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
                    var user = parameter.Guild.GetUser(ownerId);
                    await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(tempChannelId, parameter.GuildUser.Id);
                    await UnblockAllUsersFromPreviousOwner(user, voiceChannel);
                    await BlockAllUserFromOwner(parameter.GuildUser, parameter.Client, null, voiceChannel);
                    _ = UnmuteIfNewOwnerAndMuted(parameter);
                    await RemoveManageChannelRightsToUserVc(user, voiceChannel);
                    await GiveManageChannelRightsToUserVc(parameter.GuildUser, parameter.GuildID, null, parameter.GuildUser.VoiceChannel);

                    await SendOwnerUpdatedMessage(voiceChannel, parameter.Guild, parameter.GuildUser.Id, parameter.Language);
                }
            }
            catch (Exception)
            {
                //nothing
            }
        }

        public static async Task UnmuteIfNewOwnerAndMuted(SlashCommandParameter parameter)
        {
            var voice = parameter.GuildUser.VoiceChannel;
            if (UsedFunctionsHelper.GetMutedUsedFunctions(voice.Id).Result.SingleOrDefault(f => f.affecteduserid == parameter.GuildUser.Id) != null)
            {
                var permissions = voice.PermissionOverwrites.ToList();

                EditPermissionSpeak(PermValue.Allow, permissions, parameter.GuildUser, voice);

                await voice.ModifyAsync(v => v.PermissionOverwrites = permissions);
                _ = parameter.GuildUser.ModifyAsync(u => u.Channel = voice);
                _ =parameter.GuildUser.ModifyAsync(u => u.Mute = false);
                await UsedFunctionsHelper.RemoveUsedFunction(voice.Id, GlobalStrings.mute, parameter.GuildUser.Id); ;
            }
        }

        public static async Task GiveManageChannelRightsToUserVc(SocketUser user, ulong guildId, RestVoiceChannel restVoiceChannel, SocketVoiceChannel socketVoiceChannel)
        {
            var permissionOverrrides = new OverwritePermissions().Modify(viewChannel: PermValue.Allow, connect: PermValue.Allow);
            if (restVoiceChannel != null)
            {
                var permissions = restVoiceChannel.PermissionOverwrites.ToList();
                permissions = EditPermissionViewChannel(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);
                permissions = EditPermissionConnect(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);

                var tempChannelEntity = TempChannelsHelper.GetTempChannel(restVoiceChannel.Id).Result;
                if (!TempCommandsHelper.DoesCommandExist(guildId, tempChannelEntity.createchannelid.Value, "ownerpermissions").Result)
                {
                    permissions = EditPermissionManageChannel(PermValue.Allow, permissions, user as SocketGuildUser, restVoiceChannel);
                }

                await restVoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
            }
            else
            {
                var permissions = socketVoiceChannel.PermissionOverwrites.ToList();
                permissions = EditPermissionViewChannel(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);
                permissions = EditPermissionConnect(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);

                var tempChannelEntity = TempChannelsHelper.GetTempChannel(socketVoiceChannel.Id).Result;
                if (!TempCommandsHelper.DoesCommandExist(guildId, tempChannelEntity.createchannelid.Value, "ownerpermissions").Result)
                {
                    permissions = EditPermissionManageChannel(PermValue.Allow, permissions, user as SocketGuildUser, socketVoiceChannel);
                }

                await socketVoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
            }
        }

        public static async Task RemoveManageChannelRightsToUserVc(SocketUser user, SocketVoiceChannel voiceChannel)
        {
            var permissions = voiceChannel.GetPermissionOverwrite(user);
            if (permissions == null)
            {
                return;
            }

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(voiceChannel.Id).Result;
            permissions = permissions.Value.Modify(viewChannel: PermValue.Inherit, connect: PermValue.Inherit);
            if (!TempCommandsHelper.DoesCommandExist(voiceChannel.Guild.Id, tempChannelEntity.createchannelid.Value, "ownerpermissions").Result)
            {
                permissions = permissions.Value.Modify(manageChannel: PermValue.Inherit);
            }


            await voiceChannel.AddPermissionOverwriteAsync(user, permissions.Value);
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
            var tempChannel = CreateVoiceChannel(user, channelName, newVoice, createTempChannel, channelSize, client).Result;
            await ConnectToVoice(tempChannel, user as IGuildUser);

            await GiveManageChannelRightsToUserVc(user, ((SocketGuildUser)user).Guild.Id, tempChannel, null);
            await BlockAllUserFromOwner(user as SocketGuildUser, client, tempChannel, null);

            var tempChannelEntity = TempChannelsHelper.GetTempChannel(tempChannel.Id).Result;
            if (!TempCommandsHelper.DoesCommandExist(((SocketGuildUser)user).Guild.Id, tempChannelEntity.createchannelid.Value, "interface").Result)
            {
                await WriteInterfaceInVoiceChannel(tempChannel, client);
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

        public static async Task UnblockAllUsersFromPreviousOwner(SocketGuildUser user, SocketVoiceChannel voiceChannel)
        {
            try
            {
                var tempChannel = TempChannelsHelper.GetTempChannel(voiceChannel.Id).Result;
                var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(user.Guild.Id, tempChannel.createchannelid.Value).Result;

                if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.block) != null)
                {
                    return;
                }

                var usedFunctions = UsedFunctionsHelper.GetUsedFunctions(user.Id, user.Guild.Id).Result.Where(u => u.function == GlobalStrings.block).ToList();

                var permissions = voiceChannel.PermissionOverwrites.ToList();
                foreach (var usedFunction in usedFunctions)
                {
                    var userToBeUnblocked = user.Guild.GetUser(usedFunction.affecteduserid);
                    if (userToBeUnblocked != null)
                    {
                        EditPermissionConnect(PermValue.Inherit, permissions, userToBeUnblocked, voiceChannel);

                        if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.hidevoicefromblockedusers) == null)
                        {
                            EditPermissionViewChannel(PermValue.Inherit, permissions, userToBeUnblocked, voiceChannel);
                        }
                    }
                }

                await voiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task BlockUserFormBannedVoiceAfterJoining(SocketGuildUser user)
        {
            var usedFunctions = UsedFunctionsHelper.GetUsedFrunctionsFromAffectedUser(user.Id, user.Guild.Id)
                .Result
                .Where(u => u.function == GlobalStrings.block)
                .ToList();

            if (usedFunctions.Count() != 0)
            {
                var tempChannels = TempChannelsHelper.GetTempChannelListFromGuild(user.Guild.Id).Result;
                var affectedTempChannels = tempChannels.Where(t => usedFunctions.Select(u => u.userid).ToList().Contains(t.channelownerid.Value)).ToList();

                foreach (var affectedTempChannel in affectedTempChannels)
                {
                    var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(user.Guild.Id, affectedTempChannel.createchannelid.Value).Result;
                    if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.block) != null)
                    {
                        continue;
                    }

                    var hideVoie = disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.hidevoicefromblockedusers) == null;

                    var tempChannel = user.Guild.GetVoiceChannel(affectedTempChannel.channelid);
                    var newPermissionOverride = new OverwritePermissions().Modify(connect: PermValue.Deny);
                    if (hideVoie)
                    {
                        newPermissionOverride = newPermissionOverride.Modify(viewChannel: PermValue.Deny);
                    }

                    await tempChannel.AddPermissionOverwriteAsync(user, newPermissionOverride);
                }
            }
        }

        public static async Task BlockAllUserFromOwner(SocketGuildUser user, DiscordSocketClient client, RestVoiceChannel restVoiceChannel, SocketVoiceChannel socketVoiceChannel)
        {
            try
            {
                var usedFunctions = UsedFunctionsHelper.GetUsedFunctions(user.Id, user.Guild.Id).Result.Where(u => u.function == GlobalStrings.block).ToList();
                tempchannels tempChannel;
                var disabledCommands = new List<tempcommands>();
                var hideVoie = false;
                var permissions = new List<Overwrite>();

                if (socketVoiceChannel != null)
                {
                    tempChannel = TempChannelsHelper.GetTempChannel(socketVoiceChannel.Id).Result;
                    permissions = socketVoiceChannel.PermissionOverwrites.ToList();
                }
                else
                {
                    tempChannel = TempChannelsHelper.GetTempChannel(restVoiceChannel.Id).Result;
                    permissions = restVoiceChannel.PermissionOverwrites.ToList();
                }

                disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(user.Guild.Id, tempChannel.createchannelid.Value).Result;
                hideVoie = disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.hidevoicefromblockedusers) == null;

                if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.block) != null)
                {
                    return;
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

                if (socketVoiceChannel != null)
                {
                    await socketVoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                }
                else
                {
                    await restVoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C266", parameter.Language).Result, GeneralHelper.GetCaption("C249", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result }, ephemeral: true);

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

                _ = UsedFunctionsHelper.AddUsedFunction(parameter.GuildUser.Id, 0, GlobalStrings.LockKlein, voiceChannel.Id, parameter.GuildID);

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

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.LockKlein, parameter.GuildUser.VoiceChannel.Id).Result == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C266", parameter.Language).Result, GeneralHelper.GetCaption("C250", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result }, ephemeral: true);

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

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.hide, parameter.GuildUser.VoiceChannel.Id).Result != null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C266", parameter.Language).Result, GeneralHelper.GetCaption("C251", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result }, ephemeral: true);

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

                _ = UsedFunctionsHelper.AddUsedFunction(parameter.GuildUser.Id, 0, GlobalStrings.hide, parameter.GuildUser.VoiceChannel.Id, parameter.GuildID);

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
                var user = parameter.Guild.GetUser(ownerId);
                await TempChannelsHelper.ChangeOwner(parameter.GuildUser.VoiceChannel.Id, parameter.GuildUser.Id);

                await TempChannelHelper.RemoveManageChannelRightsToUserVc(user, voiceChannel);
                await TempChannelHelper.GiveManageChannelRightsToUserVc(parameter.GuildUser, parameter.GuildID, null, voiceChannel);

                await UnblockAllUsersFromPreviousOwner(user, voiceChannel);
                await BlockAllUserFromOwner(parameter.GuildUser, parameter.Client, null, voiceChannel);
                _ = UnmuteIfNewOwnerAndMuted(parameter);

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

            if (UsedFunctionsHelper.GetUsedFunction(GlobalStrings.hide, parameter.GuildUser.VoiceChannel.Id).Result == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C266", parameter.Language).Result, GeneralHelper.GetCaption("C252", parameter.Language).Result),
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
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
                        if (UserIsMuted(voiceChannel.Id, parameter.GuildUser.Id, user.Id))
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
                    await UsedFunctionsHelper.AddUsedFunction(parameter.GuildUser.Id, user, GlobalStrings.mute, voiceChannel.Id, parameter.GuildID);
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
                        GeneralHelper.GetContent("C265", parameter.Language).Result,
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
                        if (!UserIsMuted(voiceChannel.Id, parameter.GuildUser.Id, user.Id))
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
                    _ = guildUser.ModifyAsync(u => u.Channel = voiceChannel);
                    _ = guildUser.ModifyAsync(u => u.Mute = false);

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
                        GeneralHelper.GetContent("C265", parameter.Language).Result,
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

        public static bool UserIsMuted(ulong voiceChannelId, ulong userId, ulong affectedUserId)
        {
            return UsedFunctionsHelper.GetUsedFunction(GlobalStrings.mute, voiceChannelId, userId, affectedUserId).Result != null;
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
                        if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, ulong.Parse(userId), GlobalStrings.block, parameter.GuildID).Result == null)
                        {
                            checkPermissionString = String.Format(GeneralHelper.GetContent("C258", parameter.Language).Result, GeneralHelper.GetCaption("C247", parameter.Language).Result);
                        }
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
                    _ = UsedFunctionsHelper.RemoveUsedFunction(parameter.GuildUser.Id, user, GlobalStrings.block, parameter.GuildID);
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
                        GeneralHelper.GetContent("C265", parameter.Language).Result,
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
                        if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, userToBeBlocked.Id, GlobalStrings.block, parameter.GuildID).Result != null)
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
                    _ = UsedFunctionsHelper.AddUsedFunction(parameter.GuildUser.Id, user, GlobalStrings.block, 0, parameter.GuildID);
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
                        GeneralHelper.GetContent("C265", parameter.Language).Result,
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

        public static async Task TempInfo(SlashCommandParameter parameter)
        {
            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
            if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempInfo)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempInfo)).Result)
            {
                return;
            }

            var infoString = GetTempInfoString(parameter);
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            infoString,
                            parameter.GuildUser.VoiceChannel.Name).Result }, ephemeral: true);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempInfo), new SlashCommandParameter() { Guild = parameter.Guild, GuildUser = parameter.GuildUser },
                message: "/temp info successfully used");
        }

        public static string GetTempInfoString(SlashCommandParameter parameter)
        {
            var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
            var sb = new StringBuilder();
            sb.AppendLine(String.Format(GeneralHelper.GetContent("C278", parameter.Language).Result, tempChannel.unixtimestamp));
            sb.AppendLine();
            var appendLine = false;

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

            if (appendLine)
            {
                sb.AppendLine();
            }

            sb.AppendLine(String.Format(GeneralHelper.GetContent("C263", parameter.Language).Result, tempChannel.channelownerid.Value));
            sb.AppendLine();


            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(parameter.Guild.Id, tempChannel.createchannelid.Value).Result;
            if (disabledCommands.FirstOrDefault(d => d.commandname == GlobalStrings.block) == null)
            {
                var blockedUsers = UsedFunctionsHelper.GetUsedFunctions(tempChannel.channelownerid.Value, tempChannel.guildid).Result
                    .Where(u => u.function == GlobalStrings.block)
                    .ToList();

                if (blockedUsers.Count > 0)
                {
                    sb.AppendLine(GeneralHelper.GetContent("C264", parameter.Language).Result);
                }
                foreach (var blockedUser in blockedUsers)
                {
                    sb.AppendLine($"<@{blockedUser.affecteduserid}>");
                }
                sb.AppendLine();
            }

            var mutedUsers = UsedFunctionsHelper.GetMutedUsedFunctions(tempChannel.channelid).Result;

            if (mutedUsers.Count > 0)
            {
                sb.AppendLine(GeneralHelper.GetContent("C277", parameter.Language).Result);
            }
            foreach (var user in mutedUsers)
            {
                sb.AppendLine($"<@{user.affecteduserid}>");
            }

            return sb.ToString();
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

                var voiceChannel = parameter.Guild.GetVoiceChannel(tempChannel.channelid);

                await TempChannelHelper.RemoveManageChannelRightsToUserVc(currentOwner, voiceChannel);
                await TempChannelHelper.GiveManageChannelRightsToUserVc(newOwner, parameter.GuildID, null, voiceChannel);

                await TempChannelsHelper.ChangeOwner(parameter.GuildUser.VoiceChannel.Id, userId.ToUlong());
                await UnblockAllUsersFromPreviousOwner(parameter.GuildUser, parameter.GuildUser.VoiceChannel);
                var guildUser = parameter.Guild.GetUser(userId.ToUlong());
                await BlockAllUserFromOwner(guildUser, parameter.Client, null, voiceChannel);
                _ = UnmuteIfNewOwnerAndMuted(parameter);

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

        public static Bitmap GetButtonsBitmap(Dictionary<ButtonBuilder, System.Drawing.Image> dict)
        {
            var bitmap = GetRightSizedBitmap(dict.Count());

            using Graphics g = Graphics.FromImage(bitmap);
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.Clear(System.Drawing.Color.Transparent);

            var x = 0;
            var y = 0;
            var count = 0;
            foreach (var image in dict.Values)
            {
                count++;
                g.DrawImage(image, x, y, 200, 60);
                x += 240;
                if (count == 4)
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
                case 1: case 2: case 3: case 4:
                    return new Bitmap(920, 60);
                case 5: case 6: case 7: case 8:
                    return new Bitmap(920, 150);
                case 9: case 10: case 11: case 12:
                    return new Bitmap(920, 240);
                case 13: case 14: case 15: case 16:
                    return new Bitmap(920, 330);
                default:
                    return new Bitmap(920, 330);

            }
        }


        public static async Task SaveNewInterfaceButtonPicture(DiscordSocketClient client, List<tempcommands> disabledCommands, ulong createTempChannelId)
        {
            var buttonsMitBildern = GetInterfaceButtonsMitBild(client, disabledCommands).Result;
            var buttonComponentBuilder = GetButtonsComponentBuilder(buttonsMitBildern);
            var img = GetButtonsBitmap(buttonsMitBildern);
            img.Save($"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public static string GetOrSaveAndGetButtonsImageName(DiscordSocketClient client, List<tempcommands> disabledCommands, ulong createTempChannelId)
        {
            var filePath = $"{Directory.GetCurrentDirectory()}/{createTempChannelId}_buttons.png";
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

            var imgFileNameAttachement = "";
            var fileName = "";
            //try
            //{
            //    fileName = GetOrSaveAndGetButtonsImageName(client, disabledCommands, tempChannelEntity.createchannelid.Value);
            //    imgFileNameAttachement = $"attachment://{fileName}";
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
                imgFileNameAttachement = "https://cdn.discordapp.com/attachments/910868343030960129/1152272115039481887/964126199603400705_buttons.png";
            //}

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
                //if(CommandDisabled(disabledCommands, command))
                //{
                //    continue;
                //}

                var button = GetButton($"temp-interface-{command}", Emojis()[command], command, CommandDisabled(disabledCommands, command));
                System.Drawing.Image image;
                try
                {
                    image = System.Drawing.Image.FromFile($"{Directory.GetCurrentDirectory()}/buttons/{command}button.png");
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
                { "hide", "<:hidenew:1149745796057669775>"},
                { "unhide", "<:unhidenew:1149745799136280606>"},
                { "kick", "<:userkickednew:1149730990680461426>" },
                { "block", "<:userblockednew:1149731203205845002>"},
                { "unblock", "<:userunblockednew:1149731419195707592>"},
                { "saveconfig", "<:config:1138181363338588351>"},
                { "deleteconfig", "<:noconfig:1138181406799966209>"},
                { "size", "<:userlimitnew:1151507242651238421>"},
                { "giveowner", "<:ownergive:1149325094045356072>"},
                { "claimowner", "<:ownerclaim:1149325095488204810>" },
                { "info", "<:info:1150356769873342516>"},
                // nicht wirklich das mute emote, das ist das unmute aber wegen dem Slashcommand wird das hier unter mute benutzt
                { "mute", "<:unmute:1151506858750775326>"},
                { "muteemote", "<:mute:1151506855659585626>"}

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
            sb.AppendLine(GetCommandsTable(parameter, disabledCommands, new List<string>() { "interface", "ownerpermissions", GlobalStrings.kickblockedusersonownerchange, GlobalStrings.hidevoicefromblockedusers }, "C243", false));

            return GeneralHelper.CreateEmbed(parameter.Interaction, sb.ToString(), header).Result;
        }

        public static string GetCommandsTable(SlashCommandParameter parameter, List<tempcommands> disabledCommands, List<string> commands, string spc, bool tempCommands = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine("╔════════════════════════════════╦═══════════╗");
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

            sb.AppendLine("╚════════════════════════════════╩═══════════╝");
            sb.AppendLine("```");

            return sb.ToString();
        }

        public static void AddRow(StringBuilder sb, string command, string active, bool lastRow = false, string temp = "/temp ")
        {
            var str = $"║ {temp}{command}";
            str = Auffuellen(str, 34, "║");

            str += $" {active}";
            str = Auffuellen(str, 46, "║");

            sb.AppendLine(str);
            if (!lastRow)
            {
                sb.AppendLine("╠════════════════════════════════╬═══════════╣");
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