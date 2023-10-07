using Bobii.src.EntityFramework.Entities;
using Bobii.src.Helper;
using Bobii.src.Models;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Timers;

namespace Bobii.src.TempChannel
{
    public class AutoDeleteDateWrapper : IDisposable
    {
        #region Declarations
        private Timer _timer;
        #endregion

        #region Properties
        private Dictionary<IMessage, DateTime> _messagesToBeDeleted;
        #endregion

        #region Constructor
        public AutoDeleteDateWrapper()
        {
            _messagesToBeDeleted = new Dictionary<IMessage, DateTime>();
            _timer = new Timer();
            _timer.Interval = 60000; // 1 Minute
            _timer.Elapsed += new ElapsedEventHandler(Delete);
            _timer.Start();
        }
        #endregion

        #region Public Methods
        public void AddMessageToBeDeleted(IMessage message, int delay)
        {
            var deleteDate = DateTime.Now.AddMinutes(delay);
            _messagesToBeDeleted.Add(message, deleteDate);
        }

        public void RemoveMessageToBeDetletedIfOnDict(ulong messageId)
        {
            var message = _messagesToBeDeleted.Where(m => m.Key.Id == messageId);
            if (message.Count() > 0)
            {
                _messagesToBeDeleted.Remove(message.First().Key);
            }
        }
        #endregion

        #region Private Functions
        private void Delete(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                var messagesToBeDeleted = _messagesToBeDeleted.Where(p => p.Value < DateTime.Now).ToList();

                var messagesGrouped = messagesToBeDeleted.ToLookup(m => m.Key.Channel.Id);

                foreach (var messageGroup in messagesGrouped)
                {
                    try
                    {
                        var channel = (SocketTextChannel)messageGroup.First().Key.Channel;
                        var messages = messageGroup.Select(m => m.Key);
                        await channel.DeleteMessagesAsync(messages);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Missing Permissions"))
                        {
                            var channel = (SocketTextChannel)messageGroup.First().Key.Channel;
                            var lang = LanguageHelper.GetLanguage(channel.Guild.Id).Result.langugeshort;
                            await channel.SendMessageAsync(GeneralHelper.GetContent("C316", lang).Result);
                        }
                    }
                }


            });
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            _timer.Elapsed -= Delete;
            _timer.Dispose();
        }
        #endregion
    }
}
