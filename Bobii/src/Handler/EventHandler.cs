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
using System.Collections.Generic;
using System.IO;

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
        public static ISocketMessageChannel _dmChannel;
        private SocketTextChannel _joinLeaveLogChannel;
        public static Helper BobiiHelper;
        public static Cache Cache;
        public static SocketTextChannel _consoleChannel;
        public static SocketGuild _bobStyDEGuild;
        public static SocketGuild _developerGuild;
        public static TempChannel.DelayOnDelete _delayOnDelete;
        public TempChannel.VoiceUpdateHandler VoiceUpdatedHandler;
        #endregion

        #region Constructor  
        public HandlingService(IServiceProvider services, InteractionService interactionService)
        {
            _serviceProvider = services;
            _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            _interactionService = interactionService;

            BobiiHelper = new Bobii.Helper();
            Cache = new Bobii.Cache();

            _client.InteractionCreated += HandleInteractionCreated;
            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleMessageReceived;
            _client.SelectMenuExecuted += SelectionMenuExecuted;
            _client.LeftGuild += HandleLeftGuild;
            _client.JoinedGuild += HandleJoinGuild;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
            _client.ChannelDestroyed += HandleChannelDestroyed;
            _client.UserLeft += HandleUserLeftGuild;
            _client.ModalSubmitted += HandleModalSubmitted;

            BobiiHelper.WriteConsoleEventHandler += HandleWriteToConsole;
        }
        #endregion

        #region Tasks
        public async Task HandleModalSubmitted(SocketModal modal)
        {
            await ModalHandler.HandleModal(modal, _client);
        }

        public async Task HandleWriteToConsole(object src, Bobii.EventArg.WriteConsoleEventArg eventArg)
        {
            await _consoleChannel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(_bobStyDEGuild, eventArg.Message.Remove(0, 9), error: eventArg.Error).Result);
        }

        private async Task HandleUserLeftGuild(SocketGuild guild, SocketUser user)
        {
            if (FilterLink.EntityFramework.FilterLinkUserGuildHelper.IsUserOnWhitelistInGuild(guild.Id, user.Id).Result)
            {
                await FilterLink.EntityFramework.FilterLinkUserGuildHelper.RemoveWhiteListUserFromGuild(guild.Id, user.Id);
            }
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            _ = Task.Run(async () => MessageReceivedHandler.FilterMessageHandler(message, _client, _dmChannel));
        }

        private async Task SelectionMenuExecuted(SocketMessageComponent component)
        {

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

            //switch (interaction.Type)
            //{
            //    case InteractionType.ApplicationCommand:
            //        await SlashCommandHandlingService.SlashCommandHandler(interaction, _client);
            //        break;
            //    case InteractionType.ApplicationCommandAutocomplete:
            //        await AutocompletionHandlingService.HandleAutocompletion((SocketAutocompleteInteraction)interaction);
            //        break;
            //    case InteractionType.MessageComponent:
            //        await MessageComponentHandlingService.MessageComponentHandler(interaction, _client);
            //        break;
            //    default: // We dont support it
            //        Console.WriteLine("Unsupported interaction type: " + interaction.Type);
            //        break;
            //}
        }

        private async Task HandleChannelDestroyed(SocketChannel channel)
        {
            //Temp Channels
            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(channel.Id).Result;

            if (tempChannel != null)
            {
                if (tempChannel.textchannelid != 0)
                {
                    var textChannel = (SocketTextChannel)_client.GetChannel(tempChannel.textchannelid.Value);
                    if (textChannel != null)
                    {
                        _ = textChannel.DeleteAsync();
                    }
                }
                _ = TempChannel.EntityFramework.TempChannelsHelper.RemoveTC(0, channel.Id);
            }

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
            await TempChannel.VoiceUpdateHandler.HandleVoiceUpdated(oldVoice, newVoice, user, _client, _delayOnDelete);
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
        }

        public async Task AddGuildCommandsToMainGuild()
        {
            try
            {
                // TODO
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

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

            using (StringReader reader = new StringReader(Bobii.Helper.CreateServerCount(_client).Result))
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

        public async Task AddGobalCommandsAsync()
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

                }
                else
                {
                    //await _interactionService.RegisterCommandsGloballyAsync(true);
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<CreateTempChannelSlashCommands>());
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<TempChannelSlashCommands>());
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<HelpShlashCommands>());
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<TextUtilitySlashCommands>());
                    //await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<StealEmojiSlashCommands>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task BescheidGebenUeberEnde()
        {
            var noListe = new List<ulong>();
            noListe.Add(953687533848498198);
            noListe.Add(926398364457775184);
            noListe.Add(850721153634271243);
            noListe.Add(884506177155780708);
            noListe.Add(935928750573514782);
            noListe.Add(732244366210498642);
            noListe.Add(517074117351178255);
            noListe.Add(816502684185657374);
            noListe.Add(464877296742825986);
            noListe.Add(224172713323921409);
            noListe.Add(307276150449766402);
            noListe.Add(752877780005552208);
            noListe.Add(815171865436880897);
            noListe.Add(379902138874200066);
            noListe.Add(565982485825978386);
            noListe.Add(524300063552307223);
            noListe.Add(712502173535174667);
            noListe.Add(330242912308822022);
            noListe.Add(432900576812990475);
            noListe.Add(920780577467150356);

            var liste = new List<ulong>();
            foreach (var guild in _client.Guilds)
            {
                if (noListe.Contains(guild.OwnerId))
                {
                    continue;
                }

                if (liste.Contains(guild.OwnerId))
                {
                    await guild.Owner.SendMessageAsync($"_The same applies for your server {guild.Name}!_");
                    continue;
                }

                await guild.Owner.SendMessageAsync($@"Hello {guild.Owner.Username} :>
Im currently part of your server '{guild.Name}'.

I'm sorry to inform you that I will **leave your server** by the end of the week **(Friday).**

- BobSty
Unfortunately I don't have the time to take care of Bobii and to develop him further. Furthermore I don't see any sense to pay for the running server costs for such a big server. Therefore I will remove Bobii from your server on Friday and take it to a smaller server with less capacity afterwards.
_Only a few selected servers will be able to use Bobii afterwards, if you want your server to be one of those, feel free to answer to this message in the chat below._

**Thank you for using Bobii!**
It has been fun to watch Bobii grow but the time has come to end the project for the public.

");
                liste.Add(guild.OwnerId);
                Console.WriteLine($"Der Nutzer {guild.Owner.Username} weiß bescheid ({guild.Name})");
            }
        }

        private async Task ClientReadyAsync()
        {
            _bobStyDEGuild = _client.GetGuild(Helper.ReadBobiiConfig(ConfigKeys.MainGuildID).ToUlong());
            _developerGuild = _client.GetGuild(Helper.ReadBobiiConfig(ConfigKeys.DeveloperGuildID).ToUlong());

            await InitializeInteractionModules();

            //await AddGobalCommandsAsync();
            //await AddGuildCommandsToMainGuild();

            _client.Ready -= ClientReadyAsync;
            VoiceUpdatedHandler = new TempChannel.VoiceUpdateHandler();
            var bobiiSupportServerGuild = _client.GetGuild(Helper.ReadBobiiConfig(ConfigKeys.SupportGuildID).ToUlong());

            _serverCountChannelBobii = bobiiSupportServerGuild.GetChannel(Helper.ReadBobiiConfig(ConfigKeys.SupportGuildCountChannelID).ToUlong());
            _serverCountChannelBobStyDE = _bobStyDEGuild.GetChannel(Helper.ReadBobiiConfig(ConfigKeys.MainGuildCountChannelID).ToUlong());
            _joinLeaveLogChannel = _bobStyDEGuild.GetTextChannel(Helper.ReadBobiiConfig(ConfigKeys.JoinLeaveLogChannelID).ToUlong());
            _dmChannel = _bobStyDEGuild.GetTextChannel(Helper.ReadBobiiConfig(ConfigKeys.DMChannelID).ToUlong());
            _consoleChannel = _bobStyDEGuild.GetTextChannel(Helper.ReadBobiiConfig(ConfigKeys.ConsoleChannelID).ToUlong());

            Cache.Captions = Bobii.EntityFramework.BobiiHelper.GetCaptions().Result;
            Cache.Contents = Bobii.EntityFramework.BobiiHelper.GetContents().Result;
            Cache.Commands = Bobii.EntityFramework.BobiiHelper.GetCommands().Result;

            _delayOnDelete = new TempChannel.DelayOnDelete();

            await _delayOnDelete.InitializeDelayDelete(_client);
            await TempChannel.Helper.CheckAndDeleteEmptyVoiceChannels(_client);


            _ = Task.Run(async () => RefreshServerCountChannels());
            await Program.SetBotStatusAsync(_client);

            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Client Ready");

            //_ = ServerCount();
            _ = BescheidGebenUeberEnde();
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
