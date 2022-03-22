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
                await client.Rest.CreateGuildCommand(command, 712373862179930144);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "Help", exceptionMessage: ex.Message);
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
        public static async Task DeleteVoice(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("deletevoice")
                .WithDescription("Deletes a voice channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("voicechannelid")
                    .WithDescription("ChannelId of the channel which should be removed")
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
                .WithDescription("Makes Bobii leave a guild")
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
