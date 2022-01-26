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
            var possibleChoices = EntityFramework.FilterLinkOptionsHelper.GetAllOptionsFuerGuild(guildId).Result;
            var filterLinksOfGuild = EntityFramework.FilterLinksGuildHelper.GetLinks(guildId).Result;

            foreach (var choice in possibleChoices)
            {
                foreach (var filterLink in filterLinksOfGuild)
                {
                    if (filterLink.bezeichnung.Trim() == choice)
                    {
                        //Im selecting all the choices except the one which is already used by the guild
                        possibleChoices = possibleChoices.Where(ch => ch != choice).ToArray();
                    }
                }
            }
            await Task.CompletedTask;
            return possibleChoices;
        }

        public static async Task WriteMessageToFilterLinkLog(DiscordSocketClient client, ulong guildid, SocketMessage message, SocketGuild guild)
        {
            var filterLinkLogChannelID = EntityFramework.FilterLinkLogsHelper.GetFilterLinkLogChannelID(guildid).Result;
            var channel = client.Guilds
                .SelectMany(g => g.Channels)
                .SingleOrDefault(c => c.Id == filterLinkLogChannelID);
            if (channel == null)
            {
                await EntityFramework.FilterLinkLogsHelper.RemoveFilterLinkLogChannel(filterLinkLogChannelID);
                return;
            }
            var textChannel = (ISocketMessageChannel)channel;
            await textChannel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(guild, $"**Author: **<@{message.Author.Id}> \n**Author ID:** {message.Author.Id} \n\n**Content:**\n{message.Content}", "Filtered Link:").Result);
        }

        public static async Task<string> GetLinkBody(ulong guildid, string msg, string linkType)
        {
            var allowedLinks = EntityFramework.FilterLinksGuildHelper.GetLinks(guildid).Result;
            if (allowedLinks == null)
            {
                return "";
            }

            var linkOptions = EntityFramework.FilterLinkOptionsHelper.GetLinkOptions(allowedLinks, guildid).Result;

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
                    foreach (var option in linkOptions)
                    {
                        if (splitMsg[countZ].StartsWith(option.linkbody.Trim()))
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

            var whitelistedUsers = EntityFramework.FilterLinkUserGuildHelper.GetUsers(parsedSocketGuildUser.Guild.Id).Result;

            if (!EntityFramework.FilterlLinksHelper.FilterLinkAktive(parsedSocketGuildUser.Guild.Id).Result)
            {
                return true;
            }

            var whitelistedUser = whitelistedUsers.Where(user => user.userid == message.Author.Id).FirstOrDefault();
            if (whitelistedUser != null)
            {
                return true;
            }

            if (message.Content.Contains("https://"))
            {
                var link = GetLinkBody(parsedSocketGuildUser.Guild.Id, message.Content, "https://").Result;
                if (link != "")
                {
                    await message.DeleteAsync();
                    var msg = await channel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(parsedSocketGuildUser.Guild, "This link is not allowed on this Server!", "Forbidden Link!").Result);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("MsgRecievd", false, "FilterForFilterLinks", new Entities.SlashCommandParameter() { Guild = parsedSocketGuildUser.Guild, GuildUser = parsedSocketGuildUser },
                        message: "Filtered a Link!", link: link);
                    if (EntityFramework.FilterLinkLogsHelper.DoesALogChannelExist(parsedSocketGuildUser.Guild.Id).Result)
                    {
                        await WriteMessageToFilterLinkLog(client, parsedSocketGuildUser.Guild.Id, message, parsedSocketGuildUser.Guild);
                    }
                    _ = DelayMessage(msg, 10);
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
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("MsgRecievd", false, "FilterForFilterLinks", new Entities.SlashCommandParameter() { Guild = parsedSocketGuildUser.Guild, GuildUser = parsedSocketGuildUser },
                        message: "Filtered a Link!", link: link);
                    if (EntityFramework.FilterLinkLogsHelper.DoesALogChannelExist(parsedSocketGuildUser.Guild.Id).Result)
                    {
                        await WriteMessageToFilterLinkLog(client, parsedSocketGuildUser.Guild.Id, message, parsedSocketGuildUser.Guild);
                    }
                    _ = DelayMessage(msg, 10);
                    return false;
                }
            }
            return true;
        }

        public static async Task<Embed> CreateFilterLinkUserWhitelistInfoEmbed(SocketInteraction interaction, ulong guildid)
        {
            StringBuilder sb = new StringBuilder();
            var userOnWhitelist = EntityFramework.FilterLinkUserGuildHelper.GetUsers(guildid).Result;
            string header = null;

            if (userOnWhitelist.Count == 0)
            {
                header = "No whitelisted users yet!";
                sb.AppendLine("You dont have any users on the whitelist yet!\nYou can add users to the whitelist with:\n **/fluadd <link>**");
            }
            else
            {
                header = "Here is a list of all the users on the whitelist of this guild";
            }

            foreach (var user in userOnWhitelist)
            {
                sb.AppendLine("");
                sb.AppendFormat($"<@{user.userid}>");
            }

            var filterLinkActiveText = "";
            if (!EntityFramework.FilterlLinksHelper.FilterLinkAktive(guildid).Result)
            {
                filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
            }
            await Task.CompletedTask;
            return Bobii.Helper.CreateEmbed(interaction, sb.ToString() + filterLinkActiveText, header).Result;
        }

        public static async Task<Embed> CreateFilterLinkLinkWhitelistInfoEmbed(SocketInteraction interaction, ulong guildId)
        {
            StringBuilder sb = new StringBuilder();
            var filterLinksOnWhitelist = EntityFramework.FilterLinksGuildHelper.GetLinks(guildId).Result;
            string header = null;

            if (filterLinksOnWhitelist.Count == 0)
            {
                header = "No whitelisted links yet!";
                sb.AppendLine("You dont have any links on the whitelist yet!\nYou can add links to the whitelist with:\n **/flladd <link>**");
            }
            else
            {
                header = "Here is a list of all the links on the whitelist of this guild";
            }

            foreach (var filterlink in filterLinksOnWhitelist)
            {
                sb.AppendLine($"{filterlink.bezeichnung.Trim()}");
            }

            var filterLinkActiveText = "";
            if (!EntityFramework.FilterlLinksHelper.FilterLinkAktive(guildId).Result)
            {
                filterLinkActiveText = "\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
            }
            await Task.CompletedTask;
            return Bobii.Helper.CreateEmbed(interaction, sb.ToString() + filterLinkActiveText, header).Result;
        }

        private static async Task<string> FLLHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "\n_Manage the links of the whitelist:_", "fll").Result;
        }
        private static async Task<string> FLCreateDeleteHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "\n_Create and delete filter-link-options_", "flcreate", "fldelete").Result;
        }

        private static async Task<string> FLUHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "\n_Manage the users of the whitelist:_", "flu").Result;
        }

        private static async Task<string> FLLogHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "\n_Manage the log channel of filter link:_", "log").Result;
        }

        public static async Task<string> HelpFilterLinkInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "Filter link will block every kind of links as soon as " +
                "you activated it. You can then start whitelisting links which wont be blocked and users which will not " +
                "be affected by filter link. I currently only have a couple of choices so if you need a choice, simply use `/flcreate` to create ur own.", 
                "flinfo", "flset").Result +
                FLCreateDeleteHelpTeil(commandList).Result + 
                FLLHelpTeil(commandList).Result +
                FLUHelpTeil(commandList).Result +
                FLLogHelpTeil(commandList).Result;
        }
        #endregion
    }
}
