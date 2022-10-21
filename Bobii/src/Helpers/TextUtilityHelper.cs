using Bobii.src.Helper;
using Bobii.src.Models;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Helper
{
    class TextUtilityHelper
    {
        public static async Task<UserMessages>GetUserMessages(SlashCommandParameter parameter, ulong messageID)
        {
            RestUserMessage restUserMessage = null;
            SocketUserMessage socketUserMessage = null;
            var channel = (SocketTextChannel)parameter.Client.GetChannel(parameter.Interaction.Channel.Id);
            try
            {
                restUserMessage = (RestUserMessage)channel.GetMessageAsync(messageID).Result;
            }
            catch (Exception)
            {
                socketUserMessage = (SocketUserMessage)channel.GetMessageAsync(messageID).Result;
            }
            await Task.CompletedTask;
            return new UserMessages() { SocketUserMessage = socketUserMessage, RestUserMessage = restUserMessage};
        }

        public static async Task<string> GetContent(UserMessages userMessages)
        {
            var content = String.Empty;

            if (userMessages.RestUserMessage != null)
            {
                content = userMessages.RestUserMessage.Embeds.First().Description;
            }
            else
            {
                content = userMessages.SocketUserMessage.Embeds.First().Description;
            }
            await Task.CompletedTask;
            return content;
        }

        public static async Task<string> GetTitle(UserMessages userMessages)
        {
            var title = String.Empty;

            if (userMessages.RestUserMessage != null)
            {
                title = userMessages.RestUserMessage.Embeds.First().Title;
            }
            else
            {
                title = userMessages.SocketUserMessage.Embeds.First().Title;
            }
            await Task.CompletedTask;
            return title;
        }

        public static async Task<List<SocketMessage>> GetBobiiEmbedMessages(ISocketMessageChannel channel)
        {
            var messageList = new List<SocketMessage>();
            foreach(var message in channel.GetCachedMessages())
            {
                if (!message.Author.IsBot || !(message.Author.Id == 776028262740393985 || message.Author.Id == 869180143363584060))
                {
                    continue;
                }

                if (message.Interaction != null)
                {
                    continue;
                }

                messageList.Add(message);
            }

            return messageList;
        }

        public static async Task<string> HelpTextUtilityInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            return GeneralHelper.CreateInfoPart(commandList, language, GeneralHelper.GetContent("C163", language).Result, "textutility").Result;
        }
    }
}
