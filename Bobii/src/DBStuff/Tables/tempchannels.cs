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
        public static void AddTC(string guildid, string tempChannelID)
        {
            try
            {
                DBStuff.DBFactory.ExecuteQuery($"INSERT INTO tempchannels VALUES ('{DBFactory.GetNewID("tempchannels")}', '{tempChannelID}', '{guildid}')");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying add the TempChannel: '" + tempChannelID + "' of the Guild: '" + guildid + "'\nException: " + ex.Message);
                return;
            }
        }

        public static void RemoveTC(string gildId, string tempChannelID)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM tempchannels WHERE  channelid = '{tempChannelID}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to remove the TempChannel from the Guild: {gildId}\nException: " + ex.Message);
            }
        }
        #endregion

        #region Functions
        public static DataTable GetTempChannelList(string guildid)
        {
            try
            {
                return DBStuff.DBFactory.SelectData($"SELECT * FROM tempchannels WHERE guildid = '{guildid}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to get a List of the TempChannels of the Guild: '{guildid}'\nException: " + ex.Message);
                return null;
            }
        }
        #endregion
    }
}
