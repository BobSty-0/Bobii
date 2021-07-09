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

        [Command("tempinfo")]
        [Summary("Returns a list of all CreateTempVoiceCannels with:\n**[prefix]tempinfo**")]
        public async Task TempVoiceChannelInof()
        {
            //await Context.Message.ReplyAsync("", false, TempVoiceChannel.TempVoiceChannel.CreateVoiceChatInfoEmbed(Context.Guild.Id.ToString(), Context.Client));
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'vcinfo was used by \"{Context.User}\"");
        }

        [Command("tempadd")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Adds a new create temp voice channel with:\n**[prefix]tempeadd <VoiceChannelID> <\"TempChannelName\">**\nNote: The word 'User' will be replaced with the one joining the CreateTempChannel")]
        public async Task AddCreateVoiceChannel(string id, string tempChannelName = "User’s Channel")
        {
            tempChannelName = tempChannelName.Replace("'", "’");
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(id, out _) && id.Length != 18)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The given ID: '{id}' is not valid!**\nMake sure to copy the ID from the voice channel directly!"));
                return;
            }

            if (DBStuff.createtempchannels.CheckIfCreateVoiceChannelExist(Context.Guild.Id.ToString(), id.ToString()))
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The create temp voice channel with the ID: '{id}' already exists!**"));
                return;
            }

            if (tempChannelName.Length > 50)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The name **{tempChannelName}** has more than 50 characters, pls make sure the name is shorter than 50 characters !"));
                return;
            }
            try
            {
                DBStuff.createtempchannels.AddCC(Context.Guild.Id.ToString(), tempChannelName, id);
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**" + Context.Guild.GetChannel(ulong.Parse(id)).Name + $"** was sucessfully added by **{Context.User.Username}**"));
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Voicechat: \"{Context.Guild.GetChannel(ulong.Parse(id)).Name}\" with the ID: '{id}' was successfully added by '{Context.User}'");
            }
            catch (Exception)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**CreateTempChannel could not be added**"));
                throw;
            }
        }

        [Command("tempremove")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Removes an existing CreateTempVoiceChannel with:\n**[prefix]tempremove <VoiceChannelID>**")]
        public async Task RemoveCreateVoiceChannel(string id)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(id, out _) && id.Length != 18)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The given ID: '{id}' is not valid!**\nMake sure to copy the ID from the voice channel directly!"));
                return;
            }

            if (!DBStuff.createtempchannels.CheckIfCreateVoiceChannelExist(Context.Guild.Id.ToString(), id.ToString()))
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The create temp voice channel with the ID: '{id}' does not exist!**"));
                return;
            }

            try
            {
                DBStuff.createtempchannels.RemoveCC(Context.Guild.Id.ToString(), id);
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**" + Context.Guild.GetChannel(ulong.Parse(id)).Name + $"** was sucessfully removed by **{Context.User.Username}**"));
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Voicechat: \"{Context.Guild.GetChannel(ulong.Parse(id)).Name}\" with the ID: '{id}' was successfully removed by '{Context.User}'");
            }
            catch (Exception)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**CreateTempChannel could not be removed**"));
                throw;
            }
        }

        [Command("tempchangename")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Command to change the TempChannel name with:\n**[prefix]tempchangename <ChannelID> <\"NewName\">**")]
        public async Task ChangeVoiceChatName(string id, string voiceNameNew)
        {
            voiceNameNew = voiceNameNew.Replace("'", "’");
            if (!ulong.TryParse(id, out _) && id.Length != 18)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The given ID: '{id}' is not valid!**\nMake sure to copy the ID from the voice channel directly!"));
                return;
            }
            if (voiceNameNew.Length > 50)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The name **{voiceNameNew}** has more than 50 characters, pls make sure the name is shorter than 50 characters !"));
                return;
            }
            if (!DBStuff.createtempchannels.CheckIfCreateVoiceChannelExist(Context.Guild.Id.ToString(), id.ToString()))
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**The create temp voice channel with the ID: '{id}' does not exist!**"));
                return;
            }

            try
            {
                DBStuff.createtempchannels.ChangeTempChannelName(voiceNameNew, id);
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed($"**TempChannel name succesfully changed to: '{voiceNameNew}'**"));
            }
            catch (Exception)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**TempChannel name could not be changed**"));
                throw;
            }
            DBStuff.createtempchannels.ChangeTempChannelName(voiceNameNew, id);
            // TODO JG 01.07.2021
            await Task.CompletedTask;
        }

        [Command("switchprefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Switching the prefix with:\n**[prefix]switchprefix <newprefix>**\nNote: max. 3 characters")]
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
            catch (Exception)
            {
                await ReplyAsync(null, false, TempVoiceChannel.TempVoiceChannel.CreateEmbed("**Prefix could not be changed:**"));
                throw;
            }

            await Task.CompletedTask;
        }
    }
}
