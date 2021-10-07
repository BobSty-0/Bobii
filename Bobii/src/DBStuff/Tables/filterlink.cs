using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff.Tables
{
    class filterlink
    {
        #region Functions
        public static bool IsFilterLinkActive(ulong guildid)
        {
            try
            {
                var table = DBStuff.DBFactory.SelectData($"SELECT * FROM filterlink WHERE guildid = '{guildid}'");
                if (table.Rows.Count != 0)
                {
                    return table.Rows[0].Field<bool>("filterlinkactive");
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: IsFilterWordActive | Guild: {guildid} | {ex.Message}");
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

        public static async void ActivateFilterLink(ulong guildid)
        {
            try
            {
                DBFactory.ExecuteQuery($"INSERT INTO filterlink VALUES ('{DBFactory.GetNewID("filterlink")}', '{guildid}', {true})");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: ActivateFilterLink | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void DeactivateFilterLink(ulong guildid)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM filterlink WHERE guildid = '{guildid}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: DeactivateFilterLink | Guild: {guildid} | {ex.Message}");
                return;
            }
        }
        #endregion
    }
}
