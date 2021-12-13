using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DMSupport
{
    class Helper
    {
        #region Functions
        private static SocketThreadChannel CheckIfThreadExists(SocketMessage message, SocketTextChannel dmChannel)
        {
            foreach (SocketThreadChannel thread in dmChannel.Threads)
            {
                if (thread.Name == message.Author.Id.ToString())
                {
                    return thread;
                }
            }
            return null;
        }

        private static SocketThreadChannel CreateThread(SocketMessage message, SocketTextChannel dmChannel)
        {
            return dmChannel.CreateThreadAsync(message.Author.Id.ToString()).Result;
        }

        private static Embed CreateDMEmbed(SocketMessage message)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor(message.Author)
                .WithColor(74, 171, 189)
                .WithDescription(message.Content);
            return embed.Build();
        }
        #endregion

        #region Methods
        private static async Task SendMessageToThread(SocketThreadChannel thread, SocketMessage message)
        {
            if (message.Attachments.Count > 0)
            {
                await SendMessageWithAttachments(message, thread);
                await message.AddReactionAsync(Emote.Parse("<:delivered:917731122299940904>"));
            }
            else
            {
                await thread.SendMessageAsync(embed: CreateDMEmbed(message));
                await message.AddReactionAsync(Emote.Parse("<:delivered:917731122299940904>"));
            }
        }
        #endregion

        #region Tasks
        public static async Task SendMessageWithAttachments(SocketMessage message, SocketThreadChannel thread = null, IUser user = null)
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

            if (thread != null)
            {
                var msg = thread.SendFilesAsync(attachments, "", embed: CreateDMEmbed(message)).Result;
            }

            if (user != null)
            {
                var filePath = @$"{exepath}{attachments.First().FileName}";
                var msg = Discord.UserExtensions.SendFileAsync(user, filePath, embed: CreateDMEmbed(message));
            }


            foreach (var file in attachments)
            {
                file.Dispose();
                File.Delete($@"{exepath}\{file.FileName}");
            }
            await Task.CompletedTask;
        }

        public static async Task<bool> IsPrivateMessage(SocketMessage msg)
        {
            await Task.CompletedTask;
            return (msg.Channel.GetType() == typeof(SocketDMChannel));
        }

        public static async Task HandleSendDMs(SocketMessage message, string userID, DiscordSocketClient client)
        {
            try
            {
                if (message.Attachments.Count > 0)
                {
                    var user = client.GetUserAsync(ulong.Parse(userID)).Result;
                    await SendMessageWithAttachments(message, user: user);
                    await message.AddReactionAsync(Emote.Parse("<:delivered:917731122299940904>"));
                }
                else
                {
                    var user = client.GetUserAsync(ulong.Parse(userID)).Result;
                    var privateChannel = Discord.UserExtensions.SendMessageAsync(user, embed: CreateDMEmbed(message));
                    await message.AddReactionAsync(Emote.Parse("<:delivered:917731122299940904>"));
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("MsgRecievd", true, "HandleSendDMs", message: "The dm could not be delivered!", exceptionMessage: ex.Message);
                await message.AddReactionAsync(Emote.Parse("<:deliverfail:917731174162526208>"));
            }

        }

        public static async Task HandleDMs(SocketMessage message, SocketTextChannel dmChannel, DiscordSocketClient client)
        {
            try
            {
                var thread = CheckIfThreadExists(message, dmChannel);
                if (thread == null)
                {
                    thread = CreateThread(message, dmChannel);
                    var myGuild = client.GetGuild(712373862179930144);
                    var myGuildRest = client.Rest.GetGuildAsync(712373862179930144).Result;
                    await thread.AddUserAsync((IGuildUser)myGuildRest.GetUserAsync(410312323409117185).Result);
                }
                await SendMessageToThread(thread, message);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("MsgRecievd", true, "HandleDMs", message: "The dm could not be delivered!", exceptionMessage: ex.Message);
                await message.AddReactionAsync(Emote.Parse("<:deliverfail:917731174162526208>"));
            }
        }
        #endregion
    }
}
