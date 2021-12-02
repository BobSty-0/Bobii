using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.StealEmoji
{
    class RegisterCommands
    {
        #region Tasks
        public static async Task StealEmoji(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("stealemoji")
                .WithDescription("Adds the used emoji to your server")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("emoji")
                    .WithDescription("Insert the emoji which you want to add here!")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("emojiname")
                    .WithDescription("This will of the emoji in your server")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                await Bobii.Helper.WriteToConsol("SCommRegis", true, "StealEmoji", exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
