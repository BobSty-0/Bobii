using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class AutoHelper
    {
        public static async Task AddAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var possibleChoices = DBStuff.Tables.filterlinkoptions.GetAllOptions();
            var filterLinksOfGuild = DBStuff.Tables.filterlinksguild.GetLinks(guildUser.GuildId);

            foreach(var choice in possibleChoices)
            {
                foreach(DataRow row in filterLinksOfGuild.Rows)
                {
                    if (row.Field<string>("bezeichnung").Trim() == choice)
                    {
                        //Im selecting all the choices except the one which is already used by the guild
                        possibleChoices = possibleChoices.Where(ch => ch != choice).ToArray();
                    }
                }
            }

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { "Already all links added" };
            }

            // lets get the current value they have typed. Note that were converting it to a string for this example, the autocomplete works with int and doubles as well.
            var current = interaction.Data.Current.Value.ToString();

            // We will get the first 20 options inside our string array that start with whatever the user has typed.
            var opt = possibleChoices.Where(x => x.StartsWith(current)).Take(20);

            // Then we can send them to the client
            await interaction.RespondAsync(opt.Select(x => new AutocompleteResult(x, x.ToLower())));
        }

        public static async Task RemoveAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var filterLinksOfGuild = DBStuff.Tables.filterlinksguild.GetLinks(guildUser.GuildId);
            var choicesList = new List<string>();
            foreach(DataRow row in filterLinksOfGuild.Rows)
            {
                choicesList.Add(row.Field<string>("bezeichnung"));
            }

            var possibleChoices = new string[] { };
            if (choicesList.Count == 0)
            {
                possibleChoices = new string[] { "No links to remove yet" };
            }
            else
            {
                possibleChoices = choicesList.ToArray() ;
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
