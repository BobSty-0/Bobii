using Bobii.src.EntityFramework;
using Bobii.src.Models;
using Bobii.src.TempChannel.EntityFramework;
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
                    context.SaveChanges();
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.NukeDataGu, false, nameof(DeleteEverythingFromGuild), new SlashCommandParameter() { Guild = guild}, message: "Successfully nuked guild information!");
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.NukeDataGu, true, nameof(DeleteEverythingFromGuild), new SlashCommandParameter() { Guild = guild }, exceptionMessage: ex.Message);
            }
        }

        public static async Task<List<src.EntityFramework.Entities.caption>> GetCaptions()
        {
            try
            {
                using (var context = new BobiiLngCodes())
                {
                    return context.Captions.ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.BobiiHelp, true, "GetMsg", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<src.EntityFramework.Entities.commands>> GetCommands()
        {
            try
            {
                using (var context = new BobiiLngCodes())
                {
                    return context.Commands.ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.BobiiHelp, true, "GetMsg", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<string> GetLanguage(ulong guildId)
        {
            try
            {
                var language = LanguageHelper.GetLanguage(guildId).Result;

                if (language == null)
                {
                    return "en";
                }

                return language.langugeshort;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.BobiiHelp, true, "GetLanguage", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<src.EntityFramework.Entities.content>> GetContents()
        {
            try
            {
                using (var context = new BobiiLngCodes())
                {
                    return context.Contents.ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.BobiiHelp, true, "GetMsg", exceptionMessage: ex.Message);
                return null;
            }
        }
        #endregion
    }
}
