﻿using Bobii.src.AutocompleteHandler;
using Bobii.src.Bobii;
using Bobii.src.Helper;
using Bobii.src.Models;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.Slashcommands
{
    public class TempChannelSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // im createcommandlist ignorieren
        [SlashCommand("temptoggle", "Enables or disables a temp command")]
        public async Task TempToggle(
             [Summary("command", "Choose the command which you want to toggle on or off")][Autocomplete(typeof(TempCommandToggleHandler))] string command,
             [Summary("enabled", "Choose if the command should be enabled or not")] bool enabled)
        {
            var parameter = Context.ContextToParameter();
            var tempCommandGroup = parameter.Client.GetGlobalApplicationCommandsAsync().Result.Single(c => c.Name == "temp").Options;
            // TODO hier die die Option mit dran hängen
            var slashTemp = "/temp ";

            if (tempCommandGroup.FirstOrDefault(c => c.Name == command) == null && command != "ownerpermissions")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C181", parameter.Language).Result,
                             GeneralHelper.GetCaption("C181", parameter.Language).Result).Result }, ephemeral: true);
                return;
            }

            if (CheckDatas.CheckUserPermission(parameter, nameof(TempToggle)).Result)
            {
                return;
            }

            if (enabled)
            {
                if (!TempCommandsHelper.DoesCommandExist(parameter.GuildID, command).Result)
                {
                    if (command == "ownerpermissions")
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C190", parameter.Language).Result, command),
                             GeneralHelper.GetCaption("C182", parameter.Language).Result).Result }, ephemeral: true);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempToggle), parameter,
                            message: $"/temptoggel - {command} already enabled");
                    }
                    else
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C182", parameter.Language).Result, slashTemp + command),
                             GeneralHelper.GetCaption("C182", parameter.Language).Result).Result }, ephemeral: true);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempToggle), parameter,
                            message: $"/temptoggel - /temp {command} already enabled");
                    }

                    return;
                }

                await TempCommandsHelper.RemoveCommand(parameter.GuildID, command);
                if (command == "ownerpermissions")
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C191", parameter.Language).Result, command),
                             GeneralHelper.GetCaption("C183", parameter.Language).Result).Result }, ephemeral: true);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempToggle), parameter,
                        message: $"/temptoggel successfully used - {command} enabled");
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C183", parameter.Language).Result, slashTemp + command),
                             GeneralHelper.GetCaption("C183", parameter.Language).Result).Result }, ephemeral: true);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempToggle), parameter,
                        message: $"/temptoggel successfully used - /temp {command} enabled");
                }

                return;
            }

            if (TempCommandsHelper.DoesCommandExist(parameter.GuildID, command).Result)
            {
                if (command == "ownerpermissions")
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C192", parameter.Language).Result, command),
                             GeneralHelper.GetCaption("C184", parameter.Language).Result).Result }, ephemeral: true);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempToggle), parameter,
                        message: $"/temptoggel - {command} already disabled");
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C184", parameter.Language).Result, slashTemp + command),
                             GeneralHelper.GetCaption("C184", parameter.Language).Result).Result }, ephemeral: true);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TempToggle), parameter,
                        message: $"/temptoggel - /temp {command} already disabled");
                }
                return;
            }

            await TempCommandsHelper.AddCommand(parameter.GuildID, command, true);

            if (command == "ownerpermissions")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C193", parameter.Language).Result, command),
                             GeneralHelper.GetCaption("C185", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempToggle), parameter,
                    message: $"/temptoggel successfully used - {command} disabled");
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C185", parameter.Language).Result, slashTemp + command),
                             GeneralHelper.GetCaption("C185", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempToggle), parameter,
                    message: $"/temptoggel successfully used - /temp {command} disabled");
            }
            return;
        }

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
                CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempName)).Result ||
                CheckDatas.CheckIfCommandIsDisabled(parameter, "name").Result)
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

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TempName), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                            message: "/tempname successfully used");
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
                        .AddTextInput(GeneralHelper.GetContent("C170", parameter.Language).Result, "new_name", TextInputStyle.Short, required: true, maxLength: 50, value: parameter.GuildUser.VoiceChannel.Name);
                    await parameter.Interaction.RespondWithModalAsync(mb.Build());
                }
            }

            [SlashCommand("size", "Updates the size of the temp channel")]
            public async Task TempSize(
            [Summary("newsize", "This will be the new temp-channel size")] int newsize)
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempSize(parameter, newsize);
            }

            [SlashCommand("saveconfig", "Saves the temp-channel config for the creation of new temp-channels")]
            public async Task SaveConfig()
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempSaveConfig(parameter);
            }

            [SlashCommand("deleteconfig", "Deletes the current config for the used create-temp-channel")]
            public async Task DeleteConfig()
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempDeleteConfig(parameter);
            }

            [SlashCommand("owner", "Updates the owner of the temp channel")]
            public async Task TempOwner()
            {
                var parameter = Context.ContextToParameter();
                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder(GeneralHelper.GetCaption("C234", parameter.Language).Result)
                    .WithCustomId("temp-interface-owner-menu")
                    .WithType(ComponentType.UserSelect);

                await parameter.Interaction.RespondAsync(
                    "",
                    components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                    ephemeral: true);
            }

            [SlashCommand("kick", "Removes a User from the temp channel")]
            public async Task TempKick()
            {
                var parameter = Context.ContextToParameter();

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

            [SlashCommand("block", "Removes a User from the temp channel")]
            public async Task TempBlock()
            {
                var parameter = Context.ContextToParameter();

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

            [SlashCommand("unblock", "Removes a User from the temp channel")]
            public async Task TempUnBlock()
            {
                var parameter = Context.ContextToParameter();

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

            [SlashCommand("lock", "Locks the temp channel")]
            public async Task TempLock()
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempLock(parameter);
            }

            [SlashCommand("unlock", "Unlocks the temp channel")]
            public async Task TempUnLock()
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempUnLock(parameter);
            }

            [SlashCommand("hide", "Hides the temp channel")]
            public async Task TempHide()
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempHide(parameter);
            }

            [SlashCommand("unhide", "Makes the temp channel visible again")]
            public async Task TempUnHide()
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempUnHide(parameter);
            }
        }
    }
}
