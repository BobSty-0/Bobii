using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole;
using System;
using System.Threading.Tasks;

namespace DiscordBotBobSty
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "Nzc2MDI4MjYyNzQwMzkzOTg1.X6u6ow.uKgEZFbL9RFQrYkKANWwvtJzaRo",
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug
            }) ;

            discord.UseInteractivity(new DSharpPlus.Interactivity.InteractivityConfiguration()
            {
                PollBehaviour = DSharpPlus.Interactivity.Enums.PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1", 
                Port = 2333 
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "sanoj", 
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = discord.UseLavalink();


            var service = new ServiceCollection()
                .AddSingleton<Random>()
                .BuildServiceProvider();

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "'" },
                Services = service
            }) ;

            commands.RegisterCommands<Commands.ChatCommands> ();
            commands.RegisterCommands<Commands.MusicCommands>();
            commands.SetHelpFormatter<Help.CustomHelpFormatter>();
            

            await discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }
    }
}
