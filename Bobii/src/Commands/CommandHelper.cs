using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    class CommandHelper
    {
        #region Methods
        public static void ReplyAndDeleteMessage(SocketCommandContext context, String textString = null, Embed textEmbed = null)
        {
            if (textEmbed == null)
            {
                context.Message.ReplyAsync(textString);
            }
            if (textString == null)
            {
                context.Message.ReplyAsync("", false, textEmbed);
            }
            if (textString != null & textEmbed != null)
            {
                context.Message.ReplyAsync(textString, false, textEmbed);
            }

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

        public static Embed CreateHelpInfo(CommandService commandService)
        {
            var config = BobiiHelper.GetConfig();
            var prefixList = JsonConvert.DeserializeObject<string[]>(config["BobiiConfig"][0]["prefixes"].ToString());

            var sb = new StringBuilder();
            sb.AppendLine("**Here is a Summary of all my commands!**");

            foreach (var module in commandService.Modules)
            {
                foreach (var cmd in module.Commands)
                {
                    sb.AppendLine("");
                    sb.AppendLine("**" + prefixList[0] + cmd.Name + "**\n" + cmd.Summary);
                }
            }
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(0, 225, 225)
                 .WithDescription(
                     sb.ToString());
            return embed.Build();
        }

        public static Embed CreateVoiceChatInfo()
        {
            var config = BobiiHelper.GetConfig();
            StringBuilder sb = new StringBuilder();
            if (config["CreateTempChannels"].ToString() == "[\r\n  {}\r\n]")
            {
                sb.AppendLine("**You dont have any create temp voicechannels yet!**\nYou can add some with: \"'cvcadd <id>\"");
            }
            else
            {
                sb.AppendLine("**Here a list of all create temp voicechannels:**");
            }

            foreach (JToken token in config["CreateTempChannels"])
            {
                foreach (JToken key in token)
                {
                    string keyText = key.ToString().Replace("\"", "");
                    keyText = keyText.Replace(":", "");
                    var keyValueName = keyText.Split(" ");
                    sb.AppendLine("");
                    
                    var count = keyValueName.Count();
                    if (count > 2)
                    {
                        //In case there are spacebars in the voicechannel name
                        sb.Append("**Name:**");
                        for (int zaehler = 0; zaehler < count - 1; zaehler++)
                        {
                            if (zaehler == count - 2)
                            {
                                sb.AppendLine(" " + keyValueName[zaehler]);
                            }
                            else
                            {
                                sb.Append(" " + keyValueName[zaehler]);
                            }
                        }
                        sb.AppendLine("**Voicechat ID:** " + keyValueName[count - 1]);
                    }
                    else
                    {
                        sb.AppendLine("**Name:** "+ keyValueName[0]);
                        sb.AppendLine("**Voicechat ID:** " + keyValueName[1]);
                    }
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
