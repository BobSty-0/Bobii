using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            catch (ApplicationCommandException ex)
            {
                Bobii.RegisterCommands.WriteToConsol($"Error | {ex.Message}");
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
            catch (ApplicationCommandException ex)
            {
                Bobii.RegisterCommands.WriteToConsol($"Error | {ex.Message}");
            }
        }

        #region Log
        public static async Task LogSet(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("logset")
                .WithDescription("Sets the given channel to the filter link log")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithDescription("# the channel which you want set as filter link log")
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

        public static async Task LogUpdate(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("logupdate")
                .WithDescription("Updates the log channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithDescription("# the new channel which you want set as filter link log")
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
            catch (ApplicationCommandException ex)
            {
                Bobii.RegisterCommands.WriteToConsol($"Error | {ex.Message}");
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

        public static async Task UserRemove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fluremove")
                .WithDescription("Removes an user from the filter link whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("@ the user which you want to remove from the whitelist")
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
        #endregion

        #region Links
        public static async Task LinkAdd(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flladd")
                .WithDescription("Adds an link to the whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("link")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                    .AddChoice("youtube", "YouTube")
                    .AddChoice("twitter", "Twitter")
                    .AddChoice("twitch", "Twitch")
                    .AddChoice("steam", "Steam")
                    .AddChoice("reddit", "Reddit")
                    .AddChoice("instagram", "Instagram")
                    .AddChoice("stackoverflow", "Stackoverflow")
                    .AddChoice("discord", "Discord")
                    .AddChoice("github", "Github")
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

        public static async Task LinkRemove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fllremove")
                .WithDescription("Removes an link of the whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("link")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                    .AddChoice("youtube", "YouTube")
                    .AddChoice("twitter", "Twitter")
                    .AddChoice("twitch", "Twitch")
                    .AddChoice("steam", "Steam")
                    .AddChoice("reddit", "Reddit")
                    .AddChoice("instagram", "Instagram")
                    .AddChoice("stackoverflow", "Stackoverflow")
                    .AddChoice("discord", "Discord")
                    .AddChoice("github", "Github")
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
        #endregion
        #endregion
    }
}
