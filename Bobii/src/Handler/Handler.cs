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
            //Doing it in two steps to avoid the Exception
            var parsedSocketUser = (SocketUser)message.Author;
            var parsedSocketGuildUser = (SocketGuildUser)parsedSocketUser;

            var filterWords = filterwords.GetCreateFilterWordListFromGuild(parsedSocketGuildUser. Guild.Id.ToString());

            string editMessage = message.Content;
            bool messageContainsFilterWord = false;

            foreach (DataRow row in filterWords.Rows)
            {
                if (editMessage.Contains(row.Field<string>("filterword").Trim()))
                {
                    editMessage = editMessage.Replace(row.Field<string>("filterword").Trim(), row.Field<string>("replaceword").Trim());
                    messageContainsFilterWord = true;
                }
            }

            if (messageContainsFilterWord)
            {
                await message.Channel.SendMessageAsync($"**{message.Author.Username}** was trying to say the following:", false, TextChannel.TextChannel.CreateEmbedWithoutTitle(editMessage, parsedSocketGuildUser.Guild.ToString()));
                await message.DeleteAsync();
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
    }
}
