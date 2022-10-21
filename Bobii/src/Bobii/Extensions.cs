﻿using Bobii.src.Models;
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

            var liste = new List<RestThreadChannel>();
            await AddThreadsToList(liste, activeThreads);
            await AddThreadsToList(liste, publicArchivedThreads);

            return liste;
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
