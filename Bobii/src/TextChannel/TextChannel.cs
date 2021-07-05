using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace Bobii.src.TextChannel
{
    class TextChannel
    {
        #region Methods
        public static void DeletCommandMessage(SocketMessage message)
        {
            message.DeleteAsync();
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Message: \"{message.Author}\" was successfully deleted");
        }

        public static void EditConfig(string configObject, string keyName, string keyValue)
        {
            var config = Program.GetConfig();
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

        public static Embed CreateHelpInfo(CommandService commandService, string guildId)
        {
            var config = Program.GetConfig();
            var prefix = DBStuff.Prefixes.GetPrefixFromGuild(guildId).Trim();

            var sb = new StringBuilder();
            sb.AppendLine("**Here is a Summary of all my commands!**");

            foreach (var module in commandService.Modules)
            {
                foreach (var cmd in module.Commands)
                {
                    sb.AppendLine("");
                    sb.AppendLine("**" + prefix + cmd.Name + "**\n" + cmd.Summary);
                }
            }
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(0, 225, 225)
                 .WithDescription(
                     sb.ToString().Replace("[prefix]", prefix));
            return embed.Build();
        }
        #endregion
    }
}
