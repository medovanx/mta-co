using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace MTA.Database
{
    public class EntityVariableTable
    {
        public static void Load(Client.GameState client)
        {
            Load(client.Entity.UID, out client.Variables);
        }
        public static void Load(uint uid, out Game.VariableVault vars)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("variablevault").Where("entityid", uid))
            using (var reader = cmd.CreateReader())
            {
                vars = new Game.VariableVault();
                if (reader.Read())
                {
                    vars.Deserialize(reader.ReadBlob("data"));
                }
                else
                {
                    string SQL = "INSERT INTO `variablevault` (entityid, data) VALUES (@EntityID, @Data)";
                    byte[] rawData = vars.Serialize();
                    using (var conn = Database.DataHolder.MySqlConnection)
                    {
                        conn.Open();
                        using (var cmd2 = new MySql.Data.MySqlClient.MySqlCommand(SQL, conn))
                        {
                            cmd2.Parameters.AddWithValue("@EntityID", uid);
                            cmd2.Parameters.AddWithValue("@Data", rawData);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
      
        public static void Save(Client.GameState client)
        {
            Save(client.Entity.UID, client.Variables);
        }
        public static void Save(uint uid, Game.VariableVault vars)
        {
            if (vars == null) return;
            if (!vars.Changed) return;
            vars.Changed = false; 
            string SQL = "UPDATE `variablevault` set data=@Data where entityid=@EntityID;";
            byte[] rawData = vars.Serialize();
            using (var conn = Database.DataHolder.MySqlConnection)
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(SQL, conn))
                {
                    cmd.Parameters.AddWithValue("@EntityID", uid);
                    cmd.Parameters.AddWithValue("@Data", rawData);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void Save(Client.GameState GameState, MySqlConnection conn)
        {
           
            var vars = GameState.Variables;
            if (vars == null) return;
            if (!vars.Changed) return;
            vars.Changed = false;
            string SQL = "UPDATE `variablevault` set data=@Data where entityid=@EntityID;";
            byte[] rawData = vars.Serialize();

            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(SQL, conn))
            {
                cmd.Parameters.AddWithValue("@EntityID", GameState.Entity.UID);
                cmd.Parameters.AddWithValue("@Data", rawData);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
