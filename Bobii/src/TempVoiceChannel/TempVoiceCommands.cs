using Bobii.src.TextChannel;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempVoiceChannel
{
    class TempVoiceCommands : ModuleBase<SocketCommandContext>
    {
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
            await Context.Message.ReplyAsync("", false, CommandHelper.CreateVoiceChatInfo());
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'vcinfo was used by \"{Context.User}\"");
        }

        [Command("cvcadd")]
        [Summary("Adds a new create temp voice channel with: cvcadd <VoiceChannelID>")]
        public async Task AddCreateVoiceChannel(string id)
        {
            // TODO JG 01.07.2021
            await Task.CompletedTask;

            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(id, out _) && id.Length != 18)
            {
                CommandHelper.ReplyAndDeleteMessage(Context, null, CommandHelper.CreateOneLineEmbed($"The given ID: \"{id}\" is not valid! Make sure to copy the ID from the voice channel directly!"));
                return;
            }

            if (CommandHelper.CheckIfConfigKeyExistsAlready("CreateTempChannels", id.ToString()))
            {
                CommandHelper.ReplyAndDeleteMessage(Context, null, CommandHelper.CreateOneLineEmbed($"The create temp voice channel with the ID: \"{id}\" exists already!"));
                return;
            }

            CommandHelper.EditConfig("CreateTempChannels", id, Context.Guild.GetChannel(ulong.Parse(id)).Name);
            CommandHelper.ReplyAndDeleteMessage(Context, null, CommandHelper.CreateOneLineEmbed("\"" + Context.Guild.GetChannel(ulong.Parse(id)).Name + $"\" was sucessfully added by \"{Context.User}\" to the create temp voicechannel list!"));
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Voicechat: \"{Context.Guild.GetChannel(ulong.Parse(id)).Name}\" with the ID: \"{id}\" was successfully added by {Context.User}");

            //TODO JG 18.06.2021 Check if cvc already exists and reply with message! 
            //Also check if I need ReplyAndDeleteMessage
        }
    }
}
