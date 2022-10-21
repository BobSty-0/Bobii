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
    class EditEmbedHandler : Discord.Interactions.AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var guildUser = (SocketGuildUser)context.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = new string[] { };

            var guild = (SocketGuild)guildUser.Guild;


            var messages = TextUtilityHelper.GetBobiiEmbedMessages((ISocketMessageChannel)context.Channel).Result;
            var choicesList = new Dictionary<ulong, string>();

            var messagesList = new List<string>();
            foreach (var message in messages)
            {
                var embed = message.Embeds.First();
                var textMessage = string.Empty;
                if (embed.Title != null)
                {
                    textMessage = $"{new string(embed.Title.Take(30).ToArray())}...";
                }
                else if (embed.Description != null)
                {
                    textMessage = $"{new string(embed.Description.Take(20).ToArray())}...";
                }
                else
                {
                    textMessage = "";
                }

                choicesList.Add(message.Id, textMessage);
            }

            if (choicesList.Count == 0)
            {
                choicesList.Add(0, GeneralHelper.GetContent("C138", language).Result);
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                choicesList = new Dictionary<ulong, string>();
                choicesList.Add(1, GeneralHelper.GetCaption("C028", language).Result);
            }
            var current = autocompleteInteraction.Data.Current.Value.ToString();

            var autocompleteResults = choicesList.Where(c => c.Value.Contains(current)).Select(s => new AutocompleteResult
            {
                Name = s.Value.ToString(),
                Value = s.Key.ToString()
            });

            return Task.FromResult(AutocompletionResult.FromSuccess(autocompleteResults));
        }
    }
}
