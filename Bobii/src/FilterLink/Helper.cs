using Bobii.src.Models;
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
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildid).Result;
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
            await textChannel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(guild,
                string.Format(Bobii.Helper.GetContent("C073", language).Result, message.Author.Id, message.Content),
                Bobii.Helper.GetCaption("C073", language).Result).Result);
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
                    var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(parsedSocketGuildUser.Guild.Id).Result;
                    var msg = await channel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(parsedSocketGuildUser.Guild, 
                        Bobii.Helper.GetContent("C074", language).Result, 
                        Bobii.Helper.GetCaption("C074", language).Result).Result);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("MsgRecievd", false, "FilterForFilterLinks", 
                        new SlashCommandParameter() { Guild = parsedSocketGuildUser.Guild, GuildUser = parsedSocketGuildUser },
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
                    var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(parsedSocketGuildUser.Guild.Id).Result;
                    var msg = await channel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(parsedSocketGuildUser.Guild,
                        Bobii.Helper.GetContent("C074", language).Result,
                        Bobii.Helper.GetCaption("C074", language).Result).Result);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("MsgRecievd", false, "FilterForFilterLinks", 
                        new SlashCommandParameter() { Guild = parsedSocketGuildUser.Guild, GuildUser = parsedSocketGuildUser },
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

        public static async Task<Embed> CreateFilterLinkUserWhitelistInfoEmbed(SlashCommandParameter parameter)
        {
            StringBuilder sb = new StringBuilder();
            var userOnWhitelist = EntityFramework.FilterLinkUserGuildHelper.GetUsers(parameter.GuildID).Result;
            string header = null;

            if (userOnWhitelist.Count == 0)
            {
                header = Bobii.Helper.GetCaption("C075", parameter.Language).Result;
                sb.AppendLine(Bobii.Helper.GetContent("C075", parameter.Language).Result);
            }
            else
            {
                header = Bobii.Helper.GetContent("C076 ", parameter.Language).Result;
            }

            foreach (var user in userOnWhitelist)
            {
                sb.AppendLine("");
                sb.AppendFormat($"<@{user.userid}>");
            }

            await Task.CompletedTask;
            return Bobii.Helper.CreateEmbed(parameter.Interaction, sb.ToString(), header).Result;
        }

        public static async Task<Embed> CreateFilterLinkLinkWhitelistInfoEmbed(SlashCommandParameter parameter)
        {
            StringBuilder sb = new StringBuilder();
            var filterLinksOnWhitelist = EntityFramework.FilterLinksGuildHelper.GetLinks(parameter.GuildID).Result;
            string header = null;

            if (filterLinksOnWhitelist.Count == 0)
            {
                header = Bobii.Helper.GetCaption("C077", parameter.Language).Result;
                sb.AppendLine(Bobii.Helper.GetContent("C077", parameter.Language).Result);
            }
            else
            {
                header = Bobii.Helper.GetContent("C078", parameter.Language).Result;
            }

            foreach (var filterlink in filterLinksOnWhitelist)
            {
                sb.AppendLine($"{filterlink.bezeichnung.Trim()}");
            }

            await Task.CompletedTask;
            return Bobii.Helper.CreateEmbed(parameter.Interaction, sb.ToString(), header).Result;
        }

        private static async Task<string> FLLHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList, string language)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, language, Bobii.Helper.GetCaption("C078", language).Result, "fll").Result;
        }
        private static async Task<string> FLCreateDeleteHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList, string language)
        {
            await Task.CompletedTask;
            return Bobii.Helper.GetContent("C079", language).Result +  Bobii.Helper.CreateInfoPart(commandList, language, Bobii.Helper.GetCaption("C080", language).Result, "flcreate", "fldelete").Result;
        }

        private static async Task<string> FLUHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList, string language)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, language, Bobii.Helper.GetCaption("C081", language).Result, "flu").Result;
        }

        private static async Task<string> FLLogHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList, string language)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, language, Bobii.Helper.GetCaption("C082", language).Result, "log").Result;
        }

        public static async Task<string> HelpFilterLinkInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            return Bobii.Helper.CreateInfoPart(commandList, language, Bobii.Helper.GetContent("C083", language).Result, 
                "flinfo", "flset").Result +
                FLCreateDeleteHelpTeil(commandList, language).Result + 
                FLLHelpTeil(commandList, language).Result +
                FLUHelpTeil(commandList, language).Result +
                FLLogHelpTeil(commandList, language).Result;
        }

        public static async Task<Embed> CreateFLGuildInfo(SocketInteraction interaction, ulong guildId)
        {
            var guildLinks = EntityFramework.FilterLinkOptionsHelper.GetOptionsFromGuild(guildId).Result;
            var guildOptions = EntityFramework.FilterLinkOptionsHelper.GetAllOptionsFromGuildOrderByBezeichnung(guildId).Result;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;

            var sb = new StringBuilder();

            var title = string.Empty;
            if (guildOptions.Count() == 0)
            {
                title = Bobii.Helper.GetCaption("C084", language).Result;
                sb.AppendLine(Bobii.Helper.GetContent("C084", language).Result);
            }
            else
            {
                title = Bobii.Helper.GetContent("C085", language).Result;
            }

            foreach(var option in guildOptions)
            {
                var listOfGuildLinks = guildLinks.Where(l => l.bezeichnung == option);

                sb.AppendLine();
                sb.AppendLine($"**{option}:**");
                foreach (var link in listOfGuildLinks)
                {
                    sb.AppendLine($"https://{link.linkbody}");
                }
            }

            return Bobii.Helper.CreateEmbed(interaction, sb.ToString(), title).Result;
        }
        #endregion
    }
}
