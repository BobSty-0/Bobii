using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
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

        [Command("vcinfo")]
        [Summary("Gives info about the currently set create temp voicechannels")]
        public async Task TempVoiceChannelInof()
        {
            await ReplyAsync($"TempChannelInfo",false, CommandHelper.CreateVoiceChatInfo());
        }

        [Command("setvc1")]
        [Summary("Set the first create temp voicechannel with: setvc1 <VoiceChannelID>")]
        public async Task SetFirstCreateVoiceChannel(ulong id)
        {
            CommandHelper.EditConfig("CreateTempChannels", "createvoicechannel1", id);
        }

        [Command("setvc2")]
        [Summary("Set the second create temp voicechannel with: setvc1 <VoiceChannelID>")]
        public async Task SetSecondCreateVoiceChannel(ulong id)
        {
            CommandHelper.EditConfig("CreateTempChannels", "createvoicechannel2", id);
        }
    }
}
