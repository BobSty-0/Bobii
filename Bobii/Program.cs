using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Bobii.src.TextChannel;

namespace Bobii
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using var services = BobiiHelper.ConfigureServices();

            var client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += BobiiHelper.Log;

            JObject config = BobiiHelper.GetConfig();
            string token = config["BobiiConfig"][0]["token"].Value<string>();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }
    }
}
