using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Bobii.src.Bobii;
using Bobii.src.Enums;

namespace Bobii.src.Helper
{
    class DMSupportHelper
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
            if (message.Attachments.Count > 0)
            {
                await GeneralHelper.SendMessageWithAttachments(message, TextChannel.DiscordWebhookClient, thread: thread);
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
                    await GeneralHelper.SendMessageWithAttachments(message, TextChannel.RestDMChannel, restDMChannel: channel);
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
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.MsgRecievd, true, nameof(HandleSendDMs), message: "The dm could not be delivered!", exceptionMessage: ex.Message);
                await AddDeliveredFailReaction(message);
            }

        }

        public static async Task AddDeliveredReaction(IMessage message)
        {
            await message.AddReactionAsync(Emote.Parse(GeneralHelper.GetConfigKeyValue(ConfigKeys.DeliveredEmojiString)));
        }

        public static async Task AddDeliveredFailReaction(IMessage message)
        {
            await message.AddReactionAsync(Emote.Parse(GeneralHelper.GetConfigKeyValue(ConfigKeys.DeliveredFailedEmojiString)));
        }

        public static async Task HandleDMs(IMessage message, SocketForumChannel dmChannel, DiscordSocketClient client, RestWebhook webhook)
        {
            try
            {
                var thread  = CheckIfThreadExists(message, dmChannel).Result;
                if (thread == null)
                {
                    thread = CreateForumPost(message, dmChannel, client).Result;
                    var mainGuidId = GeneralHelper.GetConfigKeyValue(ConfigKeys.MainGuildID).ToUlong();
                    var myGuild = client.GetGuild(mainGuidId);
                    var myGuildRest = client.Rest.GetGuildAsync(mainGuidId).Result;
                    await thread.AddUserAsync((IGuildUser)myGuildRest.GetUserAsync(GeneralHelper.GetConfigKeyValue(ConfigKeys.DeveloperUserID).ToUlong()).Result);
                }
                await SendMessageToThread(thread, message, webhook);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.MsgRecievd, true, nameof(HandleDMs), message: "The dm could not be delivered!", exceptionMessage: ex.Message);
                await AddDeliveredFailReaction(message);
            }
        }
        #endregion
    }
}
