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
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "Help", exceptionMessage: ex.Message);
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
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "Guides", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region GuildUtility
        public static async Task Refresh(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("refresh")
                .WithDescription("Refreshes servercounts channel")
                .Build();

            try
            {
                // 712373862179930144 -> My GuildId
                await client.Rest.CreateGuildCommand(command, 712373862179930144);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "Refresh", exceptionMessage: ex.Message);
            }
        }


        public static async Task LeaveGuild(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("leaveguild")
                .WithDescription("Makes Bobii leave ")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("guildid")
                    .WithDescription("Guild Id from the guild which Bobii should leave")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                // 712373862179930144 -> My GuildId
                await client.Rest.CreateGuildCommand(command, 712373862179930144);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "LeaveGuild", exceptionMessage: ex.Message);
            }
        }

        public static async Task SerververCount(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("servercount")
                .WithDescription("Shows a servercount of all servers in which bobii is in")
                .Build();

            try
            {
                // 712373862179930144 -> My GuildId
                await client.Rest.CreateGuildCommand(command, 712373862179930144);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "SerververCount", exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
