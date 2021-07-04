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
        public static void AddCC(string guildid, string createChannelName, string creatChannelId )
        {
            try
            {
                DBStuff.DBFactory.ExecuteQuery($"INSERT INTO createtempchannels VALUES ('{DBFactory.GetNewID("prefixes")}', '{guildid}', '{createChannelName}', '{creatChannelId}')");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying add the CreateTempChannel: '" + creatChannelId + "' of the Guild: '" + guildid + "'\nException: " + ex.Message);
                return;
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
        #endregion
    }
}
