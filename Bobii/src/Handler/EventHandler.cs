using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace Bobii.src.Handler
{
    public class HandlingService
    {
        #region Declarations 
        public static DiscordSocketClient _client;
        public static SocketGuildChannel _serverCountChannelBobStyDE;
        public static SocketGuildChannel _serverCountChannelBobii;
        public static ISocketMessageChannel _dmChannel;
        private SocketTextChannel _joinLeaveLogChannel;
        public static Bobii.Helper _bobiiHelper;
        public static Bobii.Cache _cache;
        public static SocketTextChannel _consoleChannel;
        public static SocketGuild _bobStyDEGuild;
        public static List<SocketUser> _cooldownList;
        #endregion

        #region Constructor  
        public HandlingService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _cooldownList = new List<SocketUser>();
            _bobiiHelper = new Bobii.Helper();
            _cache = new Bobii.Cache();

            _client.InteractionCreated += HandleInteractionCreated;
            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleMessageReceived;
            _client.LeftGuild += HandleLeftGuild;
            _client.JoinedGuild += HandleJoinGuild;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
            _client.ChannelDestroyed += HandleChannelDestroyed;
            _client.UserLeft += HandleUserLeftGuild;

            _bobiiHelper.WriteConsoleEventHandler += HandleWriteToConsole;
        }
        #endregion

        #region Tasks
        public async Task HandleWriteToConsole(object src, Bobii.EventArg.WriteConsoleEventArg eventArg)
        {
            await _consoleChannel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(_bobStyDEGuild, eventArg.Message.Remove(0, 9), error: eventArg.Error).Result);
        }

        private async Task HandleUserLeftGuild(SocketGuildUser user)
        {
            if (FilterLink.EntityFramework.FilterLinkUserGuildHelper.IsUserOnWhitelistInGuild(user.Guild.Id, user.Id).Result)
            {
                await FilterLink.EntityFramework.FilterLinkUserGuildHelper.RemoveWhiteListUserFromGuild(user.Guild.Id, user.Id);
            }
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            _ = Task.Run(async () => MessageReceivedHandler.FilterMessageHandler(message, _client, _dmChannel));
        }

        private async Task HandleInteractionCreated(SocketInteraction interaction)
        {
            switch (interaction.Type)
            {
                case InteractionType.ApplicationCommand:
                    await SlashCommandHandlingService.SlashCommandHandler(interaction, _client);
                    break;
                case InteractionType.ApplicationCommandAutocomplete:
                    await AutocompletionHandlingService.HandleAutocompletion((SocketAutocompleteInteraction)interaction);
                    break;
                case InteractionType.MessageComponent:
                    await MessageComponentHandlingService.MessageComponentHandler(interaction, _client);
                    break;
                default: // We dont support it
                    Console.WriteLine("Unsupported interaction type: " + interaction.Type);
                    break;
            }
        }

        private async Task HandleChannelDestroyed(SocketChannel channel)
        {
            //Create Temp Channels
            var createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelList()
                .Result.Where(ch => ch.createchannelid == channel.Id)
                .FirstOrDefault();

            if (createTempChannel != null)
            {
                _ = TempChannel.EntityFramework.CreateTempChannelsHelper.RemoveCC("No Guild supplyed", channel.Id);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Channel: '{channel.Id}' was successfully deleted");
            }

            //FilterLinkLogs
            var filterLinkLog = FilterLink.EntityFramework.FilterLinkLogsHelper.GetFilterLinkLogChannels()
                .Result
                .Where(ch => ch.channelid == channel.Id)
                .FirstOrDefault();

            if (filterLinkLog != null)
            {
                var guildChannel = (SocketGuildChannel)channel;
                _ = FilterLink.EntityFramework.FilterLinkLogsHelper.RemoveFilterLinkLogChannel(guildChannel.Guild.Id);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Channel: '{channel.Id}' was successfully deleted");
            }

            await Task.CompletedTask;
        }

        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            await TempChannelHandler.VoiceChannelActions(user, oldVoice, newVoice, _client, _cooldownList);
            if (newVoice.VoiceChannel != null)
            {
                if (!_cooldownList.Contains(user))
                {
                    _cooldownList.Add(user);
                    _ = RemoveUserFromCoolDownList(user);
                }
            }
        }

        private async Task RemoveUserFromCoolDownList(SocketUser user)
        {
            try
            {
                await Task.Delay(3000);
                if (_cooldownList.Contains(user))
                {
                    _cooldownList.Remove(user);
                }
            }
            catch
            {
                // Do nothing
            }
        }

        private async Task HandleLeftGuild(SocketGuild guild)
        {
            _ = Task.Run(async () => RefreshServerCountChannels());
            _ = _joinLeaveLogChannel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(_joinLeaveLogChannel.Guild, $"**Membercount:** {guild.MemberCount}", $"I left: {guild.Name}").Result);
            _ = Bobii.EntityFramework.BobiiHelper.DeleteEverythingFromGuild(guild);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot left the guild: {guild.Name} | ID: {guild.Id}");
        }

        private async Task HandleJoinGuild(SocketGuild guild)
        {
            _ = Task.Run(async () => RefreshServerCountChannels());
            var owner = _client.Rest.GetUserAsync(guild.OwnerId).Result;
            await _joinLeaveLogChannel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(_joinLeaveLogChannel.Guild, $"**Owner ID:** {guild.OwnerId}\n**Owner Name:** {owner}\n**Membercount:** {guild.MemberCount}", $"I joined: {guild.Name}").Result);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot joined the guild: {guild.Name} | ID: {guild.Id}");
            var test = guild.GetAuditLogsAsync(limit: 100, actionType: ActionType.BotAdded).FlattenAsync().Result;
        }

        private async Task ClientReadyAsync()
        {
            _client.Ready -= ClientReadyAsync;
            _bobStyDEGuild = _client.GetGuild(712373862179930144);
            var bobiiGuild = _client.GetGuild(908075925810335794);

            _serverCountChannelBobii = bobiiGuild.GetChannel(911621554180333629);
            _serverCountChannelBobStyDE = _bobStyDEGuild.GetChannel(876523329048182785);
            _joinLeaveLogChannel = _bobStyDEGuild.GetTextChannel(878209146850263051);

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                _dmChannel = _bobStyDEGuild.GetTextChannel(892460268473446490);
                _consoleChannel = _bobStyDEGuild.GetTextChannel(917825660959993906);
            }
            else
            {
                _dmChannel = _bobStyDEGuild.GetTextChannel(918556493849169981);
                _consoleChannel = _bobStyDEGuild.GetTextChannel(917825775808421898);
            }
            _cache.Captions = Bobii.EntityFramework.BobiiHelper.GetCaptions().Result;
            _cache.Contents = Bobii.EntityFramework.BobiiHelper.GetContents().Result;
            _cache.Commands = Bobii.EntityFramework.BobiiHelper.GetCommands().Result;

            _ = Task.Run(async () => RefreshServerCountChannels());
            await Program.SetBotStatusAsync(_client);

            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Client Ready");
        }

        /// <summary>
        /// Resets the Content and Caption Cache
        /// </summary>
        /// <returns></returns>
        public static async Task ResetCache()
        {
            _cache.Captions = Bobii.EntityFramework.BobiiHelper.GetCaptions().Result;
            _cache.Contents = Bobii.EntityFramework.BobiiHelper.GetContents().Result;
            _cache.Commands = Bobii.EntityFramework.BobiiHelper.GetCommands().Result;
            await Task.CompletedTask;
        }

        public static async Task RefreshServerCountChannels()
        {
            try
            {
                if (!System.Diagnostics.Debugger.IsAttached)
                {
                    await _serverCountChannelBobStyDE.ModifyAsync(channel => channel.Name = $"Server count: {_client.Guilds.Count}");
                    await _serverCountChannelBobii.ModifyAsync(channel => channel.Name = $"Server count: {_client.Guilds.Count}");
                }
            }
            catch (Exception)
            {
                //Do nothing because sometimes it cant do it ... This is not an important Task anyways
            }
        }
        #endregion
    }
}
