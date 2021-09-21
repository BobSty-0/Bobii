using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff.Tables
{
    class filterlinkuserguild
    {
        #region Functions
        public static DataTable GetUsers(ulong guildid)
        {
            try
            {
                return DBStuff.DBFactory.SelectData($"SELECT * FROM filterlinkuserguild WHERE guildid = '{guildid}'");
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetUsers | Guild: {guildid} | {ex.Message}");
                return null;
            }
        }

        public static bool IsUserOnWhitelistInGuild(ulong guildid, ulong userId)
        {
            try
            {
                var table = DBStuff.DBFactory.SelectData($"SELECT * FROM filterlinkuserguild WHERE userid = '{userId}' AND guildid = '{guildid}'");
                if (table.Rows.Count != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: IsUserOnWhitelistInGuild | Guild: {guildid} | {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} FilterLinksU   {message}");
            await Task.CompletedTask;
        }

        public static async void AddWhiteListUserToGuild(ulong guildid, ulong userId)
        {
            try
            {
                DBFactory.ExecuteQuery($"INSERT INTO filterlinkuserguild VALUES ('{DBFactory.GetNewID("filterlinkuserguild")}', '{guildid}', '{userId}')");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: AddWhiteListUserToGuild | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void RemoveWhiteListUserFromGuild(ulong guildid, ulong userId)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM filterlinkuserguild WHERE userid = '{userId}' AND guildid = '{guildid}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: RemoveWhiteListUserFromGuild | Guild: {guildid} | {ex.Message}");
                return;
            }
        }
        #endregion
    }
}
