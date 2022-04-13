using Bobii.src.Models;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    static class Extensions
    {
        public static string Link2LinkOptions(this string str)
        {
            str = str.Replace("https://", "");
            str = str.Replace("http://", "");
            str = str.Split('/')[0];
            str = $"{str}/";
            return str;
        }

        public static ulong ToUlong(this string str)
        {
            return ulong.Parse(str);
        }

        public static SlashCommandParameter ContextToParameter(this SocketInteractionContext context)
        {
            var parameter = new SlashCommandParameter();
            parameter.Client = context.Client;
            parameter.Guild = context.Guild;
            parameter.GuildID = context.Guild.Id;
            parameter.GuildUser = (SocketGuildUser)context.User;
            parameter.Interaction = context.Interaction;
            parameter.Language = Bobii.EntityFramework.BobiiHelper.GetLanguage(parameter.GuildID).Result;
            parameter.SlashCommand = (SocketSlashCommand)context.Interaction;
            parameter.SlashCommandData = parameter.SlashCommand.Data;

            return parameter;
        }
    }
}
