using Bobii.src.Bobii;
using Bobii.src.Helper;
using Bobii.src.TempChannel;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class MessageReceivedHandler
    {
        public static async Task<RestThreadChannel> GetCurrentThread(ulong channelId, DiscordShardedClient client, SocketForumChannel dmChannel)
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

        public static async Task HandleMassage(IMessage message, DiscordShardedClient client, SocketForumChannel dmChannel, RestWebhook webhook, AutoDeleteDateWrapper autoDeleteDateWrapper)
        {
            try
            {
                if (HandlingService.Cache.TempChannels.Select(t => t.channelid).Contains(message.Channel.Id))
                {
                    var tempChannel = HandlingService.Cache.TempChannels.First(t => t.channelid == message.Channel.Id);
                    
                    if (message.Author.Id == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationID).ToUlong() ||
                        HandlingService.Cache.TempCommands.FirstOrDefault(c => c.createchannelid == tempChannel.createchannelid && c.commandname == GlobalStrings.chat) != null)
                    {
                        return;
                    }

                    var createTempChannel = HandlingService.Cache.CreateTempChannels.First(c => c.createchannelid == tempChannel.createchannelid.Value);
                    var userConfig = HandlingService.Cache.TempChannelUserConfigs.FirstOrDefault(c => c.userid == tempChannel.channelownerid.Value);

                    if (userConfig != null && userConfig.autodelete.HasValue &&  userConfig.autodelete > 0)
                    {
                        autoDeleteDateWrapper.AddMessageToBeDeleted(message, userConfig.autodelete.Value);
                        return;
                    }

                    if (createTempChannel.autodelete.HasValue && createTempChannel.autodelete > 0)
                    {
                        autoDeleteDateWrapper.AddMessageToBeDeleted(message, createTempChannel.autodelete.Value);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            if (message.Author.IsBot)
            {
                return;
            }

            if (await DMSupportHelper.IsPrivateMessage((SocketMessage)message))
            {
                await DMSupportHelper.HandleDMs(message, dmChannel, client, webhook);
                return;
            }

            var guild = ((IGuildChannel)message.Channel).Guild.Id;

            if (guild == GeneralHelper.GetConfigKeyValue(ConfigKeys.SupportGuildID).ToUlong())
            {
                var thread = GetCurrentThread(message.Channel.Id, client, dmChannel).Result;
                if (thread != null)
                {
                    await DMSupportHelper.HandleSendDMs(message, thread.Name, client);
                }
            }
        }
    }
}
