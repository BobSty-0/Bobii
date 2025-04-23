using bobii_rework.EntityFramework;
using bobii_rework.Extensions;
using bobii_rework.Handler.UtilityHandler;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace bobii_rework
{
    public class App
    {
        #region Methods
        public async Task Init()
        {
            MigrateDatabase();

            await using var services = ConfigureServices();
            var interactionService = services.GetRequiredService<InteractionService>();
            var client = services.GetRequiredService<DiscordShardedClient>(); 

            client.Log += LogToConsole;
            interactionService.Log += LogToConsole;

            await client.LoginAsync(TokenType.Bot, Configuration.GetConfigValue<string>(Configuration.Token));
            await client.StartAsync();

            _ = services.GetRequiredService<TempChannelDelayHandler>();
            _ = services.GetRequiredService<AppInitializer>();
            await Task.Delay(-1);
        }
        #endregion

        #region Private Functions
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(GetDiscordConfig()))
                .AddSingleton(new CommandService(GetCommandServiceConfig()))
                .AddSingleton(GetInteractionService)
                .AddSingleton(GetTempChannelDelayHandler)
                .AddSingleton<AppInitializer>()
                .BuildServiceProvider();
        }

        private void MigrateDatabase()
        {
            using var bobiiContext = new BobiiContext();
            bobiiContext.Database.Migrate();

            using var bobiiLngContext = new BobiiLngContext();
            bobiiContext.Database.Migrate();

            this.WriteLineToConsole("Datenbank auf dem neusten Stand");
        }

        private Task LogToConsole(LogMessage msg)
        {

            this.WriteLineToConsole(msg.ToString());

            if (msg.Exception?.StackTrace != null)
            {
                this.WriteLineToConsole(msg.Exception.StackTrace);
            }

            return Task.CompletedTask;
        }

        private InteractionService GetInteractionService(IServiceProvider serviceProvider)
        {
            return new InteractionService(serviceProvider.GetRequiredService<DiscordShardedClient>().Rest);
        }

        private TempChannelDelayHandler GetTempChannelDelayHandler(IServiceProvider serviceProvider)
        {
            return new TempChannelDelayHandler(serviceProvider.GetRequiredService<DiscordShardedClient>());
        }

        private CommandServiceConfig GetCommandServiceConfig()
        {
            return new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                DefaultRunMode = Discord.Commands.RunMode.Async,
                CaseSensitiveCommands = false
            };
        }

        private DiscordSocketConfig GetDiscordConfig()
        {
            return new DiscordSocketConfig
            {
                ResponseInternalTimeCheck = false,
                MessageCacheSize = 500,
                LogLevel = LogSeverity.Info,
                GatewayIntents = GetGatewayIntents(),
                AlwaysDownloadUsers = true,
                UseInteractionSnowflakeDate = true,
                TotalShards = Configuration.GetConfigValue<int>(Configuration.ShardCount)
            };
        }

        private GatewayIntents GetGatewayIntents()
        {
            return GatewayIntents.DirectMessageTyping |
                   GatewayIntents.GuildMessageTyping |
                   GatewayIntents.MessageContent |
                   GatewayIntents.GuildMembers |
                   GatewayIntents.DirectMessages |
                   GatewayIntents.GuildMessages |
                   GatewayIntents.GuildVoiceStates |
                   GatewayIntents.Guilds |
                   GatewayIntents.GuildEmojis;
        }
        #endregion
    }
}
