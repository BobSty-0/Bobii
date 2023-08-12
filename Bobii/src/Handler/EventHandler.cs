using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord;
using System.Data;
using System.Linq;
using Bobii.src.Bobii;
using Discord.Interactions;
using Bobii.src.InteractionModules.Slashcommands;
using Bobii.src.InteractionModules.ModalInteractions;
using Bobii.src.InteractionModules.ComponentInteractions;
using System.IO;
using System.Collections.Generic;
using Discord.Rest;
using Bobii.src.Helper;
using src.InteractionModules.Slashcommands;
using Bobii.src.EventArg;
using Bobii.src.Bobii.EntityFramework;

namespace Bobii.src.Handler
{
    public class HandlingService
    {
        #region Declarations 
        public static DiscordSocketClient _client;
        public static InteractionService _interactionService;
        public static IServiceProvider _serviceProvider;

        public static SocketGuildChannel _serverCountChannelBobStyDE;
        public static SocketGuildChannel _serverCountChannelBobii;
        public static SocketForumChannel _dmChannel;
        private SocketTextChannel _joinLeaveLogChannel;
        public static GeneralHelper BobiiHelper;
        public static Cache Cache;
        public static SocketTextChannel _consoleChannel;
        public static SocketGuild _bobStyDEGuild;
        public static SocketGuild _developerGuild;
        public static SocketGuild _supportGuild;
        public static TempChannel.DelayOnDelete _delayOnDelete;
        public TempChannel.VoiceUpdateHandler VoiceUpdatedHandler;
        public static Dictionary<IUser, RestThreadChannel> _dmThreads;
        public static RestWebhook _webhookClient;
        #endregion

        #region Constructor  
        public HandlingService(IServiceProvider services, InteractionService interactionService)
        {
            _serviceProvider = services;
            _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            _interactionService = interactionService;

            BobiiHelper = new GeneralHelper();
            Cache = new Cache();

            _client.InteractionCreated += HandleInteractionCreated;
            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleMessageReceived;
            _client.LeftGuild += HandleLeftGuild;
            _client.JoinedGuild += HandleJoinGuild;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
            _client.ChannelDestroyed += HandleChannelDestroyed;
            _client.ModalSubmitted += HandleModalSubmitted;
            _client.UserIsTyping += HandleUserIsTyping;

            BobiiHelper.WriteConsoleEventHandler += HandleWriteToConsole;
        }
        #endregion

        #region Tasks
        public async Task HandleUserIsTyping(Cacheable<IUser, ulong> iUser, Cacheable<IMessageChannel, ulong> iMessageChannel)
        {
            try
            {
                IUser user = iUser.DownloadAsync().Result;
                if (user.IsBot)
                {
                    return;
                }

                IMessageChannel channel = iMessageChannel.DownloadAsync().Result;

                _dmThreads.TryGetValue(user, out RestThreadChannel thread);
                if (thread != null && channel.GetType() == typeof(RestDMChannel))
                {
                    _ = thread.TriggerTypingAsync();
                    return;
                }

                if (!_dmThreads.Any(x => x.Value.Id == channel.Id))
                {
                    return;
                }

                var currentThread = MessageReceivedHandler.GetCurrentThread(channel.Id, _client, _dmChannel);
                if (currentThread != null && channel.Id == currentThread.Result.Id)
                {
                    var dm = user.CreateDMChannelAsync().Result;
                    _ = dm.TriggerTypingAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task HandleModalSubmitted(SocketModal modal)
        {
            await ModalHandler.HandleModal(modal, _client);
        }

        public async Task HandleWriteToConsole(object src, WriteConsoleEventArg eventArg)
        {
            await _consoleChannel.SendMessageAsync(embed: GeneralHelper.CreateEmbed(_bobStyDEGuild, eventArg.Message.Remove(0, 10), error: eventArg.Error).Result);
        }

        private async Task HandleMessageReceived(IMessage message)
        {
            await Task.Run(async () => MessageReceivedHandler.HandleMassage(message, _client, _dmChannel, _webhookClient));
            // Wenn potentiell ein neuer dm channel hinzugefügt wurde, dann müssen die dmThreads aktuallisiert werden
            if (DMSupportHelper.IsPrivateMessage((SocketMessage)message).Result)
            {
                _dmThreads = GetAllDMThreads(_dmChannel).Result;
            }
        }

        private async Task HandleInteractionCreated(SocketInteraction interaction)
        {
            try
            {
                if (InteractionType.MessageComponent == interaction.Type)
                {
                    await MessageComponentHandlingService.MessageComponentHandler(interaction, _client);
                    return;
                }

                var context = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task HandleChannelDestroyed(SocketChannel channel)
        {
            //Temp Channels
            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(channel.Id).Result;

            if (tempChannel != null)
            {
                _ = TempChannel.EntityFramework.TempChannelsHelper.RemoveTC(0, channel.Id);
                var createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannel(tempChannel.createchannelid.Value).Result;
                if (createTempChannel.tempchannelname.Contains("{count}"))
                {
                    _ = TempChannelHelper.SortCountNeu(createTempChannel, _client);
                }
            }

            //Create Temp Channels
            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelList()
                .Result.Where(ch => ch.createchannelid == channel.Id)
                .FirstOrDefault();

            if (createTempChannels != null)
            {
                _ = TempChannel.EntityFramework.CreateTempChannelsHelper.RemoveCC("No Guild supplyed", channel.Id);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Channel: '{channel.Id}' was successfully deleted");
            }

            await Task.CompletedTask;
        }

        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            await TempChannel.VoiceUpdateHandler.HandleVoiceUpdated(oldVoice, newVoice, user, _client, _delayOnDelete);
        }


        private async Task HandleLeftGuild(SocketGuild guild)
        {
            _ = Task.Run(async () => RefreshServerCountChannels());
            _ = _joinLeaveLogChannel.SendMessageAsync(null, false, GeneralHelper.CreateEmbed(_joinLeaveLogChannel.Guild, $"**Membercount:** {guild.MemberCount}", $"I left: {guild.Name}").Result);
            _ = Bobii.EntityFramework.BobiiHelper.DeleteEverythingFromGuild(guild);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot left the guild: {guild.Name} | ID: {guild.Id}");
        }

        private async Task HandleJoinGuild(SocketGuild guild)
        {
            _ = Task.Run(async () => RefreshServerCountChannels());
            var owner = _client.Rest.GetUserAsync(guild.OwnerId).Result;
            await _joinLeaveLogChannel.SendMessageAsync(null, false, GeneralHelper.CreateEmbed(_joinLeaveLogChannel.Guild, $"**Owner ID:** {guild.OwnerId}\n**Owner Name:** {owner}\n**Membercount:** {guild.MemberCount}", $"I joined: {guild.Name}").Result);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot joined the guild: {guild.Name} | ID: {guild.Id}");
            var test = guild.GetAuditLogsAsync(limit: 100, actionType: ActionType.BotAdded).FlattenAsync().Result;
        }

        private async Task InitializeInteractionModules()
        {
            // Tempchannel
            await _interactionService.AddModuleAsync<CreateTempChannelSlashCommands>(_serviceProvider);
            await _interactionService.AddModuleAsync<TempChannelModalInteractions>(_serviceProvider);
            await _interactionService.AddModuleAsync<TempChannelSlashCommands>(_serviceProvider);

            // Help
            await _interactionService.AddModuleAsync<HelpShlashCommands>(_serviceProvider);
            await _interactionService.AddModuleAsync<HelpSelectionMenus>(_serviceProvider);

            // Text Utility
            await _interactionService.AddModuleAsync<TextUtilitySlashCommands>(_serviceProvider);
            await _interactionService.AddModuleAsync<StealEmojiSlashCommands>(_serviceProvider);

            // Language
            await _interactionService.AddModuleAsync<LanguageShlashCommands>(_serviceProvider);
        }

        public async Task<Dictionary<IUser, RestThreadChannel>> GetAllDMThreads(SocketForumChannel forumChannel)
        {
            var dict = new Dictionary<IUser, RestThreadChannel>();
            var threads = forumChannel.GetAllThreads().Result;

            foreach (var thread in threads)
            {
                if (!ulong.TryParse(thread.Name, out ulong _))
                {
                    continue;
                }

                dict.Add(_client.GetUserAsync(thread.Name.ToUlong()).Result, thread);
            }

            return dict;
        }

        public static async Task ServerCount()
        {
            var path = $"Servercount_{DateTime.Now}.md";
            path = path.Replace(' ', '_');
            path = path.Replace(':', '.');
            path = path.Replace('/', '.');

            using (FileStream fs = File.Create(path))
            {
                path = fs.Name;
            }

            using (StringReader reader = new StringReader(GeneralHelper.CreateServerCount(_client).Result))
            {
                using (var tw = new StreamWriter(path, true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        tw.WriteLine(line);
                    }
                }
            }

            await _consoleChannel.SendMessageAsync("Here is the server list:");
            await _consoleChannel.SendFileAsync(path, "");

            File.Delete(path);
        }

        public async Task AddGlobalCommandsAsync()
        {
            try
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // TODO hier noch die commands in die richtige Reihenfolge bringen
                    //await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<HelpShlashCommands>());
                    //await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<CreateTempChannelSlashCommands>());
                    //await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<TempChannelSlashCommands>());
                    //await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<TextUtilitySlashCommands>());
                    //await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<StealEmojiSlashCommands>());
                    //await _interactionService.AddModulesToGuildAsync(_bobStyDEGuild, false, _interactionService.GetModuleInfo<StealEmojiSlashCommands>());
                    //await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<LanguageShlashCommands>());

                }
                else
                {
                    //await _interactionService.RegisterCommandsGloballyAsync(true);
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<CreateTempChannelSlashCommands>());
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<TempChannelSlashCommands>());
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<HelpShlashCommands>());
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<TextUtilitySlashCommands>());
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<StealEmojiSlashCommands>());
                    await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<LanguageShlashCommands>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ClientReadyAsync()
        {
            _bobStyDEGuild = _client.GetGuild(GeneralHelper.GetConfigKeyValue(ConfigKeys.MainGuildID).ToUlong());
            _developerGuild = _client.GetGuild(GeneralHelper.GetConfigKeyValue(ConfigKeys.DeveloperGuildID).ToUlong());
            _supportGuild = _client.GetGuild(GeneralHelper.GetConfigKeyValue(ConfigKeys.SupportGuildID).ToUlong());

            await InitializeInteractionModules();

            await AddGlobalCommandsAsync();
            //await AddGuildCommandsToMainGuild();

            _client.Ready -= ClientReadyAsync;
            VoiceUpdatedHandler = new TempChannel.VoiceUpdateHandler();

            _serverCountChannelBobii = _supportGuild.GetChannel(GeneralHelper.GetConfigKeyValue(ConfigKeys.SupportGuildCountChannelID).ToUlong());
            _serverCountChannelBobStyDE = _bobStyDEGuild.GetChannel(GeneralHelper.GetConfigKeyValue(ConfigKeys.MainGuildCountChannelID).ToUlong());
            _joinLeaveLogChannel = _supportGuild.GetTextChannel(GeneralHelper.GetConfigKeyValue(ConfigKeys.JoinLeaveLogChannelID).ToUlong());
            _dmChannel = _supportGuild.GetForumChannel(GeneralHelper.GetConfigKeyValue(ConfigKeys.DMChannelID).ToUlong());
            _consoleChannel = _supportGuild.GetTextChannel(GeneralHelper.GetConfigKeyValue(ConfigKeys.ConsoleChannelID).ToUlong());
            _dmThreads = GetAllDMThreads(_dmChannel).Result;
            //_webhookClient = ((RestTextChannel)_client.Rest.GetChannelAsync(910868343030960129).Result).CreateWebhookAsync("test").Result;

            Cache.Captions = Bobii.EntityFramework.BobiiHelper.GetCaptions().Result;
            Cache.Contents = Bobii.EntityFramework.BobiiHelper.GetContents().Result;
            Cache.Commands = Bobii.EntityFramework.BobiiHelper.GetCommands().Result;

            _delayOnDelete = new TempChannel.DelayOnDelete();

            await _delayOnDelete.InitializeDelayDelete(_client);
            await TempChannelHelper.CheckAndDeleteEmptyVoiceChannels(_client);


            _ = Task.Run(async () => RefreshServerCountChannels());
            await Program.SetBotStatusAsync(_client);

            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Client Ready");
        }

        /// <summary>
        /// Resets the Cache
        /// </summary>
        /// <returns></returns>
        public static async Task ResetCache()
        {
            Cache.Captions = Bobii.EntityFramework.BobiiHelper.GetCaptions().Result;
            Cache.Contents = Bobii.EntityFramework.BobiiHelper.GetContents().Result;
            Cache.Commands = Bobii.EntityFramework.BobiiHelper.GetCommands().Result;
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
