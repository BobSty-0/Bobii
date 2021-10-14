using Discord.WebSocket;

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

        public SocketSlashCommandData SlashCommandData;
    }
}
