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
using Bobii.src.DBStuff.Tables;

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
            _client.MessageReceived += HandleMessageRecieved;
            _client.LeftGuild += HandleLeftGuild;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
            _client.ChannelDestroyed += HandleChannelDestroyed;
        }
        #endregion

        #region Tasks
        private async Task HandleMessageRecieved(SocketMessage message)
        {
            if (message.Content.Contains("<@!776028262740393985>"))
            {
                if (message.Content.Contains("Guildcount"))
                {
                    await message.Channel.SendMessageAsync($"Guilds: {_client.Guilds.Count()}\n");
                    return;
                }
                await message.Channel.SendMessageAsync("Please dont ping me!");
            }
        }

        private async Task HandleInteractionCreated(SocketInteraction interaction)
        {
            switch (interaction.Type) // We want to check the type of this interaction
            {
                case InteractionType.ApplicationCommand: // If it is a command
                    await Commands.SlashCommands.SlashCommandHandler(interaction, _client); // Handle the command somewhere
                    break;
                default: // We dont support it
                    Console.WriteLine("Unsupported interaction type: " + interaction.Type);
                    break;
            }
        }

        private async Task HandleChannelDestroyed(SocketChannel channel)
        {
            var table = createtempchannels.CraeteTempChannelListWithAll();
            foreach (DataRow row in table.Rows)
            {
                if (row.Field<string>("createchannelid") == channel.Id.ToString()) 
                {
                    createtempchannels.RemoveCC("No Guild supplyed", channel.Id.ToString());
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler      Channel: '{channel.Id.ToString()}' was succesfully deleted");

                }
            }
            await Task.CompletedTask;
        }

        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            await TempVoiceChannel.TempVoiceChannel.VoiceChannelActions(user, oldVoice, newVoice, _client);
        }

        private async Task HandleLeftGuild(SocketGuild guild)
        {
            // §TODO 11.07.2021/JG Delete everything if Bot leaves the Guild
            await Task.CompletedTask;
        }

        private async Task ClientReadyAsync()
    => await Program.SetBotStatusAsync(_client);

        // §TODO 10.07.2021/JG schauen wie ich dass hier ersetzt bekomme, da eigentlich keine Commands mehr auf diesem weg gebaut werden
        public async Task InitializeAsync()
    => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        #endregion

        #region RegisterCommandTasks
        public async Task RegisterHelpCommand()
        {
            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "help",
                Description = "Returns a list of all my Commands",
            });
        }

        public async Task RegisterTempInfoCommand()
        {
            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempinfo",
                Description = "Returns all the TempChannels of this Guild",
            });
        }

        public async Task RegisterTempAddCommand()
        {
            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempadd",
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
                        Description = "This will be the name of the TempChannel. Note: User = Username",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });
        }

        public async Task RegisterTempRemoveCommand()
        {
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

                }
            });
        }

        public async Task RegisterTempChangeName()
        {
            await _client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempchangename",
                Description = "Changes the TempChannel name fo an already existing CreateTempChannel",
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
        }

        public async Task RegisterRemoveCoommand()
        {
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
        #endregion
    }
}
