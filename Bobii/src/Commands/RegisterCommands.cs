using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    class RegisterCommands
    {
        #region Declarations
        private static ulong _myGuildID = 712373862179930144;
        #endregion

        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} SComRegi   {message}");
            await Task.CompletedTask;
        }
        #endregion

        #region Register Tasks 
        public static async Task RegisterMPlay(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("mplay")
                .WithDescription("Plays the music of the given Link")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("link")
                    .WithDescription("link of the song which should be played")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterRGetServer(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("rgetserver")
                .WithDescription("Gets a list of Rust server")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("multiplicator")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .AddChoice("Vanilla", 1)
                    .AddChoice("2x", 2)
                    .AddChoice("3x", 3))  
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
        
        public static async Task RegisterTestHelp(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("testhelp")
                .WithDescription("Help to test some things")
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
        
        public static async Task RegisterFilterWordInfoCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwinfo")
                .WithDescription("Returns a list of all the filter words of this Guild")
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

        public static async Task RegisterFilterWordAddCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwadd")
                .WithDescription("Adds a filter word")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("filterword")
                    .WithDescription("The filter word which should be replaced")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("replaceword")
                    .WithDescription("The word with which the filtered word should be replaced with")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterWordRemoveCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwremove")
                .WithDescription("Removes a filter word")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("filterword")
                    .WithDescription("The filer word which should be removed")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterWordUpdateCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwupdate")
                .WithDescription("Updates the word which will replace the filter word")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("filterword")
                    .WithDescription("The filter word to update")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("newreplaceword")
                    .WithDescription("The new word which will replace the filter word")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterBobiiGuidesCommand(DiscordSocketClient client)
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

        public static async Task RegisterHelpCommand(DiscordSocketClient client)
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

        public static async Task RegisterTempInfoCommand(DiscordSocketClient client)
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
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterTempAddCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcadd")
                .WithDescription("Adds an create-temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("voicechannelid")
                    .WithDescription("ID of the create-temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
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
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterTempRemoveCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcremove")
                .WithDescription("Removes an create-temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("voicechannelid")
                    .WithDescription("ID of the create-temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterTempUpdate(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcupdate")
                .WithDescription("Updates the temp-channel name of an existing create-temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("voicechannelid")
                    .WithDescription("ID of the create-temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
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
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterComRegisterCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comregister")
            .WithDescription("Registers a slashcommand")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be registered")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterComRemoveGuildCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comdeleteguild")
            .WithDescription("Removes a slashcommand from a guild")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be removed")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("guildid")
                .WithDescription("The guild in wich the command to delete is")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterComRemoveCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comdelete")
            .WithDescription("Removes a slashcommand")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be removed")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }
        #endregion
    }
}
