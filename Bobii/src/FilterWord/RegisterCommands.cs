using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterWord
{
    class RegisterCommands
    {
        #region Info
        public static async Task Info(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwinfo")
                .WithDescription("Returns a list of all the filter words of this Guild")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterWordInfo", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region Utility
        public static async Task Add(DiscordSocketClient client)
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
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterWordAdd", exceptionMessage: ex.Message);
            }
        }

        public static async Task Update(DiscordSocketClient client)
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
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterWordUpdate", exceptionMessage: ex.Message);
            }
        }

        public static async Task Remove(DiscordSocketClient client)
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
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "FilterWordRemove", exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
