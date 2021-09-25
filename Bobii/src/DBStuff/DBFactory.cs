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
        //Gehört hier nicht hin, Lösung finden!
        public static async Task DeleteEverythingFromGuild(string guildID)
        {
            var tableList = new List<string>();
            tableList.Add("createtempchannels");
            tableList.Add("filterwords");
            tableList.Add("tempchannels");
            tableList.Add("filterlinksguild");
            tableList.Add("filterlinkuserguild");
            tableList.Add("filterlink");

            foreach (string table in tableList)
            {
                try
                {
                    DBFactory.ExecuteQuery($"DELETE FROM {table} WHERE guildid = '{guildID}'");
                    WriteToConsol($"Information: | Method: RemoveGuild_{table} | Guild: {guildID} | Successfull removed");
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    WriteToConsol($"Error: | Method: RemoveGuild_{table} | Guild: {guildID} | {ex.Message}");
                    return;
                }
            }
            await Task.CompletedTask;
        }

        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} DBFactory   {message}");
            await Task.CompletedTask;
        }




        public static void ExecuteQuery(string query)
        {
            using (NpgsqlConnection connection = GetConnection())
            {
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
                        connection.Close();
                        return;
                    }
                }
            }
        }
        #endregion 

        #region Functions
        public static DataTable SelectData(string query)
        {
            using (NpgsqlConnection connection = GetConnection())
            {
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
                        connection.Close();
                        return null;
                    }

                    connection.Close();
                    return _dt;
                }
            }
        }

        public static NpgsqlConnection GetConnection()
        {
            var config = Program.GetConfig();
            return new NpgsqlConnection(config["BobiiConfig"][0].Value<string>("ConnectionString"));
        }

        public static int GetCountOfAllRows(string table)
        {
            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                var query = $"SELECT count(*) FROM {table}";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Prepare();
                        var returnableInt = Convert.ToInt32(cmd.ExecuteScalar());

                        connection.Close();

                        return returnableInt;
                    }
                    catch (Exception ex)
                    {
                        WriteToConsol($"Error: | Function: GetNewID | {ex.Message} ");
                        connection.Close();
                        return 0;
                    }
                }
            }

        }

        public static long GetNewID(string table)
        {
            using (NpgsqlConnection connection = GetConnection())
            {
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

                        connection.Close();

                        foreach (DataRow row in rowsTable.Rows)
                        {
                            int id = row.Field<int>("id");
                            return id + 1;
                        }
                        return 1;
                    }
                    catch (Exception ex)
                    {
                        WriteToConsol($"Error: | Function: GetNewID | {ex.Message} ");
                        connection.Close();
                        return 0;
                    }
                }
            }
        }
        #endregion
    }
}
