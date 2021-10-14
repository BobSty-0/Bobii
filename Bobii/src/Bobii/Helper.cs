using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    class Helper
    {
        #region Tasks
        public static async Task CreateServerCount(SocketMessage message, DiscordSocketClient client)
        {
            var sb = new StringBuilder();
            foreach (var guild in client.Guilds)
            {
                sb.AppendLine(guild.Name);
            }
            sb.AppendLine();
            sb.AppendLine($"Servercount: {client.Guilds.Count}");
            await message.Channel.SendMessageAsync(sb.ToString());
        }

        public static async Task RefreshBobiiStats()
        {
            await Handler.HandlingService.RefreshServerCount();
        }
        #endregion
    }
}
