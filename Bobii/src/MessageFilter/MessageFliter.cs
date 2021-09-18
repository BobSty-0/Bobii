using Bobii.src.DBStuff.Tables;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.MessageFilter
{
    class MessageFliter
    {
        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} FilterMsg   {message}");
            await Task.CompletedTask;
        }
        #endregion
          
        public static async Task DelayMessage(IUserMessage message, int timeInSeconds)
        {
            timeInSeconds = timeInSeconds * 1000;
            await Task.Delay(timeInSeconds);
            await message.DeleteAsync();

        }

        public static async Task FilterMessageHandler(SocketMessage message, DiscordSocketClient client)
        {
            if (message.Content == "<@!776028262740393985> servercount" && message.Author.Id == 410312323409117185)
            {
                await CreateServerCount(message, client);
            }

            if (message.Content == "<@!776028262740393985> refresh" && message.Author.Id == 410312323409117185)
            {
                await RefreshBobiiStats();
            }

            try
            {
                if (message.Channel is ITextChannel channel)
                {
                    await FilterForFilterWords(message, channel);
                    await FilterForFilterLinks(message, channel);
                }
            }
            catch (InvalidCastException)
            {
                //No need to do anything ... just to provide the bot from spaming the console
            }
        }

        public static async Task RefreshBobiiStats()
        {
            await Handler.HandlingService.RefreshServerCount();
        }

        public static async Task CreateServerCount(SocketMessage message, DiscordSocketClient client)
        {
            var sb = new StringBuilder();
            foreach (var guild in client.Guilds)
            {
                sb.AppendLine(guild.Name);
            }
            sb.AppendLine();
            sb.AppendLine($"Servercount: {client.Guilds.Count}");
            await message.Channel.SendMessageAsync(sb.ToString());
        }

        public static async Task FilterForFilterWords(SocketMessage message, ITextChannel channel)
        {
            var filterWords = filterwords.GetFilterWordListFromGuild(channel.Guild.Id.ToString());
            var parsedSocketUser = (SocketUser)message.Author;
            var parsedSocketGuildUser = (SocketGuildUser)parsedSocketUser;



            string editMessage = message.Content;
            bool messageContainsFilterWord = false;

            foreach (DataRow row in filterWords.Rows)
            {
                if (editMessage.Contains(row.Field<string>("filterword").Trim()))
                {
                    editMessage = editMessage.Replace(row.Field<string>("filterword").Trim(), row.Field<string>("replaceword").Trim());
                    messageContainsFilterWord = true;
                    WriteToConsol($"Information: {parsedSocketGuildUser.Guild.Name} | Task: FilterForFilterWords | Guild: {parsedSocketGuildUser.Guild.Id} | Channel: {channel.Name} | FilterWord: {row.Field<string>("filterword").Trim()}  | FilterWord detected");
                }
            }

            if (messageContainsFilterWord)
            {
                await message.Channel.SendMessageAsync("", false, TextChannel.TextChannel.CreateFilterWordEmbed(parsedSocketUser, parsedSocketGuildUser.Guild.ToString(), editMessage));
                await message.DeleteAsync();
            }
        }

        public static async Task FilterForFilterLinks(SocketMessage message, ITextChannel channel)
        {
            var parsedSocketUser = (SocketUser)message.Author;
            var parsedSocketGuildUser = (SocketGuildUser)parsedSocketUser;

            if (!filterlink.IsFilterLinkActive(parsedSocketGuildUser.Guild.Id.ToString()))
            {
                return;
            }

            if (message.Content.Contains("https://"))
            {
                var link = GetLinkBody(parsedSocketGuildUser.Guild.Id, message.Content, "https://").Result;
                if (link != "")
                {
                    await message.DeleteAsync();
                    var msg = await channel.SendMessageAsync(null, false, TextChannel.TextChannel.CreateEmbed(parsedSocketGuildUser.Guild, "This link is not allowed on this Server!", "Forbidden Link!"));
                    WriteToConsol($"Information: {parsedSocketGuildUser.Guild.Name} | Task: FilterForFilterWords | Guild: {parsedSocketGuildUser.Guild.Id} | Channel: {channel.Name} | FilteredLink: {link} | Filtered a Link!");
                    await DelayMessage(msg, 10);
                }
            }

            if (message.Content.Contains("http://"))
            {
                var link = GetLinkBody(parsedSocketGuildUser.Guild.Id, message.Content, "http://").Result;
                if (link != "")
                {
                    await message.DeleteAsync();
                    var msg = await channel.SendMessageAsync(null, false, TextChannel.TextChannel.CreateEmbed(parsedSocketGuildUser.Guild, "This link is not allowed on this Server!", "Forbidden Link!"));
                    WriteToConsol($"Information: {parsedSocketGuildUser.Guild.Name} | Task: FilterForFilterWords | Guild: {parsedSocketGuildUser.Guild.Id} | Channel: {channel.Name} | FilteredLink: {link} | Filtered a Link!");
                    await DelayMessage(msg, 10);
                }
            }
        }

        public static async Task<string> GetLinkBody(ulong guildid, string msg, string linkType)
        {
            var allowedLinks = filterlinksguild.GetLinks(guildid);
            if (allowedLinks == null)
            {
                return "";
            }
            var listOfAllowedLinks = new List<string>();
            foreach(DataRow link in allowedLinks.Rows)
            {
                listOfAllowedLinks.Add(link.Field<string>("bezeichnung"));
            }

            var linkOptions = filterlinksguild.GetLinkOptions(listOfAllowedLinks);


            var splitMsg = msg.Split(linkType);
            var count = 0;

            var table = new DataTable();
            table.Columns.Add("link", typeof(string));
            table.Columns.Add("bool", typeof(bool));

            DataRow linkRow = table.NewRow();
            var linkIsOnWhitelist = new bool();

            foreach (string frag in splitMsg)
            {
                linkIsOnWhitelist = false;
                count++;
                if (count == splitMsg.Count<string>())
                {
                    continue;
                }
                foreach (DataRow row in linkOptions.Rows)
                {
                    if (splitMsg[count].StartsWith(row.Field<string>("linkbody")))
                    {
                        linkIsOnWhitelist = true;
                    }
                }
                linkRow["link"] = splitMsg[count].Split(" ")[0];
                linkRow["bool"] = linkIsOnWhitelist;
                table.Rows.Add(linkRow);
            }

            foreach (DataRow row in table.Rows)
            {
                if (!row.Field<bool>("bool"))
                {
                    return linkType + row.Field<string>("link");
                }
            }
            return "";
        }
    }
}
