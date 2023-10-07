using Bobii.src.EntityFramework.Entities;
using Bobii.src.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;
using Bobii.src.Bobii;

namespace Bobii.src.TempChannel.EntityFramework
{
    public class UsedFunctionsHelper
    {
        public static async Task<usedfunctions> GetUsedFunction(string function, ulong channelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.SingleOrDefault(c => c.function == function && c.channelid == channelId);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunction), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<usedfunctions> GetBlockedUserFunction(ulong guildid, ulong userId, ulong affectedUserId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.SingleOrDefault(c => c.function == GlobalStrings.block && c.guildid == guildid && c.affecteduserid == affectedUserId && c.userid == userId);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetBlockedUserFunction), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<usedfunctions> GetUsedFunction(string function, ulong channelId, ulong userId, ulong affectedUser)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.SingleOrDefault(c => c.function == function && c.channelid == channelId && c.userid == userId && c.affecteduserid == affectedUser);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunction), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<usedfunctions>> GetAllModeratorsFromUser(ulong userId, ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsEnumerable().Where(c => c.userid == userId && c.function == GlobalStrings.moderator && c.guildid == guildId).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunction), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<usedfunctions> GetUsedFunction(ulong userId, ulong affectedUserId, string function, ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.SingleOrDefault(c => c.userid == userId && c.affecteduserid == affectedUserId && c.function == function && c.guildid == guildId);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunction), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<usedfunctions>> GetUsedFrunctionsFromAffectedUser(ulong affectedUser, ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsQueryable().Where(u => u.affecteduserid == affectedUser && u.guildid == guildId).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFrunctionsFromAffectedUser), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<usedfunctions>> GetMutedUsedFunctions( ulong channelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsQueryable().Where(u => u.function == GlobalStrings.mute && u.channelid == channelId).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunctions), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<usedfunctions>> GetChatMutedUserUsedFunctions(ulong channelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsQueryable().Where(u => u.function == GlobalStrings.mutechatuser && u.channelid == channelId).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunctions), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<usedfunctions> GetWhitelistUsedFunction(ulong userId, ulong affectedUserId, ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsQueryable().SingleOrDefault(u => u.function == GlobalStrings.whitelist && u.userid == userId && u.guildid == guildId && u.affecteduserid == affectedUserId);
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunctions), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<usedfunctions>> GetWhitelistUsedFunctions(ulong userId, ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsQueryable().Where(u => u.function == GlobalStrings.whitelist && u.userid == userId && u.guildid == guildId).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunctions), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<List<usedfunctions>> GetUsedFunctions(ulong userId, ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.UsedFunctions.AsQueryable().Where(u => u.userid == userId && u.guildid == guildid).ToList();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(GetUsedFunctions), exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task RemoveWhitelistedUsersFromUser(ulong guildId, ulong userId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var functions = context.UsedFunctions.AsEnumerable().Where(c => c.guildid == guildId && c.userid == userId && c.function == GlobalStrings.whitelist).ToList();
                    context.RemoveRange(functions);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveBlockedUsersFromUser(ulong guildId, ulong userId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var functions = context.UsedFunctions.AsEnumerable().Where(c => c.guildid == guildId && c.userid == userId && c.function == GlobalStrings.block).ToList();
                    context.RemoveRange(functions);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveUsedFunction(ulong channelId, string function, ulong affectedUser)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var functions = context.UsedFunctions.SingleOrDefault(c => c.channelid == channelId && c.function == function && c.affecteduserid == affectedUser);
                    context.Remove(functions);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveUsedFunction(ulong channelId, String function)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var functions = context.UsedFunctions.SingleOrDefault(c => c.channelid == channelId && c.function == function);
                    context.Remove(functions);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveUsedFunctionsModerator(ulong userId, ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var functions = context.UsedFunctions.AsQueryable().Where(c => c.userid == userId && c.guildid == guildId && c.function == GlobalStrings.moderator).ToList();
                    if (functions.Count > 0)
                    {
                        context.RemoveRange(functions);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveUsedFunctionsFromModerator(ulong userId, ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var functions = context.UsedFunctions.AsQueryable().Where(c => c.affecteduserid == userId && c.guildid == guildId && c.function == GlobalStrings.moderator).ToList();
                    if (functions.Count > 0)
                    {
                        context.RemoveRange(functions);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveUsedFunction(ulong channelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var functions = context.UsedFunctions.AsQueryable().Where(c => c.channelid == channelId).ToList();
                    context.RemoveRange(functions);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveUsedFunction(ulong userId, ulong affectedUserId, string function, ulong guildid)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var lang = context.UsedFunctions.Single(c => c.userid == userId && c.affecteduserid == affectedUserId && c.function == function && c.guildid == guildid);

                    context.UsedFunctions.Remove(lang);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol("TempCommand", true, nameof(RemoveUsedFunction), exceptionMessage: ex.Message);
            }
        }

        public static async Task AddUsedFunction(ulong userId, ulong affectedUserId, string function, ulong channelId, ulong guildid, bool isuser = true)
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
                    usedfunction.channelid = channelId;
                    usedfunction.guildid = guildid;
                    usedfunction.isuser = isuser;
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
