using Bobii.src.EntityFramework.Entities;
using Bobii.src.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Bobii.src.TempChannel.EntityFramework
{
    public class UsedFunctionsHelper
    {
        public static async Task<usedfunctions> GetUsedFunction(ulong userId, ulong affectedUserId, string function)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.SingleOrDefault(c => c.userid == userId && c.affecteduserid == affectedUserId && c.function == function);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunction), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<usedfunctions>> GetUsedFrunctionsFromAffectedUser(ulong affectedUser)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsQueryable().Where(u => u.affecteduserid == affectedUser).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFrunctionsFromAffectedUser), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<usedfunctions>> GetUsedFunctions(ulong userId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsQueryable().Where(u => u.userid == userId).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunctions), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task RemoveUsedFunction(ulong userId, ulong affectedUserId, string function)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var lang = context.UsedFunctions.Single(c => c.userid == userId && c.affecteduserid == affectedUserId && c.function == function);

                    context.UsedFunctions.Remove(lang);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task AddUsedFunction(ulong userId, ulong affectedUserId, string function)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var usedfunction = new usedfunctions();
                    usedfunction.userid = userId;
                    usedfunction.affecteduserid = affectedUserId;
                    usedfunction.function = function;
                    usedfunction.doneat = DateTime.Now;

                    context.UsedFunctions.Add(usedfunction);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(AddUsedFunction), exceptionMessage: ex.Message);
            }
        }
    }
}
