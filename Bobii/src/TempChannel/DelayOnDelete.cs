using Bobii.src.EntityFramework.Entities;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    public class DelayOnDelete
    {
        #region Declarations
        public List<Entities.TempChannelDelay> TempChannelDelayThreads = new List<Entities.TempChannelDelay>();
        #endregion

        #region Public Methods
        public async Task InitializeDelayDelete(DiscordSocketClient client)
        {
            var tempChannels = EntityFramework.TempChannelsHelper.GetTempChannelList().Result;
            foreach (var tempChannel in tempChannels)
            {
                var dateDifference = tempChannel.deletedate - DateTime.Now;
                var user = client.GetUser(410312323409117185);
                var socketGuildChannel = (SocketGuildChannel)client.GetChannel(tempChannel.channelid);
                if (socketGuildChannel == null)
                {
                    continue;
                }
                var guild = socketGuildChannel.Guild;

                if (dateDifference.Value.TotalMinutes <= 0)
                {
                    await Helper.CheckAndDeleteEmptyVoiceChannels(client, guild, tempChannels, user, true);
                }
                else
                {
                    var createTempChannels = EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelList().Result;
                    var createTempChannel = createTempChannels.FirstOrDefault(ch => ch.createchannelid == tempChannel.createchannelid);
                    await StartDelay(tempChannel, createTempChannel, client, guild, user, tempChannels);
                }
            }
        }

        public async Task StartDelay(tempchannels tempChannel, createtempchannels createTempChannel, DiscordSocketClient client, SocketGuild guild, SocketUser user, List<tempchannels> tempChannelList)
        {
            var task = Task.Run(() => DelayAndDelete(tempChannel, createTempChannel, client, guild, user, tempChannelList));
            await Task.CompletedTask;
        }

        public async Task StopDelay(tempchannels tempChannel)
        {
            var tempChannelDelayThread = TempChannelDelayThreads.FirstOrDefault(t => t.TempChannel.id == tempChannel.id);
            var thread = tempChannelDelayThread.Thread;
            thread.Interrupt();
            await EntityFramework.TempChannelsHelper.UpdateDeleteDelay(tempChannel.id, null);
            TempChannelDelayThreads.Remove(tempChannelDelayThread);
            await Task.CompletedTask;
        }
        #endregion

        #region Private Methods
        private async Task DelayAndDelete(tempchannels tempChannel, createtempchannels createTempChannel, DiscordSocketClient client, SocketGuild guild, SocketUser user, List<tempchannels> tempChannelList)
        {
            TempChannelDelayThreads.Add(new Entities.TempChannelDelay() { TempChannel = tempChannel, Thread = Thread.CurrentThread });

            var delayInMinutes = createTempChannel.delay;
            var delayInSeconds = delayInMinutes * 60;
            var delay = delayInSeconds * 1000;
            await EntityFramework.TempChannelsHelper.UpdateDeleteDelay(tempChannel.id, DateTime.Now.AddMinutes(delayInMinutes.Value));
            await Task.Delay(delay.Value);

            var tempChannelDelayThread = TempChannelDelayThreads.FirstOrDefault(t => t.TempChannel.id == tempChannel.id);

            await Helper.CheckAndDeleteEmptyVoiceChannels(client, guild, tempChannelList, user, true);
            TempChannelDelayThreads.Remove(tempChannelDelayThread);
            await Task.CompletedTask;
        }
        #endregion
    }
}
