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
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "fwinfo").Result)
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
            var filterWord = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            var replaceWord = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");
            replaceWord = replaceWord.Replace("'", "’");

            //Check for valid input and permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterWordAdd").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, replaceWord, "FilterWordAdd", 20, false, parameter.Language).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordAdd", 20, false, parameter.Language).Result ||
                Bobii.CheckDatas.CheckIfFilterWordExists(parameter.Interaction, filterWord, parameter.Guild, "FilterWordAdd", parameter.Language).Result)
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
            }

            if (filterWord == "not enough rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command!", "Not enough rights!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWUpdate", parameter, filterWord: filterWord, message: "Not enough rights");
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");
            newReplaceWord = newReplaceWord.Replace("'", "’");

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterWordRemove").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, newReplaceWord, "FilterWordUpdate", 20, false, parameter.Language).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordUpdate", 20, false, parameter.Language).Result ||
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
            }

            if (filterWord == "not enough rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command!", "Not enough rights!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "FWRemove", parameter, filterWord: filterWord, message: "Not enough rights");
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterWordRemove").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordRemove", 20, false, parameter.Language).Result ||
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
