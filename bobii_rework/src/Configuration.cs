using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Color = Discord.Color;

namespace bobii_rework
{
    static class Configuration
    {
        #region Konstanten
        public const string Token = nameof(Token);
        public const string ConnectionString = nameof(ConnectionString);
        public const string ConnectionStringLng = nameof(ConnectionStringLng);
        public const string SupportGuildID = nameof(SupportGuildID);
        public const string ApplicationID = nameof(ApplicationID);
        public const string ShardCount = nameof(ShardCount);
        public const string ThemeColorHex = nameof(ThemeColorHex);
        public const string ApplicationName = nameof(ApplicationName);
        public const string DashboardUrl = nameof(DashboardUrl);
        public const string DokumentationUrl = nameof(DokumentationUrl);
        public const string SupportServerInviteLink = nameof(SupportServerInviteLink);
        public const string StatusText = nameof(StatusText);
        public const string ActivityType = nameof(ActivityType);
        public const string UserStatus = nameof(UserStatus);

        private const string ConfigFileName = "config.json";
        #endregion

        #region Declarations
        private static JToken? _config;
        #endregion

        #region Methods
        public static Color GetBobiiColor()
        {
            var value = Convert.ToUInt32(GetConfigValue<string>(ThemeColorHex), 16);
            return new Color(value);
        }

        public static T? GetConfigValue<T>(string key)
        {
            _config ??= GetConfiguration();
            var configValue = _config?[key];

            return configValue != null ? configValue.ToObject<T>() : default(T);
        }

        private static JToken? GetConfiguration()
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
            using var configJson = new StreamReader(configPath);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(configJson.ReadToEnd()) ?? new JObject();

            return jsonObject;
        }
        #endregion
    }
}
