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
using System.Collections.Generic;

namespace Bobii.src.Handler
{
    public class HandlingService
    {
        #region Declarations 
        private readonly CommandService _commands;
        public DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        #endregion

        #region Constructor  
        public HandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _client.Ready += ClientReadyAsync;
            _client.Ready += RegisterCommands;
            _client.MessageReceived += HandleCommandAsync;
            _client.JoinedGuild += HandleJoinGuild;
            _client.LeftGuild += HandleLeftGuild;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
            _client.InteractionCreated += HandleInteractionCreated;
        }
        #endregion

        #region Tasks
        private async Task HandleInteractionCreated(SocketInteraction interaction)
        {
            var test = interaction;
            switch (interaction.Type) // We want to check the type of this interaction
            {
                case InteractionType.ApplicationCommand: // If it is a command
                    await Commands.ShlashCommands.SlashCommandHandler(interaction); // Handle the command somewhere
                    break;
                default: // We dont support it
                    Console.WriteLine("Unsupported interaction type: " + interaction.Type);
                    break;
            }
        }

        private async Task RegisterCommands()
        {
            var test = _client.Rest.GetGlobalApplicationCommands();
            // Creating a global command
            var myGlobalCommand = await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "switchprefix",
                Description = "Switches the prefix",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "prefix",
                        Required = true,
                        Description = "NewPrefix",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });



            //// Creating a guild command
            //var myGuildCommand = await _client.Rest.CreateGuildCommand(new Discord.SlashCommandCreationProperties()
            //{
            //    Name = "examplelol",
            //    Description = "Runs the guild example command",
            //    Options = new List<Discord.ApplicationCommandOptionProperties>()
            //    {
            //        new ApplicationCommandOptionProperties()
            //        {
            //            Name = "Guild example option",
            //            Required = false,
            //            Description = "Guild option description",
            //            Type = Discord.ApplicationCommandOptionType.String,
            //        }
            //    }
            //}, 712373862179930144); // <- the guild id
        }


        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            await TempVoiceChannel.TempVoiceChannel.VoiceChannelActions(user, oldVoice, newVoice, _client);
        }

        private async Task HandleJoinGuild(SocketGuild guild)
        {
            DBStuff.Prefixes.AddPrefix(guild);
            await Task.CompletedTask;
        }

        private async Task HandleLeftGuild(SocketGuild guild)
        {
            DBStuff.Prefixes.RemovePrefix(guild);
            await Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            if (rawMessage.Author.IsBot || !(rawMessage is SocketUserMessage message) || message.Channel is IDMChannel)
                return;

            var context = new SocketCommandContext(_client, message);

            int argPos = 0;

            JObject config = Program.GetConfig();
            var channel = rawMessage.Channel as SocketGuildChannel;
            string prefix = DBStuff.Prefixes.GetPrefixFromGuild(channel.Guild.Id.ToString());
            prefix = prefix.Trim();

            if (message.HasStringPrefix(prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute the command.
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess && result.Error.HasValue)
                    await context.Channel.SendMessageAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"{result.ErrorReason}"));
            }
        }

        private async Task ClientReadyAsync()
    => await Program.SetBotStatusAsync(_client);

        public async Task InitializeAsync()
    => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        #endregion
    }
}
