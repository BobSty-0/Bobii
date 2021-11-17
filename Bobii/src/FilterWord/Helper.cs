using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterWord
{
    class Helper
    {
        #region Tasks
        public static async Task<bool> FilterForFilterWords(SocketMessage message, ITextChannel channel)
        {
            var filterWords = EntityFramework.FilterWordsHelper.GetFilterWordsFromGuildAsList(channel.Guild.Id).Result;
            var parsedSocketUser = (SocketUser)message.Author;
            var parsedSocketGuildUser = (SocketGuildUser)parsedSocketUser;

            string editMessage = message.Content;
            bool messageContainsFilterWord = false;

            foreach (var filterWord in filterWords)
            {
                var messageWords = message.Content.Split(" ");
                foreach (string word in messageWords)
                {
                    if (word.Contains(filterWord.filterword.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        //Forbiddeen words in links should not be replaced
                        if (word.Contains("https://") || word.Contains("http://"))
                        {
                            continue;
                        }
                        messageContainsFilterWord = true;
                        editMessage = editMessage.Replace(word, filterWord.replaceword.Trim());
                    }
                }
            }

            if (messageContainsFilterWord)
            {
                await message.Channel.SendMessageAsync("", false, FilterWord.Helper.CreateFilterWordEmbed(parsedSocketUser, parsedSocketGuildUser.Guild.ToString(), editMessage, message).Result);
                await message.DeleteAsync();
                return false;
            }
            return false;
        }

        public static async Task<Embed> CreateFilterWordEmbed(SocketInteraction interaction, ulong guildId)
        {
            StringBuilder sb = new StringBuilder();
            var filterWordList = EntityFramework.FilterWordsHelper.GetFilterWordsFromGuildAsList(guildId).Result;
            string header = null;
            if (filterWordList.Count == 0)
            {
                header = "No filter words yet!";
                sb.AppendLine("You dont have any filter words yet!\nYou can add some with:\n **/fwadd <FilterWord> <ReplaceWord>**");
            }
            else
            {
                header = "Here a list of all filter words of this Guild:";
            }

            foreach (var filterWord in filterWordList)
            {
                sb.AppendLine("");
                sb.Append($"Filter word: **{filterWord.filterword}**");
                sb.AppendLine($" -> Replaced with: **{filterWord.replaceword}**");
            }
            await Task.CompletedTask;
            return Bobii.Helper.CreateEmbed(interaction, sb.ToString(), header).Result;
        }

        //Double Code -> Find solution one day!
        public static async Task<string> HelpFilterWordInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You can replace unwanted words from users' messages. I will automatically detect the words, delete the user's message and create a new message in which the unwanted words are replaced.\nTo start, simply add a filter word.");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("fw"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append("**/" + command.Name);
                        foreach (var option in command.Options)
                        {
                            sb.Append(" <" + option.Name + ">");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }

        public static async Task<Embed> CreateFilterWordEmbed(SocketUser user, string guildName, string body, SocketMessage message)
        {
            if (message.Attachments.Count != 0)
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .WithAuthor(user)
                    .WithColor(0, 225, 225)
                    .WithDescription(body)
                    .WithFooter("Some attachments might have been deleted and not resend.\n" + guildName + DateTime.Now.ToString(" • dd/MM/yyyy"))
                    .WithUrl(message.Attachments.First().ProxyUrl)
                    .WithImageUrl(message.Attachments.First().Url);
                await Task.CompletedTask;
                return embed.Build();
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .WithAuthor(user)
                    .WithColor(0, 225, 225)
                    .WithDescription(body)
                    .WithFooter(guildName + DateTime.Now.ToString(" • dd/MM/yyyy"));
                await Task.CompletedTask;
                return embed.Build();
            }
            
        }
        #endregion
    }
}
