using Bobii.src.Entities;
using Discord;
using System;
using System.Threading.Tasks;

namespace Bobii.src.FilterWord
{
    class SlashCommands
    {
        #region Info
        public static async Task FWInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, "fwinfo").Result)
            {
                return;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { FilterWord.Helper.CreateFilterWordEmbed(parameter.Interaction, parameter.GuildID).Result });
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "FWInfo", parameter, message: "/fwinfo successfully used");
        }
        #endregion

        #region Utility
        public static async Task FWAdd(SlashCommandParameter parameter)
        {
            var filterWord = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "filterword").Result.String;
            var replaceWord = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "replaceword").Result.String;

            //Check for valid input and permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter, "FilterWordAdd").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter, "", replaceWord, "FilterWordAdd", 20, false).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter, "", filterWord, "FilterWordAdd", 20, false).Result ||
                Bobii.CheckDatas.CheckIfFilterWordExists(parameter, filterWord, "FilterWordAdd").Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterWordsHelper.AddFilterWord(parameter.GuildID, filterWord, replaceWord);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** was successfully added by **'{parameter.GuildUser.Username}'**", "Filter word successfully added!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "FWAdd", parameter, message: "/fwadd successfully used" , filterWord: filterWord);
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "The filter word could not be added!", "Error!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWAdd", parameter, message: "Failed to add filter word", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task FWUpdate(SlashCommandParameter parameter)
        {
            var filterWord = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            var newReplaceWord = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();

            if (filterWord == "could not find any filter word")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have any filter-words yet, you can add a filter-word by using:\n`/fwadd`", "No filter words yet!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWUpdate", parameter, filterWord: filterWord, message: "No filter words yet");
                return;
            }

            if (filterWord == "not enough rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command!", "Not enough rights!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWUpdate", parameter, filterWord: filterWord, message: "Not enough rights");
                return;
            }

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter, "FilterWordRemove").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter, "", newReplaceWord, "FilterWordUpdate", 20, false).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter, "", filterWord, "FilterWordUpdate", 20, false).Result ||
                Bobii.CheckDatas.CheckIfFilterWordAlreadyExists(parameter.Interaction, filterWord, parameter.Guild, "FilterWordUpdate", parameter.Language).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterWordsHelper.UpdateFilterWord(filterWord, newReplaceWord, parameter.GuildID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** will now be replaced with **'{newReplaceWord}'**", "Successfully updated!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "FWUpdate", parameter, filterWord: filterWord, message: "/fwupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The word which should replace **'{filterWord}'** could not be changed!", "Error!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWUpdate", parameter, filterWord: filterWord, message: "Failed to update the replace word", exceptionMessage: ex.Message);
                return;
            }
        }


        public static async Task FWRemove(SlashCommandParameter parameter)
        {
            var filterWord = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            if (filterWord == "could not find any filter word")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have any filter-words yet, you can add a filter-word by using:\n`/fwadd`", "No filter words yet!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWRemove", parameter, filterWord: filterWord, message: "No filter words yet");
                return;
            }

            if (filterWord == "not enough rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command!", "Not enough rights!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWRemove", parameter, filterWord: filterWord, message: "Not enough rights");
                return;
            }

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter, "FilterWordRemove").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter, "", filterWord, "FilterWordRemove", 20, false).Result ||
                Bobii.CheckDatas.CheckIfFilterWordAlreadyExists(parameter.Interaction, filterWord, parameter.Guild, "FilterWordRemove", parameter.Language).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterWordsHelper.RemoveFilterWord(filterWord, parameter.GuildID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** was successfully removed by **'{parameter.GuildUser.Username}'**", "Filter word successfully removed!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "FWRemove", parameter, filterWord: filterWord, message: "/fwremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "The filter word could not be removed!", "Error!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWRemove", parameter, filterWord: filterWord, message: "Failed to remove filter word", exceptionMessage: ex.Message);
                return;
            }
        }
        #endregion
    }
}
