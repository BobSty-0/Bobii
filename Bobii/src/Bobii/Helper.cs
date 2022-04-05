using Bobii.src.Entities;
using Bobii.src.EntityFramework;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    public delegate Task WriteToConsoleEventHandler(object source, EventArg.WriteConsoleEventArg e);
    public class Helper
    {
        #region Declarations
        public event WriteToConsoleEventHandler WriteConsoleEventHandler;
        #endregion

        #region Tasks
        public static string ReadBobiiConfig(string key)
        {
            try
            {
                JObject config = Program.GetConfig();
                return config["BobiiConfig"][0][key].Value<string>();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"---------------------------------------------- No {key} key in the config.json file detected! ----------------------------------------------");
                return "";
            }
        }

        public static async Task DoUpdate(SlashCommandParameter parameter, Enums.DatabaseConnectionString connectionstring)
        {
            // Enum einbauen
            // Split testen
            var pgBinPath = ReadBobiiConfig(ConfigKeys.PGBinPath);
            var connectionString = "";
            if (connectionstring == Enums.DatabaseConnectionString.ConnectionString)
            {
                connectionString = ReadBobiiConfig(ConfigKeys.ConnectionString);
            }
            else
            {
                connectionString = ReadBobiiConfig(ConfigKeys.ConnectionStringLng);
            }
            var databaseName = connectionString.Split('=')[3].Split(';')[0]; // "Bobii";
            var server = connectionString.Split('=')[1].Split(';')[0]; //  "localhost"; 
            var port = connectionString.Split('=')[2].Split(';')[0]; //  "5432";
            var username = connectionString.Split('=')[4].Split(';')[0]; //" postgres";
            var password = connectionString.Split('=')[5].Split(';')[0]; // imgaine

            try
            {

                StreamWriter sw = new StreamWriter("DBRestore.bat");
                // Do not change lines / spaces b/w words.
                //sw.WriteLine($"\"SET PGPASSWORD={password}\"");
                var test = $"\"{pgBinPath}pg_dump.exe\" --no-owner --dbname=postgesql://{username}:{password}@{server}:{port}/{databaseName}";
                sw.WriteLine(test);
                //sw.WriteLine($"PGPASSWORD={password}&& \"{pgBinPath}pg_dump.exe\" -U {username} -h {server} -p {port}  { databaseName} > {Directory.GetCurrentDirectory() + "\\Backup\\test.sql"}");
                sw.Dispose();
                sw.Close();
                Process processDB = Process.Start("DBRestore.bat");
                do
                {//dont perform anything
                }
                while (!processDB.HasExited);
                {
                    Console.WriteLine("Sprachcode einbauen aber erfolgreich");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex:" + ex.Message);
            }
        }

        public static async Task RespondToAutocomplete(SocketAutocompleteInteraction interaction, string[] possibleChoices)
        {
            // lets get the current value they have typed. Note that were converting it to a string for this example, the autocomplete works with int and doubles as well.
            var current = interaction.Data.Current.Value.ToString();

            // We will get the first 20 options inside our string array that start with whatever the user has typed.
            var opt = possibleChoices.Where(x => x.StartsWith(current)).Take(20);

            // Then we can send them to the client
            await interaction.RespondAsync(opt.Select(x => new AutocompleteResult(x, x.ToLower())));
        }

        public static async Task KeepBobiiBusy()
        {
            while (true == true)
            {
                System.Threading.Thread.Sleep(30000);
                await Task.CompletedTask;
            }
        }

        public static async Task<string> GetContent(string msgId, string language)
        {
            var content = string.Empty;
            var cache = src.Handler.HandlingService._cache;
            switch (language)
            {
                case "en":
                    content = cache.Contents.First(m => m.msgid == msgId).en;
                    break;
                case "de":
                    content = cache.Contents.First(m => m.msgid == msgId).de;
                    break;
                default:
                    content = cache.Contents.First(m => m.msgid == msgId).en;
                    break;
            }
            await Task.CompletedTask;
            return content;
        }

        public static async Task<string> GetCaption(string msgId, string language)
        {
            var caption = string.Empty;
            var cache = src.Handler.HandlingService._cache;
            switch (language)
            {
                case "en":
                    caption = cache.Captions.Where(m => m.msgid == msgId).First().en;
                    break;
                case "de":
                    caption = cache.Captions.Where(m => m.msgid == msgId).First().de;
                    break;
                default:
                    caption = cache.Captions.Where(m => m.msgid == msgId).First().en;
                    break;
            }
            await Task.CompletedTask;
            return caption;
        }

        public static async Task<string> GetCommandDescription(string command, string language)
        {
            var description = string.Empty;
            var cache = src.Handler.HandlingService._cache;
            switch (language)
            {
                case "en":
                    description = cache.Commands.Where(c => c.command == command).First().en;
                    break;
                case "de":
                    description = cache.Commands.Where(c => c.command == command).First().de;
                    break;
                default:
                    description = cache.Commands.Where(c => c.command == command).First().en;
                    break;
            }
            await Task.CompletedTask;
            return description;
        }

        public static async Task SendMessageWithAttachments(SocketMessage message, Bobii.Enums.TextChannel channel,
            SocketThreadChannel thread = null, RestDMChannel restDMChannel = null, ISocketMessageChannel socketMessageChannel = null,
            Embed filterWordEmbed = null, DiscordWebhookClient webhookClient = null, string editedMessage = null)
        {
            try
            {
                var attachments = new List<FileAttachment>();
                var exepath = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var file in message.Attachments)
                {
                    var attachmentUrl = ((IAttachment)file).Url;
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(attachmentUrl), @$"{exepath}\{file.Filename}");
                    }
                    attachments.Add(new FileAttachment(@$"{exepath}\{file.Filename}"));
                }

                switch (channel)
                {
                    case Enums.TextChannel.DiscordWebhookClient:
                        await webhookClient.SendMessageAsync(editedMessage, username: message.Author.Username, avatarUrl: message.Author.GetAvatarUrl());
                        await webhookClient.SendFilesAsync(attachments, "", username: message.Author.Username, avatarUrl: message.Author.GetAvatarUrl());
                        break;
                    case Bobii.Enums.TextChannel.ISocketMessageChannel:
                        await socketMessageChannel.SendMessageAsync(embed: filterWordEmbed);
                        await socketMessageChannel.SendFilesAsync(attachments, "");
                        break;
                    case Bobii.Enums.TextChannel.Thread:
                        await thread.SendMessageAsync(embed: DMSupport.Helper.CreateDMEmbed(message).Result);
                        await thread.SendFilesAsync(attachments, "");
                        break;
                    case Bobii.Enums.TextChannel.RestDMChannel:
                        await restDMChannel.SendMessageAsync(embed: DMSupport.Helper.CreateDMEmbed(message).Result);
                        await restDMChannel.SendFilesAsync(attachments, "");
                        break;
                }

                foreach (var file in attachments)
                {
                    file.Dispose();
                    File.Delete($@"{exepath}\{file.FileName}");
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("MsgRecievd", true, "SendMessageWithAttachments", message: "The dm could not be delivered!", exceptionMessage: ex.Message);
                await DMSupport.Helper.AddDeliveredFailReaction(message);
            }
        }

        public async Task WriteToConsol(string chategorie, bool error, string task, Entities.SlashCommandParameter parameter = null, ulong createChannelID = 0, ulong tempChannelID = 0,
            string filterWord = "", string message = "", string exceptionMessage = "", string hilfeSection = "", string filterLinkState = "", ulong logID = 0, string link = "", string emojiString = "",
            string iD = "", string messageID = "", string parameterName = "")
        {
            var sb = new StringBuilder();
            sb.Append($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} **{chategorie}**");
            if (error)
            {
                sb.Append($"    Error: ");
            }
            else
            {
                sb.Append($"    Information: ");
            }

            if (parameter != null && parameter.Guild != null)
            {
                sb.Append($"**{parameter.Guild.Name}** | GuildID: {parameter.Guild.Id}");
            }


            if (task != "")
            {
                sb.Append($" | **Task: {task}**");
            }

            if (parameter != null && parameter.GuildUser != null)
            {
                sb.Append($" | UserName: {parameter.GuildUser.Username} | UserID: {parameter.GuildUser.Id}");
            }

            if (createChannelID != 0)
            {
                sb.Append($" | CreateChannelID: {createChannelID}");
            }

            if (tempChannelID != 0)
            {
                sb.Append($" | TempChannelID: {tempChannelID}");
            }

            if (filterWord != "")
            {
                sb.Append($" | FilterWord: {filterWord}");
            }

            if (hilfeSection != "")
            {
                sb.Append($" | HilfeSection: {hilfeSection}");
            }

            if (filterLinkState != "")
            {
                sb.Append($" | FilterLinkState: {filterLinkState}");
            }

            if (iD != "")
            {
                sb.Append($" | CheckedID: {iD}");
            }

            if (parameterName != "")
            {
                sb.Append($" | Parameter: {parameterName} ");
            }

            if (messageID != "")
            {
                sb.Append($" | MessageID: {messageID} ");
            }

            if (logID != 0)
            {
                sb.Append($" | LogID: {logID}");
            }

            if (link != "")
            {
                sb.Append($" | Link: {link}");
            }

            if (emojiString != "")
            {
                sb.Append($" | EmojiString: {emojiString} ");
            }

            if (message != "")
            {
                sb.Append($" | **{message}**");
            }

            if (exceptionMessage != "")
            {
                sb.Append($" | **{exceptionMessage}**");
            }
            Console.WriteLine(sb.ToString().Replace("**", ""));
            await Task.CompletedTask;

            _ = WriteConsoleEventHandler(this, new EventArg.WriteConsoleEventArg() { Message = sb.ToString(), Error = error });
        }

        public static async Task<string> CreateInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, string language, string header, string startOfCommand, string startOfSecondCommand = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine(header);
            if (startOfSecondCommand != "")
            {
                if (startOfSecondCommand != "")
                {
                    commandList = commandList.Where(cmd => cmd.Name.StartsWith(startOfCommand) || cmd.Name.StartsWith(startOfSecondCommand)).ToList();
                }
                else
                {
                    commandList = commandList.Where(cmd => cmd.Name.StartsWith(startOfCommand)).ToList();
                }

                foreach (Discord.Rest.RestGlobalCommand command in commandList)
                {
                    if (command.Name.StartsWith(startOfCommand) || command.Name.StartsWith(startOfSecondCommand))
                    {
                        sb.AppendLine("");
                        sb.AppendLine("**/" + command.Name + "**");
                        sb.AppendLine(GetCommandDescription(command.Name, language).Result);
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
            }
            else
            {
                if (startOfCommand != "")
                {
                    foreach (RestGlobalCommand command in commandList)
                    {
                        if (command.Name.StartsWith(startOfCommand))
                        {
                            sb.AppendLine("");
                            sb.AppendLine("**/" + command.Name + "**");
                            sb.AppendLine(GetCommandDescription(command.Name, language).Result);
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
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }

        public static async Task<string> CreateServerCount(DiscordSocketClient client)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Servercount: {client.Guilds.Count}");
            sb.AppendLine();

            foreach (var guild in client.Guilds.OrderByDescending(g => g.MemberCount))
            {
                sb.AppendLine($"Name: {guild.Name} \nMembercount: {guild.MemberCount}\nGuildID: {guild.Id}\n");
            }

            await Task.CompletedTask;
            return sb.ToString();
        }

        /// <summary>
        /// Refreshes the servercount channels
        /// </summary>
        /// <returns></returns>
        public static async Task RefreshBobiiStats()
        {
            _ = Task.Run(async () => Handler.HandlingService.RefreshServerCountChannels());
        }

        public static async Task<string> HelpSupportPart(ulong guildId)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            return CreateInfoPart(null, language, GetContent("C087", language).Result, "").Result;
        }

        public static async Task<Embed> CreateEmbed(SocketGuild guild, string body, string header = null, bool error = false)
        {
            Color embedColor;
            if (error)
            {
                embedColor = Color.Red;
            }
            else
            {
                embedColor = new Color(74, 171, 189);
            }

            EmbedBuilder embed = new EmbedBuilder()
            .WithTitle(header)
            .WithColor(embedColor)
            .WithDescription(body)
            .WithFooter(guild.Name + DateTime.Now.ToString(" • dd/MM/yyyy") + " - Console");

            await Task.CompletedTask;
            return embed.Build();
        }

        public static async Task<Embed> CreateEmbed(SocketInteraction interaction, string body, string header = null, bool useLinebreak = true)
        {
            var sbHeader = new StringBuilder();
            var sbBody = new StringBuilder();
            if (useLinebreak)
            {
                var headerParts = header.Split(@"<br>");
                if (headerParts.Count() == 1)
                {
                    sbHeader.Append(header);
                }
                else
                {
                    foreach (var part in headerParts)
                    {
                        sbHeader.AppendLine(part.Replace(@"<br>", ""));
                    }
                }

                if (body == null)
                {
                    sbBody.Append("");
                }
                else
                {
                    var bodyParts = body.Split(@"<br>");
                    if (bodyParts.Count() == 1)
                    {
                        sbBody.Append(body);
                    }
                    else
                    {
                        foreach (var part in bodyParts)
                        {
                            sbBody.AppendLine(part.Replace("<br>", ""));
                        }
                    }
                }
            }
            else
            {
                sbBody.Append(body);
                sbHeader.Append(header);
            }


            var parsedGuild = GetGuildWithInteraction(interaction);

            EmbedBuilder embed = new EmbedBuilder()
            .WithTitle(sbHeader.ToString())
            .WithColor(74, 171, 189)
            .WithDescription(sbBody.ToString())
            .WithFooter(parsedGuild.Result.Name + DateTime.Now.ToString(" • dd/MM/yyyy"));

            await Task.CompletedTask;
            return embed.Build();
        }

        public static async Task<Embed> CreateTUEmbed(SocketGuild guild, string body, string header, string footer)
        {
            footer = footer + DateTime.Now.ToString(" • dd/MM/yyyy");

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(header)
                .WithColor(74, 171, 189)
                .WithDescription(body)
                .WithFooter(footer);
            await Task.CompletedTask;
            return embed.Build();
        }

        public static async Task<Embed> CreateEmbed(SocketGuild guild, string body, string header = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(header)
                .WithColor(74, 171, 189)
                .WithDescription(body)
                .WithFooter(guild.Name + DateTime.Now.ToString(" • dd/MM/yyyy"));
            await Task.CompletedTask;
            return embed.Build();
        }

        public static async Task<SocketGuild> GetGuildWithInteraction(SocketInteraction interaction)
        {
            if (interaction.Type == InteractionType.MessageComponent)
            {
                var parsedArg = (SocketMessageComponent)interaction;
                var parsedGuildUser = (SocketGuildUser)parsedArg.User;
                return (SocketGuild)parsedGuildUser.Guild;
            }
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                var parsedArg = (SocketSlashCommand)interaction;
                var parsedGuildUser = (SocketGuildUser)parsedArg.User;
                return (SocketGuild)parsedGuildUser.Guild;
            }
            //Should never happen!
            await Task.CompletedTask;
            return null;
        }
        #endregion
    }
}
