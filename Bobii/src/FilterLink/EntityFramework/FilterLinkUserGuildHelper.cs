using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;

namespace Bobii.src.FilterLink.EntityFramework
{
    class FilterLinkUserGuildHelper
    {
        #region Tasks
        public static async Task AddWhiteListUserToGuild(ulong guildid, ulong userId)
        {
            try
            {
                using( var context = new BobiiEntities())
                {
                    var user = new filterlinkuserguild();
                    user.guildid = guildid;
                    user.userid = userId;
                    context.FilterLinkUserGuild.Add(user);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await Bobii.Helper.WriteToConsol("FilterLink", true, "AddWhiteListUserToGuild", exceptionMessage: ex.Message);
            }
        }

        public static async Task RemoveWhiteListUserFromGuild(ulong guildid, ulong userId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var user = context.FilterLinkUserGuild.AsQueryable().Where(user => user.userid == userId && user.guildid == guildid).FirstOrDefault();
                    context.FilterLinkUserGuild.Remove(user);
                    context.SaveChanges();
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                await Bobii.Helper.WriteToConsol("FilterLink", true, "RemoveWhiteListUserFromGuild", exceptionMessage: ex.Message);
            }
        }

        public static async Task<List<filterlinkuserguild>> GetUsers(ulong guildId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.FilterLinkUserGuild.AsQueryable().Where(user => user.guildid == guildId).ToList();
                }
            }
            catch (Exception ex)
            {
                await Bobii.Helper.WriteToConsol("FilterLink", true, "GetUsers", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static async Task<bool> IsUserOnWhitelistInGuild(ulong guildId, ulong userId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var user = context.FilterLinkUserGuild.AsQueryable().Where(user => user.guildid == guildId && user.userid == userId).FirstOrDefault();
                    if (user != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await Bobii.Helper.WriteToConsol("FilterLink", true, "IsUserOnWhitelistInGuild", exceptionMessage: ex.Message);
                return false;
            }
        }
        #endregion
    }
}
