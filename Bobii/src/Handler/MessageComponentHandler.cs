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
using System.Reflection.Metadata;
using System.Threading.Tasks;

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
                        switch (test)
                        {
                            case "temp-interface-owner-menu":
                                await TempChannelHelper.TempOwner(parameter, commandName, true);
                                await parsedArg.DeferAsync();
                                break;
                            case "temp-interface-kick-menu":
                                var userIds = parsedArg.Data.Values.ToList<String>();
                                await TempChannelHelper.TempKick(parameter, userIds, true);
                                await parsedArg.DeferAsync();
                                break;
                        }
                    }
                    else
                    {
                        switch (commandName)
                        {
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
                            await TempChannelHelper.GiveOwnerIfOwnerIDZero(parameter);

                            if (CheckDatas.CheckIfUserInVoice(parameter, "TempName").Result ||
                            CheckDatas.CheckIfUserInTempVoice(parameter, "TempName").Result ||
                            CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, "TempName").Result ||
                            CheckDatas.CheckIfCommandIsDisabled(parameter, "name").Result)
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
                        case "temp-interface-openchannel":
                            await TempChannelHelper.TempUnLock(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-closechannel":
                            await TempChannelHelper.TempLock(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-hidechannel":
                            await TempChannelHelper.TempHide(parameter);
                            await interaction.DeferAsync();
                            break;
                        case "temp-interface-unhidechannel":
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
                            mb = new ModalBuilder()
                                .WithTitle(GeneralHelper.GetCaption("C175", parameter.Language).Result)
                                .WithCustomId($"tempchannel_update_size_modal")
                                .AddTextInput(GeneralHelper.GetContent("C205", parameter.Language).Result, "new_size", TextInputStyle.Short, required: true, maxLength: 3, value: parameter.GuildUser.VoiceChannel.UserLimit.ToString());
                            await parameter.Interaction.RespondWithModalAsync(mb.Build());
                            break;
                        case "temp-interface-owner":
                            var menuBuilder = new SelectMenuBuilder()
                                .WithPlaceholder(GeneralHelper.GetCaption("C234", parameter.Language).Result)
                                .WithCustomId("temp-interface-owner-menu")
                                .WithType(ComponentType.UserSelect);

                            await parameter.Interaction.RespondAsync(
                                "",
                                embeds: new Embed[] { GeneralHelper.CreateEmbed(
                                    parameter.Interaction,
                                    "",
                                    GeneralHelper.GetContent("C232", parameter.Language).Result
                                    ).Result },
                                components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                                ephemeral: true);
                            break;
                        case "temp-interface-kick":
                            menuBuilder = new SelectMenuBuilder()
                                .WithPlaceholder(GeneralHelper.GetCaption("C235", parameter.Language).Result)
                                .WithMinValues(1)
                                .WithMaxValues(5)
                                .WithCustomId("temp-interface-kick-menu")
                                .WithType(ComponentType.UserSelect);

                            await parameter.Interaction.RespondAsync(
                                    "",
                                    embeds: new Embed[] { GeneralHelper.CreateEmbed(
                                        parameter.Interaction,
                                        "",
                                        GeneralHelper.GetContent("C233", parameter.Language).Result
                                    ).Result },
                                    components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build(),
                                    ephemeral: true);
                            break;
                        case "temp-interface-block":
                            mb = new ModalBuilder()
                                .WithTitle(GeneralHelper.GetCaption("C209", parameter.Language).Result)
                                .WithCustomId($"tempchannel_block_user_modal")
                                .AddTextInput(GeneralHelper.GetContent("C207", parameter.Language).Result, "user", TextInputStyle.Short, required: true, maxLength: 50);
                            await parameter.Interaction.RespondWithModalAsync(mb.Build());
                            break;
                        case "temp-interface-unblock":
                            mb = new ModalBuilder()
                                .WithTitle(GeneralHelper.GetCaption("C210", parameter.Language).Result)
                                .WithCustomId($"tempchannel_unblock_user_modal")
                                .AddTextInput(GeneralHelper.GetContent("C207", parameter.Language).Result, "user", TextInputStyle.Short, required: true, maxLength: 50);
                            await parameter.Interaction.RespondWithModalAsync(mb.Build());
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
