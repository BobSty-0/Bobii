using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Watch2Gether
{
    class RegisterCommands
    {
        #region Tasks
        public static async Task W2GStart(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("w2gstart")
                .WithDescription("Creates a watch 2 gether event for the chosen voice channel")
                .AddOption("voicechannel", ApplicationCommandOptionType.String, "Choose the channel which you want to start the event in!", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                Bobii.RegisterCommands.WriteToConsol($"Error | {ex.Message}");
            }
        }
        #endregion
    }
}
