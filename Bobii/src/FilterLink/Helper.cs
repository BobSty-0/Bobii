using Bobii.src.DBStuff.Tables;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class Helper
    {
        #region Tasks
        public static async Task<string[]> GetFilterLinksOfGuild(ulong guildId)
        {
            var possibleChoices = DBStuff.Tables.filterlinkoptions.GetAllOptions();
            var filterLinksOfGuild = DBStuff.Tables.filterlinksguild.GetLinks(guildId);

            foreach (var choice in possibleChoices)
            {
                foreach (DataRow row in filterLinksOfGuild.Rows)
                {
                    if (row.Field<string>("bezeichnung").Trim() == choice)
                    {
                        //Im selecting all the choices except the one which is already used by the guild
                        possibleChoices = possibleChoices.Where(ch => ch != choice).ToArray();
                    }
                }
            }
            await Task.CompletedTask;
            return possibleChoices;
        }

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
                    var msg = await channel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(parsedSocketGuildUser.Guild, "This link is not allowed on this Server!", "Forbidden Link!").Result);
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
                    var msg = await channel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(parsedSocketGuildUser.Guild, "This link is not allowed on this Server!", "Forbidden Link!").Result);
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

        public static async Task<Embed> CreateFilterLinkUserWhitelistInfoEmbed(SocketInteraction interaction, ulong guildid)
        {
            StringBuilder sb = new StringBuilder();
            var userOnWhitelist = DBStuff.Tables.filterlinkuserguild.GetUsers(guildid);
            string header = null;

            if (userOnWhitelist.Rows.Count == 0)
            {
                header = "No whitelisted users yet!";
                sb.AppendLine("You dont have any users on the whitelist yet!\nYou can add users to the whitelist with:\n **/fluadd <link>**");
            }
            else
            {
                header = "Here is a list of all the users on the whitelist of this guild";
            }

            foreach (DataRow row in userOnWhitelist.Rows)
            {
                sb.AppendLine("");
                sb.AppendFormat($"<@{row.Field<string>("userid")}>");
            }

            var filterLinkActiveText = "";
            if (!filterlink.IsFilterLinkActive(guildid))
            {
                filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
            }
            await Task.CompletedTask;
            return Bobii.Helper.CreateEmbed(interaction, sb.ToString() + filterLinkActiveText, header).Result;
        }

        public static async Task<Embed> CreateFilterLinkLinkWhitelistInfoEmbed(SocketInteraction interaction, ulong guildId)
        {
            StringBuilder sb = new StringBuilder();
            var filterLinksOnWhitelist = DBStuff.Tables.filterlinksguild.GetLinks(guildId);
            string header = null;

            if (filterLinksOnWhitelist.Rows.Count == 0)
            {
                header = "No whitelisted links yet!";
                sb.AppendLine("You dont have any links on the whitelist yet!\nYou can add links to the whitelist with:\n **/flladd <link>**");
            }
            else
            {
                header = "Here is a list of all the links on the whitelist of this guild";
            }

            foreach (DataRow row in filterLinksOnWhitelist.Rows)
            {
                sb.AppendLine($"{row.Field<string>("bezeichnung")}");
            }

            var filterLinkActiveText = "";
            if (!filterlink.IsFilterLinkActive(guildId))
            {
                filterLinkActiveText = "\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
            }
            await Task.CompletedTask;
            return Bobii.Helper.CreateEmbed(interaction, sb.ToString() + filterLinkActiveText, header).Result;
        }

        private static async Task<string> FLLHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("_Manage the links of the whitelist:_");
            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("fll"))
                {
                    sb.AppendLine("");
                    sb.AppendLine($"**/{command.Name}**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append($"**/{command.Name}");
                        foreach (var option in command.Options)
                        {
                            sb.Append($" <{option.Name}>");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }

        private static async Task<string> FLUHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("_Manage the users of the whitelist:_");
            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("flu"))
                {
                    sb.AppendLine("");
                    sb.AppendLine($"**/{command.Name}**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append($"**/{command.Name}");
                        foreach (var option in command.Options)
                        {
                            sb.Append($" <{option.Name}>");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }

        private static async Task<string> FLLogHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("_Manage the log channel of filter link:_");
            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("log"))
                {
                    sb.AppendLine("");
                    sb.AppendLine($"**/{command.Name}**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append($"**/{command.Name}");
                        foreach (var option in command.Options)
                        {
                            sb.Append($" <{option.Name}>");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }

        //Double Code -> Find solution one day!
        public static async Task<string> HelpFilterLinkInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Filter link will block every kind of links as soon as you activated it. You can then start whitelisting links which wont be blocked and users which will not be affected by filter link. I currently only have a couple of choices for links to whitelist so if you want to whitelist an link which I forgot to provide as choice feel free to send a direct message to <@776028262740393985>");

            //Filterlink in generall
            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("flinfo") || command.Name.Contains("flset"))
                {
                    sb.AppendLine("");
                    sb.AppendLine($"**/{command.Name}**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append($"**/{command.Name}");
                        foreach (var option in command.Options)
                        {
                            sb.Append($" <{option.Name}>");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString() + FLLHelpTeil(commandList).Result + FLUHelpTeil(commandList).Result + FLLogHelpTeil(commandList).Result;
        }
        #endregion
    }
}
