using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Discord;
using System.Linq;
using Newtonsoft.Json;
using Bobii.src.Commands;
using Bobii.src.TempVoice;


namespace Bobii
{
    public class CommandHandlingService
    {
        #region Declarations 
        private readonly CommandService _commands;
        private DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        public ulong _createTempChannelID;
        #endregion

        #region Constructor  
        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleCommandAsync;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
        }
        #endregion

        #region Tasks
        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            TempVoiceChannel.VoiceChannelActions(user, oldVoice, newVoice, _client);
        }

        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            if (rawMessage.Author.IsBot || !(rawMessage is SocketUserMessage message) || message.Channel is IDMChannel)
                return;

            var context = new SocketCommandContext(_client, message);

            int argPos = 0;

            JObject config = BobiiHelper.GetConfig();
            string[] prefixes = JsonConvert.DeserializeObject<string[]>(config["prefixes"].ToString());

            if (prefixes.Any(x => message.HasStringPrefix(x, ref argPos)) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute the command.
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess && result.Error.HasValue)
                    await context.Channel.SendMessageAsync($":x: {result.ErrorReason}");
            }
        }

        private async Task ClientReadyAsync()
    => await BobiiHelper.SetBotStatusAsync(_client);

        public async Task InitializeAsync()
    => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        #endregion
    }
}
