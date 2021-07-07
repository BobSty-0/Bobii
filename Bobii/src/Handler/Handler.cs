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
using System.Data;

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

            _client.InteractionCreated += HandleInteractionCreated;
            _client.Ready += ClientReadyAsync;
            //_client.Ready += RegisterCommands;
            _client.MessageReceived += HandleCommandAsync;
            _client.JoinedGuild += HandleJoinGuild;
            _client.LeftGuild += HandleLeftGuild;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
            _client.ChannelDestroyed += HandleChannelDestroyed;
        }
        #endregion

        #region Tasks

        public async Task RegisterCommands()
        {
            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "help",
                Description = "Returns a list of all my Commands",
            });

            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempinfo",
                Description = "Returns all the Tempannels of this Guild",
            });

            //await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            //{
            //    Name = "tempadd",
            //    Description = "Adds an CreateTempChannel",
            //    Options = new List<Discord.ApplicationCommandOptionProperties>()
            //    {
            //        new ApplicationCommandOptionProperties()
            //        {
            //            Name = "channelid",
            //            Required = true,
            //            Description = "ID of the CreateTempChannel",
            //            Type = Discord.ApplicationCommandOptionType.String,
            //        },

            //        new ApplicationCommandOptionProperties()
            //        {
            //            Name = "tempchannelname",
            //            Required = true,
            //            Description = "This will be the name of the TempChannel",
            //            Type = Discord.ApplicationCommandOptionType.String,
            //        }
            //    }
            //});

            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempremove",
                Description = "Removes an CreateTempChannel",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "channelid",
                        Required = true,
                        Description = "ID of the CreateTempChannel",
                        Type = Discord.ApplicationCommandOptionType.String
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "tempchannelname",
                        Required = true,
                        Description = "This will be the new name of the TempChannel",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },
                }
            });

            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempchangename",
                Description = "Adds an CreateTempChannel",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "channelid",
                        Required = true,
                        Description = "ID of the CreateTempChannel",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "tempchannelname",
                        Required = true,
                        Description = "This will be the new name of the TempChannel",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });

            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "removecommand",
                Description = "removes a slashcommand",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "commandname",
                        Required = true,
                        Description = "The name oft he command which should be removed",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });
        }

        private async Task HandleInteractionCreated(SocketInteraction interaction)
        {
            switch (interaction.Type) // We want to check the type of this interaction
            {
                case InteractionType.ApplicationCommand: // If it is a command
                    await Commands.ShlashCommands.SlashCommandHandler(interaction, _client); // Handle the command somewhere
                    break;
                default: // We dont support it
                    Console.WriteLine("Unsupported interaction type: " + interaction.Type);
                    break;
            }
        }

        private async Task HandleChannelDestroyed(SocketChannel channel)
        {
            var table = DBStuff.createtempchannels.CraeteTempChannelListWithAll();
            foreach (DataRow row in table.Rows)
            {
                if (row.Field<string>("createchannelid") == channel.Id.ToString()) 
                {
                    DBStuff.createtempchannels.RemoveCC("No Guild supplyed", channel.Id.ToString());
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler      Channel: '{channel.Id.ToString()}' was succesfully deleted");

                }
            }
            await Task.CompletedTask;
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
