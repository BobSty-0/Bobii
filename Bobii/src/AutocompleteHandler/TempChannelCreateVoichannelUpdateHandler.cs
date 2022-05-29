using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.AutocompleteHandler
{
    class TempChannelCreateVoichannelUpdateHandler : Discord.Interactions.AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var guildUser = (SocketGuildUser)autocompleteInteraction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = new string[] { };

            var guild = (SocketGuild)guildUser.Guild;

            var choicesList = new Dictionary<ulong, string>();

            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild);

            foreach (var channel in guild.VoiceChannels)
            {
                var createTempChannel = createTempChannels.Result.Where(ch => ch.createchannelid == channel.Id).FirstOrDefault();
                if (createTempChannel == null)
                {
                    continue;
                }
                choicesList.Add(channel.Id, channel.Name);
            }

            if (choicesList.Count == 0)
            {
                choicesList.Add(0, Bobii.Helper.GetContent("C095", language).Result);
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                choicesList = new Dictionary<ulong, string>();
                choicesList.Add(1, Bobii.Helper.GetCaption("C028", language).Result);
            }

            var current = autocompleteInteraction.Data.Current.Value.ToString();

            var autocompleteResults = choicesList.Where(c => c.Value.Contains(current)).Select(s => new AutocompleteResult {
                Name = s.Value.ToString(),
                Value = s.Key.ToString()
            }) ;

            return Task.FromResult(AutocompletionResult.FromSuccess(autocompleteResults)); ;
        }
    }
}
