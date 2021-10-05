using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Entities
{
    class SlashCommandParameter
    {
        public SocketSlashCommand SlashCommand;

        public SocketGuildUser GuildUser;

        public SocketGuild Guild;

        public DiscordSocketClient Client;

        public SocketInteraction Interaction;

        public ulong GuildID;
    }
}
