using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace Conquer_Online_Server.Database
{
    public class KillConnections
    {

        public static void Kill()
        {
            string command = "SHOW processlist";
            List<ulong> processes = new List<ulong>();

            MySqlConnection conn = new MySqlConnection(DataHolder.ConnectionString);
            MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(command, conn);
            MySqlDataReader reader = null;
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ulong identity = ulong.Parse(reader["Id"].ToString());
                    if (reader["Command"].ToString() == "Sleep"
                        && uint.Parse(reader["Time"].ToString()) >= conn.ConnectionTimeout
                        && identity > 0)
                        processes.Add(identity);
                }
                reader.Close();
                reader.Dispose();
                reader = null;

                foreach (int identity in processes)
                {
                    command = "KILL " + identity;
                    cmd.CommandText = command;
                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
                cmd = null;


            }
            catch { }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    conn.Dispose();
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                processes.Clear();
            }
        }
    }
}