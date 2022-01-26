using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class AutoComplete
    {
        public static async Task DeleteLinkAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;

            var name = string.Empty;
            foreach(var option in interaction.Data.Options)
            {
                if (option.Name == "name")
                {
                    name = option.Value.ToString();
                }
            }
            var possibleChoices = FilterLink.EntityFramework.FilterLinkOptionsHelper.GetAllOptionsLinksFromGuild(name, guildUser.Guild.Id).Result;

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { "No links created yet!" };
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

        public static async Task DeleteNameAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var possibleChoices = FilterLink.EntityFramework.FilterLinkOptionsHelper.GetAllOptionsFromGuild(guildUser.Guild.Id).Result;

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { "No names created yet!" };
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

        public static async Task CreateAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var possibleChoices = FilterLink.EntityFramework.FilterLinkOptionsHelper.GetAllOptionsFromGuild(guildUser.Guild.Id).Result;

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { "No name successtions yet, just use a new name :)" };
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

        public static async Task AddAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var possibleChoices = FilterLink.Helper.GetFilterLinksOfGuild(guildUser.GuildId).Result;

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { "Already all links added" };
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

        public static async Task RemoveAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var filterLinksOfGuild = EntityFramework.FilterLinksGuildHelper.GetLinks(guildUser.GuildId).Result;
            var choicesList = new List<string>();
            foreach(var filterlink in filterLinksOfGuild)
            {
                choicesList.Add(filterlink.bezeichnung.Trim());
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
