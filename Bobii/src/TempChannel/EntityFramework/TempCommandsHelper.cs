using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel.EntityFramework
{
    class TempCommandsHelper
    {
        public static async Task<bool> DoesCommandExist(ulong guildId, string command)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempCommand = context.Commands.SingleOrDefault(c => c.guildguid == guildId && c.commandname == command);
                    return tempCommand != null;
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(DoesCommandExist), exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task RemoveCommand(ulong guildId, string command)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempCommand = context.Commands.Single(c => c.guildguid == guildId && c.commandname == command);

                    context.Commands.Remove(tempCommand);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveCommand), exceptionMessage: ex.Message);
            }
        }

        public static async Task AddCommand(ulong guildId, string command, bool enabled)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var tempCommand = new tempcommands();
                    tempCommand.commandname = command;
                    tempCommand.enabled = enabled;
                    tempCommand.guildguid = guildId;

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
