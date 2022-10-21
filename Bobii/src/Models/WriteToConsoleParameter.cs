using Discord.WebSocket;

namespace Bobii.src.Models
{
    public class WriteToConsoleParameter
    {
        public string Category { get; set; }
        public bool Error { get; set; }
        public string Task { get; set; }
        public SocketGuild Guild { get; set; }
        public SocketGuildUser GuildUser { get; set; }
        public ulong CreateTempChannelID { get; set; }
        public ulong TempChannelID { get; set; }
        public string HelpSection { get; set; }
        public string EmojiString { get; set; }
        public string CheckedID { get; set; }
        public string MessageID { get; set; }
        public string ParameterName {get; set;}
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
    }
}
