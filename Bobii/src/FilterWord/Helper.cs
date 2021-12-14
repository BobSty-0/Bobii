using Discord;
using Discord.Rest;
using Discord.Webhook;
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
        public static async Task<bool> FilterForFilterWords(SocketMessage message, ITextChannel channel, DiscordSocketClient client)
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
                var socketGuildChannel = (SocketGuildChannel)message.Channel;
                var guild = socketGuildChannel.Guild;
                SocketGuildUser bobii = null;

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    bobii = guild.GetUser(869180143363584060);
                }
                else
                {
                    bobii = guild.GetUser(776028262740393985);
                }

                if (bobii.GuildPermissions.ManageWebhooks)
                {
                    var webhook = ((ITextChannel)message.Channel).CreateWebhookAsync(socketGuildChannel.Name).Result;
                    using (var webhookClient = new DiscordWebhookClient(webhook))
                    {
                        if (message.Attachments.Count > 0)
                        {
                            await Bobii.Helper.SendMessageWithAttachments(message, Bobii.Enums.TextChannel.DiscordWebhookClient, 
                                editedMessage: editMessage, webhookClient: webhookClient);
                        }
                        else
                        {
                            await webhookClient.SendMessageAsync(editMessage, username: message.Author.Username, avatarUrl: message.Author.GetAvatarUrl());
                        }
                    }
                    await webhook.DeleteAsync();
                }
                else
                {
                    if (message.Attachments.Count > 0)
                    {
                        await Bobii.Helper.SendMessageWithAttachments(message, Bobii.Enums.TextChannel.ISocketMessageChannel, socketMessageChannel: message.Channel,
                            filterWordEmbed: FilterWord.Helper.CreateFilterWordEmbed(parsedSocketUser, parsedSocketGuildUser.Guild.ToString(), editMessage).Result);
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("", false, FilterWord.Helper.CreateFilterWordEmbed(parsedSocketUser, parsedSocketGuildUser.Guild.ToString(), editMessage).Result);
                    }
                }
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
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "You can replace unwanted words from users' messages. I will automatically" +
                " detect the words, delete the user's message and create a new message in which the unwanted words are replaced." +
                "\nTo start, simply add a filter word.", "fw").Result;
        }

        public static async Task<Embed> CreateFilterWordEmbed(SocketUser user, string guildName, string body)
        {
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .WithAuthor(user)
                    .WithColor(74, 171, 189)
                    .WithDescription(body)
                    .WithFooter(guildName + DateTime.Now.ToString(" • dd/MM/yyyy"));
                await Task.CompletedTask;
                return embed.Build();
            }

        }
        #endregion
    }
}
