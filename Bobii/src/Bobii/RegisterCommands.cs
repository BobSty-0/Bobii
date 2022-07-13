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
        #region Test
        public static async Task Test(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("test")
                .WithDescription("test")
                 .AddOption(new SlashCommandOptionBuilder()
                        .WithName("field-a")
                        .WithDescription("Gets or sets the field A")
                        .WithType(ApplicationCommandOptionType.SubCommandGroup)
                         .AddOption(new SlashCommandOptionBuilder()
                                .WithName("set")
                                .WithDescription("Sets the field A")
                                .WithType(ApplicationCommandOptionType.SubCommand)
                                        .AddOption("value", ApplicationCommandOptionType.String, "the value to set the field", isRequired: true)
                        ).AddOption(new SlashCommandOptionBuilder()
                            .WithName("get")
                            .WithDescription("Gets the value of field A.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                        )
                )
                .Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, Helper.ReadBobiiConfig(ConfigKeys.MainGuildID).ToUlong());
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("SCommRegis", true, "Help", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region Help
        public static async Task Help(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("help")
                .WithDescription("Help commands for Bobii's utility")
                 .AddOption(new SlashCommandOptionBuilder()
                       .WithName("commands")
                       .WithDescription("This will show all my commands")
                       .WithType(ApplicationCommandOptionType.SubCommand)
                  ).AddOption(new SlashCommandOptionBuilder()
                       .WithName("guides")
                       .WithDescription("This will show all my guides")
                       .WithType(ApplicationCommandOptionType.SubCommand)
                   ).AddOption(new SlashCommandOptionBuilder()
                       .WithName("support")
                       .WithDescription("This will show how you can get support")
                       .WithType(ApplicationCommandOptionType.SubCommand)
                  ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("SCommRegis", true, "Help", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region GuildUtility
        public static async Task Backup(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                 .WithName("backup")
                 .WithDescription("Does a backup from Bobii's databases")
                 .Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, Helper.ReadBobiiConfig(ConfigKeys.MainGuildID).ToUlong());
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("SCommRegis", true, "Backup", exceptionMessage: ex.Message);
            }
        }

        public static async Task Refresh(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("refresh")
                .WithDescription("Refreshes servercounts channel")
                .Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, Helper.ReadBobiiConfig(ConfigKeys.MainGuildID).ToUlong());
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("SCommRegis", true, "Refresh", exceptionMessage: ex.Message);
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
                await client.Rest.CreateGuildCommand(command, Helper.ReadBobiiConfig(ConfigKeys.MainGuildID).ToUlong());
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("SCommRegis", true, "SerververCount", exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
