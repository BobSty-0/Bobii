using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Functions
{
    public static class Functions
    {
        public static async Task SetTheBotStatus(DiscordSocketClient client)
        {
            await client.SetActivityAsync(new Game("BobSty", ActivityType.Listening));
            await client.SetStatusAsync(UserStatus.Online);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Status wurde erfolgreich gesetzt.");
        }

        public static JObject GetConfiguration()
        {
            using StreamReader configJson = new StreamReader(Directory.GetCurrentDirectory() + @"/Config.json");
            return (JObject)JsonConvert.DeserializeObject(configJson.ReadToEnd());
        }
    }
}
