using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Bobii.src.Bobii;
using Bobii.src.Enums;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
                await GeneralHelper.SendMessageWithAttachments(message, TextChannel.Thread, thread: thread);
                await AddDeliveredReaction(message);
            }
            else
            {
                await thread.SendMessageAsync(message.Content);
                await AddDeliveredReaction(message);
            }
        }

        private static async Task<RestThreadChannel> CreateForumPost(IMessage message, SocketForumChannel dmChannel, DiscordSocketClient discordClient)
        {
            using var client = new WebClient();
            var sb = new StringBuilder();

            var file = $@"{Directory.GetCurrentDirectory()}\Avatar_{message.Author.Id}.png";
            var avatarUrl = message.Author.GetAvatarUrl(ImageFormat.Png, 2048);
            client.DownloadFile(avatarUrl, file);

            var ownedGuilds = discordClient.Guilds.Where(g => g.OwnerId == message.Author.Id);

            sb.AppendLine($"**{message.Author.GlobalName}**");
            sb.AppendLine(message.Author.Username);
            sb.AppendLine($"Created at: { message.Author.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy")}");

            sb.AppendLine();

            if (ownedGuilds.Any())
            {
                sb.AppendLine("**Owned servers:**");
                foreach (SocketGuild guild in ownedGuilds)
                {
                    sb.AppendLine($"{guild.Name}: {guild.MemberCount}");
                }
            }

            await Task.CompletedTask;
            var channel = dmChannel.CreatePostWithFileAsync(
                message.Author.Id.ToString(),
                file,
                ThreadArchiveDuration.OneWeek,
                text: sb.ToString()).Result;

            File.Delete(file);
            return channel;
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
                    var privateChannel = Discord.UserExtensions.SendMessageAsync(user, text: message.Content);
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
                var thread = CheckIfThreadExists(message, dmChannel).Result;
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
