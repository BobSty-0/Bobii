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
                WriteToConsol($"Error: | Function: GetLinks | Guild: {guildid} | {ex.Message}");
                return null;
            }
        }

        // §TODO 17.09.2021/JG Move this to filterlinkoptions class once it exists
        public static DataTable GetLinkOptions(DataTable bezeichnungen)
        {
            if (bezeichnungen.Rows.Count == 0)
            {
                return new DataTable();
            }
            var sb = new StringBuilder();
            sb.Append("SELECT * FROM filterlinkoptions WHERE (bezeichnung = ");
            var count = 0;
            foreach (DataRow row in bezeichnungen.Rows)
            {
                count++;
                if (count == bezeichnungen.Rows.Count)
                {
                    sb.Append($"'{row.Field<string>("bezeichnung").Trim()}')");
                    continue;
                }
                sb.Append($"'{row.Field<string>("bezeichnung").Trim()}' or bezeichnung = ");
            }


            var test = sb.ToString().TrimEnd(' ');
            test = test.TrimEnd(',');

            try
            {
                return DBStuff.DBFactory.SelectData(test);
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetLinkOptions | {ex.Message}");
                return new DataTable();
            }
        }

        public static bool IsFilterlinkAllowedInGuild(string guildid, string bezeichnung)
        {
            try
            {
                var table = DBStuff.DBFactory.SelectData($"SELECT * FROM filterlinksguild WHERE bezeichnung = '{bezeichnung}' AND guildid = '{guildid}'");
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
                WriteToConsol($"Error: | Method: AddToGuild | Guild: {guildid} | {ex.Message}");
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
                WriteToConsol($"Error: | Method: RemoveFromGuild | Guild: {guildid} | {ex.Message}");
                return;
            }
        }
        #endregion
    }
}
