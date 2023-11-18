using Bobii.src.Bobii;
using Bobii.src.EntityFramework.Entities;
using Bobii.src.Enums;
using Bobii.src.EventArg;
using Bobii.src.Models;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Helper
{
    public delegate Task WriteToConsoleEventHandler(object source, WriteConsoleEventArg e);
    public class GeneralHelper
    {
        #region Declarations
        public event WriteToConsoleEventHandler WriteConsoleEventHandler;
        #endregion

        #region Tasks
        public static Embed GetWelcomeEmbed(SocketGuild guild)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Thank You For Inviting Bobii");
            sb.AppendLine();
            sb.AppendLine($"<@{GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationID).ToUlong()}> has two main sections:");
            sb.AppendLine();
            sb.AppendLine("### **Temp Channel**");
            sb.AppendLine("You can create temorary voice channels which are automatically created and deleted. You can setup your own temp channel by clicking the button below.");
            sb.AppendLine("Click [here](https://www.youtube.com/watch?v=HJVJ2R7gfyo) to watch a tutorial about Temp Channels.");
            sb.AppendLine();
            sb.AppendLine("### **Text Utility**");
            sb.AppendLine("You can steal emojis or create clean looking announcements.");
            sb.AppendLine("Click [here](https://youtu.be/-N4Ko6PbEX8) to watch a tutorial about Text Utility.");
            sb.AppendLine();
            sb.AppendLine("You can join the support server by clicking the button below if you have any questions.");

            return GeneralHelper.CreateEmbed(guild, sb.ToString(), "").Result;
        }

        public static ComponentBuilder GetSupportButtonComponentBuilder()
        {
            var componentBuilder = new ComponentBuilder();
            componentBuilder.WithButton("Setup Temp Channel", "setup-temp-channel", style: ButtonStyle.Primary);
            componentBuilder.WithButton("Join Support Server", style: ButtonStyle.Link, url: "https://discord.gg/xpEKTUh5j2");
            return componentBuilder;
        }

        public static bool CanWriteInChannel(SocketTextChannel channel, SocketGuildUser bot)
        {
            return bot.GetPermissions(channel).SendMessages;
        }

        public static string GetConfigKeyValue(string key)
        {
            try
            {
                JObject config = Program.GetConfig();
                var value = config["BobiiConfig"][0][key].Value<string>();
                return value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------------------- No {key} key in the config.json file detected! --------------------------");
                return "";
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

        public static async Task<string> GetContent(string msgId, string language)
        {
            var content = string.Empty;
            var cache = src.Handler.HandlingService.Cache;
            switch (language)
            {
                case "en":
                    content = cache.Contents.First(m => m.msgid == msgId).en;
                    break;
                case "de":
                    content = cache.Contents.First(m => m.msgid == msgId).de;
                    break;
                case "ru":
                    content = cache.Contents.FirstOrDefault(m => m.msgid == msgId).ru;
                    if (content == null || content == "")
                    {
                        content = cache.Contents.First(m => m.msgid == msgId).en;
                    }
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
            var cache = src.Handler.HandlingService.Cache;
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
            try
            {
                var description = string.Empty;
                var cache = src.Handler.HandlingService.Cache;
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
            catch (Exception ex)
            {
                Console.Write(ex.Message + $"-- COMMAND {command} --");
                return "";
            }
        }

        private static List<FileAttachment> GetAttachmentsFromMessage(IMessage message, string exepath)
        {
            var attachments = new List<FileAttachment>();

            foreach (var file in message.Attachments)
            {
                var attachmentUrl = ((IAttachment)file).Url;
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(attachmentUrl), @$"{exepath}\{file.Filename}");
                }
                attachments.Add(new FileAttachment(@$"{exepath}\{file.Filename}"));
            }

            return attachments;
        }

        public static void SendMessageWithWebhook(IMessage message, RestThreadChannel thread, RestWebhook webhook)
        {
            // TODO
            var exepath = AppDomain.CurrentDomain.BaseDirectory;
            var attachments = GetAttachmentsFromMessage(message, exepath);
            var discordWebhookClient = new DiscordWebhookClient(webhook);

            discordWebhookClient.SendMessageAsync(message.Content, username: message.Author.Username, avatarUrl: message.Author.GetAvatarUrl(), threadId: thread.Id);

            if (attachments.Count > 0)
            {
                discordWebhookClient.SendFilesAsync(attachments, "", username: message.Author.Username, avatarUrl: message.Author.GetAvatarUrl(), threadId: thread.Id);
            }

            DeleteAllAttachments(exepath, attachments);
        }

        public static async Task<string> SpracheInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;

            return GeneralHelper.GetContent("C196", language).Result + GeneralHelper.GetContent("C197", language).Result;
        }

        public static void DeleteAllAttachments(string exepath, List<FileAttachment> attachments)
        {
            foreach (var file in attachments)
            {
                file.Dispose();
                File.Delete($@"{exepath}\{file.FileName}");
            }
        }

        public static async Task SendMessageWithAttachments(IMessage message, TextChannel channel,
            RestThreadChannel thread = null, RestDMChannel restDMChannel = null, ISocketMessageChannel socketMessageChannel = null,
            Embed filterWordEmbed = null, DiscordWebhookClient webhookClient = null, string editedMessage = null)
        {
            var exepath = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                var attachments = GetAttachmentsFromMessage(message, exepath);

                switch (channel)
                {
                    case TextChannel.DiscordWebhookClient:
                        if (editedMessage != null)
                        {
                            await webhookClient.SendMessageAsync(editedMessage, username: message.Author.Username, avatarUrl: message.Author.GetAvatarUrl());
                        }
                        await webhookClient.SendFilesAsync(attachments, "", username: message.Author.Username, avatarUrl: message.Author.GetAvatarUrl());
                        break;
                    case TextChannel.ISocketMessageChannel:
                        await socketMessageChannel.SendMessageAsync(embed: filterWordEmbed);
                        await socketMessageChannel.SendFilesAsync(attachments, "");
                        break;
                    case TextChannel.Thread:
                        await thread.SendFilesAsync(attachments, message.Content);
                        break;
                    case TextChannel.RestDMChannel:
                        await restDMChannel.SendFilesAsync(attachments, message.Content);
                        break;
                }

                DeleteAllAttachments(exepath, attachments);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("MsgRecievd", true, "SendMessageWithAttachments", message: "The dm could not be delivered!", exceptionMessage: ex.Message);
                await DMSupportHelper.AddDeliveredFailReaction(message);
            }
        }

        public async Task WriteToConsol(WriteToConsoleParameter parameter)
        {
            var sb = new StringBuilder();
            sb.Append($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss}");

            foreach (var property in parameter.GetType().GetProperties())
            {
                var value = property.GetValue(parameter);
                var type = property.PropertyType.Name;

                if (value == null)
                {
                    continue;
                }

                switch (type)
                {
                    case "String":
                        if (value.ToString() == "")
                        {
                            continue;
                        }
                        break;

                    case "UInt64":
                        if (value.ToString() == "0")
                        {
                            continue;
                        }
                        break;

                    case "Boolean":
                        continue;
                }
                sb.Append($"| **{property.Name}** : {value} ");
            }

            Console.WriteLine(sb.ToString().Replace("**", ""));
            await Task.CompletedTask;

            Task.Run(()=> WriteConsoleEventHandler(this, new WriteConsoleEventArg() { Message = sb.ToString(), Error = parameter.Error }));
        }

        public async Task WriteToConsol(string chategorie, bool error, string task, SlashCommandParameter parameter = null, ulong createChannelID = 0, ulong tempChannelID = 0,
            string filterWord = "", string message = "", string exceptionMessage = "", string hilfeSection = "", string filterLinkState = "", ulong logID = 0, string link = "", string emojiString = "",
            string iD = "", string messageID = "", string parameterName = "")
        {
            var sb = new StringBuilder();
            sb.Append($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss}  **{chategorie}**");
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

            Task.Run(async () =>
            {
                WriteConsoleEventHandler(this, new WriteConsoleEventArg() { Message = sb.ToString(), Error = error });
            });
        }

        public static async Task<string> CreateInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, string language, string header, string startOfCommand, ulong guildId, bool helpCommand = true, string createChannelId = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine(header);

            foreach (RestGlobalCommand command in commandList)
            {
                if (command.Name == "temptoggle")
                {
                    continue;
                }

                var fistLetterOfMainCommand = command.Name[0];
                if (command.Name.StartsWith(startOfCommand))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(GetCommandDescription(command.Name, language).Result);

                    IReadOnlyCollection<RestApplicationCommandOption> optionList;
                    if (command.Options.FirstOrDefault(c => c.Name == "createcommandlist") == null)
                    {
                        optionList = command.Options;
                    }
                    else
                    {
                        optionList = command.Options.OrderBy(c => c.Name == "createcommandlist").ToList();
                    }

                    foreach (var cmd in optionList)
                    {
                        if (createChannelId != "")
                        {
                            // The command was disabled in that guild
                            if (TempCommandsHelper.DoesCommandExist(guildId, ulong.Parse(createChannelId), cmd.Name).Result && !helpCommand)
                            {
                                continue;
                            }
                        }

                        sb.AppendLine("");
                        if (cmd.Options.Count > 0 && GetCommandDescription($"{cmd.Name} {cmd.Options.First()}", language).Result == "")
                        {
                            sb.AppendLine($"**</{command.Name} {cmd.Name}:{command.Id}>**");
                        }
                        else
                        {
                            sb.AppendLine($"**</{command.Name} {cmd.Name}:{command.Id}>**");
                        }

                        if (command.Name == "temp" && cmd.Name == "info")
                        {
                            sb.AppendLine(GetCommandDescription(command.Name + cmd.Name, language).Result);
                        }
                        else
                        {
                            sb.AppendLine(GetCommandDescription(cmd.Name, language).Result);
                        }


                        foreach (var cmd2 in cmd.Options)
                        {
                            var cmdDesc = GetCommandDescription($"{cmd.Name} {cmd2.Name}", language).Result;
                            if (cmdDesc == "")
                            {
                                continue;
                            }
                            sb.AppendLine("");
                            sb.AppendLine($"**</{command.Name} {cmd.Name} {cmd2.Name}:{command.Id}>**");
                            sb.AppendLine(cmdDesc);
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

            foreach (var guild in client.Guilds)
            {
                sb.AppendLine($"Name: {guild.Name} \nMembercount: {guild.MemberCount}\nGuildID: {guild.Id}\nOnwerId: {guild.OwnerId}");
            }

            await Task.CompletedTask;
            return sb.ToString();
        }

        public static async Task<string> HelpSupportPart(ulong guildId)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            return GetContent("C087", language).Result;
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

        public static async Task<Embed> CreateEmbed(SocketInteraction interaction, string body, string header = "", bool useLinebreak = true)
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

        public static async Task<Embed> CreateTUEmbed(SocketGuild guild, string body, string header, string footer, string imageUrl, string otherUrl)
        {
            footer = footer + DateTime.Now.ToString(" • dd/MM/yyyy");

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(header)
                .WithColor(74, 171, 189)
                .WithDescription(body)
                .WithImageUrl(imageUrl)
                .WithUrl(otherUrl)
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

        public static async Task<Embed> CreateFehlerEmbed(SocketGuild guild, string body, string header = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(header)
                .WithColor(Color.Red)
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
                try
                {
                    var parsedArg = (SocketSlashCommand)interaction;
                    var parsedGuildUser = (SocketGuildUser)parsedArg.User;
                    return (SocketGuild)parsedGuildUser.Guild;
                }
                catch
                {
                    var parsedArg = (SocketUserCommand)interaction;
                    var parsedGuildUser = (SocketGuildUser)parsedArg.User;
                    return (SocketGuild)parsedGuildUser.Guild;
                }

            }
            if (interaction.Type == InteractionType.ModalSubmit)
            {
                var parsedArg = (SocketModal)interaction;
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
