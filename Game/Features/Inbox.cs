using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Interfaces;
using MTA.Network;

namespace MTA.MaTrix {
    public class Inbox : Writer, IPacket {
        ///////////////////////////////////////////////////
        ///////////////////////////////////////////////////       


        byte[] Buffer;

        List<PrizeInfo> list = [];
        ushort offset = 16;
        Random R = new Random();

        public Inbox() {
            Buffer = new byte[20];
            Ushort((ushort)(Buffer.Length - 8), 0, Buffer);
            Ushort(1046, 2, Buffer);
        }

        public uint Count {
            get { return BitConverter.ReadUint(Buffer, 4); }
            set { WriteUint(value, 4, Buffer); }
        }

        public uint Page {
            get { return BitConverter.ReadUint(Buffer, 8); }
            set { WriteUint(value, 8, Buffer); }
        }

        public uint unknown {
            get { return BitConverter.ReadUint(Buffer, 12); }
            set { WriteUint(value, 12, Buffer); }
        }

        public void Send(GameState client) {
            client.Send(ToArray());
        }

        public void Deserialize(byte[] Data) {
            Buffer = Data;
        }

        public byte[] ToArray() {
            return Buffer;
        }

        public static void SendInbox(GameState client, bool On) {
            byte[] test = new byte[12 + 8];
            WriteUshort((ushort)(test.Length - 8), 0, test);
            WriteUshort(1047, 2, test);
            if (On)
                Byte(3, 4, test);
            else
                Byte(2, 4, test);
            client.Send(test);
        }

        public static void AddPrize(GameState client, string Sender = "Matrix", string Subject = "Inbox Test",
            string Message = "Message...", uint money = 500000, uint cps = 500000, uint Time = 600,
            Action<GameState> action = null) {
            int id = 0;
            do {
                id = Kernel.Random.Next();
            } while (client.Prizes.ContainsKey((uint)id));

            PrizeInfo prize = new PrizeInfo();
            prize.ID = (uint)id;
            prize.Sender = Sender;
            prize.Subject = Subject;
            prize.Message = Message;
            prize.goldprize = money;
            prize.cpsprize = cps;
            prize.Time = Time;
            prize.itemprize = action;
            client.Prizes.Add(prize.ID, prize);
            if (client.Prizes.Count > 0)
                SendInbox(client, true);
        }

        ///////////////////////////////////////////////////
        ///////////////////////////////////////////////////
        public static void Load(GameState client) {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT)) {
                cmd.Select("prizes").Where("UID", client.Entity.UID);
                using (MySqlReader rdr = new MySqlReader(cmd)) {
                    if (rdr.Read()) {
                        byte[] data = rdr.ReadBlob("Prizes");
                        if (data.Length > 0) {
                            using (var stream = new MemoryStream(data))
                            using (var reader = new BinaryReader(stream)) {
                                int count = reader.ReadByte();
                                for (uint x = 0; x < count; x++) {
                                    PrizeInfo item = new PrizeInfo();
                                    item = item.ReadItem(reader);
                                    client.Prizes.Add(item.ID, item);
                                }
                            }
                        }
                    }
                    else {
                        using (var command = new MySqlCommand(MySqlCommandType.INSERT)) {
                            command.Insert("prizes").Insert("UID", client.Entity.UID)
                                .Insert("Name", client.Entity.Name);
                            command.Execute();
                        }
                    }
                }
            }
        }

        ///////////////////////////////////////////////////
        ///////////////////////////////////////////////////
        public static void Save(GameState client) {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)client.Prizes.Count);
            foreach (var prize in client.Prizes.Values)
                prize.WriteItem(writer);
            string SQL = "UPDATE `prizes` SET Prizes=@Prizes where UID = " + client.Entity.UID + " ;";
            byte[] rawData = stream.ToArray();
            using (var conn = DataHolder.MySqlConnection) {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand()) {
                    cmd.Connection = conn;
                    cmd.CommandText = SQL;
                    cmd.Parameters.AddWithValue("@Prizes", rawData);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void check(SafeDictionary<uint, PrizeInfo> Prizes, uint page) {
            List<PrizeInfo> prizes = Prizes.Values.ToList();
            list.Clear();
            for (int i = (int)page; i < page + 7; i++) {
                if (i < prizes.Count) {
                    list.Add(prizes[i]);
                }
            }

            if (list.Count > 0) {
                Buffer = new byte[20 + 92 * list.Count];
                Ushort((ushort)(Buffer.Length - 8), 0, Buffer);
                Ushort(1046, 2, Buffer);
                Count = (uint)list.Count;
                Page = page;
                unknown = (uint)prizes.Count;
                for (int i = 0; i < list.Count; i++)
                    Apend(list[i]);
            }
        }

        public void Apend(PrizeInfo prize) {
            Uint(prize.ID, offset, Buffer); //uid 
            offset += 4;
            String(prize.Sender, offset, Buffer); //sender
            offset += 32;
            String(prize.Subject, offset, Buffer); //Subject
            offset += 32;

            Uint(prize.goldprize, offset, Buffer); //attachment
            offset += 4;

            Uint(prize.cpsprize, offset, Buffer); //attachment
            offset += 4;

            Uint(prize.Time, offset, Buffer); //Time
            offset += 4;

            Uint(prize.MessageOrGift ? (byte)1 : (byte)0, offset, Buffer); // image
            offset += 4;

            Uint(1 /*prize.itemprize != null ? (byte)1 : (byte)0*/, offset, Buffer); //attachment
            offset += 4;
        }

        public class PrizeInfo {
            public uint cpsprize;
            public uint goldprize;
            public uint ID;
            public Action<GameState> itemprize;
            public string Message;
            public bool MessageOrGift;
            public string Sender;
            public string Subject;
            public uint Time;

            public void WriteItem(BinaryWriter writer) {
                writer.Write(ID); //= reader.ReadUInt32();
                writer.Write(Time);
                writer.Write(Sender);
                writer.Write(Subject);
                writer.Write(Message);
                writer.Write(goldprize);
                writer.Write(cpsprize);
            }

            public PrizeInfo ReadItem(BinaryReader reader) {
                ID = reader.ReadUInt32(); //4
                Time = reader.ReadUInt32(); //8
                Sender = reader.ReadString(); //10
                Subject = reader.ReadString(); //12
                Message = reader.ReadString(); //14
                goldprize = reader.ReadUInt32(); //18
                cpsprize = reader.ReadUInt32(); //22            
                return this;
            }
        }
    }
}