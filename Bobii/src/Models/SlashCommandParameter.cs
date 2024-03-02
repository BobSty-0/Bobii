using Discord.WebSocket;

namespace Bobii.src.Models
{
    public class SlashCommandParameter
    {
        public SocketSlashCommand SlashCommand;

        public SocketGuildUser GuildUser;

        public SocketGuild Guild;

        public DiscordShardedClient Client;

        public SocketInteraction Interaction;

        public ulong GuildID;

        public SocketSlashCommandData SlashCommandData;

        public string Language;

        public SocketModal Modal;
    }
}
