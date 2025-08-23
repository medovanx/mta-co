using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquer_Online_Server.Database
{

    public class KissSystemTable
    {
        public static uint[] TopLetters = new uint[2];
        public static uint[] TopWine = new uint[2];
        public static List<Game.Features.Kisses.Kisses> kisstoday = new List<Game.Features.Kisses.Kisses>();
        public static uint[] TopKisses = new uint[2];
        public static uint[] TopJades = new uint[2];
        private static bool Exists(uint id)
        {
            try
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("kisses").Where("id", id))
                using (var reader = new MySqlReader(cmd))
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        public static void Kisses(Client.GameState Client)
        {
            Client.Entity.Kisses = new Conquer_Online_Server.Game.Features.Kisses.Kisses();
            if (!Exists(Client.Entity.UID))
            {
                Insert(Client);
            }
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("kisses").Where("id", Client.Entity.UID))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    Client.Entity.Kisses.id = reader.ReadUInt32("id");
                    try
                    {
                        Client.Entity.Kisses.LastKissesSent = DateTime.FromBinary(reader.ReadInt64("last_kisses_sent"));
                    }
                    catch
                    {
                    }
                    if ((Client.Entity.Body == 1003) || (Client.Entity.Body == 1004 || Client.Entity.Body == 2003) || (Client.Entity.Body == 2004))
                    {
                        if (Client.Entity.Kisses.LastKissesSent.AddDays(1) <= DateTime.Now)
                        {
                            Client.Entity.Kisses.LastKissesSent = DateTime.Now;
                            Client.Entity.Kisses.Kisses2day = 0;
                            Client.Entity.Kisses.LetterToday1 = 0;
                            Client.Entity.Kisses.Jades2day = 0;
                            Client.Entity.Kisses.Wine2day = 0;
                        }
                        else
                        {
                            Client.Entity.Kisses.Kisses2day = reader.ReadUInt32("kissestoday");
                            Client.Entity.Kisses.LetterToday1 = reader.ReadUInt32("letterstoday");
                            Client.Entity.Kisses.Jades2day = reader.ReadUInt32("Jadestoday");
                            Client.Entity.Kisses.Wine2day = reader.ReadUInt32("winetoday");
                        }
                        Client.Entity.Kisses.Kisses2 = reader.ReadUInt32("kisses");
                        Client.Entity.Kisses.Letters1 = reader.ReadUInt32("letters");
                        Client.Entity.Kisses.Jades = reader.ReadUInt32("Jades");
                        Client.Entity.Kisses.Wine = reader.ReadUInt32("wine");
                        Client.Entity.Kisses.name = reader.ReadString("name");
                        if (kisstoday.Contains(Client.Entity.Kisses))
                        {
                            kisstoday.Remove(Client.Entity.Kisses);
                        }
                        kisstoday.Add(Client.Entity.Kisses);
                    }
                }
            }
        }
        public struct Kissess
        {
            public uint Kisses;
            public string name;
            public uint id;
            public uint Wine;
            public uint Jades;
            public uint Letters;
            public short Body;
        }
        public static List<Kissess> KissList = new List<Kissess>();
        public static void Load()
        {
            Kissess kiss = new Kissess();
            //kiss.Clear();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("kisses"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    kiss.id = reader.ReadUInt32("id");
                    kiss.Kisses = reader.ReadUInt32("kisses");
                    kiss.Letters = reader.ReadUInt32("letters");
                    kiss.Jades = reader.ReadUInt32("Jades");
                    kiss.Wine = reader.ReadUInt32("wine");
                    kiss.name = reader.ReadString("name");
                    kiss.Body = reader.ReadInt16("body");
                    if (kiss.Body == 2001 || kiss.Body == 2002 || kiss.Body == 1002 || kiss.Body == 1001)
                    {
                        KissList.Add(kiss);
                    }
                }
            }
        }

        public static void Insert(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("kisses"))
                cmd.Insert("id", client.Entity.UID)
             .Insert("kisses", client.Entity.Kisses.Kisses2)
             .Insert("kissestoday", client.Entity.Kisses.Kisses2day)
             .Insert("letters", client.Entity.Kisses.Letters1)
             .Insert("letterstoday", client.Entity.Kisses.LetterToday1)
             .Insert("Jades", client.Entity.Kisses.Jades)
             .Insert("Jadestoday", client.Entity.Kisses.Jades2day)
             .Insert("wine", client.Entity.Kisses.Wine)
             .Insert("winetoday", client.Entity.Kisses.Wine2day)
             .Insert("last_kiss_sent", DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToBinary())
             .Insert("name", client.Entity.Name)
             .Insert("body", client.Entity.Body).Execute();
        }

        public static void SaveKissTable(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("kisses")
               .Set("letters", client.Entity.Kisses.Letters1)
               .Set("kisses", client.Entity.Kisses.Kisses2)
               .Set("Jades", client.Entity.Kisses.Jades)
               .Set("wine", client.Entity.Kisses.Wine)
               .Set("letterstoday", client.Entity.Kisses.LetterToday1)
               .Set("kissestoday", client.Entity.Kisses.Kisses2day)
               .Set("Jadestoday", client.Entity.Kisses.Jades2day)
               .Set("winetoday", client.Entity.Kisses.Wine2day)
               .Set("last_kiss_sent", client.Entity.Kisses.LastKissesSent.ToBinary())
               .Where("id", client.Entity.UID).Execute();
        }
    }
}