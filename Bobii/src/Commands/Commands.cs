using Discord;
using Discord.Commands;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Summary("Summary of all my commands")]
        public async Task Help()
        {
            var sb = new StringBuilder();
            sb.AppendLine("**Icon**");
            sb.Append("Shows your Avatar");
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(0, 225, 225)
                 .WithDescription(
                     sb.ToString());
            await ReplyAsync(embed: embed.Build());
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'help was used by {Context.User}");
        }

        [Command("vname")]
        [Summary("Command to edit VoiceChat Name")]
        public async Task ChangeVoiceChatName(string voiceNameNew)
        {
        }
    }
}
