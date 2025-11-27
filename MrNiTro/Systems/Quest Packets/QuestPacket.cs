using System;
using System.Collections.Generic;
using MTA.Network;
using MTA.Client;
using System.Linq;
using MTA.Network.GamePackets;
using System.IO;
using MTA.Database;

namespace MTA.MaTrix
{
    public enum QuestType : uint
    {
        RecruitQuest = 1,
        TutorialQuest = 2,
        DailyQuest = 3,
        EquipmentBonus = 4,
        Event = 5,
        Nezha_Feud = 6,
        RegionQuests = 7
    }
    public enum QuestID : uint
    {
        WorshipLeaders = 6329,
        Spirit_Beads = 2375,
        Exorcism = 6126,
        Magnolias = 6014,
        Eth_has_price = 6245,
        Release_the_souls = 6049,
    }

    public class QuestPacket : Writer, Interfaces.IPacket
    {
        public enum QuestAction : ushort
        {
            Begin = 1,
            QuitQuest = 2,
            List = 3,
            Complete = 4
        }

        public struct QuestData
        {
            public enum QuestStatus : uint
            {
                Accepted = 0,
                Finished = 1,
                Available = 2
            }

            public QuestID UID;
            public QuestStatus Status;
            public uint Time;

            public static QuestData Create(QuestID _uid, QuestStatus _status, uint _time)
            {
                QuestData qItem = new QuestData();
                qItem.UID = _uid;
                qItem.Status = _status;
                qItem.Time = _time;
                return qItem;
            }
        }

        byte[] Buffer;
        public QuestPacket(bool Create, int count = 0)
        {
            if (Create)
            {
                Buffer = new byte[8 + 12 + 8 * count];
                WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                WriteUInt16(1134, 2, Buffer);
                Amount = (ushort)count;
            }
        }
        ushort offset = 8;
        public void Apend(QuestData quest)
        {
            Uint((uint)quest.UID, offset, Buffer);//uid 
            offset += 4;
            Uint((uint)quest.Status, offset, Buffer);//sender
            offset += 4;
            Uint(quest.Time, offset, Buffer);//Subject
            offset += 4;
        }

        public QuestAction Action
        {
            get { return (QuestAction)BitConverter.ToUInt16(Buffer, 4); }
            set { WriteUInt16((ushort)value, 4, Buffer); }
        }
        public ushort Amount
        {
            get { return BitConverter.ToUInt16(Buffer, 6); }
            set { WriteUInt16(value, 6, Buffer); }
        }

        public QuestData this[int index]
        {
            get
            {
                QuestData data = new QuestData();
                data.UID = (QuestID)BitConverter.ToUInt32(Buffer, 8 + 12 * index);
                data.Status = (QuestData.QuestStatus)BitConverter.ToUInt32(Buffer, 8 + 12 * index + 4);
                data.Time = BitConverter.ToUInt32(Buffer, 8 + 12 * index + 8);
                return data;
            }
            set
            {
                WriteUInt32((uint)value.UID, 8 + 12 * index, Buffer);
                WriteUInt32((uint)value.Status, 8 + 12 * index + 4, Buffer);
                WriteUInt32(value.Time, 8 + 12 * index + 8, Buffer);
            }
        }

        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
            // check();
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
    public class Quests
    {
        public class QuestItem
        {
            public QuestPacket.QuestData QItem;
            public uint Kills;
            public string Mob;

            public static QuestItem Create(QuestPacket.QuestData Q_Item, uint Kills, string Mob = "")
            {
                QuestItem retn = new QuestItem();
                retn.QItem = Q_Item;
                retn.Mob = Mob;
                retn.Kills = Kills;
                return retn;
            }

            public void WriteItem(BinaryWriter writer)
            {
                writer.Write((uint)QItem.UID); //= reader.ReadUInt32();
                writer.Write((uint)QItem.Status);
                writer.Write(QItem.Time);
                writer.Write(Kills);
                writer.Write(Mob);
            }
            public QuestItem ReadItem(BinaryReader reader)
            {
                QItem = new QuestPacket.QuestData();
                QItem.UID = (QuestID)reader.ReadUInt32();//4
                QItem.Status = (QuestPacket.QuestData.QuestStatus)reader.ReadUInt32();//8
                QItem.Time = reader.ReadUInt32();//10
                Kills = reader.ReadUInt32();//12
                Mob = reader.ReadString();//14                         
                return this;
            }

        }
        public DateTime LastResetTime;
        public GameState Player;
        private SafeDictionary<QuestID, QuestItem> src;
        public Quests(GameState _owner)
        {
            Player = _owner;
            src = new SafeDictionary<QuestID, QuestItem>();
        }
        ///////////////////////////////////////////////////
        ///////////////////////////////////////////////////
        public void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
            {
                cmd.Select("quests").Where("UID", Player.Entity.UID);
                using (MySqlReader rdr = new MySqlReader(cmd))
                {
                    if (rdr.Read())
                    {

                        byte[] data = rdr.ReadBlob("quests");
                        if (data.Length > 0)
                        {
                            using (var stream = new MemoryStream(data))
                            using (var reader = new BinaryReader(stream))
                            {
                                int count = reader.ReadByte();
                                for (uint x = 0; x < count; x++)
                                {
                                    QuestItem item = new QuestItem();
                                    item = item.ReadItem(reader);
                                    src.Add(item.QItem.UID, item);
                                }
                            }
                        }
                        LastResetTime = DateTime.FromBinary(rdr.ReadInt64("LastResetTime"));
                        if (DateTime.Now.DayOfYear != LastResetTime.DayOfYear)
                        {
                            var array = src.Values.Where(p => QuestInfo.CheckType((QuestID)p.QItem.UID) == QuestType.DailyQuest && p.QItem.Status == QuestPacket.QuestData.QuestStatus.Finished).ToArray();
                            for (int i = 0; i < array.Length; i++)
                            {
                                src.Remove(array[i].QItem.UID);
                            }
                            array = null;
                            LastResetTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        using (var command = new MySqlCommand(MySqlCommandType.INSERT))
                        {
                            command.Insert("quests").Insert("UID", Player.Entity.UID).Insert("Name", Player.Entity.Name);
                            command.Execute();
                        }
                    }
                }
            }
        }
        ///////////////////////////////////////////////////
        ///////////////////////////////////////////////////
        public void Save()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)src.Count);
            foreach (var quest in src.Values)
                quest.WriteItem(writer);
            string SQL = "UPDATE `quests` SET quests=@quests,LastResetTime=@rest where UID = " + Player.Entity.UID + " ;";
            byte[] rawData = stream.ToArray();
            using (var conn = DataHolder.MySqlConnection)
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = SQL;
                    cmd.Parameters.AddWithValue("@rest", LastResetTime.Ticks);
                    cmd.Parameters.AddWithValue("@quests", rawData);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        ///////////////////////////////////////////////////
        ///////////////////////////////////////////////////       

        public void IncreaseQuestKills(QuestID UID, uint Kills)
        {
            if (src.ContainsKey(UID) && IsActiveQuest(UID))
            {
                var item = src[UID];
                item.Kills += Kills;
                src[UID] = item;

                QuestData dataq = new QuestData(true);
                dataq.UID = UID;
                dataq[1] = Kills;
                dataq[2] = item.Kills;
                Player.Send(dataq);
            }
        }
        private bool IsActiveQuest(QuestID UID)
        {
            return CheckQuest(UID, QuestPacket.QuestData.QuestStatus.Accepted);
        }
        private bool CheckQuest(QuestID UID, QuestPacket.QuestData.QuestStatus status)
        {
            return src.Values.Where((p) => p.QItem.UID == UID && p.QItem.Status == status).Count() == 1;
        }
        public bool HasQuest(QuestID UID)
        {
            if (src.ContainsKey(UID))
                return true;
            return false;
        }
        public QuestPacket.QuestData.QuestStatus CheckQuest(QuestID UID)
        {
            return src[UID].QItem.Status;
        }
        public QuestItem GetQuest(QuestID UID)
        {
            return src[UID];
        }
        public bool FinishQuest(QuestID UID)
        {
            if (src.ContainsKey(UID))
            {
                var item = src[UID];
                if (item.QItem.Status != QuestPacket.QuestData.QuestStatus.Finished)
                {
                    item.QItem.Status = QuestPacket.QuestData.QuestStatus.Finished;
                    src[UID] = item;
                    SendSinglePacket(item.QItem, QuestPacket.QuestAction.Complete);
                    return true;
                }
            }
            return false;
        }
        public QuestItem Accept(QuestID UID, uint time = 0)
        {
            if (!src.ContainsKey(UID))
            {
                var n_quest = QuestItem.Create(QuestPacket.QuestData.Create(UID, QuestPacket.QuestData.QuestStatus.Accepted, time), 0);
                src.Add(n_quest.QItem.UID, n_quest);
                SendSinglePacket(n_quest.QItem, QuestPacket.QuestAction.Begin);
                return n_quest;
            }
            return new QuestItem();
        }

        private void SendSinglePacket(QuestPacket.QuestData data, QuestPacket.QuestAction mode)
        {
            QuestPacket Packet = new QuestPacket(true, 1);
            Packet.Action = mode;
            Packet.Apend(data);
            Player.Send(Packet);
        }
        private int AcceptQuestsCount()
        {
            return src.Values.Where((p) => p.QItem.Status == QuestPacket.QuestData.QuestStatus.Accepted).Count();
        }
        public bool AllowAccept()
        {
            return AcceptQuestsCount() < 5;
        }
        private void SendAutoPacket(string Text, ushort map, ushort x, ushort y, uint NpcUid)
        {
            Player.MessageBox(Text, p =>
            {
                Data datapacket = new Data(true);
                datapacket.UID = p.Entity.UID;
                datapacket.ID = Data.AutoPatcher;
                datapacket.TimeStamp2 = NpcUid;
                datapacket.wParam1 = x;
                datapacket.wParam2 = y;
                p.Send(datapacket);

            }, null, 0);
        }
        public void SendFullGUI()
        {
            //if (!src.ContainsKey(804))//first Quest 804
            //{
            //    var n_quest = QuestItem.Create(QuestPacket.QuestData.Create(804, QuestPacket.QuestData.QuestStatus.Accepted, 0), 0);
            //    src.Add(n_quest.QItem.UID, n_quest);

            //    SendAutoPacket("Welcome!~Pay~a~visit~to~the~Kungfu~Boy~to~learn~more~about~the~world!", 1002, 270, 223, 5673);
            //}

            Dictionary<int, Queue<QuestPacket.QuestData>> Collection = new Dictionary<int, Queue<QuestPacket.QuestData>>();
            Collection.Add(0, new Queue<QuestPacket.QuestData>());

            int count = 0;
            var Array = QuestInfo.AllQuests.Values.ToArray();

            for (uint x = 0; x < Array.Length; x++)
            {
                if (x % 80 == 0)
                {
                    count++;
                    Collection.Add(count, new Queue<QuestPacket.QuestData>());
                }
                if (src.ContainsKey((QuestID)Array[x].MissionId))
                    Collection[count].Enqueue(src[Array[x].MissionId].QItem);
                else
                {
                    var quest = QuestPacket.QuestData.Create(Array[x].MissionId,
                        QuestPacket.QuestData.QuestStatus.Available, 0);
                    Collection[count].Enqueue(quest);
                }
            }
            foreach (var aray in Collection.Values)
            {
                Queue<QuestPacket.QuestData> ItemArray = aray;

                QuestPacket Packet = new QuestPacket(true, ItemArray.Count);
                Packet.Action = QuestPacket.QuestAction.List;
                for (byte x = 0; x < Packet.Amount; x++)
                {
                    var item = ItemArray.Dequeue();
                    if (QuestInfo.CheckType(item.UID) == QuestType.DailyQuest)
                        item.Status = QuestPacket.QuestData.QuestStatus.Available;
                    Packet.Apend(item);
                }
                Player.Send(Packet);
            }
        }

        public bool QuitQuest(QuestID UID)
        {
            if (src.ContainsKey(UID))
            {
                var item = src[UID];
                if (item.QItem.Status == QuestPacket.QuestData.QuestStatus.Accepted)
                {
                    item.QItem.Status = QuestPacket.QuestData.QuestStatus.Available;
                    src[UID] = item;
                    SendSinglePacket(item.QItem, QuestPacket.QuestAction.QuitQuest);
                    src.Remove(UID);
                    return true;
                }
            }
            return false;
        }
    }
    public class QuestInfo
    {
        public struct Info
        {
            public QuestID MissionId;
            public QuestType TypeId;
            // public uint NextMissionId;

        }
        public static uint ActionBase;
        public static SafeDictionary<QuestID, Info> AllQuests = new SafeDictionary<QuestID, Info>();
        public static void Load()
        {
            string[] text = File.ReadAllLines(Constants.DatabaseBasePath + "Questinfo.ini");
            Info info = new Info();
            for (int x = 0; x < text.Length; x++)
            {
                string line = text[x];
                string[] split = line.Split('=');
                if (split[0] == "ActionBase")
                    ActionBase = uint.Parse(split[1]);
                else if (split[0] == "TotalMission")
                    AllQuests = new SafeDictionary<QuestID, Info>(int.Parse(split[1]));
                else if (split[0] == "MissionId")
                {
                    var id = (QuestID)uint.Parse(split[1]);

                    if (!AllQuests.ContainsKey(id))
                        AllQuests.Add(id, info);
                    else
                        info = AllQuests[id];

                    info.MissionId = id;

                }
                else if (split[0] == "TypeId")
                    info.TypeId = (QuestType)byte.Parse(split[1]);
            }
        }
        public static QuestType CheckType(QuestID UID)
        {
            return AllQuests[UID].TypeId;
        }
    }
    public class QuestData : Writer, Interfaces.IPacket
    {
        byte[] Buffer;
        public QuestData(bool Create)
        {
            if (true)
            {
                Buffer = new byte[60];
                WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                WriteUInt16(1135, 2, Buffer);
            }
        }

        public QuestID UID
        {
            get { return (QuestID)BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32((uint)value, 8, Buffer); }
        }

        public uint this[int index]
        {
            get { return BitConverter.ToUInt32(Buffer, 8 + 4 * index); }
            set { WriteUInt32(value, 8 + 4 * index, Buffer); }
        }

        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}
