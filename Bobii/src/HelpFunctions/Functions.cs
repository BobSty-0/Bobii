using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bobii.src.HelpFunctions
{
    public static class Functions
    {
        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static JObject GetConfig()
        {
            using StreamReader configJson = new StreamReader(Directory.GetCurrentDirectory() + @"/Config.json");
            return (JObject)JsonConvert.DeserializeObject(configJson.ReadToEnd());
        }

        public static string GetAvatarUrl(SocketUser user, ushort size = 1024)
        {
            return user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();
        }

        public static RestVoiceChannel CreateVoiceChannel(SocketGuildUser user, string name, ulong catergoryId)
        {
            var channel = user.Guild.CreateVoiceChannelAsync(name, prop => prop.CategoryId = catergoryId);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    {user} created a new Channel -> ID: {channel.Result.Id}");
            return channel.Result;
        }
    }
}
