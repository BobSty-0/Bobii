using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    class RegisterCommands
    {
        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} SComRegi    {message}");
            await Task.CompletedTask;
        }
        #endregion

        #region Help
        public static async Task Help(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("helpbobii")
                .WithDescription("Returns a list of all my Commands")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }
        #endregion

        #region Guide
        public static async Task Guides(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("bobiiguides")
                .WithDescription("Returns all my guides for a better understanding of Bobii")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }
        #endregion
    }
}
