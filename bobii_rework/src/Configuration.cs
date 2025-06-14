using bobii_rework.Extensions;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public const string ThemeColorRgb = nameof(ThemeColorRgb);
        public const string ApplicationName = nameof(ApplicationName);
        public const string DashboardUrl = nameof(DashboardUrl);
        public const string DokumentationUrl = nameof(DokumentationUrl);
        public const string SupportServerInviteLink = nameof(SupportServerInviteLink);
        public const string StatusText = nameof(StatusText);
        public const string ActivityType = nameof(ActivityType);
        public const string UserStatus = nameof(UserStatus);
        public const string SetupEmoteString = nameof(SetupEmoteString);
        public const string DashboardEmoteString = nameof(DashboardEmoteString);
        public const string DocumentationEmoteString = nameof(DocumentationEmoteString);
        public const string AppLogoEmoteString = nameof(AppLogoEmoteString);

        private const string BobiiConfig = nameof(BobiiConfig);
        private const string ConfigFileName = "config.json";
        #endregion

        #region Declarations
        private static JToken? _config;
        #endregion

        #region Methods
        public static Color GetBobiiColor()
        {
            var themeColorRgb = GetConfigValue<string>(ThemeColorRgb)!.Split(";");
            return new(themeColorRgb[0].ToByte(), themeColorRgb[1].ToByte(), themeColorRgb[2].ToByte());
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

            return jsonObject![BobiiConfig]![0];
        }
        #endregion
    }
}
