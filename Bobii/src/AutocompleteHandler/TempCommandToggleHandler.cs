using Bobii.src.Bobii;
using Bobii.src.Helper;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.AutocompleteHandler
{
    class TempCommandToggleHandler : Discord.Interactions.AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var client = context.Client;
            var tempCommandGroup = client.GetGlobalApplicationCommandsAsync().Result.Single(c => c.Name == "temp").Options;

            var choices = new Dictionary<string, string>();

            foreach(var command in tempCommandGroup)
            {
                choices.Add($"/temp {command.Name}", command.Name);
            }

            choices.Add("Owner permissions", "ownerpermissions");
            choices.Add("Intefrace", GlobalStrings.InterfaceKlein);
            choices.Add("Kick blocked users on owner change", GlobalStrings.kickblockedusersonownerchange);

            var current = autocompleteInteraction.Data.Current.Value.ToString();

            var autocompleteResults = choices.Where(c => c.Value.Contains(current)).Select(s => new AutocompleteResult
            {
                Name = s.Key.ToString(),
                Value = s.Value.ToString()
            });

            return Task.FromResult(AutocompletionResult.FromSuccess(autocompleteResults)); 
        }
    }
}
