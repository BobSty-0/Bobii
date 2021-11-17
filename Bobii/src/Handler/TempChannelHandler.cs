using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class TempChannelHandler
    {
        #region Tasks
        public static async Task WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TempVoice   {message}");
            await Task.CompletedTask;
        }
        #endregion

        #region Handler

        public static async Task VoiceChannelActions(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            SocketGuild guild;
            if (newVoice.VoiceChannel != null)
            {
                guild = newVoice.VoiceChannel.Guild;
            }
            else
            {
                guild = oldVoice.VoiceChannel.Guild;
            }
            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild);
            var tempchannelIDs = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList(guild.Id).Result;

            if (oldVoice.VoiceChannel != null)
            {
                if (tempchannelIDs.Count > 0)
                {
                    await TempChannel.Helper.CheckAndDeleteEmptyVoiceChannels(client, guild, tempchannelIDs);
                    if (newVoice.VoiceChannel == null)
                    {
                        return;
                    }
                }
            }

            if (newVoice.VoiceChannel != null)
            {

                var createTempChannel = createTempChannels.Result.Where(ch => ch.createchannelid == newVoice.VoiceChannel.Id).FirstOrDefault();
                if (createTempChannel != null)
                {
                    await TempChannel.Helper.CreateAndConnectToVoiceChannel(user, newVoice, createTempChannel.tempchannelname);
                }
            }
            else
            {
                return;
            }
        }
        #endregion
    }
}
