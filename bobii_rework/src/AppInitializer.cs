using System.Globalization;
using System.Reflection;
using bobii_rework.Extensions;
using bobii_rework.Handler;
using bobii_rework.Interactions.SelectionMenus;
using bobii_rework.Interactions.SlashCommands;
using bobii_rework.Modals;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace bobii_rework
{
    public class AppInitializer
    {
        #region Declarations
        private readonly IServiceProvider _serviceProviderProvider;
        private readonly InteractionService _interactionService;
        private readonly DiscordShardedClient _discordShardedClient;
        private int _shardCount;
        #endregion

        #region Contsructors
        public AppInitializer(IServiceProvider serviceProvider, DiscordShardedClient shardedClient, DiscordShardedClient discordShardedClient)
        {
            _serviceProviderProvider = serviceProvider;
            _discordShardedClient = discordShardedClient;
            _interactionService = _serviceProviderProvider.GetRequiredService<InteractionService>();

            shardedClient.ShardReady += ShardReady;

            InitShardHandler(shardedClient);
        }
        #endregion

        #region Private Functions
        private void InitShardHandler(DiscordShardedClient shardedClient)
        {
            foreach (var shard in shardedClient.Shards)
            {
                _ = new ShardEventHandler(shard, _serviceProviderProvider);
            }
        }

        private async Task ShardReady(DiscordSocketClient client)
        {
            _shardCount++;
            this.WriteLineToConsole($"Shard [{_shardCount}] ist bereit");

            // Erst wenn alle Clients ready sind, wird der Interaction Service initialisiert
            if (_shardCount == _discordShardedClient.Shards.Count)
            {
                _discordShardedClient.ShardReady -= ShardReady;
                await InitInteractionService();
            }
        }

        private async Task InitInteractionService()
        {
            await InitInteractionModules();
            await InitCommandLocalization();
            await InitGlobalCommandsAsync();
            await InitGuildCommandsAsync()
;

            this.WriteLineToConsole("Services sind initialisiert");
        }

        private async Task InitInteractionModules()
        {
            await _interactionService.AddModuleAsync<TestSlashCommands>(_serviceProviderProvider);

            await _interactionService.AddModuleAsync<CreatorSlashCommands>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<CreatorInfoSelectionMenus>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<TempChannelModalInteractions>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<TempChannelSlashCommands>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<HelpSlashCommands>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<HelpSelectionMenus>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<TextUtilitySlashCommands>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<StealEmojiSlashCommands>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<LanguageShlashCommands>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<SetUpdateModeSlashCommand>(_serviceProviderProvider);
            await _interactionService.AddModuleAsync<AutoScaleVoiceChannelCommands>(_serviceProviderProvider);
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
