using Bobii.src.Models;
using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class MessageComponentHandlingService
    {
        public static async Task MessageComponentHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parsedArg = (SocketMessageComponent)interaction;
            var parsedUser = (SocketGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(parsedUser.Guild.Id).Result;
            //If SelectMenu
            if (parsedArg.Data.Values != null)
            {
                var commandName = parsedArg.Data.Values.First<string>();

                try
                {
                    switch (commandName)
                    {
                        case "temp-channel-help-selectmenuoption":
                            await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] {
                            Bobii.Helper.CreateEmbed(interaction, TempChannel.Helper.HelpTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result +
                            TempChannel.Helper.HelpEditTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result, Bobii.Helper.GetCaption("C170", language).Result).Result });
                            await Handler.HandlingService._bobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                message: "Temp channel help was chosen", hilfeSection: "Temp Channel");
                            await parsedArg.DeferAsync();
                            break;
                        case "how-to-cereate-temp-channel-guide":
                            await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, Bobii.Helper.GetContent("C169", language).Result, Bobii.Helper.GetContent("C168", language).Result).Result });
                            await Handler.HandlingService._bobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Guide", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                 message: "temp-channel guide was chosen", hilfeSection: "Temp Channel");
                            await parsedArg.DeferAsync();
                            break;
                        case "filter-link-help-selectmenuotion":
                            await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, FilterLink.Helper.HelpFilterLinkInfoPart(client.Rest.GetGlobalApplicationCommands().Result,
                                parsedUser.Guild.Id).Result, Bobii.Helper.GetCaption("C171", language).Result).Result });
                            await Handler.HandlingService._bobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                message: "Filter link help was chosen", hilfeSection: "Filter Link");
                            await parsedArg.DeferAsync();
                            break;
                        case "text-utility-help-selectmenuotion":
                            await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, TextUtility.Helper.HelpTextUtilityInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result +
                           "\n\n" + StealEmoji.Helper.HelpSteaEmojiInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result, Bobii.Helper.GetCaption("C172", language).Result, false).Result });
                            await Handler.HandlingService._bobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                                message: "Text Utility help was chosen", hilfeSection: "Text Utility");
                            await parsedArg.DeferAsync();
                            break;
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
                            await interaction.FollowupAsync("", new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"{parsedArg.User.Username} went stupid", "").Result
    });
                            break;
                        case "wtog-button":
                            await interaction.DeferAsync();
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
