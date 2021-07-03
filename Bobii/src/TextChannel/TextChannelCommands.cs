using Discord;
using Discord.Commands;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Bobii.src.TextChannel
{
    public class TextChannelCommands : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;

        public TextChannelCommands(CommandService service)
        {
            _commandService = service;
        }

        [Command("help")]
        [Summary("Summary of all my commands")]
        public async Task Help()
        {
            await Context.Message.ReplyAsync("", false, TextChannel.CreateHelpInfo(_commandService));
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'help was used by {Context.User}");
        }
    }
}
