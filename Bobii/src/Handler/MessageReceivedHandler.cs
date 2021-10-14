using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class MessageReceivedHandler
    {
        private static bool _useFilterWord = false;

        #region Methods
        public static async Task WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} MessageRec  {message}");
            await Task.CompletedTask;
        }
        #endregion

        public static async Task FilterMessageHandler(SocketMessage message, DiscordSocketClient client, ISocketMessageChannel dmChannel)
        {

            if (message.Author.IsBot)
            {
                return;
            }

            if (await DMSupport.Helper.IsPrivateMessage(message))
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    return;
                }
                await DMSupport.Helper.HandleDMs(message, (SocketTextChannel)dmChannel, client);
                return;
            }

            var guild = ((IGuildChannel)message.Channel).Guild.Id;

            if (guild == 712373862179930144)
            {
                foreach (SocketThreadChannel thread in ((SocketTextChannel)dmChannel).Threads)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        continue;
                    }
                    if (ulong.TryParse(thread.Name, out _) && thread.Name.Length == 18 && message.Channel.Id == thread.Id)
                    {
                        await DMSupport.Helper.HandleSendDMs(message, thread.Name, client);
                        return;
                    }
                }
            }

            if (message.Content == "<@!776028262740393985> servercount" && message.Author.Id == 410312323409117185)
            {
                await Bobii.Helper.CreateServerCount(message, client);
            }

            if (message.Content == "<@!776028262740393985> refresh" && message.Author.Id == 410312323409117185)
            {
                await Bobii.Helper.RefreshBobiiStats();
            }

            try
            {
                if (message.Channel is ITextChannel channel)
                {
                    _useFilterWord =  await FilterLink.Helper.FilterForFilterLinks(message, channel, client);
                    if (_useFilterWord)
                    {
                        _useFilterWord = await FilterWord.Helper.FilterForFilterWords(message, channel);
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
