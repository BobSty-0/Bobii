using Bobii.src.Helper;
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
                        case "text-utility-help-selectmenuotion":
                            await parsedArg.UpdateAsync(msg => msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(interaction, TextUtilityHelper.HelpTextUtilityInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result +
                           "\n\n" + StealEmojiHelper.HelpSteaEmojiInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result, GeneralHelper.GetCaption("C172", language).Result, false).Result });
                            await Handler.HandlingService.BobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
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
                            await interaction.FollowupAsync("", new Embed[] { GeneralHelper.CreateEmbed(interaction, $"{parsedArg.User.Username} went stupid", "").Result
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
