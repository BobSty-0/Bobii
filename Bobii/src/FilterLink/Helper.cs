using Bobii.src.DBStuff.Tables;
using Discord;
using Discord.WebSocket;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class Helper
    {
        #region Tasks
        public static async Task WriteMessageToFilterLinkLog(DiscordSocketClient client, ulong guildid, SocketMessage message)
        {
            var channel = client.Guilds
                .SelectMany(g => g.Channels)
                .SingleOrDefault(c => c.Id == ulong.Parse(filterlinklogs.GetFilterLinkLogChannelID(guildid)));
            if (channel == null)
            {
                filterlinklogs.RemoveFilterLinkLog(ulong.Parse(filterlinklogs.GetFilterLinkLogChannelID(guildid)));
                return;
            }
            var textChannel = (ISocketMessageChannel)channel;
            await textChannel.SendMessageAsync($"**Blocked message from:** ID: {message.Author.Id} - <@{message.Author.Id}> \n**Content:**\n{message.Content}");
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

            var linkIsOnWhitelist = new bool();

            for (var countZ = 1; countZ < splitMsg.Count<string>(); countZ++)
            {
                linkIsOnWhitelist = false;
                DataRow linkRow = table.NewRow();
                if (linkOptions != null)
                {
                    foreach (DataRow row in linkOptions.Rows)
                    {
                        if (splitMsg[countZ].StartsWith(row.Field<string>("linkbody").Trim()))
                        {
                            linkIsOnWhitelist = true;
                        }
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
            await Task.CompletedTask;
            return "";
        }

        public static async Task DelayMessage(IUserMessage message, int timeInSeconds)
        {
            timeInSeconds = timeInSeconds * 1000;
            await Task.Delay(timeInSeconds);
            await message.DeleteAsync();

        }

        public static async Task<bool> FilterForFilterLinks(SocketMessage message, ITextChannel channel, DiscordSocketClient client)
        {
            var parsedSocketUser = (SocketUser)message.Author;
            var parsedSocketGuildUser = (SocketGuildUser)parsedSocketUser;

            var whitelistedUsers = filterlinkuserguild.GetUsers(parsedSocketGuildUser.Guild.Id);

            if (!filterlink.IsFilterLinkActive(parsedSocketGuildUser.Guild.Id))
            {
                return true;
            }

            foreach (DataRow row in whitelistedUsers.Rows)
            {
                if (row.Field<string>("userid") == message.Author.Id.ToString())
                {
                    return true;
                }
            }

            if (message.Content.Contains("https://"))
            {
                var link = GetLinkBody(parsedSocketGuildUser.Guild.Id, message.Content, "https://").Result;
                if (link != "")
                {
                    await message.DeleteAsync();
                    var msg = await channel.SendMessageAsync(null, false, TextChannel.TextChannel.CreateEmbed(parsedSocketGuildUser.Guild, "This link is not allowed on this Server!", "Forbidden Link!"));
                    await Handler.MessageReceivedHandler.WriteToConsol($"Information: {parsedSocketGuildUser.Guild.Name} | Task: FilterForFilterWords | Guild: {parsedSocketGuildUser.Guild.Id} | Channel: {channel.Name} | FilteredLink: {link} | Filtered a Link!");
                    if (filterlinklogs.DoesALogChannelExist(parsedSocketGuildUser.Guild.Id))
                    {
                        await WriteMessageToFilterLinkLog(client, parsedSocketGuildUser.Guild.Id, message);
                    }
                    await DelayMessage(msg, 10);
                    return false;
                }
            }

            if (message.Content.Contains("http://"))
            {
                var link = GetLinkBody(parsedSocketGuildUser.Guild.Id, message.Content, "http://").Result;
                if (link != "")
                {
                    await message.DeleteAsync();
                    var msg = await channel.SendMessageAsync(null, false, TextChannel.TextChannel.CreateEmbed(parsedSocketGuildUser.Guild, "This link is not allowed on this Server!", "Forbidden Link!"));
                    await Handler.MessageReceivedHandler.WriteToConsol($"Information: {parsedSocketGuildUser.Guild.Name} | Task: FilterForFilterWords | Guild: {parsedSocketGuildUser.Guild.Id} | Channel: {channel.Name} | FilteredLink: {link} | Filtered a Link!");
                    if (filterlinklogs.DoesALogChannelExist(parsedSocketGuildUser.Guild.Id))
                    {
                        await WriteMessageToFilterLinkLog(client, parsedSocketGuildUser.Guild.Id, message);
                    }
                    await DelayMessage(msg, 10);
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
