using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Database
{
    public class GuildArsenalTable
    {
        public static void Load(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guildarsenal").Where("ID", guild.ID))
            using (var rdr = new MySqlReader(cmd))
            {
                if (rdr.Read())
                {
                    var array = rdr.ReadBlob("Data");
                    if (array == null) return;
                    MemoryStream stream = new MemoryStream(rdr.ReadBlob("Data"));
                    BinaryReader reader = new BinaryReader(stream);
                    for (int i = 0; i < 8; i++)
                        guild.Arsenals[i].Load(reader);
                    guild.ArsenalBPChanged = true;
                    guild.GetMaxSharedBattlepower();
                }
                else
                {
                    Insert(guild.ID);
                }
            }
        }
        public static void Save(Guild guild)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            for (int i = 0; i < 8; i++)
                guild.Arsenals[i].Save(writer);
            string SQL = "UPDATE `guildarsenal` SET data=@Data, datalength=@DataLength where ID = " + guild.ID + " ;";
            byte[] rawData = stream.ToArray();
            using (var conn = Database.DataHolder.MySqlConnection)
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = SQL;
                    cmd.Parameters.AddWithValue("@Data", rawData);
                    cmd.Parameters.AddWithValue("@DataLength", rawData.Length);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void SaveAll()
        {
            foreach (var guild in Kernel.Guilds.Values)
                Save(guild);
        }
        public static void Insert(uint id)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
            {
                cmd.Insert("guildarsenal").Insert("ID", id);
                cmd.Execute();
            }
        }
    }
}
