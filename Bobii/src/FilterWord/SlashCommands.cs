using Bobii.src.DBStuff.Tables;
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
            await parameter.Interaction.RespondAsync(null, new Embed[] { FilterWord.Helper.CreateFilterWordEmbed(parameter.Interaction, parameter.GuildID.ToString()).Result });
            await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterWordInfo | Guild: {parameter.GuildID} | /fwinfo successfully used");
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
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, replaceWord, "FilterWordAdd", 20, false).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordAdd", 20, false).Result ||
                Bobii.CheckDatas.CheckIfFilterWordDouble(parameter.Interaction, filterWord, parameter.Guild, "FilterWordAdd").Result)
            {
                return;
            }

            try
            {
                filterwords.AddFilterWord(parameter.GuildID, filterWord, replaceWord);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** was successfully added by **'{parameter.GuildUser.Username}'**", "Filter word successfully added!").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterWordAdd | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | /fwadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "The filter word could not be added!", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterWordAdd | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | Failed to add filter word | {ex.Message}");
                return;
            }
        }

        public static async Task FWUpdate(SlashCommandParameter parameter)
        {
            var filterWord = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            var newReplaceWord = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");
            newReplaceWord = newReplaceWord.Replace("'", "’");

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterWordRemove").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, newReplaceWord, "FilterWordUpdate", 20, false).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordUpdate", 20, false).Result ||
                Bobii.CheckDatas.CheckIfFilterWordExists(parameter.Interaction, filterWord, parameter.Guild, "FilterWordUpdate").Result)
            {
                return;
            }
            try
            {
                filterwords.UpdateFilterWord(filterWord, newReplaceWord, parameter.GuildID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** will now be replaced with **'{newReplaceWord}'**", "Successfully updated!").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterWordUpdate | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | /fwupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The word which should replace **'{filterWord}'** could not be changed!", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterWordUpdate | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | Failed to update the replace word | {ex.Message}");
                return;
            }
        }


        public static async Task FWRemove(SlashCommandParameter parameter)
        {
            var filterWord = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterWordRemove").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordRemove", 20, false).Result ||
                Bobii.CheckDatas.CheckIfFilterWordExists(parameter.Interaction, filterWord, parameter.Guild, "FilterWordRemove").Result)
            {
                return;
            }

            try
            {
                filterwords.RemoveFilterWord(filterWord, parameter.GuildID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** was successfully removed by **'{parameter.GuildUser.Username}'**", "Filter word successfully removed!").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterWordRemove | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | /fwremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "The filter word could not be removed!", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterWordRemove | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | Failed to remove filter word | {ex.Message}");
                return;
            }
        }
        #endregion
    }
}
