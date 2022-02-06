using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class RegisterCommands
    {
        #region Info
        public static async Task Info(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flinfo")
                .WithDescription("Returns a list of links or users on the whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("usersorlinks")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer)
                    .AddChoice("Whitelisted links", 1)
                    .AddChoice("Whitelisted users", 2)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkInfo", exceptionMessage: ex.Message);
            }
        }

        public static async Task GuildInfo(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flguildinfo")
                .WithDescription("Returns a list of links which you have created with `/flcreate`")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkInfo", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region Utility
        public static async Task Set(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flset")
                .WithDescription("Activates or deactivates filterlink")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("state")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer)
                    .AddChoice("active", 1)
                    .AddChoice("inactive", 2))
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkSet", exceptionMessage: ex.Message);
            }
        }

        public static async Task Create(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flcreate")
                .WithDescription("Creates a filter link which you can use later on the white list")
                .AddOption("name", ApplicationCommandOptionType.String, "Choose from existing names or create a new one", true, isAutocomplete: true)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("link")
                    .WithDescription("Enter the link which you want to create as choice")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkSet", exceptionMessage: ex.Message);
            }
        }

        public static async Task Delete(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fldelete")
                .WithDescription("This will delete a filter link from the name which was used")
                .AddOption("name", ApplicationCommandOptionType.String, "Choose the name form which you want to remove the link", true, isAutocomplete: true)
                .AddOption("link", ApplicationCommandOptionType.String, "Choose the link which you want to delete", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkSet", exceptionMessage: ex.Message);
            }
        }

        #region Log
        public static async Task LogSet(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("logset")
                .WithDescription("Sets the given channel to the link filter log")
                .AddOption("channel", ApplicationCommandOptionType.String, "Choose the channel which you want to use as log-channel", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkLogSet", exceptionMessage: ex.Message);
            }
        }

        public static async Task LogUpdate(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("logupdate")
                .WithDescription("Updates the log channel")
                .AddOption("channel", ApplicationCommandOptionType.String, "Choose the channel which you want to use as new log-channel", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkUpdate", exceptionMessage: ex.Message);
            }
        }

        public static async Task LogRemove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("logremove")
                .WithDescription("Removes the filter link log")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkLogRemove", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region User
        public static async Task UserAdd(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fluadd")
                .WithDescription("Adds an user to the filter link whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("@ the user which you want to add to the whitelist")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.User)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkUserAdd", exceptionMessage: ex.Message);
            }
        }

        public static async Task UserRemove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fluremove")
                .WithDescription("Removes an user from the filter link whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("@ the user which you want to remove from the whitelist")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.User)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkUserRemove", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region Links
        public static async Task LinkAdd(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flladd")
                .WithDescription("Adds an link to the whitelist")
                .AddOption("link", ApplicationCommandOptionType.String, "Choose the link which you want to add", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkAdd", exceptionMessage: ex.Message);
            }
        }

        public static async Task LinkRemove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fllremove")
                .WithDescription("Removes an link of the whitelist")
                .AddOption("link", ApplicationCommandOptionType.String, "Choose the link which you want to remove", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterLinkRemove", exceptionMessage: ex.Message);
            }
        }
        #endregion
        #endregion
    }
}
