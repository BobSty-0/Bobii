using Bobii.src.AutocompleteHandler;
using Bobii.src.Bobii;
using Bobii.src.Helper;
using Bobii.src.Modals;
using Bobii.src.Models;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.Slashcommands
{
    public class TempChannelSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("temp", "Includes all commands to edit temp channels")]
        public class CreateTempChannel : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("name", "Updates the name of the temp channel")]
            public async Task TempName(
            [Summary("newname", "This will be the new temp-channel name")] string newname = "")
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempName)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempName)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempName)).Result)
                {
                    return;
                }

                if (newname != "")
                {
                    try
                    {
                        if (CheckDatas.CheckStringLength(parameter, newname, 50, "the channel name", nameof(TempName)).Result)
                        {
                            return;
                        }

                        _ = Task.Run(async () => parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.Name = newname));

                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C118", parameter.Language).Result, newname),
                             GeneralHelper.GetCaption("C118", parameter.Language).Result).Result }, ephemeral: true);

                        var wtcParameter = new WriteToConsoleParameter
                        {
                            Category = Actions.SlashComms,
                            Error = false,
                            Task = nameof(TempName),
                            Guild = parameter.Guild,
                            GuildUser = parameter.GuildUser,
                            TempChannelID = parameter.GuildUser.VoiceChannel.Id,
                            Message = "/tempname successfully used"
                        };

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(wtcParameter);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempName), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "/tempname successfully used");
                    }
                    catch (Exception ex)
                    {
                        var wtcParameter = new WriteToConsoleParameter
                        {
                            Category = Actions.SlashComms,
                            Error = true,
                            Task = nameof(TempName),
                            Guild = parameter.Guild,
                            GuildUser = parameter.GuildUser,
                            TempChannelID = parameter.GuildUser.VoiceChannel.Id,
                            Message = "Failed to change temp-channel name",
                            ErrorMessage = ex.Message
                        };

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(wtcParameter);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempName), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "Failed to change temp-channel name", exceptionMessage: ex.Message);


                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C119", parameter.Language).Result,
                            GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                        return;
                    }
                }
                else
                {
                    var mb = new ModalBuilder()
                        .WithTitle(GeneralHelper.GetCaption("C173", parameter.Language).Result)
                        .WithCustomId($"tempchannel_update_name_modal{parameter.GuildUser.VoiceChannel.Id},{parameter.Language}")
                        .AddTextInput(GeneralHelper.GetContent("C170", parameter.Language).Result, "new_name", TextInputStyle.Short, required: true, maxLength: 50, value: parameter.GuildUser.VoiceChannel.Name);
                    await parameter.Interaction.RespondWithModalAsync(mb.Build());
                }
            }

            [SlashCommand("size", "Updates the size of the temp channel")]
            public async Task TempSize(
            [Summary("newsize", "This will be the new temp-channel size")] int newsize)
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);


                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempSize)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempSize)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempSize)).Result)
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

            [SlashCommand("saveconfig", "Saves the temp-channel config for the creation of new temp-channels")]
            public async Task SaveConfig()
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(SaveConfig)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(SaveConfig)).Result)
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

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(SaveConfig), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp saveconfig successfully used");
            }

            [SlashCommand("deleteconfig", "Deletes the current config for the used create-temp-channel")]
            public async Task DeleteConfig()
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(DeleteConfig)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(DeleteConfig)).Result ||
                CheckDatas.CheckIfUserTempChannelConfigExists(parameter, nameof(DeleteConfig)).Result)
                {
                    return;
                }

                var currentVC = parameter.GuildUser.VoiceState.Value.VoiceChannel;
                var tempChannel = TempChannelsHelper.GetTempChannel(currentVC.Id).Result;

                await TempChannelUserConfig.DeleteConfig(parameter.GuildUser.Id, tempChannel.createchannelid.Value);

                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C180", parameter.Language).Result,
                             GeneralHelper.GetCaption("C180", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(DeleteConfig), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temp deleteconfig successfully used");
            }

            [SlashCommand("owner", "Updates the owner of the temp channel")]
            public async Task TempOwner(
            [Summary("newowner", "Choose the user which you want to promote to the owner")][Autocomplete(typeof(TempChannelUpdateChannelOwnerHandler))] string userId)
            {
                var parameter = Context.ContextToParameter();

                if (userId == GeneralHelper.GetContent("C094", parameter.Language).Result.ToLower())
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C123", parameter.Language).Result,
                    GeneralHelper.GetCaption("C123", parameter.Language).Result).Result });
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempOwner), parameter, message: "Could not find any users");
                    return;
                }

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckUserID(parameter, userId, nameof(TempOwner)).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempOwner)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempOwner)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempOwner)).Result ||
                    CheckDatas.CheckIfUserInSameTempVoice(parameter, userId.ToUlong(), nameof(TempOwner)).Result)
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
                    await TempChannelHelper.GiveManageChannelRightsToUserVc(newOwner, null, voiceChannel as SocketVoiceChannel);


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

            [SlashCommand("kick", "Removes a User from the temp channel")]
            public async Task TempKick(
            [Summary("user", "Choose the user which you want to remove")][Autocomplete(typeof(TempChannelUpdateChannelOwnerHandler))] string userId)
            {
                var parameter = Context.ContextToParameter();

                // TODO Build here a new check system for autocomplete, and also for the other once
                if (userId == GeneralHelper.GetContent("C093", parameter.Language).Result.ToLower())
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C127", parameter.Language).Result,
                    GeneralHelper.GetCaption("C127", parameter.Language).Result).Result });
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempKick), parameter, message: "Could not find any users");
                    return;
                }

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckUserID(parameter, userId, nameof(TempKick)).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempKick)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempKick)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempKick)).Result ||
                    CheckDatas.CheckIfUserInSameTempVoice(parameter, userId.ToUlong(), nameof(TempKick)).Result)
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

            [SlashCommand("block", "Removes a User from the temp channel")]
            public async Task TempBlock(
            [Summary("user", "@ the user which you want to block")] IUser user)
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);
                var userId = user.Id;

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempBlock)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempBlock)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempBlock)).Result ||
                    CheckDatas.CheckIfUserInGuild(parameter, userId, nameof(TempBlock)).Result)
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

            [SlashCommand("unblock", "Removes a User from the temp channel")]
            public async Task TempUnBlock(
            [Summary("user", "@ the user which you want to unblock")] IUser user)
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                var userId = user.Id;

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnBlock)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnBlock)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnBlock)).Result ||
                    CheckDatas.CheckIfUserInGuild(parameter, userId, nameof(TempUnBlock)).Result)
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

            [SlashCommand("lock", "Locks the temp channel")]
            public async Task TempLock()
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock)).Result)
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

            [SlashCommand("unlock", "Unlocks the temp channel")]
            public async Task TempUnLock()
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnLock)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnLock)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnLock)).Result)
                {
                    return;
                }

                try
                {
                    var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
                    var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

                    var everyoneRole = parameter.Guild.Roles.Where(role => role.Name == "@everyone").First();
                    var value = createTempChannel.GetPermissionOverwrite(everyoneRole).Value;

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

            [SlashCommand("hide", "Hides the temp channel")]
            public async Task TempHide()
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempHide)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempHide)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempHide)).Result)
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

            [SlashCommand("unhide", "Makes the temp channel visible again")]
            public async Task TempUnHide()
            {
                var parameter = Context.ContextToParameter();

                await  TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock)).Result ||
                    CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock)).Result)
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
        }
    }
}
