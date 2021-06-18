using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

        [Command("vcname")]
        [Summary("Command to edit VoiceChat Name")]
        public async Task ChangeVoiceChatName(string voiceNameNew)
        {
        }

        [Command("cvcinfo")]
        [Summary("Gives info about the currently set create temp voicechannels")]
        public async Task TempVoiceChannelInof()
        {
            //TODO JG 18.06.2021 Schauen wie ich embeds mit in den Reply bekomme!
            await Context.Message.ReplyAsync(CommandHelper.CreateVoiceChatInfo().ToString());
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'vcinfo was used by \"{Context.User}\"");
        }

        [Command("cvcadd")]
        [Summary("Adds a new create temp voicechannel with: addvc <VoiceChannelID>")]
        public async Task AddCreateVoiceChannel(string id)
        {
            if (!ulong.TryParse(id, out _))
            {
                CommandHelper.ReplyAndDeleteMessage(Context, $"The given ID: \"{id}\" is not valid! Make sure to copy the ID from the voicechannel directly!");
                return;
            }
            CommandHelper.EditConfig("CreateTempChannels", Context.Guild.GetChannel(ulong.Parse(id)).Name, id);
            CommandHelper.ReplyAndDeleteMessage(Context,"\"" + Context.Guild.GetChannel(ulong.Parse(id)).Name + $"\" was sucessfully added by \"{Context.User}\" to the create temp voicechannel list!");
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Voicechat: \"{Context.Guild.GetChannel(ulong.Parse(id)).Name}\" with the ID: \"{id}\" was successfully added by {Context.User}");
                
                //TODO JG 18.06.2021 Check if cvc already exists and reply with message! 

        }
    }
}
