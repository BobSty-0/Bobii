using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff.Tables
{
    class filterlinkoptions
    {
        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} FLOpitons   {message}");                       
            await Task.CompletedTask;
        }
        #endregion

        #region Functions
        public static string[] GetAllOptions()
        {
            try
            {
                var options = DBStuff.DBFactory.SelectData($"SELECT bezeichnung FROM filterlinkoptions GROUP BY bezeichnung");
                var option = "";
                var list = new List<string>();
                foreach (DataRow row in options.Rows)
                {
                    if (row.Field<string>("bezeichnung").Trim() != option)
                    {
                        option = row.Field<string>("bezeichnung").Trim();
                        list.Add(option);
                    }
                }
                return list.ToArray();
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetAllOpitons | {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
