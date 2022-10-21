namespace Bobii.src.Bobii
{
    public class ConfigKeys
    {
        public static string Token { get { return "token"; } }
        public static string ConnectionString { get { return "ConnectionString"; } }
        public static string ConnectionStringLng { get { return "ConnectionStringLng"; } }
        public static string PGBinPath { get { return "PGBinPath"; } }
        public static string MainGuildID { get { return "MainGuildID"; } }
        public static string DeveloperGuildID { get { return "DeveloperGuildID"; } }
        public static string SupportGuildID { get { return "SupportGuildID"; } }
        public static string SupportGuildCountChannelID { get { return "SupportGuildCountChannelID"; } }
        public static string MainGuildCountChannelID { get { return "MainGuildCountChannelID"; } }
        public static string JoinLeaveLogChannelID { get { return "JoinLeaveLogChannelID"; } }
        public static string DMChannelID { get { return "DMChannelID";  } }
        public static string ConsoleChannelID { get { return "ConsoleChannelID";  } }
        public static string DeveloperUserID { get { return "DeveloperUserID";  } }
        public static string ApplicationName { get { return "ApplicationName"; } }
        public static string ApplicationID { get { return "ApplicationID";  } }
        public static string DeliveredEmojiString { get { return nameof(DeliveredEmojiString);  } }
        public static string DeliveredFailedEmojiString { get { return nameof(DeliveredFailedEmojiString);  } }
    }
}
