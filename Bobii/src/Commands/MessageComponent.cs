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
                        await interaction.Channel.SendMessageAsync("", false, TextChannel.TextChannel.CreateEmbed(interaction, TextChannel.TextChannel.HelpTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result), "Temporary Voice Channel Commands:"));
                        await parsedArg.Message.DeleteAsync();
                        break;
                    case "filter-word-help-selectmenuoption":
                        await interaction.Channel.SendMessageAsync("", false, TextChannel.TextChannel.CreateEmbed(interaction, TextChannel.TextChannel.HelpFilterWordInfoPart(client.Rest.GetGlobalApplicationCommands().Result), "Filter Word Commands:"));
                        await parsedArg.Message.DeleteAsync();
                        break;
                }
            }
            //Button
            else
            {
                switch (parsedArg.Data.CustomId)
                {
                    case "gostupid-button":
                        await interaction.FollowupAsync("", false, TextChannel.TextChannel.CreateEmbed(interaction, $"{parsedArg.User.Username} went stupid", ""));
                        break;
                }
            }

        }
    }
}
