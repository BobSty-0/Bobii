using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class MessageComponentHandlingService
    {
        public static async Task WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} MessageCom  {message}");
            await Task.CompletedTask;
        }

        public static async Task MessageComponentHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parsedArg = (SocketMessageComponent)interaction;
            var parsedUser = (SocketGuildUser)interaction.User;
            //If SelectMenu
            if (parsedArg.Data.Values != null)
            {
                var commandName = parsedArg.Data.Values.First<string>();

                switch (commandName)
                {
                    case "temp-channel-help-selectmenuoption":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { 
                            Bobii.Helper.CreateEmbed(interaction, TempChannel.Helper.HelpTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result).Result +
                            TempChannel.Helper.HelpEditTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result).Result, "Temporary Voice Channel Commands:").Result });
                        await WriteToConsol($"Information: {parsedUser.Guild.Name} | Task: MessageComponentHandler | Help | Guild: {parsedUser.Guild.Id} | Command: {commandName} | Temp channel help was chosen");
                        await parsedArg.DeferAsync();
                        break;
                    case "filter-word-help-selectmenuoption":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, FilterWord.Helper.HelpFilterWordInfoPart(client.Rest.GetGlobalApplicationCommands().Result).Result, "Filter Word Commands:").Result });
                        await WriteToConsol($"Information: {parsedUser.Guild.Name} | Task: MessageComponentHandler | Help | Guild: {parsedUser.Guild.Id} | Command: {commandName} | Filter word help was chosen");
                        await parsedArg.DeferAsync();
                        break;
                    case "how-to-cereate-temp-channel-guide":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, TempChannel.Guides.StepByStepTcadd().Result, "Step by step instruction on how to add a create-temp-channel").Result });
                        await WriteToConsol($"Information: | Task: MessageComponentHandler | Guides | Guild: {parsedUser.Guild.Id} | Command: {commandName} | /tcadd guide was chosen");
                        await parsedArg.DeferAsync();
                        break;
                    case "how-to-add-filter-link-guide":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, FilterLink.Guides.StepByStepFLLAdd().Result, "Step by step instruction on how to add a link to the whitelist").Result });
                        await WriteToConsol($"Information: {parsedUser.Guild.Name} | Task: MessageComponentHandler | Guides | Guild: {parsedUser.Guild.Id} | Command: {commandName} | /flladd guide was chosen");
                        await parsedArg.DeferAsync();
                        break;
                    case "filter-link-help-selectmenuotion":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, FilterLink.Helper.HelpFilterLinkInfoPart(client.Rest.GetGlobalApplicationCommands().Result).Result, "Filter Link Commands:").Result });
                        await WriteToConsol($"Information: {parsedUser.Guild.Name} | Task: MessageComponentHandler | Help | Guild: {parsedUser.Guild.Id} | Command: {commandName} | Filter link help was chosen");
                        await parsedArg.DeferAsync();
                        break;
                    case "support-help-selectmenuotion":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, Bobii.Helper.HelpSupportPart().Result, "Support:").Result });
                        await WriteToConsol($"Information: {parsedUser.Guild.Name} | Task: MessageComponentHandler | Help | Guild: {parsedUser.Guild.Id} | Command: {commandName} | Support help was chosen");
                        await parsedArg.DeferAsync();
                        break;
                    case "text-utility-help-selectmenuotion":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, TextUtility.Helper.HelpTextUtilityInfoPart(client.Rest.GetGlobalApplicationCommands().Result).Result, "Text Utility Commands:", false).Result });
                        await WriteToConsol($"Information: {parsedUser.Guild.Name} | Task: MessageComponentHandler | Help | Guild: {parsedUser.Guild.Id} | Command: {commandName} | Text Utility help was chosen");
                        await parsedArg.DeferAsync();
                        break;
                    case "steal-emoji-help-selectmenuotion":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, StealEmoji.Helper.HelpSteaEmojiInfoPart(client.Rest.GetGlobalApplicationCommands().Result).Result, "Emoji Steal Command:", false).Result });
                        await WriteToConsol($"Information: {parsedUser.Guild.Name} | Task: MessageComponentHandler | Help | Guild: {parsedUser.Guild.Id} | Command: {commandName} | Steal Emoji help was chosen");
                        await parsedArg.DeferAsync();
                        break;
                    case "w2g-help-selectmenuoption":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(interaction, Watch2Gether.Helper.HelpW2GInfoPart(client.Rest.GetGlobalApplicationCommands().Result).Result, "Watch 2 Gether Command:", false).Result });
                        await WriteToConsol($"Information: {parsedUser.Guild.Name} | Task: MessageComponentHandler | Help | Guild: {parsedUser.Guild.Id} | Command: {commandName} | W2G help was chosen");
                        await parsedArg.DeferAsync();
                        break;
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
                            await interaction.FollowupAsync("", new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"{parsedArg.User.Username} went stupid", "").Result });
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
