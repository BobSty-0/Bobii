using Bobii.src.DBStuff.Tables;
using Discord;
using Discord.WebSocket;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Bobii.src.FilterWord
{
    class Helper
    {
        #region Tasks
        public static async Task<bool> FilterForFilterWords(SocketMessage message, ITextChannel channel)
        {
            var filterWords = filterwords.GetFilterWordListFromGuild(channel.Guild.Id.ToString());
            var parsedSocketUser = (SocketUser)message.Author;
            var parsedSocketGuildUser = (SocketGuildUser)parsedSocketUser;

            string editMessage = message.Content;
            bool messageContainsFilterWord = false;

            foreach (DataRow row in filterWords.Rows)
            {
                var messageWords = message.Content.Split(" ");
                foreach (string word in messageWords)
                {
                    if (word.Contains(row.Field<string>("filterword").Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        //Forbiddeen words in links should not be replaced
                        if (word.Contains("https://") || word.Contains("http://"))
                        {
                            continue;
                        }
                        messageContainsFilterWord = true;
                        editMessage = editMessage.Replace(word, row.Field<string>("ReplaceWord").Trim());
                    }
                }
            }

            if (messageContainsFilterWord)
            {
                await message.Channel.SendMessageAsync("", false, TextChannel.TextChannel.CreateFilterWordEmbed(parsedSocketUser, parsedSocketGuildUser.Guild.ToString(), editMessage, message));
                await message.DeleteAsync();
                return false;
            }
            return false;
        }
        #endregion
    }
}
