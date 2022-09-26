using Bobii.src.Models;
using Discord.Interactions;
using Discord.Rest;
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
        #region private Methods
        private static async Task AddThreadsToList(List<RestThreadChannel> list, IReadOnlyCollection<RestThreadChannel> threads)
        {
            foreach(var thread in threads)
            {
                list.Add(thread);
            }
        }
        #endregion

        public static async Task<List<RestThreadChannel>> GetAllThreads(this SocketForumChannel channel)
        {
            var activeThreads = channel.GetActiveThreadsAsync().Result;
            var publicArchivedThreads = channel.GetPublicArchivedThreadsAsync().Result;
            //var privateArchivedThreads = channel.GetPrivateArchivedThreadsAsync().Result;
            //var joinedPrivateArchivedThreads = channel.GetJoinedPrivateArchivedThreadsAsync().Result;

            var liste = new List<RestThreadChannel>();
            await AddThreadsToList(liste, activeThreads);
            await AddThreadsToList(liste, publicArchivedThreads);
            //await AddThreadsToList(liste, privateArchivedThreads);
            //await AddThreadsToList(liste, joinedPrivateArchivedThreads);
            return liste;
        }
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

        public static ulong ToUlong(this object obj)
        {
            return ulong.Parse(obj.ToString());
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
