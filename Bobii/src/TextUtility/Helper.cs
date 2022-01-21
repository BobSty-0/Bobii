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

        public static async Task<string> HelpTextUtilityInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "You can create embeds with <@776028262740393985> for clean looking announcement " +
                "for example!\nNote: you can use **<br>** for linebreaks", "tu").Result;
        }
    }
}
