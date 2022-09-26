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
using Bobii.src.Bobii;
using Discord.Webhook;

namespace Bobii.src.DMSupport
{
    class Helper
    {
        #region Tasks
        private static async Task<RestThreadChannel> CheckIfThreadExists(IMessage message, SocketForumChannel dmChannel)
        {
            foreach (RestThreadChannel thread in dmChannel.GetAllThreads().Result)
            {
                if (thread.Name == message.Author.Id.ToString())
                {
                    return thread;
                }
            }
            await Task.CompletedTask;
            return null;
        }

        private static async Task SendMessageToThread(RestThreadChannel thread, IMessage message, RestWebhook webhook)
        {
            //Bobii.Helper.SendMessageWithWebhook(message, thread, webhook);
            //await AddDeliveredReaction(message);
            if (message.Attachments.Count > 0)
            {
                await Bobii.Helper.SendMessageWithAttachments(message, Bobii.Enums.TextChannel.DiscordWebhookClient, thread: thread);
                await AddDeliveredReaction(message);

            }
            else
            {
                await thread.SendMessageAsync(embed: CreateDMEmbed(message).Result);
                await AddDeliveredReaction(message);
            }
        }

        private static async Task<RestThreadChannel> CreateForumPost(IMessage message, SocketForumChannel dmChannel, DiscordSocketClient discordClient)
        {
            using var client = new WebClient();
            var file = $@"{Directory.GetCurrentDirectory()}\Avatar_{message.Author.Id}.png";
            client.DownloadFile(message.Author.GetAvatarUrl(ImageFormat.Png), file);

            await Task.CompletedTask;
            return dmChannel.CreatePostWithFileAsync(
                message.Author.Id.ToString(),
                file,
                ThreadArchiveDuration.OneWeek,
                text: $"**{message.Author}**").Result;

            File.Delete(file);
        }

        public static async Task<Embed> CreateDMEmbed(IMessage message)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor(message.Author)
                .WithColor(74, 171, 189)
                .WithDescription(message.Content);
            await Task.CompletedTask;
            return embed.Build();
        }

        public static async Task<bool> IsPrivateMessage(SocketMessage msg)
        {
            await Task.CompletedTask;
            return (msg.Channel.GetType() == typeof(SocketDMChannel));
        }

        public static async Task HandleSendDMs(IMessage message, string userID, DiscordSocketClient client)
        {
            try
            {
                if (message.Attachments.Count > 0)
                {
                    var user = client.GetUserAsync(ulong.Parse(userID)).Result;
                    var channel = (RestDMChannel)user.CreateDMChannelAsync().Result;
                    await Bobii.Helper.SendMessageWithAttachments(message, Bobii.Enums.TextChannel.RestDMChannel, restDMChannel: channel);
                    await AddDeliveredReaction(message);
                }
                else
                {
                    var user = client.GetUserAsync(ulong.Parse(userID)).Result;
                    var privateChannel = Discord.UserExtensions.SendMessageAsync(user, embed: CreateDMEmbed(message).Result);
                    await AddDeliveredReaction(message);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("MsgRecievd", true, "HandleSendDMs", message: "The dm could not be delivered!", exceptionMessage: ex.Message);
                await AddDeliveredFailReaction(message);
            }

        }

        public static async Task AddDeliveredReaction(IMessage message)
        {
            await message.AddReactionAsync(Emote.Parse("<:delivered:917731122299940904>"));
        }

        public static async Task AddDeliveredFailReaction(IMessage message)
        {
            await message.AddReactionAsync(Emote.Parse("<:deliverfail:917731174162526208>"));
        }

        public static async Task HandleDMs(IMessage message, SocketForumChannel dmChannel, DiscordSocketClient client, RestWebhook webhook)
        {
            try
            {
                var thread  = CheckIfThreadExists(message, dmChannel).Result;
                if (thread == null)
                {
                    thread = CreateForumPost(message, dmChannel, client).Result;
                    var myGuild = client.GetGuild(712373862179930144);
                    var myGuildRest = client.Rest.GetGuildAsync(712373862179930144).Result;
                    await thread.AddUserAsync((IGuildUser)myGuildRest.GetUserAsync(410312323409117185).Result);
                }
                await SendMessageToThread(thread, message, webhook);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("MsgRecievd", true, "HandleDMs", message: "The dm could not be delivered!", exceptionMessage: ex.Message);
                await AddDeliveredFailReaction(message);
            }
        }
        #endregion
    }
}
