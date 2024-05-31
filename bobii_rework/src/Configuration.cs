using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bobii_rework
{
    static class Configuration
    {
        #region Konstante
        public const string Token = "token";
        public const string ConnectionString = "ConnectionString";
        public const string ConnectionStringLng = "ConnectionStringLng";
        public const string PGBinPath = "PGBinPath";
        public const string MainGuildID = "MainGuildID";
        public const string DeveloperGuildID = "DeveloperGuildID";
        public const string MainGuildCountChannelID = "MainGuildCountChannelID";
        public const string SupportGuildID = "SupportGuildID";
        public const string SupportGuildCountChannelID = "SupportGuildCountChannelID";
        public const string JoinLeaveLogChannelID = "JoinLeaveLogChannelID";
        public const string DMChannelID = "DMChannelID";
        public const string ConsoleChannelID = "ConsoleChannelID";
        public const string DeveloperUserID = "DeveloperUserID";
        public const string ApplicationName = "ApplicationName";
        public const string ApplicationID = "ApplicationID";
        public const string DeliveredEmojiString = "DeliveredEmojiString";
        public const string DeliveredFailedEmojiString = "DeliveredFailedEmojiString";
        public const string ShardCount = "ShardCount";
        public const string Rot = "Rot";
        public const string Gruen = "Gruen";
        public const string Blau = "Blau";

        private const string BobiiConfig = "BobiiConfig";
        private const string ConfigFileName = "config.json";
        #endregion

        #region Declarations
        private static JToken? _config;
        #endregion

        #region Methods
        public static Color GetBobiiColor()
        {
            return new(GetConfigValue<int>(Rot), GetConfigValue<int>(Gruen), GetConfigValue<int>(Blau));
        }

        public static T? GetConfigValue<T>(string key)
        {
            _config ??= GetConfiguration();
            var configValue = _config?[key];

            if (configValue != null)
            {
                return configValue.ToObject<T>();
            }

            return default(T);
        }

        private static JToken? GetConfiguration()
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
            using StreamReader configJson = new StreamReader(configPath);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(configJson.ReadToEnd()) ?? new JObject();

            return jsonObject![BobiiConfig]![0];
        }
        #endregion
    }
}
