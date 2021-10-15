using Bobii.src.Entities;
using Discord;
using Discord.Rest;
using System;
using System.Threading.Tasks;

namespace Bobii.src.ComEdit
{
    class SlashCommands
    {
        #region Utility
        #region Global
        public static async Task ComRegister(SlashCommandParameter parameter)
        {
            var regCommand = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            if (Bobii.CheckDatas.CheckIfItsBobSty(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "ComRegister", true).Result)
            {
                return;
            }

            await Handler.RegisterHandlingService.HandleRegisterCommands(parameter.Interaction, parameter.Guild, parameter.GuildUser, regCommand, parameter.Client);
        }

        //I did not Test this after changing the parameter thing!!
        public static async Task ComDelete(SlashCommandParameter parameter)
        {
            var delCommand = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            var commands = parameter.Client.Rest.GetGlobalApplicationCommands();

            if (Bobii.CheckDatas.CheckIfItsBobSty(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "ComDelete", true).Result)
            {
                return;
            }

            foreach (RestGlobalCommand command in commands.Result)
            {
                if (command.Name == delCommand)
                {
                    try
                    {
                        await command.DeleteAsync();
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The command **'/{command.Name}'** was sucessfully deleted by the one and only **{parameter.GuildUser.Username}**", "Command successfully deleted").Result });
                        await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: ComDelete | Guild: {parameter.GuildID} | Command: /{command.Name} | User: {parameter.GuildUser} | /comdelete successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Command **'/{command.Name}'** could not be removed", "Error!").Result }, ephemeral: true);
                        await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: ComDelete | Guild: {parameter.GuildID} | Command: /{command.Name} | User: {parameter.GuildUser} | Failed to delete | {ex.Message}");
                        return;
                    }
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Command {delCommand} could not be found!", "Error!").Result }, ephemeral: true);
            await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: ComDelete | Guild: {parameter.GuildID} | Command: /{delCommand} | User: {parameter.GuildUser} | No command with this name found");
            return;
        }
        #endregion

        #region Guild
        //I did not test this after implementing the parameter thingi!!
        public static async Task ComDeleteGuild(SlashCommandParameter parameter)
        {
            var delCommand = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            var delGuildID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();

            if (Bobii.CheckDatas.CheckIfItsBobSty(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "ComDeleteGuild", true).Result ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, delGuildID, parameter.Guild, "ComDeleteGuild", false).Result)
            {
                return;
            }

            var commands = parameter.Client.Rest.GetGuildApplicationCommands(ulong.Parse(delGuildID));

            foreach (Discord.Rest.RestGuildCommand command in commands.Result)
            {
                if (command.Name == delCommand)
                {
                    try
                    {
                        await command.DeleteAsync();
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The command **'/{command.Name}'** was sucessfully deleted by the one and only **{parameter.GuildUser.Username}**", "Command successfully deleted").Result });
                        await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: ComDeleteGuild | Guild: {parameter.GuildID} | GuildWithCommand: {delGuildID} | Command: /{command.Name} | User: {parameter.GuildUser} | /comdeleteguild successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Command **'/{command.Name}'** could not be removed", "Error!").Result }, ephemeral: true);
                        await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: ComDeleteGuild | Guild: {parameter.GuildID} | Command: /{command.Name} | User: {parameter.GuildUser} | Failed to delete | {ex.Message}");
                        return;
                    }
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Command {delCommand} could not be found!", "Error!").Result }, ephemeral: true);
            await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: ComDeleteGuild | Guild: {parameter.GuildID} | Command: /{delCommand} | User: {parameter.GuildUser} | No command with this name found");
            return;
        }
        #endregion
        #endregion
    }
}
