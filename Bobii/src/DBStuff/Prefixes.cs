using Discord.WebSocket;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff
{
    class Prefixes
    {
        #region Methods 
        public static void AddPrefix(SocketGuild guild)
        {
            try
            {
                DBFactory.ExecuteQuery("INSERT INTO prefixes VALUES ('" + DBFactory.GetNewID("prefixes", DBFactory.GetConnection()) + "', '-', '" + guild.Id + "')");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Prefixes    Error while trying to add the prefix to the Guild: "+guild.Id+"\nException: " + ex.Message);

                throw;
            }
        }
        
        public static void RemovePrefix(SocketGuild guild)
        {
            try
            {
                DBFactory.ExecuteQuery("DELETE FROM prefixes WHERE guildid = '" + guild.Id+"'");
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
