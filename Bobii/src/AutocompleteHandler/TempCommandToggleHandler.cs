using Bobii.src.Bobii;
using Bobii.src.Bobii.EntityFramework;
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
            var lang = BobiiHelper.GetLanguage(context.Guild.Id).Result;
            var tempCommandGroup = client.GetGlobalApplicationCommandsAsync().Result.Single(c => c.Name == "temp").Options;

            var choices = new Dictionary<string, string>();

            foreach(var command in tempCommandGroup)
            {
                choices.Add($"/temp {command.Name}", command.Name);
            }

            choices.Add(GeneralHelper.GetCaption("C277", lang).Result, "ownerpermissions");
            choices.Add(GeneralHelper.GetCaption("C276", lang).Result, GlobalStrings.InterfaceKlein);
            choices.Add(GeneralHelper.GetCaption("C275", lang).Result, GlobalStrings.kickblockedusersonownerchange);
            choices.Add(GeneralHelper.GetCaption("C274", lang).Result, GlobalStrings.hidevoicefromblockedusers);
            choices.Add(GeneralHelper.GetCaption("C273", lang).Result, GlobalStrings.autotransferowner);

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
