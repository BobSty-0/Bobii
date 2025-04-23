using System.Collections.Concurrent;
using bobii_rework.Entities.EntityFramework;
using bobii_rework.Repositories;
using bobii_rework.Wrapper;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace bobii_rework.Handler.UtilityHandler
{
    public class TempChannelDelayHandler
    {
        #region Declarations
        private ConcurrentBag<DelayDateWrapper> _dateWrappers;
        private Dictionary<tempchannels, DelayDateWrapper> _tempChannelDelayTimers;
        private IDiscordClient _client;
        #endregion

        #region Constructor
        public TempChannelDelayHandler(IDiscordClient client)
        {
            _dateWrappers = new ConcurrentBag<DelayDateWrapper>();
            _tempChannelDelayTimers = new Dictionary<tempchannels, DelayDateWrapper>();
            _client = client;

        }
        #endregion

        #region Public Methods
        public async Task InitializeDelayDelete(DiscordShardedClient client)
        {
            var tempChannelsMitDelay = await TempChannelRepository.GetTempChannelsMitDelay();
            foreach (var tempChannel in tempChannelsMitDelay)
            {
                var voiceChannel = await client.Rest.GetChannelAsync(tempChannel.channelid);
                if (voiceChannel == null)
                {
                    continue;
                }

                var guildVoiceChannel = (RestVoiceChannel)voiceChannel;
                var timeDifference = tempChannel.deletedate.GetValueOrDefault() - DateTime.Now;

                if (timeDifference.TotalMinutes <= 0)
                {
                    await guildVoiceChannel.DeleteAsync();
                    continue;
                }

                var creatorChannel = await CreatorChannelRepository.GetCreatorChannel(tempChannel.createchannelid!.Value);
                StartDelayTask(tempChannel, creatorChannel!);
            }
        }

        public async Task StopDelayTask(tempchannels tempChannel)
        {
            var delayDateWrapper = _tempChannelDelayTimers[tempChannel];
            delayDateWrapper.Dispose();
            await TempChannelRepository.UpdateDelay(tempChannel.channelid, null);
        }

        private void StartDelayTask(tempchannels tempChannel, createtempchannels createTempChannel)
        {
            var delayInMinutes = createTempChannel.delay!;
            var delayInSeconds = delayInMinutes * 60;
            var delay = delayInSeconds * 1000;

            var delayDateWrapper = new DelayDateWrapper(
                _dateWrappers,
                DateTime.Now.AddMinutes(delayInMinutes.Value),
                delay.Value,
                tempChannel,
                _client);

            _tempChannelDelayTimers.Add(
                tempChannel,
                delayDateWrapper);
        }
        #endregion
    }
}
