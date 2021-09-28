using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Bobii.src.DBStuff.Tables
{
    class filterlinklogs
    {
        #region Functions
        public static DataTable GetAllFilterLinkLogChannels()
        {
            try
            {
                return DBStuff.DBFactory.SelectData($"SELECT * FROM filterlinklogs");
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetAllFilterLinkLogChannels | {ex.Message}");
                return null;
            }
        }

        public static string GetFilterLinkLogChannelID(ulong guildid)
        {
            try
            {
                var table = DBStuff.DBFactory.SelectData($"SELECT channelid FROM filterlinklogs WHERE guildid = '{guildid}'");
                if (table.Rows.Count == 0)
                {
                    return "";
                }
                foreach(DataRow row in table.Rows)
                {
                    return row.Field<string>("channelid");
                }
                return "";
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetFilterLinkLogChannelID | Guild: {guildid} | {ex.Message}");
                return "";
            }
        }

        public static bool DoesALogChannelExist(ulong guildid)
        {
            try
            {
                var table = DBStuff.DBFactory.SelectData($"SELECT * FROM filterlinklogs WHERE guildid = '{guildid}'");
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
                WriteToConsol($"Error: | Function: DoesALogChannelExist | Guild: {guildid} | {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} FilterLink   {message}");
            await Task.CompletedTask;
        }

        public static async void SetFilterLinkLog(ulong guildid, ulong channelId)
        {
            try
            {
                DBFactory.ExecuteQuery($"INSERT INTO filterlinklogs VALUES ('{DBFactory.GetNewID("filterlinklogs")}', '{guildid}', '{channelId}')");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: SetFilterLinkLog | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void RemoveFilterLinkLog(ulong guildid)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM filterlinklogs WHERE guildid = '{guildid}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: RemoveFilterLinkLog | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void UpdateFilterLinkLog(ulong guildid, ulong newChannel)
        {
            try
            {
                DBFactory.ExecuteQuery($"UPDATE filterlinklogs SET channelid = '{newChannel}' WHERE guildid = '{guildid}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: UpdateFilterWord | New Channel: {newChannel} | {ex.Message}");
                return;
            }
        }
        #endregion
    }
}
