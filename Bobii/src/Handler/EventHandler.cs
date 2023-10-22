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
using System.Reflection.Metadata;
using System.Text;
using Bobii.src.TempChannel.EntityFramework;
using Bobii.src.TempChannel;
using System.Data.Common;
using TwitchLib.Client.Events;

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
        public static AutoDeleteDateWrapper AutoDeleteWrapper;
        public TempChannel.VoiceUpdateHandler VoiceUpdatedHandler;
        public static Dictionary<IUser, RestThreadChannel> _dmThreads;
        public static RestWebhook _webhookClient;

        public static bool DontReact;
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
            //_client.UserIsTyping += HandleUserIsTyping;
            _client.UserJoined += HandleUserJoined;
            _client.UserLeft += HandleUserLeft;
            _client.MessageDeleted += HandleMessageDeleted;

            BobiiHelper.WriteConsoleEventHandler += HandleWriteToConsole;
            DontReact = true;
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Dont react mode aktiviert");
        }
        #endregion

        #region Tasks
        public async Task HandleUserLeft(SocketGuild guild, SocketUser user)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() =>
            {
                _ = UsedFunctionsHelper.RemoveBlockedUsersFromUser(guild.Id, user.Id);
                _ = UsedFunctionsHelper.RemoveWhitelistedUsersFromUser(guild.Id, user.Id);
                _ = UsedFunctionsHelper.RemoveUsedFunctionsFromModerator(user.Id, guild.Id);
                _ = UsedFunctionsHelper.RemoveUsedFunctionsModerator(user.Id, guild.Id);
            });
        }
        public async Task HandleUserJoined(SocketGuildUser user)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() => TempChannelHelper.BlockUserFormBannedVoiceAfterJoining(user));
        }


        public async Task HandleModalSubmitted(SocketModal modal)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() => ModalHandler.HandleModal(modal, _client));
        }

        public async Task HandleWriteToConsole(object src, WriteConsoleEventArg eventArg)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() => _consoleChannel.SendMessageAsync(embed: GeneralHelper.CreateEmbed(_bobStyDEGuild, eventArg.Message.Remove(0, 10), error: eventArg.Error).Result));
        }

        private async Task HandleMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() => AutoDeleteWrapper.RemoveMessageToBeDetletedIfOnDict(message.Id));
        }

        private async Task HandleMessageReceived(IMessage message)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(async () => MessageReceivedHandler.HandleMassage(message, _client, _dmChannel, _webhookClient, AutoDeleteWrapper));
            // Wenn potentiell ein neuer dm channel hinzugefügt wurde, dann müssen die dmThreads aktuallisiert werden
            _ = Task.Run(() =>
            {
                if (DMSupportHelper.IsPrivateMessage((SocketMessage)message).Result)
                {
                    _dmThreads = GetAllDMThreads(_dmChannel).Result;
                }
            });
        }

        private async Task HandleInteractionCreated(SocketInteraction interaction)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() =>
            {
                try
                {
                    if (InteractionType.MessageComponent == interaction.Type)
                    {
                        MessageComponentHandlingService.MessageComponentHandler(interaction, _client);
                        return;
                    }

                    var context = new SocketInteractionContext(_client, interaction);
                    _interactionService.ExecuteCommandAsync(context, _serviceProvider);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        private async Task HandleChannelDestroyed(SocketChannel channel)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() =>
            {
                //Temp Channels
                var tempChannel = TempChannelsHelper.GetTempChannel(channel.Id).Result;

                if (tempChannel != null)
                {
                    _ = TempChannelsHelper.RemoveTC(0, channel.Id);
                    if (tempChannel.autoscale)
                    {
                        var category = AutoScaleCategoriesHelper.GetAutoScaleCategory(tempChannel.autoscalercategoryid.Value).Result;

                        if (category.channelname.Contains("{count}"))
                        {
                            var autoScaleChannels = TempChannelsHelper.GetTempChannelList(true).Result
                            .Where(a => a.autoscalercategoryid.Value == category.categoryid)
                            .OrderBy(channel => channel.count)
                            .AsEnumerable();
                            _ = TempChannelHelper.SortCountNeu(category.channelname, _client, autoScaleChannels);
                        }
                    }
                    else
                    {
                        var createTempChannel = CreateTempChannelsHelper.GetCreateTempChannel(tempChannel.createchannelid.Value).Result;
                        if (createTempChannel.tempchannelname.Contains("{count}"))
                        {
                            var tempChannelsFromGuild = TempChannelsHelper.GetTempChannelList().Result
                                .Where(channel => channel.createchannelid == createTempChannel.createchannelid)
                                .OrderBy(channel => channel.count)
                                .AsEnumerable();
                            _ = TempChannelHelper.SortCountNeu(createTempChannel.tempchannelname, _client, tempChannelsFromGuild);
                        }

                    }


                }

                //Create Temp Channels
                var createTempChannels = CreateTempChannelsHelper.GetCreateTempChannelList()
                    .Result.Where(ch => ch.createchannelid == channel.Id)
                    .FirstOrDefault();

                if (createTempChannels != null)
                {
                    _ = CreateTempChannelsHelper.RemoveCC("No Guild supplyed", channel.Id);
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Channel: '{channel.Id}' was successfully deleted");
                }

                // Auto Scaling Categories
                var autoScalingCateogy = AutoScaleCategoriesHelper.GetAutoScaleCategory(channel.Id).Result;

                if (autoScalingCateogy != null)
                {
                    _ = AutoScaleCategoriesHelper.RemoveAutoScraeCategory("No Guild supplyed", channel.Id);
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Channel: '{channel.Id}' was successfully deleted");
                }
            });
        }

        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() => VoiceUpdateHandler.HandleVoiceUpdated(oldVoice, newVoice, user, _client, _delayOnDelete));
        }


        private async Task HandleLeftGuild(SocketGuild guild)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(() =>
            {
                RefreshServerCountChannels();
                _ = _joinLeaveLogChannel.SendMessageAsync(null, false, GeneralHelper.CreateFehlerEmbed(_joinLeaveLogChannel.Guild, $"**Membercount:** {guild.MemberCount}", $"I left: {guild.Name}").Result);
                _ = Bobii.EntityFramework.BobiiHelper.DeleteEverythingFromGuild(guild);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot left the guild: {guild.Name} | ID: {guild.Id}");
            });
        }

        private async Task HandleJoinGuild(SocketGuild guild)
        {
            if (DontReact)
            {
                return;
            }
            _ = Task.Run(async () =>
            {
                var owner = _client.Rest.GetUserAsync(guild.OwnerId).Result;
                await _joinLeaveLogChannel.SendMessageAsync(null, false, GeneralHelper.CreateEmbed(_joinLeaveLogChannel.Guild, $"**Owner ID:** {guild.OwnerId}\n**Owner Name:** {owner}\n**Membercount:** {guild.MemberCount}", $"I joined: {guild.Name}").Result);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot joined the guild: {guild.Name} | ID: {guild.Id}");
                try
                {
                    var voiceChannels = guild.VoiceChannels;
                    var bot = guild.GetUser(GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationID).ToUlong());
                    var channel = guild.TextChannels
                        .OrderBy(c => c.Position)
                        .Where(c => !guild.VoiceChannels.Select(v => v.Id).Contains(c.Id) && GeneralHelper.CanWriteInChannel(c, bot)).First();

                    await channel.SendMessageAsync(embeds: new Embed[] { GeneralHelper.GetWelcomeEmbed(guild) }, components: GeneralHelper.GetSupportButtonComponentBuilder().Build());
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(HandleJoinGuild),
                            message: $"Welcome message erfolgreich gesendet - {guild.Name}");
                }
                catch (Exception ex)
                {
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(HandleJoinGuild),
                        message: $"Welcome message nicht erfolgreich gesendet - {guild.Name}", exceptionMessage: ex.Message);
                    //nothing, just dont crash
                }

                await RefreshServerCountChannels();
            });
        }



        private async Task InitializeInteractionModules()
        {
            // Tempchannel
            await _interactionService.AddModuleAsync<CreateTempChannelSlashCommands>(_serviceProvider);
            await _interactionService.AddModuleAsync<TempChannelModalInteractions>(_serviceProvider);
            await _interactionService.AddModuleAsync<TempChannelSlashCommands>(_serviceProvider);

            // Help
            await _interactionService.AddModuleAsync<HelpShlashCommands>(_serviceProvider);
            await _interactionService.AddModuleAsync<SelectionMenus>(_serviceProvider);

            // Text Utility
            await _interactionService.AddModuleAsync<TextUtilitySlashCommands>(_serviceProvider);
            await _interactionService.AddModuleAsync<StealEmojiSlashCommands>(_serviceProvider);

            // Language
            await _interactionService.AddModuleAsync<LanguageShlashCommands>(_serviceProvider);

            // UpdateMode
            await _interactionService.AddModuleAsync<SetUpdateModeSlashCommand>(_serviceProvider);

            // AutoScale
            await _interactionService.AddModuleAsync<AutoScaleVoiceChannelCommands>(_serviceProvider);
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
                //await _interactionService.RegisterCommandsGloballyAsync(true);

                // await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<CreateTempChannelSlashCommands>());
                //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<TempChannelSlashCommands>());
                //await _interactionService.AddModulesToGuildAsync(_supportGuild, false, _interactionService.GetModuleInfo<SetUpdateModeSlashCommand>());

                // await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<HelpShlashCommands>());
                //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<TextUtilitySlashCommands>());
                // await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<StealEmojiSlashCommands>());
                //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<LanguageShlashCommands>());

                await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<AutoScaleVoiceChannelCommands>());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ClientReadyAsync()
        {
            await ResetCache();

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
            //_webhookClient = ((RestTextChannel)_client.Rest.GetChannelAsync(910868343030960129).Result).CreateWebhookAsync("test").Result;

            _delayOnDelete = new TempChannel.DelayOnDelete();
            AutoDeleteWrapper = new AutoDeleteDateWrapper();

            await _delayOnDelete.InitializeDelayDelete(_client);


            _ = Task.Run(async () => RefreshServerCountChannels());
            await Program.SetBotStatusAsync(_client);

            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Client Ready");

            DontReact = false;
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Dont react mode deaktiviert");

            await TempChannelHelper.CheckAndDeleteEmptyVoiceChannels(_client);
            await TempChannelHelper.CheckAndDeleteEmptyVoiceChannelsAutoScale(_client);
            _ = Task.Run(async () =>
            {
                _dmThreads = GetAllDMThreads(_dmChannel).Result;
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     DM threads loaded");
            });
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
            Cache.ResetTempChannelsCache();
            Cache.ResetCreateTempChannelsCache();
            Cache.ResetTempChanneluserConfigsCache();
            Cache.ResetTempCommandsCache();
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
