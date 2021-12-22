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
                        await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "ComDelete", parameter, message: $"/comdelete <{command.Name}> successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Command **'/{command.Name}'** could not be removed", "Error!").Result }, ephemeral: true);
                        await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "ComDelete", parameter, message: $"/comdelete <{command.Name}> failed to delete", exceptionMessage: ex.Message);
                        return;
                    }
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Command {delCommand} could not be found!", "Error!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "ComDelete", parameter, message: $"No command with this name found");
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
                Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter.Interaction, delGuildID, parameter.Guild, "ComDeleteGuild", false, parameter.Language).Result)
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
                        await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "ComDeleteGuild", parameter, message: $"/comdeleteguild <{command.Name}> successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Command **'/{command.Name}'** could not be removed", "Error!").Result }, ephemeral: true);
                        await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "ComDeleteGuild", parameter, message: $"/comdeleteguild <{command.Name}> failed to used", exceptionMessage: ex.Message);
                        return;
                    }
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Command {delCommand} could not be found!", "Error!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "ComDeleteGuild", parameter, message: $"No command with this name found {delCommand}");
            return;
        }
        #endregion
        #endregion
    }
}
