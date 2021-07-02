using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class VoiceChannelHandlingService
    {
        #region Declarations 
        private readonly CommandService _commands;
        public DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        #endregion

        #region Constructor  
        public VoiceChannelHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _client.Ready += ClientReadyAsync;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
        }
        #endregion

        #region Tasks
        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            await TempVoiceChannel.TempVoiceChannel.VoiceChannelActions(user, oldVoice, newVoice, _client);
        }

        private async Task ClientReadyAsync()
        => await Program.SetBotStatusAsync(_client);

        public async Task InitializeAsync()
         => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        #endregion
    }
}
