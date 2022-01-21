using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterWord
{
    class Autocomplete
    {
        public static async Task UpdateRemoveAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (SocketGuildUser)interaction.User;
            var possibleChoices = new string[] { };

            var guild = (SocketGuild)guildUser.Guild;


            var filterWords = EntityFramework.FilterWordsHelper.GetFilterWordsFromGuildAsList(guild.Id).Result;

            var filterWordsList = new List<string>();
            foreach(var filterWord in filterWords)
            {
                filterWordsList.Add(filterWord.filterword);
            }

            if (filterWords.Count == 0)
            {
                possibleChoices = new string[] { "Could not find any filter word" };
            }
            else
            {
                possibleChoices = filterWordsList.ToArray();
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { "Not enough rights" };
            }


            // lets get the current value they have typed. Note that were converting it to a string for this example, the autocomplete works with int and doubles as well.
            var current = interaction.Data.Current.Value.ToString();

            // We will get the first 20 options inside our string array that start with whatever the user has typed.
            var opt = possibleChoices.Where(x => x.StartsWith(current)).Take(20);

            // Then we can send them to the client
            await interaction.RespondAsync(opt.Select(x => new AutocompleteResult(x, x.ToLower())));
        }
    }
}
