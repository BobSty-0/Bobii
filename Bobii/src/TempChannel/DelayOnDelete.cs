using Bobii.src.EntityFramework.Entities;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
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
        public ConcurrentBag<DateWrapper> dateWrappers = new ConcurrentBag<DateWrapper>();
        public List<Entities.TempChannelDelay> TempChannelDelayTimers = new List<Entities.TempChannelDelay>();
        #endregion

        #region Public Methods
        public async Task InitializeDelayDelete(DiscordSocketClient client)
        {
            var tempChannels = EntityFramework.TempChannelsHelper.GetTempChannelList().Result.Where(c => c.deletedate != null).ToList();
            foreach (var tempChannel in tempChannels)
            {
                var dateDifference = tempChannel.deletedate - DateTime.Now;
                var user = client.GetUser(410312323409117185);
                var socketGuildChannel = (SocketVoiceChannel)client.GetChannel(tempChannel.channelid);
                if (socketGuildChannel == null)
                {
                    continue;
                }
                var guild = socketGuildChannel.Guild;
                var parameter = new Entities.VoiceUpdatedParameter();
                parameter.Guild = guild;
                parameter.SocketUser = user;
                parameter.OldSocketVoiceChannel = socketGuildChannel;
                parameter.Client = client;

                if (dateDifference.Value.TotalMinutes <= 0)
                {
                    await Helper.DeleteTempChannel(parameter, tempChannel);
                }
                else
                {
                    var createTempChannels = EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelList().Result;
                    var createTempChannel = createTempChannels.FirstOrDefault(ch => ch.createchannelid == tempChannel.createchannelid);
                    await StartDelay(tempChannel, createTempChannel, parameter);
                }
            }
        }

        public async Task StartDelay(tempchannels tempChannel, createtempchannels createTempChannel, Entities.VoiceUpdatedParameter parameter )
        {
            var task = Task.Run(() => DelayAndDelete(tempChannel, createTempChannel, parameter));
            await Task.CompletedTask;
        }

        public async Task StopDelay(tempchannels tempChannel)
        {
            var tempChannelDelayTimer = TempChannelDelayTimers.FirstOrDefault(t => t.TempChannel.id == tempChannel.id);
            var dataWrapper = tempChannelDelayTimer.DataWrapper;
            dataWrapper.Dispose();
            TempChannelDelayTimers.Remove(tempChannelDelayTimer);
            tempChannelDelayTimer = null;
            await EntityFramework.TempChannelsHelper.UpdateDeleteDelay(tempChannel.id, null);
            await Task.CompletedTask;
        }
        #endregion

        #region Private Methods
        private async Task DelayAndDelete(tempchannels tempChannel, createtempchannels createTempChannel, Entities.VoiceUpdatedParameter parameter)
        { 
            var delayInMinutes = createTempChannel.delay;
            var delayInSeconds = delayInMinutes * 60;
            var delay = delayInSeconds * 1000;

            TempChannelDelayTimers.Add(new Entities.TempChannelDelay() { TempChannel = tempChannel, DataWrapper = new DateWrapper(dateWrappers, DateTime.Now.AddMinutes(delayInMinutes.Value), delay.Value, tempChannel, parameter)});
            await Task.CompletedTask;
        }
        #endregion
    }
}
