using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff.Tables
{
    class createtempchannels
    {
        #region Methods 
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} CCTChannel   {message}");
            await Task.CompletedTask;
        }

        public static async void AddCC(string guildid, string createChannelName, string creatChannelId)
        {
            try
            {
                DBFactory.ExecuteQuery($"INSERT INTO createtempchannels VALUES ('{DBFactory.GetNewID("createtempchannels")}', '{guildid}', '{createChannelName}', '{creatChannelId}')");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: AddCC | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void RemoveCC(string guildid, string createChannelId)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM createtempchannels WHERE createchannelid = '{createChannelId}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: RemoveCC | Guild: {guildid} | {ex.Message}");
                return;
            }
        }

        public static async void ChangeTempChannelName(string newName, string channelId)
        {
            try
            {
                DBFactory.ExecuteQuery($"UPDATE createtempchannels SET tempchannelname = '{newName}' WHERE createchannelid = '{channelId}'");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Method: ChangeTempChannelName | channelId: {channelId} | {ex.Message}");
                return;
            }
        }
        #endregion

        #region Functions
        public static Boolean CheckIfCreateVoiceChannelExist(SocketGuild guild, string ccid)
        {
            try
            {
                var createTempChannels = DBStuff.DBFactory.SelectData($"SELECT * FROM createtempchannels WHERE guildid = '{guild.Id}'");
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
                WriteToConsol($"Error: {guild.Name} | Function: CheckIfCreateVoiceChannelExist | Guild: {guild.Id} | {ex.Message}");
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
                WriteToConsol($"Error: | Function: CraeteTempChannelListWithAll | {ex.Message}");
                return null;
            }
        }

        public static DataTable GetCreateTempChannelListFromGuild(SocketGuild guild)
        {
            try
            {
                return DBStuff.DBFactory.SelectData($"SELECT * FROM createtempchannels WHERE guildid = '{guild.Id}'");
            }
            catch (Exception ex)
            {
                WriteToConsol($"Error: {guild.Name} | Function: GetCreateTempChannelListFromGuild | Guild: {guild.Id} | {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
