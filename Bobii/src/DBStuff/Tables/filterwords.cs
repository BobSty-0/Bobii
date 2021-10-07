using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff.Tables
{
    class filterwords
    {
        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} FilterWords  {message}");
            await Task.CompletedTask;
        }

        public static async void AddFilterWord(ulong guildid, string filterWord, string replaceWord)
        {
            try
            {
                DBFactory.ExecuteQuery($"INSERT INTO filterwords VALUES ('{DBFactory.GetNewID("filterwords")}', '{guildid}', '{filterWord}', '{replaceWord}')");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: AddFilterWord | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void RemoveFilterWord(string filterWord, ulong guildid)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM filterwords WHERE filterword = '{filterWord}' AND guildid = '{guildid}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: RemoveFilterWord | Filter word: {filterWord} | {ex.Message}");
                return;
            }
        }

        public static async void UpdateFilterWord(string filterWord, string newReplaceWord, ulong guildid)
        {
            try
            {
                DBFactory.ExecuteQuery($"UPDATE filterwords SET replaceword = '{newReplaceWord}' WHERE filterword = '{filterWord}' AND guildid = '{guildid}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: UpdateFilterWord | Filter word: {filterWord} | {ex.Message}");
                return;
            }
        }
        #endregion

        #region Functions
        public static Boolean CheckIfFilterExists(string guildid, string filterWord)
        {
            try
            {
                var createTempChannels = DBStuff.DBFactory.SelectData($"SELECT * FROM filterwords WHERE guildid = '{guildid}'");
                foreach (DataRow row in createTempChannels.Rows)
                {
                    if (row.Field<string>("filterword").Trim() == filterWord)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: CheckIfFilterExists | Guild: {guildid} | {ex.Message}");
                return false;
            }
        }

        public static DataTable GetFilterWordListFromGuild(string guildid)
        {
            try
            {
                return DBStuff.DBFactory.SelectData($"SELECT * FROM filterwords WHERE guildid = '{guildid}'");
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetCreateFilterWordListFromGuild | Guild: {guildid} | {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
