using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TextUtility
{
    class Autocomplete
    {
        public static async Task ChatMessagesAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (SocketGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = new string[] { };

            var guild = (SocketGuild)guildUser.Guild;


            var messages = Helper.GetBobiiEmbedMessages(interaction.Channel).Result;

            var messagesList = new List<string>();
            foreach (var message in messages)
            {
                var textMessage = $"{message.Id} {new string(message.Embeds.First().Title.Take(10).ToArray())}...";
                messagesList.Add(textMessage);
            }

            if (messagesList.Count == 0)
            {
                possibleChoices = new string[] { "Could not find any messages" };
            }
            else
            {
                possibleChoices = messagesList.ToArray();
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(interaction, possibleChoices);
        }
    }
}
