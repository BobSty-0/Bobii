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
                var embed = message.Embeds.First();
                var textMessage = string.Empty;
                if (embed.Title != null)
                {
                    textMessage = $"{message.Id} {new string(embed.Title.Take(20).ToArray())}...";
                }
                else if (embed.Description != null)
                {
                    textMessage = $"{message.Id} {new string(embed.Description.Take(20).ToArray())}...";
                }
                else
                {
                    textMessage = $"{message.Id}";
                }
                
                messagesList.Add(textMessage);
            }

            if (messagesList.Count == 0)
            {
                possibleChoices = new string[] { Bobii.Helper.GetContent("C138", language).Result };
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
