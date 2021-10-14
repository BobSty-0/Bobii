using Bobii.src.DBStuff.Tables;
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
            var createTempChannelIDs = createtempchannels.GetCreateTempChannelListFromGuild(guild);
            var tempchannelIDs = tempchannels.GetTempChannelList(guild.Id.ToString());

            if (oldVoice.VoiceChannel != null)
            {
                if (tempchannelIDs.Rows.Count > 0)
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
                foreach (DataRow row in createTempChannelIDs.Rows)
                {
                    if (newVoice.VoiceChannel.Id.ToString() == row.Field<string>("createchannelid"))
                    {
                        await TempChannel.Helper.CreateAndConnectToVoiceChannel(user, newVoice, row.Field<string>("tempchannelname"));
                    }
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
