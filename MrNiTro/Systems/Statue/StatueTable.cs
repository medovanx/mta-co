using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MTA.Database
{
    public class Statue
    {
        public static void Save()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            bool WasAdd = false;
            foreach (var statu in Game.Statue.Statues.Values)
            {
                if (statu.SpawnPacket != null)
                {
                    if (statu.SpawnPacket.Length > 200)
                    {
                        WasAdd = true;
                        writer.Write(statu.SpawnPacket.Length);
                        writer.Write(statu.SpawnPacket);
                    }
                }
            }
            if (!WasAdd)
                writer.Write(0);
            string SQL = "UPDATE `statue` SET data=@Data where UID = " + 105175 + " ;";
            byte[] rawData = stream.ToArray();
            using (var conn = Database.DataHolder.MySqlConnection)
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = SQL;
                    cmd.Parameters.AddWithValue("@Data", rawData);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("statue").Where("UID", 105175))
            using (var reader = cmd.CreateReader())
            {
                if (reader.Read())
                {
                    using (var stream = new MemoryStream((reader.ReadBlob("data"))))
                    using (var hreader = new BinaryReader(stream))
                    {
                        uint Count = hreader.ReadUInt32();
                        byte[] array = new byte[Count];
                        for (int x = 0; x < Count; x++)
                            array[x] = hreader.ReadByte();
                        new Game.Statue(array);
                    }
                }
                else
                {

                    string SQL = "INSERT INTO `statue` (uid, data) VALUES (@UID, @Data)";
                    MemoryStream stream = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(0);
                    byte[] rawData = stream.ToArray();
                    using (var conn = Database.DataHolder.MySqlConnection)
                    {
                        conn.Open();
                        using (var cmd2 = new MySql.Data.MySqlClient.MySqlCommand(SQL, conn))
                        {
                            cmd2.Parameters.AddWithValue("@UID", 105175);
                            cmd2.Parameters.AddWithValue("@Data", rawData);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                }
            }


        }
    }
}
