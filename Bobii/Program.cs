using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using System.IO;
using Bobii.src.Handler;

namespace Bobii
{
    public class Program
    {
        #region Methods
        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        public static async Task SetBotStatusAsync(DiscordSocketClient client)
        {
            await client.SetActivityAsync(new Game("BobSty", ActivityType.Listening));
            await client.SetStatusAsync(UserStatus.Online);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Bobii       Status was set sucessfully");
        }
        #endregion

        #region Functions 
        public async Task MainAsync()
        {
            using var services = ConfigureServices();

            var client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += Log;

            JObject config = GetConfig();
            string token = config["BobiiConfig"][0]["token"].Value<string>();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
            await services.GetRequiredService<VoiceChannelHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }

        public static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 500,
                    LogLevel = LogSeverity.Info
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Info,
                    DefaultRunMode = RunMode.Async,
                    CaseSensitiveCommands = false
                }))
                .AddSingleton<CommandHandlingService>()
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
            return Task.CompletedTask;
        }
        #endregion
    }
}
