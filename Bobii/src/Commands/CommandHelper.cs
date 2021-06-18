using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    class CommandHelper
    {
        #region Methods
        public static void ReplyAndDeleteMessage(SocketCommandContext context, String textString = null)
        {
             context.Message.ReplyAsync(textString);
        }

        public static void DeletCommandMessage(SocketMessage message)
        {
            message.DeleteAsync();
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Message: \"{message.Author}\" was successfully deleted");
        }

        public static void EditConfig(string configObject, string keyName, string keyValue)
        {
            var config = BobiiHelper.GetConfig();
            config[configObject][0][keyName] = keyValue;
            File.WriteAllText(Directory.GetCurrentDirectory() + @"/Config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    The KeyValue of \"{keyName}\" was successfully changed to \"{keyValue}\"");
        }
        #endregion

        #region Functions
        public static string GetAvatarUrl(SocketUser user, ushort size = 1024)
        {
            return user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();
        }

        public static Embed CreateOneLineEmbed(string text)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(0, 225, 225)
                .WithDescription(text);

            return embed.Build();
        }

        public static Embed CreateVoiceChatInfo()
        {
            StringBuilder sb = new StringBuilder();

            var config = BobiiHelper.GetConfig();
            foreach (JToken token in config["CreateTempChannels"])
            {
                foreach(JToken key in token)
                {
                    string keyText = key.ToString().Replace("\"", "");
                    keyText = keyText.Replace(":", "");
                    var keyValueName = keyText.Split(" ");
                    sb.AppendLine("Voicechat Name: "+ keyValueName[0] + " Voicechat ID: " + keyValueName[1]);
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
            .WithColor(0, 225, 225)
            .WithDescription(sb.ToString());

            return embed.Build();
        }
        #endregion
    }
}
