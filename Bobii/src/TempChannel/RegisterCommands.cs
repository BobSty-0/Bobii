using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class RegisterCommands
    {
        #region Info
        public static async Task Info(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcinfo")
                .WithDescription("Returns a list of all the create-temp-channels of this Guild")
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

        #region Utility
        public static async Task Add(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcadd")
                .WithDescription("Adds an create-temp-channel")
                .AddOption("createvoicechannel", ApplicationCommandOptionType.String, "Choose the channel which you want to add as create-voice-channel", true, isAutocomplete: true)
                //.AddOption(new SlashCommandOptionBuilder()
                //    .WithName("voicechannelid")
                //    .WithDescription("ID of the create-temp-channel")
                //    .WithRequired(true)
                //    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("tempchannelname")
                    .WithDescription("This will be the name of the temp-channel. Note: User = Username")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                Bobii.RegisterCommands.WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task Update(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcupdate")
                .WithDescription("Updates the temp-channel name of an existing create-temp-channel")
                .AddOption("createvoicechannel", ApplicationCommandOptionType.String, "Choose the channel which you want to update", true, isAutocomplete: true)
                //.AddOption(new SlashCommandOptionBuilder()
                //    .WithName("voicechannelid")
                //    .WithDescription("ID of the create-temp-channel")
                //    .WithRequired(true)
                //    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("newtempchannelname")
                    .WithDescription("This will be the new name of the temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                Bobii.RegisterCommands.WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task Remove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcremove")
                .WithDescription("Removes an create-temp-channel")
                .AddOption("createvoicechannel", ApplicationCommandOptionType.String, "Choose the channel which you want to remove", true, isAutocomplete: true)
                //.AddOption(new SlashCommandOptionBuilder()
                //    .WithName("voicechannelid")
                //    .WithDescription("ID of the create-temp-channel")
                //    .WithRequired(true)
                //    .WithType(ApplicationCommandOptionType.String))
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
