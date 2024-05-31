using bobii_rework.Extensions;
using Discord.WebSocket;

namespace bobii_rework.Handler
{
    public class GuildHandler
    {
        public async Task ExecuteJoinedGuildActionAsync(SocketGuild guild)
        {
            this.WriteLineToConsole("");
        }

        public async Task ExecuteLeftGuildActionAsync(SocketGuild guild)
        {
            this.WriteLineToConsole("");
        }
    }
}
