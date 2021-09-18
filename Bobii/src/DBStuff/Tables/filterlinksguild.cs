using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff.Tables
{
    class filterlinksguild
    {
        #region Functions
        public static DataTable GetLinks(ulong guildid)
        {
            try
            {
                return DBStuff.DBFactory.SelectData($"SELECT * FROM filterlinksguild WHERE guildid = '{guildid}'");
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: IsFilterlinkAllowedInGuild | Guild: {guildid} | {ex.Message}");
                return null;
            }
        }

        // §TODO 17.09.2021/JG Move this to filterlinkoptions class once it exists
        public static DataTable GetLinkOptions(List<string> bezeichnungen)
        {
            if (bezeichnungen == null)
            {
                return null;
            }
            var sb = new StringBuilder();
            sb.Append("SELECT * FROM filterlinkoptions WHERE bezeichnung = ");
            foreach (var bezeichnung in bezeichnungen)
            {
                sb.Append($"'{bezeichnung}', ");
            }

            try
            {
                return DBStuff.DBFactory.SelectData(sb.ToString());
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetLinkOptions | {ex.Message}");
                return null;
            }
        }

        public static bool IsFilterlinkAllowedInGuild(string guildid, string bezeichnung)
        {
            try
            {
                var table = DBStuff.DBFactory.SelectData($"SELECT * FROM filterlinksguild WHERE bezeichnung = '{bezeichnung}'");
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
                WriteToConsol($"Error: | Function: IsFilterlinkAllowedInGuild | Guild: {guildid} | {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} FilterLinksG   {message}");
            await Task.CompletedTask;
        }

        public static async void AddToGuild(ulong guildid, string bezeichnung)
        {
            try
            {
                DBFactory.ExecuteQuery($"INSERT INTO filterlinksguild VALUES ('{DBFactory.GetNewID("filterlinksguild")}', '{guildid}', '{bezeichnung}')");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: ActivateFilterLinkToGuild | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void RemoveFromGuild(ulong guildid, string bezeichnung)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM filterlinksguild WHERE bezeichnung = '{bezeichnung}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: RemoveFilterLinkFromGuild | Guild: {guildid} | {ex.Message}");
                return;
            }
        }
        #endregion
    }
}
