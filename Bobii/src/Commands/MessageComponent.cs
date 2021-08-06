using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    class MessageComponent
    {
        public static async Task MessageComponentHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parsedArg = (SocketMessageComponent)interaction;
            //If SelectMenu
            if (parsedArg.Data.Values != null)
            {
                var commandName = parsedArg.Data.Values.First<string>();

                switch (commandName)
                {
                    case "temp-channel-help-selectmenuoption":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, TextChannel.TextChannel.HelpTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result), "Temporary Voice Channel Commands:") });
                        await interaction.DeferAsync();
                        break;
                    case "filter-word-help-selectmenuoption":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, TextChannel.TextChannel.HelpFilterWordInfoPart(client.Rest.GetGlobalApplicationCommands().Result), "Filter Word Commands:") });
                        await interaction.DeferAsync();
                        break;
                    case "how-to-cereate-temp-channel-guide":
                        await parsedArg.Message.ModifyAsync(msg => msg.Embeds = new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, TempVoiceChannel.TempVoiceChannel.StepByStepTcadd(), "Step by step instruction on how to add a create-temp-channel") });
                        await interaction.DeferAsync();
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
                        await interaction.FollowupAsync("", new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"{parsedArg.User.Username} went stupid", "") });
                        break;
                }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }

            }

        }
    }
}
