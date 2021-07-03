using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.DBStuff
{
    class DBFactory
    {
        public static DataTable SelectData(string query)
        {
            if (!CheckConnectionString())
            {
                return null;
            }

            var connection = GetConnection();
            connection.Open();
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Prepare();

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);

                DataSet _ds = new DataSet();
                DataTable _dt = new DataTable();

                da.Fill(_ds);

                try
                {
                    _dt = _ds.Tables[0];
                }
                catch (Exception ex)
                {
                    // §TODO 03.07.2021/JG throw exception 
                }

                connection.Close();
                return _dt;
            }
        }

        public static void ExecuteQuery(string query)
        {
            if (!CheckConnectionString())
            {
                return;
            }

            var connection = GetConnection();
            connection.Open();
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} DBFactory    Error while trying to Execute this query:\n" + query + "\n" + "Exception: " + ex.Message);
                    return;
                }

            }
        }

        public static NpgsqlConnection GetConnection()
        {
            var config = Program.GetConfig();
            return new NpgsqlConnection(config["BobiiConfig"][0].Value<string>("ConnectionString"));
        }

        public static bool CheckConnectionString()
        {
            var config = Program.GetConfig();
            NpgsqlConnection conn = new NpgsqlConnection(config["BobiiConfig"][0].Value<string>("ConnectionString"));

            try
            {
                conn.Open();
                conn.Close();
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} DBFactory    Error while trying to open the connection to the DataBase");
                return false;
            }
        }

        public static int GetNewID(string table, NpgsqlConnection connection)
        {
            // §TODO 03.07.2021/JG Schauen wie ich das mit dem return löse, da 0 nicht null ist...
            if (!CheckConnectionString())
            {
                return 0;
            }

            connection.Open();
            var query = "SELECT count(*) FROM " + table;
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                try
                {
                    cmd.Prepare();
                    return Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} DBFactory    Error while trying to Execute this query:\n" + query + "\n" + "Exception: " + ex.Message);
                    return 0;
                }
            }
        }
    }
}
