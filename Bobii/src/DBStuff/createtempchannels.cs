using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff
{
    class createtempchannels
    {
        #region Methods 
        public static void AddCC(string guildid, string createChannelName, string creatChannelId)
        {
            try
            {
                DBStuff.DBFactory.ExecuteQuery($"INSERT INTO createtempchannels VALUES ('{DBFactory.GetNewID("createtempchannels")}', '{guildid}', '{createChannelName}', '{creatChannelId}')");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying add the CreateTempChannel: '" + creatChannelId + "' of the Guild: '" + guildid + "'\nException: " + ex.Message);
                return;
            }
        }

        public static void RemoveCC(string guildid, string createChannelId)
        {
            try
            {
                DBStuff.DBFactory.ExecuteQuery($"DELETE FROM createtempchannels WHERE createchannelid = '{createChannelId}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying remove the CreateTempChannel: '{createChannelId}' of the Guild: '" + guildid + "'\nException: " + ex.Message);
                return;
            }
        }

        public static void ChangeTempChannelName(string newName, string channelId)
        {
            try
            {
                DBFactory.ExecuteQuery($"UPDATE createtempchannels SET tempchannelname = '{newName}' WHERE createchannelid = '{channelId}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to switch the name of the tempchannelname of the channel: '{channelId}'\nException: " + ex.Message);

            }
        }
        #endregion

        #region Functions
        public static Boolean CheckIfCreateVoiceChannelExist(string guildid, string ccid)
        {
            try
            {
                var createTempChannels = DBStuff.DBFactory.SelectData("SELECT * FROM createtempchannels WHERE guildid = '" + guildid + "'");
                foreach (DataRow row in createTempChannels.Rows)
                {
                    if (row.Field<string>("createchannelid").Trim() == ccid)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to check if a the Voicechannel:{ccid} already Exists!\nException: " + ex.Message);
                return false;
            }
        }

        public static DataTable CraeteTempChannelListWithAll()
        {
            try
            {
                return DBStuff.DBFactory.SelectData($"SELECT * FROM createtempchannels");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to get a List of the CreateTempChannels of all Guilds\nException: " + ex.Message);
                return null;
            }
        }

        public static DataTable GetCreateTempChannelListFromGuild(string guildid)
        {
            try
            {
                return DBStuff.DBFactory.SelectData($"SELECT * FROM createtempchannels WHERE guildid = '{guildid}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to get a List of the CreateTempChannels of the Guild: '{guildid}'\nException: " + ex.Message);
                return null;
            }
        }
        #endregion
    }
}
