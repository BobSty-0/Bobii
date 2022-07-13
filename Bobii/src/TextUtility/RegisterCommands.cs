using Discord;
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
            .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("SCommRegis", true, "CreateEmbed", exceptionMessage: ex.Message);
            }
        }

        public static async Task EditEmbed(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("tueditembed")
            .WithDescription("Edits an embed")
            .AddOption("messageid", ApplicationCommandOptionType.String, "Choose the message which you want to edit", true, isAutocomplete: true)
            .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("SCommRegis", true, "EditEmbed", exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
