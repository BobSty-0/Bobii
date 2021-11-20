﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TextUtility
{
    class RegisterCommands
    {
        #region Embeds
        public static async Task CreateEmbed(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("tucreateembed")
            .WithDescription("Creates an embed")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("title")
                .WithDescription("This will be the title of the embed")
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("content")
                .WithDescription("This will be the content of the embed")
                .WithRequired(false)
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

        public static async Task EditEmbed(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("tueditembed")
            .WithDescription("Edits an embed")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("messageid")
                .WithDescription("Insert the message Id from which you want to edit the embed")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("title")
                .WithDescription("This will be the title new embed (Optional)")
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.String))
             .AddOption(new SlashCommandOptionBuilder()
                .WithName("content")
                .WithDescription("This will be the new content (Optional)")
                .WithRequired(false)
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
    }
}
