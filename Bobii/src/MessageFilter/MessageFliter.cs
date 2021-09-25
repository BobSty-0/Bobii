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
        private static bool _useFilterWord = false;

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
            if (message.Author.IsBot)
            {
                return;
            }

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
                    await FilterForFilterLinks(message, channel);
                    if (_useFilterWord)
                    {
                        await FilterForFilterWords(message, channel);
                    }
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
            var sb = new StringBuilder();

            var forbiddenWords = new DataTable();
            forbiddenWords.Columns.Add("ForbiddenWord", typeof(string));
            forbiddenWords.Columns.Add("ReplaceWord", typeof(string));
            DataRow wordRow = forbiddenWords.NewRow();

            foreach (DataRow row in filterWords.Rows)
            {
                var messageWords = message.Content.Split(" ");
                foreach (string word in messageWords)
                {
                    if(word.Contains(row.Field<string>("filterword").Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        messageContainsFilterWord = true;
                        wordRow["ForbiddenWord"] = word;
                        wordRow["ReplaceWord"] = row.Field<string>("replaceword").Trim();
                        forbiddenWords.Rows.Add(wordRow);
                    }
                }
            }

            if (messageContainsFilterWord)
            {
                foreach(DataRow row in forbiddenWords.Rows)
                {
                    editMessage = editMessage.Replace(row.Field<string>("ForbiddenWord"), row.Field<string>("ReplaceWord"));
                }
                _useFilterWord = false;
                await message.Channel.SendMessageAsync("", false, TextChannel.TextChannel.CreateFilterWordEmbed(parsedSocketUser, parsedSocketGuildUser.Guild.ToString(), editMessage));
                await message.DeleteAsync();
            }
            _useFilterWord = false;
        }

        public static async Task FilterForFilterLinks(SocketMessage message, ITextChannel channel)
        {
            var parsedSocketUser = (SocketUser)message.Author;
            var parsedSocketGuildUser = (SocketGuildUser)parsedSocketUser;

            var whitelistedUsers = filterlinkuserguild.GetUsers(parsedSocketGuildUser.Guild.Id);

            if (!filterlink.IsFilterLinkActive(parsedSocketGuildUser.Guild.Id.ToString()))
            {
                _useFilterWord = true;
                return;
            }

            foreach (DataRow row in whitelistedUsers.Rows)
            {
                if (row.Field<string>("userid") == message.Author.Id.ToString())
                {
                    _useFilterWord = true;
                    return;
                }
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
                    _useFilterWord = false;
                    return;
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
                    _useFilterWord = false;
                    return;
                }
            }
            _useFilterWord = true;
        }

        public static async Task<string> GetLinkBody(ulong guildid, string msg, string linkType)
        {
            var allowedLinks = filterlinksguild.GetLinks(guildid);
            if (allowedLinks == null)
            {
                return "";
            }

            var linkOptions = filterlinksguild.GetLinkOptions(allowedLinks);


            var splitMsg = msg.Split(linkType);


            var table = new DataTable();
            table.Columns.Add("link", typeof(string));
            table.Columns.Add("bool", typeof(bool));
            DataRow linkRow = table.NewRow();

            var linkIsOnWhitelist = new bool();

            for (var countZ = 1; countZ < splitMsg.Count<string>(); countZ++)
            {
                linkIsOnWhitelist = false;

                foreach (DataRow row in linkOptions.Rows)
                {
                    if (splitMsg[countZ].StartsWith(row.Field<string>("linkbody")))
                    {
                        linkIsOnWhitelist = true;
                    }
                }
                linkRow["link"] = splitMsg[countZ].Split(" ")[0];
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
