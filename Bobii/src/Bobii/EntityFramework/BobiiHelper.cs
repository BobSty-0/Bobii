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
        public static async Task DeleteEverythingFromGuild(SocketGuild guild)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    context.TempChannels.RemoveRange(context.TempChannels.AsEnumerable().Where(tc => tc.guildid == guild.Id));
                    context.CreateTempChannels.RemoveRange(context.CreateTempChannels.AsEnumerable().Where(ctc => ctc.guildid == guild.Id));
                    context.FilterLink.RemoveRange(context.FilterLink.AsEnumerable().Where(fl => fl.guildid == guild.Id));
                    context.FilterLinkLogs.RemoveRange(context.FilterLinkLogs.AsEnumerable().Where(fl => fl.guildid == guild.Id));
                    context.FilterLinksGuild.RemoveRange(context.FilterLinksGuild.AsEnumerable().Where(fl => fl.guildid == guild.Id));
                    context.FilterLinkUserGuild.RemoveRange(context.FilterLinkUserGuild.AsEnumerable().Where(fl => fl.guildid == guild.Id));
                    context.FilterWords.RemoveRange(context.FilterWords.AsEnumerable().Where(fw => fw.guildid == guild.Id));
                    context.SaveChanges();
                    await Bobii.Helper.WriteToConsol("NukeDataGu", false, "DeleteEverythingFromGuild", new Entities.SlashCommandParameter() { Guild = guild}, message: "Successfully nuked guild information!");
                }
            }
            catch (Exception ex)
            {
                await Bobii.Helper.WriteToConsol("NukeDataGu", true, "DeleteEverythingFromGuild", new Entities.SlashCommandParameter() { Guild = guild }, exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
