//using System;
//using System.Data;
//using MySql.Data.MySqlClient;
//using System.Collections.Generic;
//using Conquer_Online_Server;


//public class KillConnections
//{

//    public static bool CloseConnection(MySqlConnection MyConn)
//    {
//        int PID = MyConn.ServerThread;
//        try
//        {
//            string SQL = "kill " + PID.ToString();
//            //Conquer_Online_Server.Console.WriteLine(" Conection " + SQL + " Closed");
//            MySqlCommand MyCommand = new MySqlCommand(SQL, MyConn);
//            MyCommand.ExecuteNonQuery();
//            MyConn.Close();
//            MyCommand.Dispose();
//        }
//        catch
//        {
//            return false;
//        }
//        return true;
//    }
//    public static string DBName = "";
//    public static string DBUser = "";
//    public static string DBPass = "";
//    public static void AddSomething(string something)
//    {
//        using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection("..."))
//        {
//            connection.Open();
//            // ...
//            connection.Close();
//        }
//    }
//    public static MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand();
//    public static MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();
//    public static string myConnectionString = "Username=" + Program.DBUser + ";Password=" + Program.DBPass + ";Host=localhost;Database=" + Program.DBName + ";Pooling=true; Max Pool Size = 2000000; Min Pool Size = 5";
//    public static void Kill2(MySqlConnection MyConn)
//    {

//        string command = "SHOW processlist";
//        List<ulong> processes = new List<ulong>();
//        MySqlCommand cmd = new MySqlCommand(command, MyConn);
//        MySqlDataReader reader = null;

//        try
//        {
//            // MyConn.Open();

//            reader = cmd.ExecuteReader();
//            while (reader.Read())
//            {
//                ulong identity = ulong.Parse(reader["Id"].ToString());
//                if (reader["Command"].ToString() == "Sleep"
//                    && uint.Parse(reader["Time"].ToString()) >= MyConn.ConnectionTimeout
//                    && identity > 0)
//                {
//                    processes.Add(identity);
//                    // Conquer_Online_Server.Console.WriteLine(" id " + identity + " Added");

//                }
//            }
//            reader.Close();
//            reader.Dispose();
//            reader = null;
//            foreach (int identity in processes)
//            {
//                command = "KILL " + identity;
//                cmd.CommandText = command;
//                cmd.ExecuteNonQuery();
//                //Conquer_Online_Server.Console.WriteLine(" id " + identity + " Closed");
//            }
//            MyConn.Close();
//            MyConn.Dispose();
//            string ConfigFileName = "configuration.ini";
//            Conquer_Online_Server.IniFile IniFile = new Conquer_Online_Server.IniFile(ConfigFileName);
//            {
//                Program.GameIP = IniFile.ReadString("configuration", "IP");
//                Program.GamePort = IniFile.ReadUInt16("configuration", "GamePort");
//               // Program.AuthPort = IniFile.ReadUInt16("configuration", "AuthPort");
//                Conquer_Online_Server.Constants.ServerName = IniFile.ReadString("configuration", "ServerName");
//                Conquer_Online_Server.Database.DataHolder.CreateConnection(IniFile.ReadString("MySql", "Username"), IniFile.ReadString("MySql", "Password"), IniFile.ReadString("MySql", "Database"), IniFile.ReadString("MySql", "Host"));
//                MyConn.Open();
//            }
//        }
//        catch (Exception e) { Conquer_Online_Server.Console.WriteLine(e); }
//    }
//    public static void Kill()
//    {
//        string command = "SHOW processlist";
//        List<ulong> processes = new List<ulong>();
//        MySqlCommand cmd = new MySqlCommand(command, conn);
//        MySqlDataReader reader = null;
//        try
//        {

//            conn.Open();
//            //conn.Open();
//            reader = cmd.ExecuteReader();
//            while (reader.Read())
//            {
//                ulong identity = ulong.Parse(reader["Id"].ToString());
//                if (reader["Command"].ToString() == "Sleep"
//                    && uint.Parse(reader["Time"].ToString()) >= conn.ConnectionTimeout
//                    && identity > 0)
//                    processes.Add(identity);
//            }
//            reader.Close();
//            reader.Dispose();
//            reader = null;

//            foreach (int identity in processes)
//            {
//                command = "KILL " + identity;
//                cmd.CommandText = command;
//                cmd.ExecuteNonQuery();
//            }
//            cmd.Dispose();
//            cmd = null;
//        }
//        catch (Exception e) { Conquer_Online_Server.Console.WriteLine(e); }
//        finally
//        {
//            if (reader != null && !reader.IsClosed)
//            {
//                reader.Close();
//                reader.Dispose();
//                reader = null;
//            }
//            if (conn != null && conn.State == ConnectionState.Open)
//            {
//                conn.Close();
//                conn.Dispose();
//            }
//            if (cmd != null)
//            {
//                cmd.Dispose();
//                cmd = null;
//            }
//        }
//    }
//}