using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Database
{
    internal class GameUpdatess
    {
        public static string Header = "";
        public static string Body1 = "";
        public static string Body2 = "";
        public static string Body3 = "";
        public static string Body4 = "";
        public static string Body5 = "";
        public static string Body6 = "";
        public static string Date = "";


        public static void LoadRates()
        {
            MySqlReader reader = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT).Select("gameupdates"));
            if (reader.Read())
            {
                Header = reader.ReadString("Header");
                Body1 = reader.ReadString("Body1");
                Body2 = reader.ReadString("Body2");
                Body3 = reader.ReadString("Body3");
                Body4 = reader.ReadString("Body4");
                Body5 = reader.ReadString("Body5");
                Body6 = reader.ReadString("Body6");
              //  Body6 = reader.ReadString("Date");


            }
            MTA.Console.WriteLine("System GameUpdates  Loaded.");

        }
    }
}
