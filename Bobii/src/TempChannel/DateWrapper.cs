using Bobii.src.EntityFramework.Entities;
using Bobii.src.Models;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Bobii.src.TempChannel
{
    public class DateWrapper : IDisposable
    {
        private ConcurrentBag<DateWrapper> TimerList;
        private DateTime Time;

        public DateTime DeleteTime
        {
            get { return Time; }
        }
        public tempchannels TempChannel { get; set; }
        public VoiceUpdatedParameter VoiceUpdatedParameter { get; set; }

        private Timer _timer;

        public DateWrapper(ConcurrentBag<DateWrapper> list, DateTime time, int delay, tempchannels tempChannel, VoiceUpdatedParameter parameter)
        {
            this.TempChannel = tempChannel;
            this.VoiceUpdatedParameter = parameter;
            this.TimerList = list;
            this.Time = time;

            this.TimerList.Add(this);

            _timer = new Timer();
            _timer.Interval = delay; // 5 Minutes
            _timer.Elapsed += new ElapsedEventHandler(Delete);
            _timer.Start();
            EntityFramework.TempChannelsHelper.UpdateDeleteDelay(tempChannel.id, time);
        }

        private void Delete(object sender, EventArgs e)
        {
            TimerList = new ConcurrentBag<DateWrapper>(TimerList.Except(new[] { this }));
            Helper.DeleteTempChannel(VoiceUpdatedParameter, TempChannel);
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
