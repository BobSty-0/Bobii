using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff.Tables
{
    class badwords
    {
        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} BadWords     {message}");
            await Task.CompletedTask;
        }

        public static async void AddBadWord(string guildid, string badWord, string replaceWord)
        {
            try
            {
                DBFactory.ExecuteQuery($"INSERT INTO badwords VALUES ('{DBFactory.GetNewID("badwords")}', '{guildid}', '{badWord}', '{replaceWord}')");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: AddBadWord | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void RemoveBadWord(string badWord)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM badwords WHERE badword = '{badWord}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: RemoveBadWord | BadWord: {badWord} | {ex.Message}");
                return;
            }
        }

        public static async void ChangeBadWordReplaceWord(string badWord, string newReplaceWord )
        {
            try
            {
                DBFactory.ExecuteQuery($"UPDATE badwords SET badword = '{badWord}' WHERE replaceword = '{newReplaceWord}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: ChangeBadWordReplaceWord | BadWord: {badWord} | {ex.Message}");
                return;
            }
        }
        #endregion

        #region Functions
        public static Boolean CheckIfBadWordExists(string guildid, string badword)
        {
            try
            {
                var createTempChannels = DBStuff.DBFactory.SelectData($"SELECT * FROM badwords WHERE guildid = '{guildid}'");
                foreach (DataRow row in createTempChannels.Rows)
                {
                    if (row.Field<string>("badword").Trim() == badword)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: CheckIfBadWordExists | Guild: {guildid} | {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}
