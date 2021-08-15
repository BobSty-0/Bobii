using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff.Tables
{
    class tempchannels
    { 
        #region Methods 
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} TChannels    {message}");
            await Task.CompletedTask;
        }

        public static async void AddTC(string guildid, string tempChannelID)
        {
            try
            {
                DBStuff.DBFactory.ExecuteQuery($"INSERT INTO tempchannels VALUES ('{DBFactory.GetNewID("tempchannels")}', '{tempChannelID}', '{guildid}')");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: AddTC | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void RemoveTC(string guildId, string tempChannelID)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM tempchannels WHERE  channelid = '{tempChannelID}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: RemoveTC | Guild: {guildId} | {ex.Message}");
                return;
            }
        }
        #endregion

        #region Functions
        public static int GetTempChannelCount()
        {
            try
            {
                return DBStuff.DBFactory.GetCountOfAllRows("tempchannels");
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetTempChannelCount | {ex.Message}");
                return 0;
            }
        }

        public static DataTable GetTempChannelList(string guildId)
        {
            try
            {
                return DBFactory.SelectData($"SELECT * FROM tempchannels WHERE guildid = '{guildId}'");
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetTempChannelList | Guild: {guildId} | {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
