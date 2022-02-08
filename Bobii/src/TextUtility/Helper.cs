using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TextUtility
{
    class Helper
    {
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
            return Bobii.Helper.CreateInfoPart(commandList, language, Bobii.Helper.GetContent("C163", language).Result, "tu").Result;
        }
    }
}
