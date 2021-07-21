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
        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} DBFactory    {message}");
            await Task.CompletedTask;
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
                    WriteToConsol($"Error: | Method: ExecuteQuery | Query: {query} | {ex.Message} ");
                    return;
                }
            }
        }
        #endregion 

        #region Functions
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
                    WriteToConsol($"Error: | Function: SelectData | Query: {query} | {ex.Message} "); 
                    return null;
                }

                connection.Close();
                return _dt;
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
            catch (Exception ex)
            {
                WriteToConsol($"Error: | Function: GetConnection | {ex.Message} "); 
                return false;
            }
        }

        public static long GetNewID(string table)
        {
            // §TODO 03.07.2021/JG Schauen wie ich das mit dem return löse, da 0 nicht null ist...
            if (!CheckConnectionString())
            {
                return 0;
            }

            var connection = GetConnection();
            connection.Open();

            var query = $"SELECT count(*) FROM {table}";
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                try
                {
                    cmd.Prepare();
                    var count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    query = $"SELECT * FROM {table} ORDER BY id DESC";
                    DataTable rowsTable = SelectData(query);
                    foreach(DataRow row in rowsTable.Rows)
                    {
                        int id = row.Field<int>("id");
                        return id + 1;
                    }
                    return 1;
                }
                catch (Exception ex)
                {
                    WriteToConsol($"Error: | Function: GetNewID | {ex.Message} "); 
                    return 0;
                }
            }
        }
        #endregion
    }
}
