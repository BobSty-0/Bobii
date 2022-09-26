using Bobii.src.Bobii;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class MessageReceivedHandler
    {
        public static async Task<RestThreadChannel> GetCurrentThread(ulong channelId, DiscordSocketClient client, SocketForumChannel dmChannel)
        {
            foreach (RestThreadChannel thread in dmChannel.GetAllThreads().Result)
            {
                if (ulong.TryParse(thread.Name, out _) && channelId == thread.Id)
                {
                    return thread;
                }
            }
            return null;
        }

        public static async Task HandleMassage(IMessage message, DiscordSocketClient client, SocketForumChannel dmChannel, RestWebhook webhook)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            if (await DMSupport.Helper.IsPrivateMessage((SocketMessage)message))
            {
                await DMSupport.Helper.HandleDMs(message, dmChannel, client, webhook);
                return;
            }

            var guild = ((IGuildChannel)message.Channel).Guild.Id;

            if (guild == Helper.ReadBobiiConfig(ConfigKeys.SupportGuildID).ToUlong())
            {
                var thread = GetCurrentThread(message.Channel.Id, client, dmChannel).Result;
                if(thread != null)
                {
                    await DMSupport.Helper.HandleSendDMs(message, thread.Name, client);
                }
            }
        }
    }
}
