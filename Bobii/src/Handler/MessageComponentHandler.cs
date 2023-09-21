using Bobii.src.Bobii;
using Bobii.src.Bobii.EntityFramework;
using Bobii.src.Helper;
using Bobii.src.Models;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using TwitchLib.Api.Helix;

namespace Bobii.src.Handler
{
    class MessageComponentHandlingService
    {
        public static async Task MessageComponentHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parameter = interaction.InteractionToParameter(client);
            var parsedArg = (SocketMessageComponent)interaction;
            var parsedUser = (SocketGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(parsedUser.Guild.Id).Result;
            //If SelectMenu
            if (parsedArg.Data.Values != null)
            {
                var commandName = parsedArg.Data.Values.First<string>();

                try
                {
                    if (ulong.TryParse(commandName, out ulong _))
                    {
                        var test = parsedArg.Data.CustomId;
                        var userIds = parsedArg.Data.Values.ToList<String>();
                        switch (test)
                        {
                            case "temp-interface-owner-menu":
                                await TempChannelHelper.TempOwner(parameter, commandName, true);
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-interface-kick-menu":
                                await TempChannelHelper.TempKick(parameter, userIds, true);
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-interface-block-menu":
                                await TempChannelHelper.TempBlock(parameter, userIds, true);
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-interface-unblock-menu":
                                await TempChannelHelper.TempUnBlock(parameter, userIds, true);
                                await parsedArg.DeferAsync();
                                break;
                            case "create-temp-channel-info":
                                await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] { TempChannelHelper.CreateCreateTempChannelInformation(parameter, ulong.Parse(parsedArg.Data.Values.First())) });
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-interface-mute-menu":
                                await TempChannelHelper.TempMute(parameter, userIds, true);
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-interface-unmute-menu":
                                await TempChannelHelper.TempUnMute(parameter, userIds, true);
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-interface-whitelist-add-menu":
                                await TempChannelHelper.TempWhiteListAdd(parameter, userIds);
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-interface-whitelist-remove-menu":
                                await TempChannelHelper.TempWhiteListRemove(parameter, userIds);
                                await parsedArg.DeferAsync();
                                break;
                        }
                    }
                    else
                    {
                        switch (commandName)
                        {
                            case "temp-channel-whitelist-remove":
                                await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
                                if (CheckDatas.CheckIfUserInVoice(parameter, "whitelistremove").Result ||
                                    CheckDatas.CheckIfUserInTempVoice(parameter, "whitelistremove").Result)
                                {
                                    return;
                                }

                                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "whitelistremove").Result)
                                {
                                    return;
                                }

                                var menuBuilder = new SelectMenuBuilder()
                                    .WithPlaceholder(GeneralHelper.GetContent("C287", parameter.Language).Result)
                                    .WithMinValues(1)
                                    .WithMaxValues(5)
                                    .WithCustomId("temp-interface-whitelist-remove-menu")
                                    .WithType(ComponentType.MentionableSelect);

                                await parsedArg.UpdateAsync(msg => msg.Components = new ComponentBuilder().WithSelectMenu(menuBuilder).Build());
                                break;
                            case "temp-channel-whitelist-add":
                                await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
                                if (CheckDatas.CheckIfUserInVoice(parameter, "whitelistadd").Result ||
                                    CheckDatas.CheckIfUserInTempVoice(parameter, "whitelistadd").Result)
                                {
                                    return;
                                }

                                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "whitelistadd").Result)
                                {
                                    return;
                                }

                                menuBuilder = new SelectMenuBuilder()
                                    .WithPlaceholder(GeneralHelper.GetContent("C280", parameter.Language).Result)
                                    .WithMinValues(1)
                                    .WithMaxValues(5)
                                    .WithCustomId("temp-interface-whitelist-add-menu")
                                    .WithType(ComponentType.MentionableSelect);

                                await parsedArg.UpdateAsync(msg => msg.Components = new ComponentBuilder().WithSelectMenu(menuBuilder).Build());
                                break;

                            case "temp-channel-whitelist-activate":
                                await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
                                if (CheckDatas.CheckIfUserInVoice(parameter, "whitelist-activate").Result ||
                                    CheckDatas.CheckIfUserInTempVoice(parameter, "whitelist-activate").Result)
                                {
                                    return;
                                }

                                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "whitelist-activate").Result)
                                {
                                    return;
                                }
                                _ =TempChannelHelper.ActivateWhiteList(parameter);
                                _ = parsedArg.DeferAsync();
                                break;
                            case "temp-channel-whitelist-deactivate":
                                await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
                                if (CheckDatas.CheckIfUserInVoice(parameter, "whitelist-deactivate").Result ||
                                    CheckDatas.CheckIfUserInTempVoice(parameter, "whitelist-deactivate").Result)
                                {
                                    return;
                                }

                                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "whitelist-deactivate").Result)
                                {
                                    return;
                                }
                                _ = TempChannelHelper.DeactivateWhiteList(parameter);
                                _ = parsedArg.DeferAsync();
                                break;
                            case "temp-channel-mute-all":
                                await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

                                if (CheckDatas.CheckIfUserInVoice(parameter, "muteall").Result ||
                                     CheckDatas.CheckIfUserInTempVoice(parameter, "muteall").Result)
                                {
                                    return;
                                }

                                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "muteall").Result)
                                {
                                    return;
                                }

                                await TempChannelHelper.TempMute(parameter, parameter.GuildUser.VoiceChannel.ConnectedUsers.Select(u => u.Id.ToString()).ToList(), true);
                                break;
                            case "temp-channel-unmute-all":
                                await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

                                if (CheckDatas.CheckIfUserInVoice(parameter, "unmuteall").Result ||
                                     CheckDatas.CheckIfUserInTempVoice(parameter, "unmuteall").Result)
                                {
                                    return;
                                }

                                if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "unmuteall").Result)
                                {
                                    return;
                                }

                                await TempChannelHelper.TempUnMute(parameter, parameter.GuildUser.VoiceChannel.ConnectedUsers.Select(u => u.Id.ToString()).ToList(), true);
                                break;
                            case "temp-channel-mute-users":
                                menuBuilder = new SelectMenuBuilder()
                                    .WithPlaceholder(GeneralHelper.GetCaption("C259", parameter.Language).Result)
                                    .WithMinValues(1)
                                    .WithMaxValues(5)
                                    .WithCustomId("temp-interface-mute-menu")
                                    .WithType(ComponentType.UserSelect);

                                await parsedArg.UpdateAsync(msg => msg.Components = new ComponentBuilder().WithSelectMenu(menuBuilder).Build());
                                break;
                            case "temp-channel-unmute-users":
                                menuBuilder = new SelectMenuBuilder()
                                    .WithPlaceholder(GeneralHelper.GetCaption("C260", parameter.Language).Result)
                                    .WithMinValues(1)
                                    .WithMaxValues(5)
                                    .WithCustomId("temp-interface-unmute-menu")
                                    .WithType(ComponentType.UserSelect);

                                await parsedArg.UpdateAsync(msg => msg.Components = new ComponentBuilder().WithSelectMenu(menuBuilder).Build());
                                break;
                            case "language-help-selectmenuotion":
                                await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] {
                            GeneralHelper.CreateEmbed(interaction, GeneralHelper.SpracheInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result, GeneralHelper.GetCaption("C196", language).Result).Result });
                                await Handler.HandlingService.BobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                    message: "Language help was chosen", hilfeSection: "language");
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-channel-help-selectmenuoption":
                                await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] {
                            GeneralHelper.CreateEmbed(interaction, TempChannelHelper.HelpTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result +
                            TempChannelHelper.HelpEditTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result, GeneralHelper.GetCaption("C170", language).Result).Result });
                                await Handler.HandlingService.BobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                    message: "Temp channel help was chosen", hilfeSection: "Temp Channel");
                                await parsedArg.DeferAsync();
                                break;
                            case "how-to-cereate-temp-channel-guide":
                                await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(interaction, GeneralHelper.GetContent("C169", language).Result, GeneralHelper.GetContent("C168", language).Result).Result });
                                await Handler.HandlingService.BobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Guide", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                     message: "temp-channel guide was chosen", hilfeSection: "Temp Channel");
                                await parsedArg.DeferAsync();
                                break;
                            case "how-to-text-utility-guide":
                                await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(interaction, GeneralHelper.GetContent("C202", language).Result, GeneralHelper.GetContent("C203", language).Result).Result });
                                await Handler.HandlingService.BobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Guide", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                    message: "text-utility guide was chosen", hilfeSection: "Text Utility");
                                await parsedArg.DeferAsync();
                                break;
                            case "text-utility-help-selectmenuotion":
                                await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(interaction, TextUtilityHelper.HelpTextUtilityInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result +
                           "\n\n" + StealEmojiHelper.HelpSteaEmojiInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result, GeneralHelper.GetCaption("C172", language).Result, false).Result });
                                await Handler.HandlingService.BobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                    message: "Text Utility help was chosen", hilfeSection: "Text Utility");
                                await parsedArg.DeferAsync();
                                break;

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //Button
            else
            {
                try
                {
                    switch (parsedArg.Data.CustomId)
                    {
                        case "gostupid-button":
                            await interaction.FollowupAsync("", new Embed[] { GeneralHelper.CreateEmbed(interaction, $"{parsedArg.User.Username} went stupid", "").Result
    });
                            break;
                        case "temp-interface-name":
                            if (CheckDatas.CheckIfUserInVoice(parameter, "TempName").Result ||
                            CheckDatas.CheckIfUserInTempVoice(parameter, "TempName").Result)
                            {
                                return;
                            }
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

                            if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "TempName").Result)
                            {
                                return;
                            }

                            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
                            if (CheckDatas.CheckIfCommandIsDisabled(parameter, "name", tempChannel.createchannelid.Value).Result)
                            {
                                return;
                            }

                            var mb = new ModalBuilder()
                                .WithTitle(GeneralHelper.GetCaption("C173", parameter.Language).Result)
                                .WithCustomId($"tempchannel_update_name_modal{parameter.GuildUser.VoiceChannel.Id},{parameter.Language}")
                                .AddTextInput(GeneralHelper.GetContent("C170", parameter.Language).Result, "new_name", TextInputStyle.Short, required: true, maxLength: 50, value: parameter.GuildUser.VoiceChannel.Name);
                            await parameter.Interaction.RespondWithModalAsync(mb.Build());
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-unlock":
                            await TempChannelHelper.TempUnLock(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-lock":
                            await TempChannelHelper.TempLock(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-hide":
                            await TempChannelHelper.TempHide(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-unhide":
                            await TempChannelHelper.TempUnHide(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-saveconfig":
                            await TempChannelHelper.TempSaveConfig(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-deleteconfig":
                            await TempChannelHelper.TempDeleteConfig(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-size":
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
                            if (CheckDatas.CheckIfUserInVoice(parameter, "TempSize").Result ||
                                CheckDatas.CheckIfUserInTempVoice(parameter, "TempSize").Result)
                            {
                                return;
                            }
                            if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "TempSize").Result)
                            {
                                return;
                            }

                            mb = new ModalBuilder()
                                .WithTitle(GeneralHelper.GetCaption("C175", parameter.Language).Result)
                                .WithCustomId($"tempchannel_update_size_modal")
                                .AddTextInput(GeneralHelper.GetContent("C205", parameter.Language).Result, "new_size", TextInputStyle.Short, required: true, maxLength: 3, value: parameter.GuildUser.VoiceChannel.UserLimit.ToString());
                            await parameter.Interaction.RespondWithModalAsync(mb.Build());
                            break;
                        case "temp-interface-claimowner":
                            await TempChannelHelper.TempClaimOwner(parameter);
                            break;
                        case "temp-interface-giveowner":
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
                            if (CheckDatas.CheckIfUserInVoice(parameter, "giveowner").Result ||
                                 CheckDatas.CheckIfUserInTempVoice(parameter, "giveowner").Result)
                            {
                                return;
                            }

                            if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "TempOwner").Result)
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
                            break;
                        case "temp-interface-kick":
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
                            if (CheckDatas.CheckIfUserInVoice(parameter, "kick").Result ||
                                CheckDatas.CheckIfUserInTempVoice(parameter, "kick").Result)
                            {
                                return;
                            }

                            if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "TempKick").Result)
                            {
                                return;
                            }

                            menuBuilder = new SelectMenuBuilder()
                                .WithPlaceholder(GeneralHelper.GetCaption("C235", parameter.Language).Result)
                                .WithMinValues(1)
                                .WithMaxValues(5)
                                .WithCustomId("temp-interface-kick-menu")
                                .WithType(ComponentType.UserSelect);

                            await parameter.Interaction.RespondAsync(
                                    "",
                                    components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                                    ephemeral: true);
                            break;
                        case "temp-interface-block":
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);
                            if (CheckDatas.CheckIfUserInVoice(parameter, "TempBlock").Result ||
                                 CheckDatas.CheckIfUserInTempVoice(parameter, "TempBlock").Result)
                            {
                                return;
                            }

                            if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "TempBlock").Result)
                            {
                                return;
                            }

                            menuBuilder = new SelectMenuBuilder()
                                .WithPlaceholder(GeneralHelper.GetCaption("C239", parameter.Language).Result)
                                .WithMinValues(1)
                                .WithMaxValues(5)
                                .WithCustomId("temp-interface-block-menu")
                                .WithType(ComponentType.UserSelect);

                            await parameter.Interaction.RespondAsync(
                                "",
                                components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                                ephemeral: true);
                            break;
                        case "temp-interface-unblock":
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

                            if (CheckDatas.CheckIfUserInVoice(parameter, "unblock").Result ||
                                 CheckDatas.CheckIfUserInTempVoice(parameter, "unblock").Result)
                            {
                                return;
                            }

                            if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "TempUnblock").Result)
                            {
                                return;
                            }

                            menuBuilder = new SelectMenuBuilder()
                                .WithPlaceholder(GeneralHelper.GetCaption("C240", parameter.Language).Result)
                                .WithMinValues(1)
                                .WithMaxValues(5)
                                .WithCustomId("temp-interface-unblock-menu")
                                .WithType(ComponentType.UserSelect);

                            await parameter.Interaction.RespondAsync(
                                "",
                                components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                                ephemeral: true);
                            break;
                        case "temp-interface-info":
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

                            if (CheckDatas.CheckIfUserInVoice(parameter, "info").Result ||
                                 CheckDatas.CheckIfUserInTempVoice(parameter, "info").Result)
                            {
                                return;
                            }
                            await TempChannelHelper.TempInfo(parameter);
                            break;
                        case "temp-interface-mute":
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

                            if (CheckDatas.CheckIfUserInVoice(parameter, "mute").Result ||
                                CheckDatas.CheckIfUserInTempVoice(parameter, "mute").Result)
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
                            break;
                        case "temp-interface-whitelist":
                            await TempChannelHelper.GiveOwnerIfOwnerNotInVoice(parameter);

                            if (CheckDatas.CheckIfUserInVoice(parameter, "whitelist").Result ||
                                CheckDatas.CheckIfUserInTempVoice(parameter, "whitelist").Result)
                            {
                                return;
                            }

                            if (CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "whitelist").Result)
                            {
                                return;
                            }

                            selectionMenuBuilder = TempChannelHelper.WhiteListSelectionMenu(parameter);
                            await parameter.Interaction.RespondAsync(
                                "",
                                        components: new ComponentBuilder().WithSelectMenu(selectionMenuBuilder).Build(),
                                        ephemeral: true);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

        }
    }
}
