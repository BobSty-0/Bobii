using Discord.WebSocket;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff
{
    class Prefixes
    {
        #region Methods 
        public static string GetPrefixFromGuild(string guildId)
        {
            try
            {
                string prefix = null;
                var table =  DBFactory.SelectData($"SELECT prefix FROM prefixes WHERE guildid ='{guildId}'");
                // §TODO 04.07.2021/JG Besser Lösung hierfür finden
                foreach(DataRow row in table.Rows)
                {
                    prefix =  row.Field<string>("prefix");
                }
                return prefix;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to get the Prefix for the Guild: "+guildId+ "\nException: "+ex.Message);
                return null;
            }
        }

        public static void AddPrefix(SocketGuild guild)
        {
            try
            {
                DBFactory.ExecuteQuery("INSERT INTO prefixes VALUES ('" + DBFactory.GetNewID("prefixes") + "', '!', '" + guild.Id + "')");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to add the prefix to the Guild: "+guild.Id+"\nException: " + ex.Message);
                return;
            }
        }
        
        public static void RemovePrefix(SocketGuild guild)
        {
            try
            {
                DBFactory.ExecuteQuery($"DELETE FROM prefixes WHERE guildid = '{guild.Id}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to remove the prefix from the Guild: "+guild.Id+"\nException: " + ex.Message);
            }
        }

        public static void SwitchPrefix(string prefix, string guildId)
        {
            try
            {
                DBFactory.ExecuteQuery("UPDATE prefixes SET prefix = '" + prefix + "' WHERE guildid = '"+guildId+"'");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to switch the prefix in Guild: "+guildId+"\nException: " + ex.Message);

            }
        }
        #endregion

        #region Functions
       
        #endregion
        
    }
}
