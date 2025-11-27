//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MTA.Client;
//using MTA.Game;
//using MTA.Database;
//using System.IO;
//using MTA.Network.GamePackets;

//namespace MTA.MaTrix
//{
//    public class Mannequin
//    {
//        public class MannequinEntity
//        {
//            public uint UID;
//            public string Name;
//            public ushort Face;
//            public ushort Body;
//            public uint Mesh
//            {
//                get
//                {
//                    return (uint)(Face * 10000 + Body);
//                }
//            }
//        }
//        public Map Map;
//        public GameState Main;
//        public SafeDictionary<uint, MannequinEntity> OtherUIDs = new SafeDictionary<uint, MannequinEntity>();
//        public Mannequin(GameState client)
//        {
//            Main = client;           
//            Main.Entity = new Entity(EntityFlag.Player, false);
//            var entity = LoadEntity(Main.Account.EntityID);
//            Main.Entity.UID = entity.UID;
//            Main.Entity.Name = entity.Name;
//            Main.Entity.Body = entity.Body;
//            Main.Entity.Face = entity.Face;
//            Load();          
//            Map = new Map((ushort)(1010 + client.Entity.UID), 1010, Database.DMaps.MapPaths[1010]);
//        //    Teleport(50,50);
//        }
//        ~Mannequin()
//        {
//            OtherUIDs.Clear();
//            Map.Dispose();
//        }
//        public void Load()
//        {
//            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
//            {
//                cmd.Select("accounts_Mannequin").Where("UID", Main.Account.EntityID);
//                using (MySqlReader rdr = new MySqlReader(cmd))
//                {
//                    if (rdr.Read())
//                    {
//                        byte[] data = rdr.ReadBlob("Mannequin");
//                        if (data.Length > 0)
//                        {
//                            using (var stream = new MemoryStream(data))
//                            using (var reader = new BinaryReader(stream))
//                            {
//                                int count = reader.ReadByte();
//                                for (uint x = 0; x < count; x++)
//                                {
//                                    var uid = reader.ReadUInt32();
//                                    var entity = LoadEntity(uid);
//                                    if (entity != null)
//                                    {
//                                        if (!OtherUIDs.ContainsKey(entity.UID))
//                                            OtherUIDs.Add(entity.UID, entity);
//                                    }
//                                }

//                            }
//                        }
//                    }
//                    else
//                    {
//                        using (var command = new MySqlCommand(MySqlCommandType.INSERT))
//                        {
//                            command.Insert("accounts_Mannequin").Insert("UID", Main.Account.EntityID).Insert("Name", Main.Entity.Name);
//                            command.Execute();
//                        }
//                    }
//                }
//            }
//        }
//        public void Save()
//        {
//            MemoryStream stream = new MemoryStream();
//            BinaryWriter writer = new BinaryWriter(stream);
//            writer.Write((byte)OtherUIDs.Count);
//            foreach (var item in OtherUIDs.Values)            
//                writer.Write(item.UID);            

//            string SQL = "UPDATE `accounts_Mannequin` SET Mannequin=@Mannequin where UID = " + Main.Account.EntityID + " ;";
//            byte[] rawData = stream.ToArray();
//            using (var conn = DataHolder.MySqlConnection)
//            {
//                conn.Open();
//                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
//                {
//                    cmd.Connection = conn;
//                    cmd.CommandText = SQL;
//                    cmd.Parameters.AddWithValue("@Mannequin", rawData);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }
//        private MannequinEntity LoadEntity(uint UID)
//        {
//            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("UID", UID))
//            using (var reader = new MySqlReader(cmd))
//            {
//                if (reader.Read())
//                {
//                    MannequinEntity entity = new MannequinEntity();
//                    entity.UID = UID;
//                    entity.Name = reader.ReadString("Name");
//                    entity.Body = reader.ReadUInt16("Body");
//                    entity.Face = reader.ReadUInt16("Face");                                   
//                    return entity;
//                }
//                else
//                    return null;
//            }
//        }
//        public void Teleport(ushort X,ushort Y)
//        {
//            Interfaces.INpc Mainnpc = new NpcSpawn();
//            Mainnpc.UID = Main.Entity.UID;
//            Mainnpc.MapID = Map.ID;
//            Mainnpc.Name = Main.Entity.Name;
//            Mainnpc.Type = Enums.NpcType.Talker;
//            Mainnpc.Mesh = (ushort)Main.Entity.Mesh;
//            Mainnpc.X = 50;
//            Mainnpc.Y = 50;
//            Map.Npcs.Add(Mainnpc.UID, Mainnpc);
//            if (OtherUIDs.Count > 0)
//            {
//                foreach (var item in OtherUIDs.Values)
//                {
//                    Interfaces.INpc npc = new NpcSpawn();
//                    npc.UID = item.UID;
//                    npc.MapID = Map.ID;
//                    npc.Name = item.Name;
//                    npc.Type = Enums.NpcType.Talker;
//                    npc.Mesh = (ushort)item.Mesh;
//                    npc.X = 50;
//                    npc.Y = 50;
//                }
//            }
//            if (Main.Entity.EntityFlag == EntityFlag.Player)
//            {
//                if (!Database.DMaps.MapPaths.ContainsKey(Map.BaseID))
//                    return;
//                Main.Entity.X = X;
//                Main.Entity.Y = Y;              
//                Main.Entity.MapID =Map.ID;
//                Network.GamePackets.Data Data = new Network.GamePackets.Data(true);
//                Data.UID = Main.Entity.UID;
//                Data.ID = Network.GamePackets.Data.Teleport;
//                Data.dwParam = Map.BaseID;
//                Data.wParam1 = X;
//                Data.wParam2 = Y;
//                Main.Send(Data);
//                Main.Entity.Action = MTA.Game.Enums.ConquerAction.None;
//                Main.ReviveStamp = Time32.Now;
//                Main.Attackable = false;
//                Main.Send(new MapStatus() { BaseID = Main.Map.BaseID, ID = Main.Map.ID, Status = Database.MapsTable.MapInformations[Main.Map.BaseID].Status, Weather = Database.MapsTable.MapInformations[Main.Map.BaseID].Weather });               
//            }
//        }
//    }
//}
