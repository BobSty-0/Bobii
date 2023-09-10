using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bobii.src.TempChannel.EntityFramework
{
    class TempCommandsHelper
    {
        public static async Task<List<tempcommands>> GetDisabledCommandsFromGuild(ulong guildId, ulong createchannelid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.Commands.AsQueryable().Where(c => c.guildguid == guildId && c.createchannelid == createchannelid).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetDisabledCommandsFromGuild), exceptionMessage: ex.Message);
                return new List<tempcommands>();
            }
        }

        public static async Task<bool> DoesCommandExist(ulong guildId, ulong createchannelid, string command)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempCommand = context.Commands.SingleOrDefault(c => c.guildguid == guildId && c.createchannelid == createchannelid && c.commandname == command);
                    return tempCommand != null;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(DoesCommandExist), exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task RemoveCommand(ulong guildId, ulong createchannelId, string command)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempCommand = context.Commands.Single(c => c.guildguid == guildId && c.createchannelid == createchannelId && c.commandname == command);

                    context.Commands.Remove(tempCommand);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveCommand), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveCommands(ulong createchannelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempCommands = context.Commands.AsQueryable().Where(c => c.createchannelid == createchannelId).ToList();
                    context.RemoveRange(tempCommands);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveCommands), exceptionMessage: ex.Message);
            }
        }

        public static async Task AddCommand(ulong guildId, string command, bool enabled, ulong createchannelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempCommand = new tempcommands();
                    tempCommand.commandname = command;
                    tempCommand.enabled = enabled;
                    tempCommand.guildguid = guildId;
                    tempCommand.createchannelid = createchannelId;

                    context.Commands.Add(tempCommand);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(AddCommand), exceptionMessage: ex.Message);
            }
        }
    }
}
