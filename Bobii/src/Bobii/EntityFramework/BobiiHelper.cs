using Bobii.src.EntityFramework;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii.EntityFramework
{
    class BobiiHelper
    {
        #region Tasks
        public static async Task WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} DeleteEve   {message}");
            await Task.CompletedTask;
        }

        public static async Task DeleteEverythingFromGuild(SocketGuild guild)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    context.TempChannels.RemoveRange(context.TempChannels.AsQueryable().Where(tc => tc.guildid == guild.Id));
                    context.CreateTempChannels.RemoveRange(context.CreateTempChannels.AsQueryable().Where(ctc => ctc.guildid == guild.Id));
                    context.FilterLink.RemoveRange(context.FilterLink.AsQueryable().Where(fl => fl.guildid == guild.Id));
                    context.FilterLinkLogs.RemoveRange(context.FilterLinkLogs.AsQueryable().Where(fl => fl.guildid == guild.Id));
                    context.FilterLinksGuild.RemoveRange(context.FilterLinksGuild.AsQueryable().Where(fl => fl.guildid == guild.Id));
                    context.FilterLinkUserGuild.RemoveRange(context.FilterLinkUserGuild.AsQueryable().Where(fl => fl.guildid == guild.Id));
                    context.FilterWords.RemoveRange(context.FilterWords.AsQueryable().Where(fw => fw.guildid == guild.Id));
                    await WriteToConsol($"Information: {guild.Name} | Method: RemoveGuild | Guild: {guild.Id} | Successfully nuked guild information!");
                }
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Method: RemoveGuild | Guild: {guild.Id} | {ex.Message}");
            }
        }
        #endregion
    }
}
