using Discord;
using Discord.Commands;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;

        public Commands(CommandService service)
        {
            _commandService = service;
        }

        [Command("help")]
        [Summary("Summary of all my commands")]
        public async Task Help()
        {
            await Context.Message.ReplyAsync("", false, TextChannel.TextChannel.CreateHelpInfo(_commandService, Context.Guild.Id.ToString()));
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'help was used by {Context.User}");
        }
        [Command("vcname")]
        [Summary("Command to edit VoiceChat Name")]
        public async Task ChangeVoiceChatName(string voiceNameNew)
        {
            // TODO JG 01.07.2021
            await Task.CompletedTask;
        }

        [Command("cvcinfo")]
        [Summary("Gives info about the currently set create temp voicechannels")]
        public async Task TempVoiceChannelInof()
        {
            await Context.Message.ReplyAsync("", false, TempVoiceChannel.TempVoiceChannel.CreateVoiceChatInfoEmbed());
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'vcinfo was used by \"{Context.User}\"");
        }

        [Command("voiceadd")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Adds a new create temp voice channel with:\nvoiceadd <VoiceChannelID> <\"NameOfTheCreatedChannel\">\nNote: Admin only!")]
        public async Task AddCreateVoiceChannel(string id, string name = "TempChannel")
        {
            // TODO JG 01.07.2021
            await Task.CompletedTask;

            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(id, out _) && id.Length != 18)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The given ID: '{id}' is not valid!**\nMake sure to copy the ID from the voice channel directly!"));
                return;
            }

            if (DBStuff.createtempchannels.CheckIfCreateVoiceChannelExist(Context.Guild.Id.ToString(), id.ToString()))
            {
                await ReplyAsync( null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The create temp voice channel with the ID: '{id}' already exists!**"));
                return;
            }

            try
            {
                DBStuff.createtempchannels.AddCC(Context.Guild.Id.ToString(), name, id);
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**" + Context.Guild.GetChannel(ulong.Parse(id)).Name + $"** was sucessfully added by **{Context.User}**"));
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Voicechat: \"{Context.Guild.GetChannel(ulong.Parse(id)).Name}\" with the ID: '{id}' was successfully added by '{Context.User}'");
            }
            catch (Exception ex)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**CreateTempChannel could not be added:**\nException: "+ ex.Message));
                throw;
            }

            //TODO JG 18.06.2021 Check if cvc already exists and reply with message! 
            //Also check if I need ReplyAndDeleteMessage
        }

        [Command("switchprefix")]
        [Summary("Can be used to switch the prefix\nNote: max. length = 3 / Admin only! ")]
        public async Task SwitchPrefix(string newPrefix)
        {
            if (newPrefix.Length > 3)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The given prefix has more than 3 characters!**\n\nGiven prefix: **{ newPrefix}**"));
                return;
            }
            if (newPrefix.Contains("'"))
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**At the moment it is not possible to use **'** in the prefix**"));
                return;
            }
            try
            {
                DBStuff.Prefixes.SwitchPrefix(newPrefix, Context.Guild.Id.ToString());
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**Prefix succesfully changed!**\n\nNew Prefix: **{newPrefix}**"));
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Guild: {Context.Guild.Id}\n Was Changed to '{newPrefix}'!");
            }
            catch (Exception ex)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**Prefix could not be changed:**\nException: " + ex.Message));
                throw;
            }


            await Task.CompletedTask;
        }
    }
}
