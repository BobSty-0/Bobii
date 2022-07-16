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
    class TempChannelUpdateChannelOwnerHandler : Discord.Interactions.AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var guildUser = (SocketGuildUser)autocompleteInteraction.User;
            var guild = (SocketGuild)guildUser.Guild;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guild.Id).Result;

            var choicesList = new Dictionary<ulong, string>();

            var userInVoice = guildUser.VoiceState.Value.VoiceChannel.ConnectedUsers.Where(u => u.Id != guildUser.Id);

            foreach (var user in userInVoice)
            {
                if (user.IsBot)
                {
                    continue;
                }
                var username = string.Empty;
                if (user.Nickname != null)
                {
                    username = user.Nickname;
                }
                else
                {
                    username = user.Username;
                }
                choicesList.Add(user.Id, username);
            }
            if (choicesList.Count == 0)
            {
                choicesList.Add(0, Bobii.Helper.GetContent("C094", language).Result);
            }

            var current = autocompleteInteraction.Data.Current.Value.ToString();

            var autocompleteResults = choicesList.Where(c => c.Value.Contains(current)).Select(s => new AutocompleteResult
            {
                Name = s.Value.ToString(),
                Value = s.Key.ToString()
            });

            return Task.FromResult(AutocompletionResult.FromSuccess(autocompleteResults)); ;
        }
    }
}
