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
                        await interaction.Channel.SendMessageAsync("", false, TextChannel.TextChannel.CreateEmbed(interaction, TextChannel.TextChannel.HelpTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result), "Temporary Voice Channel Commands:"), component: new ComponentBuilder()
                            .WithButton("How to create my first create-temp-channel", customId: "tcadd-instruction", ButtonStyle.Primary, row: 0)
                            .Build()
                            );
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
                try
                {
                switch (parsedArg.Data.CustomId)
                {
                    case "tcadd-instruction":
                        await interaction.Channel.SendMessageAsync("", false, TextChannel.TextChannel.CreateEmbed(interaction, 
                            "**Step 1:**\n" +
                            "_Make sure you are in developer mode._\n" +
                            "To do this, go into your settings, select the 'Advanced' setting in the 'App Settings' category and make sure that developer mode is turned on.\n" +
                            "\n" +
                            "**Step 2:**\n" +
                            "_Choose the voice channel you want to add as create-temp-channel._\n" +
                            "Keep in mind you need an already existing voice channel.\n" +
                            "\n" +
                            "**Step 3:**\n" +
                            "_Get the ID of the choosen voice channel._\n" +
                            "To get the ID, right click on the choosen voice channel and then click on 'Copy ID'.\n" +
                            "\n" +
                            "**Step 4:**\n" +
                            "_Choose the name of the temp-channel_\n" +
                            "This will be the name of the created temp-channel. One little thing I implemented is that the word 'User' is replaced with the username.\n" +
                            "Example:\n" +
                            "I call the temp-channel name 'User's channel' (My username = BobSty)\n" +
                            "The output would be:\n" +
                            "BobSty's channel\n" +
                            "\n" +
                            "**Step 5:**\n" +
                            "_Use the given command `/tcadd`_\n" +
                            "First write `/tcadd` in any given text channel. Then use the `Tab` Key to select the `voicechannelID` parameter, here you have to insert the ealyer copied voice channel ID." +
                            "\nNext use the `Tab` key again to switch to the next parameter which is the `tempchannelname`, here you have put in the earlyer choosen temp-channel name.\n" +
                            "After that you only have to click on the `Enter` key and the create temp channel will be created.\n" +
                            "\n" +
                            "**Step 6:**\n" +
                            "_Test your create-temp-channel_\n" +
                            "To test your channel you can now join the voice channel whose ID you used in the command `/tcadd`. This should then create a temporary voice channel with the temp-channel name.\n" +
                            "\n" +
                            "If you have any issues with this command/guid feel free to msg me on Discord: `BobSty#0815`",
                            "Step by step instruction on how to add a create-temp-channel"));
                            await parsedArg.Message.DeleteAsync();
                            break;
                    case "gostupid-button":
                        await interaction.FollowupAsync("", false, TextChannel.TextChannel.CreateEmbed(interaction, $"{parsedArg.User.Username} went stupid", ""));
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
