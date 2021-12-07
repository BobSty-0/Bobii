using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Watch2Gether
{
    class Helper
    {
        public static async Task HandleWatchTogetherButton(Entities.SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "HandleWatchTogetherButton").Result)
            {
                return;
            }

            try
            {
                var voiceChannel = parameter.GuildUser.VoiceChannel;
                var invite = (SocketInvite)voiceChannel.CreateInviteToApplicationAsync(880218394199220334, null).Result;
            }
            catch (Exception ex)
            {

            }

        }
        public static async Task<string> HelpW2GInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "You can craete a YouTube watch 2 gether session in any given voice chat, " +
                "simply click the invite link after creating!", "w2g").Result;
        }
    }
}
