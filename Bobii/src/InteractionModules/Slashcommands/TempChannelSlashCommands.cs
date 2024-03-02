using Bobii.src.AutocompleteHandler;
using Bobii.src.Bobii;
using Bobii.src.Bobii.EntityFramework;
using Bobii.src.Handler;
using Bobii.src.Helper;
using Bobii.src.Models;
using Bobii.src.TempChannel;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TwitchLib.Communication.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bobii.src.InteractionModules.Slashcommands
{
    public class TempChannelSlashCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        [UserCommand("Kick")]
        public async Task TempKickContext(SocketGuildUser affectedUser)
        {
            var parameter = Context.ContextToParameter(false);
            _ = TempChannelHelper.TempKick(parameter, new List<string> { affectedUser.Id.ToString() }, false);
        }

        [UserCommand("Mute")]
        public async Task TempMuteContext(SocketGuildUser affectedUser)
        {
            var parameter = Context.ContextToParameter(false);
            _ = TempChannelHelper.TempMute(parameter, new List<string> { affectedUser.Id.ToString() }, false);
        }

        [UserCommand("Unmute")]
        public async Task TempUnMuteContext(SocketGuildUser affectedUser)
        {
            var parameter = Context.ContextToParameter(false);
            _ = TempChannelHelper.TempUnMute(parameter, new List<string> { affectedUser.Id.ToString() }, false);
        }

        [UserCommand("Block")]
        public async Task TempBlockContext(SocketGuildUser affectedUser)
        {
            var parameter = Context.ContextToParameter(false);
            _ = TempChannelHelper.TempBlock(parameter, new List<string> { affectedUser.Id.ToString() }, false);
        }


        // im createcommandlist ignorieren
        [SlashCommand("temptoggle", "Enables or disables a temp command")]
        public async Task TempToggle(
             [Summary("createvoicechannel", "Choose the channel which you want to update")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID,
             [Summary("command", "Choose the command which you want to toggle on or off")][Autocomplete(typeof(TempCommandToggleHandler))] string command,
             [Summary("enabled", "Choose if the command should be enabled or not")] bool enabled)
        {
            var parameter = Context.ContextToParameter();

            if (CheckDatas.CheckUserPermission(parameter, nameof(TempToggle)).Result)
            {
                return;
            }

            var tempCommandGroup = HandlingService.SlashCommands.Single(c => c.Name == "temp").Parameters;
            // TODO hier die die Option mit dran hängen
            var slashTemp = "/temp ";

            if (tempCommandGroup.FirstOrDefault(c => c.Name == command) == null &&
                command != "ownerpermissions" &&
                command != GlobalStrings.InterfaceKlein &&
                command != GlobalStrings.kickblockedusersonownerchange &&
                command != GlobalStrings.hidevoicefromblockedusers &&
                command != GlobalStrings.autotransferowner)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C181", parameter.Language).Result,
                             GeneralHelper.GetCaption("C181", parameter.Language).Result).Result }, ephemeral: true);
                return;
            }

            if (createVoiceChannelID == GeneralHelper.GetContent("C096", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C110", parameter.Language).Result,
                            GeneralHelper.GetCaption("C110", parameter.Language).Result).Result },
                    ephemeral: true);
                await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempToggle), parameter, message: "Could not find any channels");
                return;
            }

            if (CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(TempToggle), true).Result ||
                    CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(TempToggle)).Result)
            {
                return;
            }

            if (enabled)
            {
                if (!TempCommandsHelper.DoesCommandExist(parameter.GuildID, ulong.Parse(createVoiceChannelID), command).Result)
                {
                    if (command == "ownerpermissions")
                    {
                        await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C296", "C277", "C182");
                    }
                    else if (command == GlobalStrings.InterfaceKlein)
                    {
                        await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C296", "C276", "C182");
                    }
                    else if (command == GlobalStrings.kickblockedusersonownerchange)
                    {
                        await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C296", "C275", "C182");
                    }
                    else if (command == GlobalStrings.hidevoicefromblockedusers)
                    {
                        await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C296", "C274", "C182");
                    }
                    else if (command == GlobalStrings.autotransferowner)
                    {
                        await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C296", "C273", "C182");
                    }
                    else
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C182", parameter.Language).Result, slashTemp + command),
                             GeneralHelper.GetCaption("C182", parameter.Language).Result).Result }, ephemeral: true);
                    }
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempToggle), parameter,
                        message: $"/temptoggel - /temp {command} already enabled");

                    return;
                }

                await TempCommandsHelper.RemoveCommand(parameter.GuildID, ulong.Parse(createVoiceChannelID), command);
                if (command == "ownerpermissions")
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C297", "C277", "C183");
                }
                else if (command == GlobalStrings.InterfaceKlein)
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C297", "C276", "C183");
                }
                else if (command == GlobalStrings.kickblockedusersonownerchange)
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C297", "C275", "C183");
                }
                else if (command == GlobalStrings.hidevoicefromblockedusers)
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C297", "C274", "C183");
                }
                else if (command == GlobalStrings.autotransferowner)
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C297", "C273", "C183");
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C183", parameter.Language).Result, slashTemp + command),
                             GeneralHelper.GetCaption("C183", parameter.Language).Result).Result }, ephemeral: true);
                    var disabledCommands1 = TempCommandsHelper.GetDisabledCommandsFromGuild(parameter.GuildID, ulong.Parse(createVoiceChannelID)).Result;
                    _ = TempChannelHelper.SaveNewInterfaceButtonPicture(parameter.Client, disabledCommands1, ulong.Parse(createVoiceChannelID));
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempToggle), parameter,
                    message: $"/temptoggel successfully used - {command} enabled");

                return;
            }

            if (TempCommandsHelper.DoesCommandExist(parameter.GuildID, ulong.Parse(createVoiceChannelID), command).Result)
            {
                if (command == "ownerpermissions")
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C298", "C277", "C184");
                }
                else if (command == GlobalStrings.InterfaceKlein)
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C298", "C276", "C184");
                }
                else if (command == GlobalStrings.kickblockedusersonownerchange)
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C298", "C275", "C184");
                }
                else if (command == GlobalStrings.hidevoicefromblockedusers)
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C298", "C274", "C184");
                }
                else if (command == GlobalStrings.autotransferowner)
                {
                    await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C298", "C273", "C184");
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C184", parameter.Language).Result, slashTemp + command),
                             GeneralHelper.GetCaption("C184", parameter.Language).Result).Result }, ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempToggle), parameter,
                    message: $"/temptoggel - {command} already disabled");
                return;
            }

            await TempCommandsHelper.AddCommand(parameter.GuildID, command, true, ulong.Parse(createVoiceChannelID));

            if (command == "ownerpermissions")
            {
                await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C299", "C277", "C185");
            }
            else if (command == GlobalStrings.InterfaceKlein)
            {
                await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C299", "C276", "C185");
            }
            else if (command == GlobalStrings.kickblockedusersonownerchange)
            {
                await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C299", "C275", "C185");
            }
            else if (command == GlobalStrings.hidevoicefromblockedusers)
            {
                await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C299", "C274", "C185");
            }
            else if (command == GlobalStrings.autotransferowner)
            {
                await TempChannelHelper.ReplyToTempToggleFunction(parameter, "C299", "C273", "C185");
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C185", parameter.Language).Result, slashTemp + command),
                             GeneralHelper.GetCaption("C185", parameter.Language).Result).Result }, ephemeral: true);
            }

            var disabledCommands = TempCommandsHelper.GetDisabledCommandsFromGuild(parameter.GuildID, ulong.Parse(createVoiceChannelID)).Result;
            _ = TempChannelHelper.SaveNewInterfaceButtonPicture(parameter.Client, disabledCommands, ulong.Parse(createVoiceChannelID));

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempToggle), parameter,
                message: $"/temptoggel successfully used - {command} disabled");
            return;
        }

        [Group("temp", "Includes all commands to edit temp channels")]
        public class CreateTempChannel : InteractionModuleBase<ShardedInteractionContext>
        {
            [SlashCommand("name", "Updates the name of the temp channel")]
            public async Task TempName(
            [Summary("newname", "This will be the new temp channel name")] string newname = "",
            [Summary("newstatus", "This will be the new temp channel status")] string status = "")
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempName)).Result ||
                CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempName)).Result ||
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempName)).Result)
                {
                    return;
                }

                var tempChannelEntity = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "name", tempChannelEntity.createchannelid.Value).Result)
                {
                    return;
                }

                if (newname != "" || status != "")
                {
                    try
                    {
                        if (CheckDatas.CheckStringLength(parameter, newname, 50, GeneralHelper.GetCaption("C300", parameter.Language).Result, nameof(TempName)).Result ||
                            CheckDatas.CheckStringLength(parameter, status, 50, GeneralHelper.GetCaption("C301", parameter.Language).Result, nameof(TempName)).Result)
                        {
                            return;
                        }

                        var rateLimitHandler = new RateLimitHandler(parameter);

                        var optionsName = new RequestOptions()
                        {
                            RatelimitCallback = rateLimitHandler.MyRatelimitCallback
                        };

                        var optionsStatus = new RequestOptions()
                        {
                            RatelimitCallback = rateLimitHandler.MyRatelimitCallback
                        };

                        _ = Task.Run(async () =>
                        {
                            if (newname == "")
                            {
                                newname = parameter.GuildUser.VoiceChannel.Name;
                            }
                            if (status == "")
                            {
                                status = parameter.GuildUser.VoiceChannel.Status;
                            }
                            if (status != parameter.GuildUser.VoiceChannel.Status)
                            {
                                await parameter.GuildUser.VoiceChannel.SetStatusAsync(status, options: optionsName);
                            }

                            if (parameter.GuildUser.VoiceChannel.Name != newname)
                            {
                                await parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.Name = newname, options: optionsStatus);
                            }

                            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C118", parameter.Language).Result, newname, status),
                             GeneralHelper.GetCaption("C118", parameter.Language).Result).Result }, ephemeral: true);

                            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempName), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                                message: "/tempname successfully used");
                        });
                    }
                    catch (Exception ex)
                    {
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
                        .AddTextInput(GeneralHelper.GetContent("C170", parameter.Language).Result, "new_name", TextInputStyle.Short, required: true, maxLength: 50, value: parameter.GuildUser.VoiceChannel.Name)
                        .AddTextInput(GeneralHelper.GetContent("C340", parameter.Language).Result, "new_status", TextInputStyle.Short, required: true, maxLength: 50, value: parameter.GuildUser.VoiceChannel.Status);
                    await parameter.Interaction.RespondWithModalAsync(mb.Build());
                }
            }

            [SlashCommand("size", "Updates the size of the temp channel")]
            public async Task TempSize(
            [Summary("newsize", "This will be the new temp channel size")] int newsize)
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempSize(parameter, newsize);
            }

            [SlashCommand("claimowner", "Updates the owner of the temp channel")]
            public async Task TempClaimOwner()
            {
                var parameter = Context.ContextToParameter();

                await TempChannelHelper.TempClaimOwner(parameter);
            }


            [SlashCommand("giveowner", "Updates the owner of the temp channel")]
            public async Task TempGiveOwner()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, "giveowner").Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, "giveowner").Result)
                {
                    return;
                }
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "giveowner", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempName)).Result)
                {
                    return;
                }

                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder(GeneralHelper.GetCaption("C234", parameter.Language).Result)
                    .WithCustomId("temp-interface-owner-menu")
                    .WithType(ComponentType.UserSelect);

                await parameter.Interaction.RespondAsync(
                    "",
                    components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                    ephemeral: true);
            }

            [SlashCommand("privacy", "Command to manage the privacy of the channel")]
            public async Task TempPrivacy()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempPrivacy)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempPrivacy)).Result)
                {
                    return;
                }

                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "privacy", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempWhitelist)).Result)
                {
                    return;
                }

                var selectionMenuBuilder = TempChannelHelper.PrivacySelectionMenu(parameter);
                await parameter.Interaction.RespondAsync(
                    "",
                            components: new ComponentBuilder().WithSelectMenu(selectionMenuBuilder).Build(),
                            ephemeral: true);
            }

            [SlashCommand("block", "Blocks users from the temp channel")]
            public async Task TempBlock()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, "block").Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, "block").Result)
                {
                    return;
                }
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "block", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempBlock)).Result)
                {
                    return;
                }

                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder(GeneralHelper.GetCaption("C239", parameter.Language).Result)
                    .WithMinValues(1)
                    .WithMaxValues(5)
                    .WithCustomId("temp-interface-block-menu")
                    .WithType(ComponentType.UserSelect);

                await parameter.Interaction.RespondAsync(
                    "",
                    components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                    ephemeral: true);
            }

            [SlashCommand("unblock", "Unblocks users from the temp channel")]
            public async Task TempUnBlock()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, "unblock").Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, "unblock").Result)
                {
                    return;
                }
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "unblock", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnBlock)).Result)
                {
                    return;
                }

                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder(GeneralHelper.GetCaption("C240", parameter.Language).Result)
                    .WithMinValues(1)
                    .WithMaxValues(5)
                    .WithCustomId("temp-interface-unblock-menu")
                    .WithType(ComponentType.UserSelect);

                await parameter.Interaction.RespondAsync(
                    "",
                    components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                    ephemeral: true);
            }

            [SlashCommand("kick", "Removes users from the temp channel")]
            public async Task TempKick()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, "kick").Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, "kick").Result)
                {
                    return;
                }
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "kick", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempKick)).Result)
                {
                    return;
                }

                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder(GeneralHelper.GetCaption("C235", parameter.Language).Result)
                    .WithMinValues(1)
                    .WithMaxValues(5)
                    .WithCustomId("temp-interface-kick-menu")
                    .WithType(ComponentType.UserSelect);

                await parameter.Interaction.RespondAsync(
                "",
                        components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                        ephemeral: true);
            }

            [SlashCommand("mute", "Mutes or unmutes user")]
            public async Task TempMute()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, "mute").Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, "mute").Result)
                {
                    return;
                }
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "mute", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }
                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "mute").Result)
                {
                    return;
                }
                var selectionMenuBuilder = TempChannelHelper.MuteSelectionMenu(parameter);
                await parameter.Interaction.RespondAsync(
                    "",
                            components: new ComponentBuilder().WithSelectMenu(selectionMenuBuilder).Build(),
                            ephemeral: true);
            }

            [SlashCommand("chat", "Manages the messages of the temp channel chat")]
            public async Task TempMessages()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempMessages)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempMessages)).Result)
                {
                    return;
                }
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, GlobalStrings.chat, tempChannel.createchannelid.Value).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempMessages)).Result)
                {
                    return;
                }
                var selectionMenuBuilder = TempChannelHelper.MessagesSelectionMenu(parameter);
                await parameter.Interaction.RespondAsync(
                    "",
                            components: new ComponentBuilder().WithSelectMenu(selectionMenuBuilder).Build(),
                            ephemeral: true);
            }

            [SlashCommand("moderator", "Manages your moderators")]
            public async Task TempModerator()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempModerator)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempModerator)).Result)
                {
                    return;
                }
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, nameof(TempModerator), tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }
                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempModerator)).Result)
                {
                    return;
                }
                var selectionMenuBuilder = TempChannelHelper.ModeratorSelectionMenu(parameter);
                await parameter.Interaction.RespondAsync(
                    "",
                            components: new ComponentBuilder().WithSelectMenu(selectionMenuBuilder).Build(),
                            ephemeral: true);
            }

            [SlashCommand("interface", "Creates an interface")]
            public async Task TempInterface()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempInterface)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempInterface)).Result)
                {
                    return;
                }

                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "interface", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempInterface)).Result)
                {
                    return;
                }

                var guild = parameter.Client.Rest.GetGuildAsync(parameter.GuildID).Result;

                if (parameter.GuildUser.GuildPermissions.Administrator || parameter.GuildUser.GuildPermissions.ManageChannels)
                {
                    var channel = guild.GetTextChannelAsync(parameter.Interaction.Channel.Id).Result;
                    _ = TempChannelHelper.WriteInterfaceInVoiceChannel(channel, parameter.Client, tempChannel.createchannelid.Value);
                }
                else
                {
                    var channel = guild.GetTextChannelAsync(parameter.GuildUser.VoiceChannel.Id).Result;
                    _ = TempChannelHelper.WriteInterfaceInVoiceChannel(channel, parameter.Client, tempChannel.createchannelid.Value);
                }

                await parameter.Interaction.DeferAsync();
                await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
            }

            [SlashCommand("whitelist", "Command to manage the whitelist")]
            public async Task TempWhitelist()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempWhitelist)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempWhitelist)).Result)
                {
                    return;
                }

                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "whitelist", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempWhitelist)).Result)
                {
                    return;
                }
                var selectionMenuBuilder = TempChannelHelper.WhiteListSelectionMenu(parameter);
                await parameter.Interaction.RespondAsync(
                    "",
                            components: new ComponentBuilder().WithSelectMenu(selectionMenuBuilder).Build(),
                            ephemeral: true);
            }

            [SlashCommand("settings", "Saves or deletes your current settings")]
            public async Task TempSettings()
            {
                var parameter = Context.ContextToParameter();

                if (CheckDatas.CheckIfUserInVoice(parameter, nameof(TempSettings)).Result ||
                    CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempSettings)).Result)
                {
                    return;
                }

                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                if (CheckDatas.CheckIfCommandIsDisabled(parameter, "settings", tempChannel.createchannelid.Value, true).Result)
                {
                    return;
                }

                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempSettings)).Result)
                {
                    return;
                }

                var selectionMenuBuilder = TempChannelHelper.SettingsSelectionMenu(parameter);
                await parameter.Interaction.RespondAsync(
                    "",
                            components: new ComponentBuilder().WithSelectMenu(selectionMenuBuilder).Build(),
                            ephemeral: true);
            }

            [SlashCommand("info", "Returns information about the current temp-channel")]
            public async Task TempInfo()
            {
                var parameter = Context.ContextToParameter();
                var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
                var tempChannelConfig = TempChannelUserConfig.GetTempChannelConfig(parameter.GuildUser.Id, tempChannel.createchannelid.Value).Result;

                await TempChannelHelper.TempInfo(parameter, tempChannelConfig != null && tempChannelConfig.usernamemode);
            }
        }
    }
}
