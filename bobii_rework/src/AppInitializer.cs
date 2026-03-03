using bobii_rework.Extensions;
using bobii_rework.Handler;
using bobii_rework.Handler.UtilityHandler;
using bobii_rework.Interactions.Buttons;
using bobii_rework.Interactions.Modals;
using bobii_rework.Interactions.SelectionMenus;
using bobii_rework.Interactions.SlashCommands;
using bobii_rework.Repositories;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;
using Exception = System.Exception;

namespace bobii_rework
{
    public class AppInitializer
    {
        #region Declarations
        private readonly IServiceProvider _serviceProviderProvider;
        private readonly InteractionService _interactionService;
        private readonly DiscordShardedClient _discordShardedClient;
        private readonly TempChannelDelayHandler _tempChannelDelayHandler;
        private int _shardCount;
        #endregion

        #region Contsructors
        public AppInitializer(IServiceProvider serviceProvider, DiscordShardedClient shardedClient, DiscordShardedClient discordShardedClient)
        {
            _serviceProviderProvider = serviceProvider;
            _discordShardedClient = discordShardedClient;

            _interactionService = _serviceProviderProvider.GetRequiredService<InteractionService>();
            _tempChannelDelayHandler = _serviceProviderProvider.GetRequiredService<TempChannelDelayHandler>();

            shardedClient.ShardReady += ShardReady;

            InitShardHandler(shardedClient);
        }
        #endregion

        #region Private Functions
        private void InitShardHandler(DiscordShardedClient shardedClient)
        {
            foreach (var shard in shardedClient.Shards)
            {
                _ = new ShardHandler(shard, _serviceProviderProvider);
            }
        }

        private async Task SetBotStatus(DiscordSocketClient client)
        {
            var statusText = Configuration.GetConfigValue<string>(Configuration.StatusText);
            var activityType = Configuration.GetConfigValue<ActivityType>(Configuration.ActivityType);
            var userStatus = Configuration.GetConfigValue<UserStatus>(Configuration.UserStatus);

            await client.SetActivityAsync(new Game(statusText, activityType));
            await client.SetStatusAsync(userStatus);
        }

        private async Task ShardReady(DiscordSocketClient client)
        {
            await SetBotStatus(client);
            this.WriteLineToConsole($"Shard #{_shardCount} ist bereit");
            _shardCount++;

            if (_shardCount != _discordShardedClient.Shards.Count)
            {
                return;
            }

            // Erst wenn alle Clients ready sind, wird der Interaction Service initialisiert
            _discordShardedClient.ShardReady -= ShardReady;
            await InitInteractionService();
            await _tempChannelDelayHandler.InitializeDelayDelete(_discordShardedClient);
            await DeleteEmptyVoiceChannel();

        }

        private async Task InitInteractionService()
        {
            await InitInteractionModules();
            await InitCommandLocalization();
            await InitGlobalCommandsAsync();
            // TODO
            await InitGuildCommandsAsync();

            this.WriteLineToConsole("Services sind initialisiert");
        }

        private async Task DeleteEmptyVoiceChannel()
        {
            var tempChannels = await TempChannelRepository.GetTempChannels();
            foreach (var tempChannel in tempChannels)
            {
                if (tempChannel.deletedate != null)
                {
                    continue;
                }

                var restVoiceChannel = (RestVoiceChannel)await _discordShardedClient.Rest.GetChannelAsync(tempChannel.channelid);
                if (restVoiceChannel == null)
                {
                    await TempChannelRepository.RemoveTempChannelIfExisting(tempChannel.channelid);
                }

                var socketVoiceChannel = (SocketVoiceChannel)_discordShardedClient.GetChannel(tempChannel.channelid);
                if (socketVoiceChannel?.ConnectedUsers.Count == 0)
                {
                    await socketVoiceChannel.DeleteAsync();
                }
            }
        }

        private async Task InitInteractionModules()
        {
            await _interactionService.AddModuleAsync<TestSlashCommands>(_serviceProviderProvider);
            try
            {
                await _interactionService.AddModuleAsync<DmButtons>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<CreatorSlashCommands>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<CreatorSelectMenus>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<CreatorButtons>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<LanguageSelectMenus>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<TempChannelModals>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<TempChannelSelectMenus>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<TempChannelSlashCommands>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<TempChannelButtons>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<HelpSlashCommands>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<HelpSelectMenus>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<TextUtilitySlashCommands>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<StealEmojiSlashCommands>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<LanguageShlashCommands>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<SetUpdateModeSlashCommand>(_serviceProviderProvider);
                await _interactionService.AddModuleAsync<AutoScaleVoiceChannelCommands>(_serviceProviderProvider);
            }
            catch (Exception ex)
            {
                this.WriteLineToConsole(ex.Message);
            }
        }

        public async Task InitCommandLocalization()
        {
            // Im Debug Modus ist der Pfad abweichend, daher wird die Lokalisierung nur auf Live initialisiert
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return;
            }

            try
            {
                _interactionService.LocalizationManager = new ResxLocalizationManager("Bobii.Bobii.Localization.Localization", Assembly.GetExecutingAssembly(), new CultureInfo[] {
                        CultureInfo.GetCultureInfo("de"),
                        CultureInfo.GetCultureInfo("en-US"),
                        CultureInfo.GetCultureInfo("ru") });
            }
            catch (Exception ex)
            {
                this.WriteLineToConsole(ex.Message);
            }

            await Task.CompletedTask;
        }

        public async Task InitGuildCommandsAsync()
        {
            await _interactionService.AddModulesToGuildAsync(
                860974744190976020,
                true,
                new[]
                {
                    _interactionService.GetModuleInfo<TestSlashCommands>(),
                });
        }

        public async Task InitGlobalCommandsAsync()
        {
            try
            {
                await _interactionService.AddModulesGloballyAsync(
                    true,
                    new[] {
                        _interactionService.GetModuleInfo<CreatorSlashCommands>(),
                        _interactionService.GetModuleInfo<TempChannelSlashCommands>(),
                        _interactionService.GetModuleInfo<HelpSlashCommands>(),
                        _interactionService.GetModuleInfo<TextUtilitySlashCommands>(),
                        _interactionService.GetModuleInfo<StealEmojiSlashCommands>(),
                        _interactionService.GetModuleInfo<LanguageShlashCommands>(),
                        _interactionService.GetModuleInfo<AutoScaleVoiceChannelCommands>()
                    });
            }
            catch (Exception ex)
            {
                this.WriteLineToConsole(ex.Message);
            }
        }
        #endregion
    }
}
