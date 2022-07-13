using Bobii.src.Models;
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
            var regCommand = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "commandname").Result.String;

            if (Bobii.CheckDatas.CheckIfItsBobSty(parameter, "ComRegister", true).Result)
            {
                return;
            }

            await Handler.RegisterHandlingService.HandleRegisterCommands(parameter, regCommand);
        }

        //I did not Test this after changing the parameter thing!!
        public static async Task ComDelete(SlashCommandParameter parameter)
        {
            var delCommand = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "commandname").Result.String;
            var commands = parameter.Client.Rest.GetGlobalApplicationCommands();

            if (Bobii.CheckDatas.CheckIfItsBobSty(parameter, "ComDelete", true).Result)
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
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                            String.Format(Bobii.Helper.GetContent("C025", parameter.Language).Result, command.Name, parameter.GuildUser.Username),
                            Bobii.Helper.GetCaption("C025", parameter.Language).Result).Result });

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "ComDelete", parameter, message: $"/comdelete <{command.Name}> successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                            String.Format(Bobii.Helper.GetContent("C026", parameter.Language).Result, command.Name),
                            Bobii.Helper.GetCaption("C026", parameter.Language).Result).Result }, ephemeral: true);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "ComDelete", parameter, message: $"/comdelete <{command.Name}> failed to delete", exceptionMessage: ex.Message);
                        return;
                    }
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                String.Format(Bobii.Helper.GetContent("C027", parameter.Language).Result, delCommand),
                Bobii.Helper.GetCaption("C027", parameter.Language).Result).Result }, ephemeral: true);

            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "ComDelete", parameter, message: $"No command with this name found");
            return;
        }
        #endregion

        #region Guild
        //I did not test this after implementing the parameter thingi!!
        public static async Task ComDeleteGuild(SlashCommandParameter parameter)
        {
            var delCommand = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "commandname").Result.String;
            var delGuildID = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "guildid").Result.String;

            if (Bobii.CheckDatas.CheckIfItsBobSty(parameter, "ComDeleteGuild", true).Result ||
                Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, delGuildID, "ComDeleteGuild", false).Result)
            {
                return;
            }

            var commands = parameter.Client.Rest.GetGuildApplicationCommands(ulong.Parse(delGuildID));

            foreach (RestGuildCommand command in commands.Result)
            {
                if (command.Name == delCommand)
                {
                    try
                    {
                        await command.DeleteAsync();
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                            String.Format(Bobii.Helper.GetContent("C025", parameter.Language).Result, command.Name, parameter.GuildUser.Username),
                            Bobii.Helper.GetCaption("C025", parameter.Language).Result).Result });

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "ComDeleteGuild", parameter, message: $"/comdeleteguild <{command.Name}> successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                            String.Format(Bobii.Helper.GetContent("C026", parameter.Language).Result, command.Name),
                            Bobii.Helper.GetCaption("C026", parameter.Language).Result).Result }, ephemeral: true);

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "ComDeleteGuild", parameter, message: $"/comdeleteguild <{command.Name}> failed to used", exceptionMessage: ex.Message);
                        return;
                    }
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                String.Format(Bobii.Helper.GetContent("C027", parameter.Language).Result, delCommand),
                Bobii.Helper.GetCaption("C027", parameter.Language).Result).Result }, ephemeral: true);

            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "ComDeleteGuild", parameter, message: $"No command with this name found {delCommand}");
            return;
        }
        #endregion
        #endregion
    }
}
