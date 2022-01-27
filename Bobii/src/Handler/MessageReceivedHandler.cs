using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class MessageReceivedHandler
    {
        private static bool _useFilterWord = false;
        public static async Task FilterMessageHandler(SocketMessage message, DiscordSocketClient client, ISocketMessageChannel dmChannel)
        {
            if (message.Content == "<@!776028262740393985> ping")
            {
                var ping = DateTime.Now - message.CreatedAt;
                await message.Channel.SendMessageAsync($"Ping: {ping.TotalMilliseconds}ms");
                return;
            }
            if (message.Author.IsBot)
            {
                return;
            }

            if (await DMSupport.Helper.IsPrivateMessage(message))
            {
                await DMSupport.Helper.HandleDMs(message, (SocketTextChannel)dmChannel, client);
                return;
            }

            var guild = ((IGuildChannel)message.Channel).Guild.Id;

            if (guild == 712373862179930144)
            {
                foreach (SocketThreadChannel thread in ((SocketTextChannel)dmChannel).Threads)
                {
                    if (ulong.TryParse(thread.Name, out _) && thread.Name.Length == 18 && message.Channel.Id == thread.Id)
                    {
                        await DMSupport.Helper.HandleSendDMs(message, thread.Name, client);
                        return;
                    }
                }
            }

            try
            {
                if (message.Channel is ITextChannel channel)
                {
                    _useFilterWord =  await FilterLink.Helper.FilterForFilterLinks(message, channel, client);
                    if (_useFilterWord)
                    {
                        _useFilterWord = await FilterWord.Helper.FilterForFilterWords(message, channel, client);
                    }
                }
            }
            catch (InvalidCastException)
            {
                //No need to do anything ... just to provide the bot from spaming the console
            }
        }
    }
}
