using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DMSupport
{
    class Helper
    {
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

        private static void SendMessageToThread(SocketThreadChannel thread, SocketMessage message)
        {
            thread.SendMessageAsync(message.Content);
        }

        public static async Task HandleSendDMs(SocketMessage message, string userID, DiscordSocketClient client)
        {
            try
            {
                var user = client.GetUserAsync(ulong.Parse(userID)).Result;
                var privateChannel = Discord.UserExtensions.SendMessageAsync(user, message.Content);
                await message.AddReactionAsync(new Emoji("🔥"));
            }
            catch (Exception ex)
            {
                MessageFilter.MessageFliter.WriteToConsol($"Error | The dm could not be delivered! {ex.Message}");
                await message.AddReactionAsync(new Emoji("🥺"));
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
                SendMessageToThread(thread, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }


            //if (message.Reference != null)
            //{

            //    try
            //    {


            //        var originalMessage = message.Channel.GetMessageAsync(message.Reference.MessageId.Value);
            //        var userId = originalMessage.Result.Content.Split(" ")[0];
            //        var user = client.GetUserAsync(ulong.Parse(userId)).Result;
            //        var privateChannel = Discord.UserExtensions.SendMessageAsync(user, message.Content);
            //        await message.AddReactionAsync(new Emoji("🔥"));
            //    }
            //    catch (Exception ex)
            //    {
            //        WriteToConsol($"Error | The dm could not be delivered! {ex.Message}");
            //        await message.AddReactionAsync(new Emoji("🥺"));
            //    }
            //}
        }
    }
}
