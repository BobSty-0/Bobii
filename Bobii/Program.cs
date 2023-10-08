using System.Threading.Tasks;
using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using System.IO;
using Bobii.src.Handler;
using Bobii.src.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Bobii.src.Helper;
using System.Diagnostics;

namespace Bobii
{
    public class Program
    {
        #region Methods
        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        public static async Task SetBotStatusAsync(DiscordSocketClient client)
        {
            await client.SetActivityAsync(new Game("/help", ActivityType.Listening));
            await client.SetStatusAsync(UserStatus.Online);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Bobii       Status was set sucessfully");
        }

        public static async Task SetBotUpdateStatusAsync(DiscordSocketClient client)
        {
            await client.SetCustomStatusAsync("Installing new update");
            await client.SetStatusAsync(UserStatus.DoNotDisturb);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Bobii       Status was set sucessfully to update status");
        }
        #endregion

        #region Functions 
        public async Task MainAsync()
        {
            // Doing migrations if there are some to do
            using (var context = new BobiiEntities())
            {
                context.Database.Migrate();
            }

            using (var context = new BobiiLngCodes())
            {
                context.Database.Migrate();
            }

            string token = GeneralHelper.GetConfigKeyValue(src.Bobii.ConfigKeys.Token);
            using var services = ConfigureServices();

            var client = services.GetRequiredService<DiscordSocketClient>();
            var interactionService = services.GetRequiredService<InteractionService>(); 
            client.Log += Log;
            interactionService.Log += Log;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            var handlingService = new HandlingService(services, interactionService);

            await Task.Delay(-1);
        }

        public static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 500,
                    LogLevel = LogSeverity.Info,
                    GatewayIntents =  
                    GatewayIntents.DirectMessageTyping |
                    GatewayIntents.GuildMessageTyping |
                    GatewayIntents.MessageContent |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.DirectMessages |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildVoiceStates |
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildEmojis,
                    AlwaysDownloadUsers = true,
                    UseInteractionSnowflakeDate = true
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Info,
                    DefaultRunMode = Discord.Commands.RunMode.Async,
                    CaseSensitiveCommands = false
                }))
                .AddSingleton(x => 
                new InteractionService(
                    x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<HandlingService>()
                .BuildServiceProvider();
        }

        public static JObject GetConfig()
        {
            using StreamReader configJson = new StreamReader(Directory.GetCurrentDirectory() + @"/Config.json");
            return (JObject)JsonConvert.DeserializeObject(configJson.ReadToEnd());
        }

        public static Task Log(LogMessage msg)
        {

            Console.WriteLine(msg.ToString());

            if(msg.Exception != null)
            {
                Console.WriteLine(msg.Exception.StackTrace);
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
