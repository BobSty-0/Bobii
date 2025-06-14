using System.Collections.Concurrent;
using System.Timers;
using bobii_rework.Entities.EntityFramework;
using bobii_rework.Entities.Interactions;
using bobii_rework.Repositories;
using Discord;
using Discord.Rest;
using Timer = System.Timers.Timer;

namespace bobii_rework.Wrapper
{
    public class DelayDateWrapper : IDisposable
    {
        #region Declarations
        private ConcurrentBag<DelayDateWrapper> _timerList;
        private DateTime _time;
        private readonly Timer _timer;
        private readonly IDiscordClient _client;
        #endregion

        #region Properties
        public TempChannel TempChannel { get; set; }
        public DateTime DeleteTime => _time;
        #endregion

        public DelayDateWrapper(ConcurrentBag<DelayDateWrapper> list, DateTime time, int delay, TempChannel tempChannel, IDiscordClient client)
        {
            TempChannel = tempChannel;
            _timerList = list;
            _time = time;
            _client = client;

            _timerList.Add(this);

            _timer = new Timer();
            _timer.Interval = delay; // 5 Minutes
            _timer.Elapsed += Delete;
            _timer.Start();

            _ = TempChannelRepository.UpdateDelay(tempChannel.channelid, time);
        }

        private void Delete(object sender, ElapsedEventArgs e)
        {
            _timerList = new ConcurrentBag<DelayDateWrapper>(_timerList.Except(new[] { this }));
            var voiceChannel = (RestVoiceChannel)_client.GetChannelAsync(TempChannel.channelid).Result;
            voiceChannel.DeleteAsync();
            _timer.Elapsed -= Delete;
            _timer.Dispose();
        }

        public void Dispose()
        {
            _timer.Elapsed -= Delete;
            _timer.Dispose();
        }
    }
}
