using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Data;
using Bobii.src.DBStuff.Tables;
using System.Text;
using DiscordBotsList.Api;
using Newtonsoft.Json.Linq;
using DiscordBotsList.Api.Objects;

namespace Bobii.src.Handler
{
    public class HandlingService
    {
        #region Declarations 
        public DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private IDblSelfBot _bot;
        #endregion

        #region Constructor  
        public HandlingService(IServiceProvider services)
        {       
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _client.InteractionCreated += HandleInteractionCreated;
            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleMessageRecieved;
            _client.LeftGuild += HandleLeftGuild;
            _client.JoinedGuild += HandleJoinGuild;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
            _client.ChannelDestroyed += HandleChannelDestroyed;
        }
        #endregion

        #region Tasks
        private async Task HandleMessageRecieved(SocketMessage message)
        {
            try
            {
                if (message.Content == "<@!776028262740393985> servercount" && message.Author.Id == 410312323409117185)
                {
                    var sb = new StringBuilder();
                    foreach (var guild in _client.Guilds)
                    {
                        sb.AppendLine(guild.Name);
                    }
                    sb.AppendLine();
                    sb.AppendLine($"Servercount: {_client.Guilds.Count}");
                    _ = message.Channel.SendMessageAsync(sb.ToString());
                }

                if (message.Channel is ITextChannel chan)
                {
                    var filterWords = filterwords.GetCreateFilterWordListFromGuild(chan.Guild.Id.ToString());
                    var parsedSocketUser = (SocketUser)message.Author;
                    var parsedSocketGuildUser = (SocketGuildUser)parsedSocketUser;



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
                        _ = message.Channel.SendMessageAsync("", false, TextChannel.TextChannel.CreateFilterWordEmbed(parsedSocketUser, parsedSocketGuildUser.Guild.ToString(), editMessage));
                        _ = message.DeleteAsync();
                    }
                }
            }
            catch (InvalidCastException ex)
            {
                //No need to do anything ... just to provide the bot from spaming the console
            }

        }

        private async Task HandleInteractionCreated(SocketInteraction interaction)
        {
            switch (interaction.Type) // We want to check the type of this interaction
            {
                case InteractionType.ApplicationCommand: // If it is a command
                    _ = Commands.SlashCommands.SlashCommandHandler(interaction, _client); // Handle the command somewhere
                    break;
                case InteractionType.MessageComponent:
                    _ = Commands.MessageComponent.MessageComponentHandler(interaction, _client);
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
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Channel: '{channel.Id}' was succesfully deleted");

                }
            }
            _ = Task.CompletedTask;
        }

        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            _ = TempVoiceChannel.TempVoiceChannel.VoiceChannelActions(user, oldVoice, newVoice, _client);
        }

        private async Task HandleLeftGuild(SocketGuild guild)
        {
            //_ = top.gg.UpdateBot.Update(_bot, _client.Guilds.Count);
            _ = DBStuff.DBFactory.DeleteEverythingFromGuild(guild.Id.ToString());
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot left the guild: {guild.Name} | ID: {guild.Id}");
        }

        private async Task HandleJoinGuild(SocketGuild guild)
        {
            //_ = top.gg.UpdateBot.Update(_bot, _client.Guilds.Count);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot joined the guild: {guild.Name} | ID: {guild.Id}");
        }

        private async Task ClientReadyAsync()
        {
            _client.Ready -= ClientReadyAsync;
            _ = Program.SetBotStatusAsync(_client);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Client Ready");
        }
        #endregion
    }
}
