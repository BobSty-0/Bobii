using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.HelpMethods
{
    class Methods
    {
        public static async Task SetBotStatusAsync(DiscordSocketClient client)
        {
            await client.SetActivityAsync(new Game("BobSty", ActivityType.Listening));
            await client.SetStatusAsync(UserStatus.Online);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Methods     Status was set sucessfully");
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            user.ModifyAsync(x => x.Channel = voiceChannel);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Methods     {user} was conneted to {voiceChannel.Id}");
        }
    }
}
