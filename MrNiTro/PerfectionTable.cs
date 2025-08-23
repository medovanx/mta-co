
//MenaMaGice///
using ProtoBuf;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using MTA;
using MTA.Database;

namespace MTA.Network.GamePackets
{
    public class PerfectionTable
    {
        public static Dictionary<uint, S> CostList = new Dictionary<uint, S>();
        public static List<U> UpgradeList = new List<U>();
        public static Dictionary<uint, A> AbilitiesList = new Dictionary<uint, A>();
        public static Dictionary<uint, St> Garments = new Dictionary<uint, St>();
        public static Dictionary<uint, St> Mounts = new Dictionary<uint, St>();
        public class S
        {
            public uint ID;
            public ushort Prog;
        }
        public class U
        {
            public byte Po;
            public ushort Le;
            public ushort Pr;
        }
        public class A
        {
            public uint Id;
            public uint Min;
            public uint Max;
            public uint Score;
            public A.AbilitiesSort Sort;
            public enum AbilitiesSort : byte
            {
                Jiang = 1,
                Chi = 2,
                InnerPower = 3,
                Enchant = 4,
                Level = 5,
                PerfectionLevel = 6,
                Attributes = 7,
                Rebirth = 8,
                Quality = 9,
                Sockets = 10,
                GemQuality = 11,
                Plus = 12,
                SoulPhase = 13,
                MaterialPhase = 14,
                Protection = 15,
                SubClass = 16,
                Garments = 17,
                MountArmors = 18,
                NobilityRank = 19,
                ItemLevel = 20,
            }
        }
        public class St
        {
            public uint ID;
            public uint Unknown;
            public uint Stars;
            public byte Type;
        }
        public static string Cost = Constants.DatabaseBasePath + "item_refine_cost.txt";
        public static string Upgrade = Constants.DatabaseBasePath + "item_refine_upgrade.txt";
        public static string Ability = Constants.DatabaseBasePath + "ability_score.txt";
        public static string Storage = Constants.DatabaseBasePath + "coat_storage_type.txt";
        public static uint AmountStarGarments(Client.GameState client, byte Star)
        {
            uint Count = 0;
            foreach (var Garment in client.Entity.StorageItems.Values)
            {
                PerfectionTable.St item;
                if (Garments.TryGetValue(Garment.ID, out item))
                {
                    if (item.Type != 1)
                        continue;
                    if (item.Stars >= Star)
                        Count++;
                }
            }
            return Count;
        }
        public static uint PerfectionPoints(Client.GameState client, bool Garment)
        {
            uint Count = 0;
            foreach (var item2 in client.Entity.StorageItems.Values)
            {
                PerfectionTable.St item;
                if (Garment)
                {
                    if (Garments.TryGetValue(item2.ID, out item))
                    {
                        if (item.Type != 1)
                            continue;
                        Count += 50;
                    }
                }
                else
                {
                    if (Mounts.TryGetValue(item2.ID, out item))
                    {
                        if (item.Type != 2)
                            continue;
                        Count += 50;
                    }
                }
            }
            return Count;
        }
        public static uint AmountStarMounts(Client.GameState client, byte Star)
        {
            uint Count = 0;
            foreach (var Mount in client.Entity.StorageItems.Values)
            {
                PerfectionTable.St item;
                if (Mounts.TryGetValue(Mount.ID, out item))
                {
                    if (item.Type != 2)
                        continue;
                    if (item.Stars >= Star)
                        Count++;
                }
            }
            return Count;
        }
        public static void Load()
        {
            if (File.Exists(Cost))
            {
                string[] L = File.ReadAllLines(Cost);
                foreach (var l in L)
                {
                    var S = l.Split(new string[] { "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    S A = new S();
                    A.ID = Convert.ToUInt32(S[0]);
                    A.Prog = Convert.ToUInt16(S[1]);
                    CostList.Add(A.ID, A);
                }
            }
            if (File.Exists(Upgrade))
            {
                string[] L = File.ReadAllLines(Upgrade);
                foreach (var l in L)
                {
                    var S = l.Split(new string[] { "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    U A = new U();
                    A.Po = Convert.ToByte(S[0]);
                    A.Le = Convert.ToUInt16(S[1]);
                    A.Pr = Convert.ToUInt16(S[2]);
                    UpgradeList.Add(A);
                }
            }
            if (File.Exists(Ability))
            {
                string[] L = File.ReadAllLines(Ability);
                foreach (var l in L)
                {
                    var S = l.Split(new string[] { "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    A AA = new A();
                    AA.Id = Convert.ToUInt32(S[0]);
                    AA.Sort = (A.AbilitiesSort)Convert.ToByte(S[1]);
                    AA.Min = Convert.ToUInt32(S[2]);
                    AA.Max = Convert.ToUInt32(S[3]);
                    AA.Score = Convert.ToUInt32(S[4]);
                    AbilitiesList.Add(AA.Id, AA);
                }
            }
            if (File.Exists(Storage))
            {
                string[] L = System.IO.File.ReadAllLines(Storage);
                foreach (var l in L)
                {
                    var S = l.Split(new string[] { "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    St SS = new St();
                    SS.ID = uint.Parse(S[0]);
                    SS.Unknown = uint.Parse(S[1]);
                    SS.Type = byte.Parse(S[2]);
                    SS.Stars = uint.Parse(S[7]);
                    if (SS.Type == 1)
                        Garments.Add(SS.ID, SS);
                    else
                        Mounts.Add(SS.ID, SS);
                }
            }
        }
    }
    public class Proto
    {
        public static byte[] FinalizeProtoBuf(object Pro, ushort ID)
        {
            byte[] ProtoBuff;
            using (var ms = new System.IO.MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, Pro);
                ProtoBuff = ms.ToArray();
                byte[] buffer;
                buffer = new byte[12 + ProtoBuff.Length];
                System.Buffer.BlockCopy(ProtoBuff, 0, buffer, 4, ProtoBuff.Length);
                Network.Writer.Write(buffer.Length - 8, 0, buffer);
                Network.Writer.Write(ID, 2, buffer);
                return buffer;
            }
        }
        public static void Pro(ConquerItem I, Client.GameState C)
        {
            var P = Proto.FinalizeProtoBuf((new PerfectionItems.Item()
            {
                IUID = I.UID,
                EUID = C.Entity.UID,
                OUID = I.OwnerID,
                OName = I.OwnerName,
                Progress = I.PerfectionProgress,
                Level = I.Perfectionlevel
            }), 3250);
            C.Send(P);
        }
    }
    public sealed class PerfectionGUI
    {
        public PerfectionGUI() { }
        public PerfectionProto Info;
        [ProtoContract]
        public class PerfectionProto
        {
            [ProtoMember(1, IsRequired = true)]
            public uint ID;
            [ProtoMember(2, IsRequired = true)]
            public uint LevelMax;
        }
        public void Handle(Client.GameState client)
        {
            switch (Info.ID)
            {
                case 0:
                    {
                        client.Send(Proto.FinalizeProtoBuf(new PerfectionProto()
                        {
                            ID = 0,
                            LevelMax = 54
                        }, 3255));
                        break;
                    }
            }
        }
        public bool R(byte[] Packet)
        {
            var P = new byte[Packet.Length - 4];
            Array.Copy(Packet, 4, P, 0, P.Length);
            using (var memoryStream = new MemoryStream(Packet))
            {
                Info = Serializer.DeserializeWithLengthPrefix<PerfectionProto>(memoryStream, PrefixStyle.Fixed32);
            }
            return true;
        }
    }
    public sealed class PerfectionItems
    {
        public PerfectionItems() { }
        public GUI Info;
        [ProtoContract]
        public class GUI
        {
            [ProtoMember(1, IsRequired = true)]
            public uint ID;
            [ProtoMember(2, IsRequired = true)]
            public uint ItemUID;
            [ProtoMember(3, IsRequired = true)]
            public string Signature;
            [ProtoMember(4, IsRequired = true)]
            public uint ItemPlusUID;
        }
        [ProtoContract]
        public class Item
        {
            [ProtoMember(1, IsRequired = true)]
            public uint IUID;
            [ProtoMember(2, IsRequired = true)]
            public uint EUID;
            [ProtoMember(3, IsRequired = true)]
            public uint Level;
            [ProtoMember(4, IsRequired = true)]
            public uint Progress;
            [ProtoMember(5, IsRequired = true)]
            public uint OUID;
            [ProtoMember(6, IsRequired = true)]
            public string OName;
        }
        public double CP(uint value)
        {
            return (value * 1.25);
        }
        public uint GetProgress(ConquerItem item)
        {
            byte U = item.Perfectionlevel;
            if (U == 0) return 30;
            if (U == 1) return 60;
            if (U == 2) return 100;
            if (U == 3) return 200;
            if (U == 4) return 350;
            if (U == 5) return 600;
            if (U == 6) return 1000;
            if (U == 7) return 1500;
            if (U == 8) return 2300;
            if (U == 9) return 3500;
            if (U == 10) return 5000;
            if (U == 11) return 6500;
            if (U == 12) return 8000;
            if (U == 13) return 9500;
            if (U == 14) return 11000;
            return 12000;
        }
        public void Handle(byte[] Packet, Client.GameState C)
        {
            try
            {
                switch (Info.ID)
                {
                    #region Temper
                    case 0:
                        {
                            ConquerItem Item;
                            ConquerItem ItemPlus;
                            if (C.Equipment.TryGetItem(Info.ItemUID, out Item) && C.Inventory.TryGetItem(Info.ItemPlusUID, out ItemPlus))
                            {
                                if (ItemPlus.ID == 3009000 || ItemPlus.ID == 3009001 || ItemPlus.ID == 3009002 || ItemPlus.ID == 3009003)
                                {
                                    Item.PerfectionProgress += PerfectionTable.CostList[ItemPlus.ID].Prog;
                                    while (Item.PerfectionProgress >= GetProgress(Item))
                                    {
                                        if (Item.Perfectionlevel == 0)
                                        {
                                            Item.OwnerID = C.Entity.UID;
                                            Item.OwnerName = C.Entity.Name;
                                        }
                                        Item.PerfectionProgress -= (ushort)GetProgress(Item);
                                        Item.Perfectionlevel++;
                                        Kernel.SendWorldMessage(new Message("Congratulations, " + C.Entity.Name + " has upgraded Perfection " + Database.ConquerItemInformation.BaseInformations[Item.ID].Name + " to level + " + Item.Perfectionlevel + " !", System.Drawing.Color.Red, Network.GamePackets.Message.TopLeft), Program.Values);
                                    }
                                    Item.Mode = Game.Enums.ItemMode.Update;
                                    Item.Send(C);
                                    Proto.Pro(Item, C);
                                    C.LoadItemStats();
                                    C.Inventory.Remove(ItemPlus.UID, Game.Enums.ItemUse.Remove, true);
                                    Database.ConquerItemTable.UpdatePerfection(Item);
                                }
                                else
                                    C.MessageBox("Sorry");
                            }
                            break;
                        }
                    #endregion
                    #region Transfer
                    case 1:
                        {
                            ConquerItem Item;
                            if (C.Equipment.TryGetItem(Info.ItemUID, out Item))
                            {
                                if (C.Entity.ConquerPoints >= 1500)
                                {
                                    C.Entity.ConquerPoints -= 1500;
                                    Item.OwnerName = C.Entity.Name;
                                    Item.OwnerID = C.Entity.UID;
                                    Item.Mode = Game.Enums.ItemMode.Update;
                                    Item.Send(C);
                                    Proto.Pro(Item, C);
                                    Database.ConquerItemTable.UpdatePerfection(Item);
                                }
                            }
                            break;
                        }
                    #endregion
                    #region Signature
                    case 2:
                        {
                            ConquerItem Item;
                            if (C.Equipment.TryGetItem(Info.ItemUID, out Item))
                            {
                                if (Item.Perfectionlevel >= 18)
                                {
                                    if (Item.Signature == String.Empty || Item.Signature == null)
                                    {
                                        Item.Signature = Info.Signature;
                                        Item.Mode = Game.Enums.ItemMode.Update;
                                        Item.Send(C);
                                        Proto.Pro(Item, C);
                                        Database.ConquerItemTable.UpdatePerfection(Item);
                                    }
                                    else
                                    {
                                        if (C.Entity.ConquerPoints >= 270)
                                        {
                                            C.Entity.ConquerPoints -= 270;
                                            Item.Signature = Info.Signature;
                                            Item.Mode = Game.Enums.ItemMode.Update;
                                            Item.Send(C);
                                            Proto.Pro(Item, C);
                                            Database.ConquerItemTable.UpdatePerfection(Item);
                                        }

                                    }
                                }
                            }
                            break;
                        }
                    #endregion
                    #region Exchange
                    case 4:
                        {
                            ConquerItem Item;
                            ConquerItem[] UsedItems;
                            if (C.Equipment.TryGetItem(Info.ItemUID, out Item) || C.Inventory.TryGetItem(Info.ItemUID, out Item))
                            {
                                if (C.Entity.ConquerPoints >= 1000)
                                {
                                    UsedItems = new ConquerItem[Info.ItemPlusUID];
                                    for (var i = 0; i < UsedItems.Length; i++)
                                    {
                                        if (!C.Inventory.TryGetItem(Info.ItemPlusUID, out UsedItems[i]) && !C.Equipment.TryGetItem(Info.ItemPlusUID, out UsedItems[i]))
                                        {
                                            return;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(UsedItems[0].OwnerName) && UsedItems[0].OwnerID != 0)
                                    {
                                        if (UsedItems[0].OwnerID != C.Entity.UID)
                                        {
                                            C.Send("Only Owner of this item can perfect it agine.");
                                            return;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(Item.OwnerName) && Item.OwnerID != 0)
                                    {
                                        if (Item.OwnerID != C.Entity.UID)
                                        {
                                            C.Send("Only Owner of this item can perfect it agine.");
                                            return;
                                        }
                                    }
                                    C.Entity.ConquerPoints -= 1000;
                                    if (Item.OwnerID == 0)
                                    {
                                        Item.OwnerID = C.Entity.UID;
                                        Item.OwnerName = C.Entity.Name;
                                    }
                                    if (UsedItems[0].OwnerID == 0)
                                    {
                                        UsedItems[0].OwnerID = C.Entity.UID;
                                        UsedItems[0].OwnerName = C.Entity.Name;
                                    }
                                    var TempItem = new ConquerItem(true)
                                    {
                                        PerfectionProgress = Item.PerfectionProgress,
                                        Perfectionlevel = Item.Perfectionlevel,

                                    };
                                    Item.PerfectionProgress = UsedItems[0].PerfectionProgress;
                                    Item.Perfectionlevel = UsedItems[0].Perfectionlevel;

                                    UsedItems[0].PerfectionProgress = TempItem.PerfectionProgress;
                                    UsedItems[0].Perfectionlevel = TempItem.Perfectionlevel;

                                    Item.Mode = Game.Enums.ItemMode.Update;
                                    C.Send(Item);
                                    UsedItems[0].Mode = Game.Enums.ItemMode.Update;
                                    C.Send(UsedItems[0]);
                                    Proto.Pro(Item, C);
                                    Database.ConquerItemTable.UpdatePerfection(UsedItems[0]);
                                    Database.ConquerItemTable.UpdatePerfection(Item);
                                }
                            }
                            break;
                        }
                    #endregion
                    #region CPBoost
                    case 3:
                        {
                           
                            break;
                        }
                    #endregion
                    #region Quick
                    case 5:
                        {
                            ConquerItem Item;
                            if (C.Equipment.TryGetItem(Info.ItemUID, out Item))
                            {
                                double percent = (double)Item.PerfectionProgress / (double)GetProgress(Item);
                                if (Kernel.Rate(percent))
                                {
                                    Item.PerfectionProgress = 0;
                                    Item.Perfectionlevel++;
                                }
                                else
                                    Item.PerfectionProgress = 0;
                                Item.Mode = Game.Enums.ItemMode.Update;
                                Item.Send(C);
                                Database.ConquerItemTable.UpdatePerfection(Item);
                                Proto.Pro(Item, C);
                            }
                            break;
                        }
                    #endregion
                    default: { Console.WriteLine("= " + Info.ID); break; }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public bool R(byte[] Packet)
        {
            try
            {
                var P = new byte[Packet.Length - 4];
                Array.Copy(Packet, 4, P, 0, P.Length);
                using (var memoryStream = new MemoryStream(Packet))
                {
                    Info = Serializer.DeserializeWithLengthPrefix<GUI>(memoryStream, PrefixStyle.Fixed32);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
    }
    public sealed class PerfectionEffect
    {
        public PerfectionEffect() { }
        [ProtoContract]
        public class PerfectionE
        {
            [ProtoMember(1, IsRequired = true)]
            public uint UID;
            [ProtoMember(2, IsRequired = true)]
            public uint Type;
            [ProtoMember(3, IsRequired = true)]
            public uint Eff;
        }
        public static void Send(Client.GameState C, uint E)
        {
            C.Send(Proto.FinalizeProtoBuf(new PerfectionE()
            {
                UID = C.Entity.UID,
                Type = 0,
                Eff = E
            }, 3254));
        }
    }
    public sealed class PerfectionScore
    {
        public PerfectionScore() { }
        public PerfectionScoreProto Info;
        [ProtoContract]
        public class PerfectionScoreProto
        {
            [ProtoMember(1, IsRequired = true)]
            public uint ActionId;
            [ProtoMember(2, IsRequired = true)]
            public uint UID;
            [ProtoMember(3, IsRequired = true)]
            public uint Level;
            [ProtoMember(4, IsRequired = true)]
            public Scores[] Scores;
        }
        [ProtoContract]
        public class Scores
        {
            [ProtoMember(1, IsRequired = true)]
            public uint Type;
            [ProtoMember(2, IsRequired = true)]
            public uint Score;
        }
        public static List<Game.Entity> RankingList;
        public void GetRankingList()
        {
            RankingList = new List<Game.Entity>();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    if (reader.ReadUInt32("TotalPerfectionScore") < 20000) continue;
                    Game.Entity client = new Game.Entity(Game.EntityFlag.Player, false);
                    client.Name = reader.ReadString("Name");
                    client.UID = reader.ReadUInt32("UID");
                    client.Level = reader.ReadByte("Level");
                    client.Class = reader.ReadByte("Class");
                    client.Body = reader.ReadUInt16("Body");
                    client.Face = reader.ReadUInt16("Face");
                    client.TotalPerfectionScore = reader.ReadUInt32("TotalPerfectionScore");
                    RankingList.Add(Kernel.GamePool.ContainsKey(client.UID) ? Kernel.GamePool[client.UID].Entity : client);

                }
            }
        }
        public bool Read(byte[] packet)
        {
            try
            {
                var mypkt = new byte[packet.Length - 4];
                Array.Copy(packet, 4, mypkt, 0, mypkt.Length);
                using (var memoryStream = new MemoryStream(packet))
                {
                    Info = Serializer.DeserializeWithLengthPrefix<PerfectionScoreProto>(memoryStream, PrefixStyle.Fixed32);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
        public void SendScore(Client.GameState client, Client.GameState Observer)
        {
            var packet = new PerfectionScoreProto();
            packet.ActionId = 0;
            packet.UID = client.Entity.UID;
            packet.Level = client.Entity.Level;
            packet.Scores = new Scores[20];
            for (int i = 0; i < packet.Scores.Length; i++)
            {
                byte Type = (byte)(i + 1);
                packet.Scores[i] = new Scores();
                packet.Scores[i].Type = Type;
                packet.Scores[i].Score = GetScoreValue(client, Type);
            }


            var proto = Kernel.FinalizeProtoBuf(packet, 3253);
            Observer.Send(proto);
        }
        public uint GetScoreValue(Client.GameState client, uint Type)
        {
            uint Score = 0;
            if (Type == 1) Score = (uint)PerfectionScore.CalculatePerfectionJiangPoints(client);
            if (Type == 2) Score = (uint)PerfectionScore.CalculatePerfectionChiPoints(client);
            if (Type == 3) Score = (uint)(client.Entity.InnerPower != null ? client.Entity.InnerPower.TotalScore * 2 : 0);
            if (Type == 4) Score = client.Equipment.GetFullEquipmentEnchantPoints;
            if (Type == 5) Score = (uint)(client.Entity.Level < 140 ? client.Entity.Level * 20 : client.Entity.Level * 25);
            if (Type == 6) Score = client.Equipment.GetFullEquipmentPerfecetionLevelPoints;
            if (Type == 7) Score = (uint)((client.Entity.Vitality + client.Entity.Atributes + client.Entity.Spirit + client.Entity.Strength + client.Entity.Agility) * 5);
            if (Type == 8) Score = (uint)(client.Entity.Reborn * 1000);
            if (Type == 9) Score = client.Equipment.GetFullEquipmentEnumPoints;
            if (Type == 10) Score = client.Equipment.GetFullEquipmentSocketPoints;
            if (Type == 11) Score = client.Equipment.GetFullEquipmentGemPoints;
            if (Type == 12) Score = client.Equipment.GetFullEquipmentPlusPoints;
            if (Type == 13) Score = client.Equipment.GetFullEquipmentRefinePoints;
            if (Type == 14) Score = client.Equipment.GetFullEquipmentSoulPoints;
            if (Type == 15) Score = client.Equipment.GetFullEquipmentBlessPoints;
            if (Type == 16) Score = CalculateSubClassPoints(client);
            if (Type == 17) Score = PerfectionTable.PerfectionPoints(client, true);
            if (Type == 18) Score = PerfectionTable.PerfectionPoints(client, false);
            if (Type == 19) Score = (uint)((uint)client.Entity.NobilityRank * 1000);
            if (Type == 20) Score = client.Equipment.GetFullEquipmentLevelPoints;
            return Score;
        }
        public void Handle(Client.GameState client)
        {
            switch (Info.ActionId)
            {
                case 1://Observ
                    {
                        if (Kernel.GamePool.ContainsKey(Info.UID))
                        {
                            SendScore(client, client);
                            SendScore(Kernel.GamePool[Info.UID], client);
                        }
                        else
                        {
                            client.Send(Kernel.FinalizeProtoBuf(new PerfectionScoreProto()
                            {
                                ActionId = 1,
                                UID = 0
                            }, 3253));
                        }
                        break;
                    }
            }
        }
        public static int CalculatePerfectionChiPoints(Client.GameState client)
        {
            int Point = 0;
            if (client.ChiData != null)
            {
                var chiScores = new int[] { client.ChiData.DragonPoints, client.ChiData.PhoenixPoints, client.ChiData.TigerPoints, client.ChiData.TurtlePoints };
                foreach (var chiScore in chiScores.Where(v => v > 0))
                {
                    Point += (int)(((decimal)chiScore / 100) * 60);
                }
            }
            return Point;
        }
        public static int CalculatePerfectionJiangPoints(Client.GameState client)
        {
            int Points = 0;
            if (client.Entity.MyJiang != null)
            {
                if (client.Entity.MyJiang.Inner_Strength == 0) return Points;
                if (client.Entity.MyJiang.Inner_Strength <= 16200) Points = (int)(client.Entity.MyJiang.Inner_Strength * 0.3);
                else if (client.Entity.MyJiang.Inner_Strength <= 40500) Points = (int)(client.Entity.MyJiang.Inner_Strength * 0.33);
                else if (client.Entity.MyJiang.Inner_Strength <= 60750) Points = (int)(client.Entity.MyJiang.Inner_Strength * 0.36);
                else if (client.Entity.MyJiang.Inner_Strength <= 72000) Points = (int)(client.Entity.MyJiang.Inner_Strength * 0.40);
                else if (client.Entity.MyJiang.Inner_Strength <= 79200) Points = (int)(client.Entity.MyJiang.Inner_Strength * 0.45);
                else if (client.Entity.MyJiang.Inner_Strength <= 80800) Points = (int)(client.Entity.MyJiang.Inner_Strength * 0.50);
                else if (client.Entity.MyJiang.Inner_Strength <= 81000) Points = (int)(client.Entity.MyJiang.Inner_Strength * 0.60);
            }
            return Points;
        }
        public static uint CalculateSubClassPoints(Client.GameState client)
        {
            uint Points = 0;
            if (client.Entity.SubClasses.Classes != null)
            {
                foreach (var sub in client.Entity.SubClasses.Classes.Values)
                {
                    if (sub == null) continue;
                    Points += (uint)(sub.Level == 9 ? 1000 : sub.Level * 100);
                }
            }
            return Points;
        }
        public static uint CalculatePerfectionItemPoints(ConquerItem item)
        {
            uint Points = 50;
            if (item == null) return 50;
            #region Plus
            if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                if (item.Plus == 1) Points += 200;
                if (item.Plus == 2) Points += 600;
                if (item.Plus == 3) Points += 1200;
                if (item.Plus == 4) Points += 1800;
                if (item.Plus == 5) Points += 2600;
                if (item.Plus == 6) Points += 3500;
                if (item.Plus == 7) Points += 4800;
                if (item.Plus == 8) Points += 5800;
                if (item.Plus == 9) Points += 6800;
                if (item.Plus == 10) Points += 7800;
                if (item.Plus == 11) Points += 8800;
                if (item.Plus == 12) Points += 10000;
            }
            else
            {
                if (item.Plus == 1) Points += 400;
                if (item.Plus == 2) Points += 1200;
                if (item.Plus == 3) Points += 2400;
                if (item.Plus == 4) Points += 3600;
                if (item.Plus == 5) Points += 5200;
                if (item.Plus == 6) Points += 7000;
                if (item.Plus == 7) Points += 9600;
                if (item.Plus == 8) Points += 11600;
                if (item.Plus == 9) Points += 13600;
                if (item.Plus == 10) Points += 15600;
                if (item.Plus == 11) Points += 17600;
                if (item.Plus == 12) Points += 20000;
            }
            #endregion
            #region Quality
            if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                if (item.ID % 10 == 9) Points += 500;
                if (item.ID % 10 == 8) Points += 300;
                if (item.ID % 10 == 7) Points += 200;
                if (item.ID % 10 == 6) Points += 100;
                if (item.ID % 10 > 0 && item.ID % 10 < 6) Points += 50;
            }
            else
            {
                if (item.ID % 10 == 9) Points += 1000;
                if (item.ID % 10 == 8) Points += 600;
                if (item.ID % 10 == 7) Points += 400;
                if (item.ID % 10 == 6) Points += 200;
                if (item.ID % 10 > 0 && item.ID % 10 < 6) Points += 100;
            }
            #endregion
            #region Soul
            if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                if (item.Purification.PurificationLevel == 1) Points += 100;
                if (item.Purification.PurificationLevel == 2) Points += 300;
                if (item.Purification.PurificationLevel == 3) Points += 500;
                if (item.Purification.PurificationLevel == 4) Points += 800;
                if (item.Purification.PurificationLevel == 5) Points += 1200;
                if (item.Purification.PurificationLevel == 6) Points += 1600;
                if (item.Purification.PurificationLevel == 7) Points += 2000;
            }
            else
            {
                if (item.Purification.PurificationLevel == 1) Points += 200;
                if (item.Purification.PurificationLevel == 2) Points += 600;
                if (item.Purification.PurificationLevel == 3) Points += 1000;
                if (item.Purification.PurificationLevel == 4) Points += 1600;
                if (item.Purification.PurificationLevel == 5) Points += 2400;
                if (item.Purification.PurificationLevel == 6) Points += 3200;
                if (item.Purification.PurificationLevel == 7) Points += 4000;
            }
            #endregion
            #region Bless
            if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                Points += (uint)(item.Bless * 100);
            }
            else
            {
                Points += (uint)(item.Bless * 200);
            }
            #region Refine
            if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                if (item.ExtraEffect.EffectLevel == 1) Points += 100;
                if (item.ExtraEffect.EffectLevel == 2) Points += 400;
                if (item.ExtraEffect.EffectLevel == 3) Points += 800;
                if (item.ExtraEffect.EffectLevel == 4) Points += 1200;
                if (item.ExtraEffect.EffectLevel == 5) Points += 1600;
                if (item.ExtraEffect.EffectLevel == 6) Points += 2000;
            }
            else
            {
                if (item.ExtraEffect.EffectLevel == 1) Points += 200;
                if (item.ExtraEffect.EffectLevel == 2) Points += 800;
                if (item.ExtraEffect.EffectLevel == 3) Points += 1600;
                if (item.ExtraEffect.EffectLevel == 4) Points += 2400;
                if (item.ExtraEffect.EffectLevel == 5) Points += 3200;
                if (item.ExtraEffect.EffectLevel == 6) Points += 4000;
            }
            #endregion
            #endregion
            #region Level
          /*  if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                var lvl = (uint)Database.ConquerItemInformation.BaseInformations[item.ID].Level;
                if (lvl <= 120)
                    Points += lvl * 3;
                else if (lvl <= 130)
                    Points += lvl * 5;
                else if (lvl <= 140)
                    Points += lvl * 6;
            }
            else
            {
                var lvl = (uint)Database.ConquerItemInformation.BaseInformations[item.ID].Level;
                if (lvl <= 120)
                    Points += lvl * 6;
                else if (lvl <= 130)
                    Points += lvl * 10;
                else if (lvl <= 140)
                    Points += lvl * 12;
            }*/
            #endregion
            #region Gem
            if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                if (item.SocketOne != (Game.Enums.Gem)0)
                {
                    if (item.SocketOne2 % 10 == 1) Points += 200;
                    if (item.SocketOne2 % 10 == 2) Points += 500;
                    if (item.SocketOne2 % 10 == 3) Points += 800;
                }
                if (item.SocketTwo != (Game.Enums.Gem)0)
                {
                    if (item.SocketTwo2 % 10 == 1) Points += 200;
                    if (item.SocketTwo2 % 10 == 2) Points += 500;
                    if (item.SocketTwo2 % 10 == 3) Points += 800;
                }
            }
            else
            {
                if (item.SocketOne != (Game.Enums.Gem)0)
                {
                    if (item.SocketOne2 % 10 == 1) Points += 400;
                    if (item.SocketOne2 % 10 == 2) Points += 1000;
                    if (item.SocketOne2 % 10 == 3) Points += 1600;
                }
                if (item.SocketTwo != (Game.Enums.Gem)0)
                {
                    if (item.SocketTwo2 % 10 == 1) Points += 400;
                    if (item.SocketTwo2 % 10 == 2) Points += 1000;
                    if (item.SocketTwo2 % 10 == 3) Points += 1600;
                }
            }
            #endregion
            #region PerfectionLevel
            if (item.Perfectionlevel >= 1) Points += 180;
            if (item.Perfectionlevel >= 2) Points += 180;
            if (item.Perfectionlevel >= 3) Points += 180;
            if (item.Perfectionlevel >= 4) Points += 180;
            if (item.Perfectionlevel >= 5) Points += 180;
            if (item.Perfectionlevel >= 6) Points += 180;
            if (item.Perfectionlevel >= 7) Points += 180;
            if (item.Perfectionlevel >= 8) Points += 180;
            if (item.Perfectionlevel >= 9) Points += 180;
            if (item.Perfectionlevel >= 10) Points += 2380;
            if (item.Perfectionlevel >= 11) Points += 400;
            if (item.Perfectionlevel >= 12) Points += 400;
            if (item.Perfectionlevel >= 13) Points += 400;
            if (item.Perfectionlevel >= 14) Points += 400;
            if (item.Perfectionlevel >= 15) Points += 400;
            if (item.Perfectionlevel >= 16) Points += 400;
            if (item.Perfectionlevel >= 17) Points += 400;
            if (item.Perfectionlevel >= 18) Points += 400;
            if (item.Perfectionlevel >= 19) Points += 5150;
            if (item.Perfectionlevel >= 20) Points += 650;
            if (item.Perfectionlevel >= 21) Points += 650;
            if (item.Perfectionlevel >= 22) Points += 650;
            if (item.Perfectionlevel >= 23) Points += 650;
            if (item.Perfectionlevel >= 24) Points += 650;
            if (item.Perfectionlevel >= 25) Points += 650;
            if (item.Perfectionlevel >= 26) Points += 650;
            if (item.Perfectionlevel >= 27) Points += 650;
            if (item.Perfectionlevel >= 28) Points += 100;
            if (item.Perfectionlevel >= 29) Points += 100;
            if (item.Perfectionlevel >= 30) Points += 100;
            if (item.Perfectionlevel >= 31) Points += 100;
            if (item.Perfectionlevel >= 32) Points += 100;
            if (item.Perfectionlevel >= 33) Points += 100;
            if (item.Perfectionlevel >= 34) Points += 100;
            if (item.Perfectionlevel >= 35) Points += 100;
            if (item.Perfectionlevel >= 36) Points += 100;
            if (item.Perfectionlevel >= 37) Points += 100;
            if (item.Perfectionlevel >= 38) Points += 100;
            if (item.Perfectionlevel >= 39) Points += 100;
            if (item.Perfectionlevel >= 40) Points += 100;
            if (item.Perfectionlevel >= 41) Points += 100;
            if (item.Perfectionlevel >= 42) Points += 100;
            if (item.Perfectionlevel >= 43) Points += 100;
            if (item.Perfectionlevel >= 44) Points += 100;
            if (item.Perfectionlevel >= 45) Points += 100;
            if (item.Perfectionlevel >= 46) Points += 100;
            if (item.Perfectionlevel >= 47) Points += 100;
            if (item.Perfectionlevel >= 48) Points += 100;
            if (item.Perfectionlevel >= 49) Points += 100;
            if (item.Perfectionlevel >= 50) Points += 100;
            if (item.Perfectionlevel >= 51) Points += 100;
            if (item.Perfectionlevel >= 52) Points += 100;
            if (item.Perfectionlevel >= 53) Points += 100;
            if (item.Perfectionlevel >= 54) Points += 100;
            if (Network.PacketHandler.IsTwoHand(item.ID))
            {
                if (item.Perfectionlevel >= 1) Points += 180;
                if (item.Perfectionlevel >= 2) Points += 180;
                if (item.Perfectionlevel >= 3) Points += 180;
                if (item.Perfectionlevel >= 4) Points += 180;
                if (item.Perfectionlevel >= 5) Points += 180;
                if (item.Perfectionlevel >= 6) Points += 180;
                if (item.Perfectionlevel >= 7) Points += 180;
                if (item.Perfectionlevel >= 8) Points += 180;
                if (item.Perfectionlevel >= 9) Points += 180;
                if (item.Perfectionlevel >= 10) Points += 2380;
                if (item.Perfectionlevel >= 11) Points += 400;
                if (item.Perfectionlevel >= 12) Points += 400;
                if (item.Perfectionlevel >= 13) Points += 400;
                if (item.Perfectionlevel >= 14) Points += 400;
                if (item.Perfectionlevel >= 15) Points += 400;
                if (item.Perfectionlevel >= 16) Points += 400;
                if (item.Perfectionlevel >= 17) Points += 400;
                if (item.Perfectionlevel >= 18) Points += 400;
                if (item.Perfectionlevel >= 19) Points += 5150;
                if (item.Perfectionlevel >= 20) Points += 650;
                if (item.Perfectionlevel >= 21) Points += 650;
                if (item.Perfectionlevel >= 22) Points += 650;
                if (item.Perfectionlevel >= 23) Points += 650;
                if (item.Perfectionlevel >= 24) Points += 650;
                if (item.Perfectionlevel >= 25) Points += 650;
                if (item.Perfectionlevel >= 26) Points += 650;
                if (item.Perfectionlevel >= 27) Points += 650;
                if (item.Perfectionlevel >= 28) Points += 100;
                if (item.Perfectionlevel >= 29) Points += 100;
                if (item.Perfectionlevel >= 30) Points += 100;
                if (item.Perfectionlevel >= 31) Points += 100;
                if (item.Perfectionlevel >= 32) Points += 100;
                if (item.Perfectionlevel >= 33) Points += 100;
                if (item.Perfectionlevel >= 34) Points += 100;
                if (item.Perfectionlevel >= 35) Points += 100;
                if (item.Perfectionlevel >= 36) Points += 100;
                if (item.Perfectionlevel >= 37) Points += 100;
                if (item.Perfectionlevel >= 38) Points += 100;
                if (item.Perfectionlevel >= 39) Points += 100;
                if (item.Perfectionlevel >= 40) Points += 100;
                if (item.Perfectionlevel >= 41) Points += 100;
                if (item.Perfectionlevel >= 42) Points += 100;
                if (item.Perfectionlevel >= 43) Points += 100;
                if (item.Perfectionlevel >= 44) Points += 100;
                if (item.Perfectionlevel >= 45) Points += 100;
                if (item.Perfectionlevel >= 46) Points += 100;
                if (item.Perfectionlevel >= 47) Points += 100;
                if (item.Perfectionlevel >= 48) Points += 100;
                if (item.Perfectionlevel >= 49) Points += 100;
                if (item.Perfectionlevel >= 50) Points += 100;
                if (item.Perfectionlevel >= 51) Points += 100;
                if (item.Perfectionlevel >= 52) Points += 100;
                if (item.Perfectionlevel >= 53) Points += 100;
                if (item.Perfectionlevel >= 54) Points += 100;
            }
            #endregion
            #region Socket
            if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                if (item.SocketOne != (Game.Enums.Gem)0) Points += 1000;
                if (item.SocketTwo != (Game.Enums.Gem)0) Points += 2500;
            }
            else
            {
                if (item.SocketOne != (Game.Enums.Gem)0) Points += 2000;
                if (item.SocketTwo != (Game.Enums.Gem)0) Points += 5000;
            }
            #endregion
            #region Enchant
            if (!Network.PacketHandler.IsTwoHand(item.ID))
            {
                var enc = (uint)(item.Enchant);
                if (enc != 0)
                {
                    if (enc <= 200) Points += enc * 1;
                    if (enc <= 240) Points += (uint)(enc * 1.3);
                    if (enc <= 254) Points += (uint)(enc * 1.6);
                    if (enc <= 255) Points += enc * 2;
                }
            }
            else
            {
                var enc = (uint)(item.Enchant);
                if (enc != 0)
                {
                    if (enc <= 200) Points += enc * 2;
                    if (enc <= 240) Points += (uint)(enc * 2.6);
                    if (enc <= 254) Points += (uint)(enc * 3.2);
                    if (enc <= 255) Points += enc * 4;
                }
            }
            #endregion
            return Points;
        }
    }
    public sealed class PerfectionRank
    {
        public PerfectionRank() { }
        public PerfectionRankProto Info;
        [Flags]
        public enum ActionID
        {
            MainRank = 0,
            RankItems = 1,
            UserItemRanking = 2,
            MyRanks = 3,
            View = 4,

        }
        [ProtoContract]
        public class PerfectionRankProto
        {
            [ProtoMember(1, IsRequired = true)]
            public ActionID Action;
            [ProtoMember(2)]
            public uint RegisteredCount;
            [ProtoMember(3)]
            public uint Page;
            [ProtoMember(4, IsRequired = true)]
            public uint Index;
            [ProtoMember(5, IsRequired = true)]
            public EquipProto[] items;
        }
        [ProtoContract]
        public class EquipProto
        {
            [ProtoMember(1, IsRequired = true)]
            public uint Rank;
            [ProtoMember(2, IsRequired = true)]
            public uint UnKnow2;
            [ProtoMember(3, IsRequired = true)]
            public uint Position;
            [ProtoMember(4, IsRequired = true)]
            public uint RankScore;
            [ProtoMember(5, IsRequired = true)]
            public uint UID;
            [ProtoMember(6, IsRequired = true)]
            public uint ItemID;
            [ProtoMember(7, IsRequired = true)]
            public uint PurificationID;
            [ProtoMember(8, IsRequired = true)]
            public uint Plus;
            [ProtoMember(9, IsRequired = true)]
            public uint PerfectionLevel;
            [ProtoMember(10, IsRequired = true)]
            public string Name = "";
        }
        public byte GetRealyPosition(byte FakePosition)
        {
            if (FakePosition == 5) return 19;
            if (FakePosition == 7) return 18;
            if (FakePosition == 9) return 12;
            return FakePosition;
        }
        public static Dictionary<byte, ConquerItem> MainRank;
        public void UpdateRanking()
        {
            AllItemsRanking = new List<ConquerItem>();
            MainRank = new Dictionary<byte, ConquerItem>();
            for (byte i = 0; i < 11; i++)
            {
                byte z = GetRealyPosition((byte)(i + 1));
                List<ConquerItem> items = new List<ConquerItem>();
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("items").Where("Position", z).And("Perfectionlevel", 0, true))
                using (var reader = new MySqlReader(cmd))
                {
                    while (reader.Read())
                    {
                        ConquerItem item = new ConquerItem(true);
                        item.ID = reader.ReadUInt32("Id");
                        item.UID = reader.ReadUInt32("Uid");
                        item.Durability = reader.ReadUInt16("Durability");
                        item.MaximDurability = reader.ReadUInt16("MaximDurability");
                        //  item.Durability = item.MaximDurability;
                        item.Position = reader.ReadUInt16("Position");
                        item.Agate = reader.ReadString("Agate");
                        item.SocketProgress = reader.ReadUInt32("SocketProgress");
                        item.PlusProgress = reader.ReadUInt32("PlusProgress");
                        item.SocketOne = (Game.Enums.Gem)reader.ReadUInt16("SocketOne");
                        item.SocketTwo = (Game.Enums.Gem)reader.ReadUInt16("SocketTwo");
                        item.Effect = (Game.Enums.ItemEffect)reader.ReadUInt16("Effect");
                        item.Mode = Game.Enums.ItemMode.Default;
                        item.Plus = reader.ReadByte("Plus");
                        item.Bless = reader.ReadByte("Bless");
                        item.Bound = reader.ReadBoolean("Bound");
                        item.Enchant = reader.ReadByte("Enchant");
                        item.Lock = reader.ReadByte("Locked");
                        item.UnlockEnd = DateTime.FromBinary(reader.ReadInt64("UnlockEnd"));
                        item.Suspicious = reader.ReadBoolean("Suspicious");
                        item.SuspiciousStart = DateTime.FromBinary(reader.ReadInt64("SuspiciousStart"));
                        item.Color = (Game.Enums.Color)reader.ReadUInt32("Color");
                        item.Warehouse = reader.ReadUInt32("Warehouse");
                        item.Perfectionlevel = reader.ReadByte("Perfectionlevel");
                        item.PerfectionProgress = reader.ReadUInt16("PerfectionProgress");
                        item.OwnerID = reader.ReadUInt32("OwnerID");
                        item.OwnerName = reader.ReadString("OwnerName");
                        item.Signature = reader.ReadString("Signature");
                        item.StackSize = reader.ReadUInt16("StackSize");
                        item.RefineItem = reader.ReadUInt32("RefineryItem");
                        item.InWardrobe = reader.ReadBoolean("InWardrobe");
                        Int64 rTime = reader.ReadInt64("RefineryTime");

                        if (item.ID == 300000)
                        {
                            uint NextSteedColor = reader.ReadUInt32("NextSteedColor");
                            item.NextGreen = (byte)(NextSteedColor & 0xFF);
                            item.NextBlue = (byte)((NextSteedColor >> 8) & 0xFF);
                            item.NextRed = (byte)((NextSteedColor >> 16) & 0xFF);
                        }
                        if (item.RefineItem > 0 && rTime != 0)
                        {
                            item.RefineryTime = DateTime.FromBinary(rTime);
                            if (DateTime.Now > item.RefineryTime)
                            {
                                item.RefineryTime = new DateTime(0);
                                item.RefineItem = 0;
                            }
                        }
                        if (item.Lock == 2)
                            if (DateTime.Now >= item.UnlockEnd)
                                item.Lock = 0;

                        item.DayStamp = DateTime.FromBinary(reader.ReadInt64("DayStamp"));
                        item.Days = reader.ReadByte("Days");
                        ItemAddingTable.GetAddingsForItem(item);
                        items.Add(item);
                        if (!AllItemsRanking.Contains(item))
                            AllItemsRanking.Add(item);

                    }
                }
                MainRank[z] = items.OrderByDescending(x => PerfectionScore.CalculatePerfectionItemPoints(x)).FirstOrDefault();
            }
        }
        public static List<ConquerItem> AllItemsRanking;
        public void Handle(Client.GameState client)
        {
            switch (Info.Action)
            {
                case ActionID.UserItemRanking:
                    {
                        var item = AllItemsRanking.Where(p => p.Position == GetRealyPosition((byte)Info.Index)).OrderByDescending(i => PerfectionScore.CalculatePerfectionItemPoints(i)).ToArray();
                        var min = Math.Min(item.Length, 50);
                        for (int i = 0; i < min; i++)
                        {
                            if (client.Equipment.IsWearingItemUID(item[i].UID))
                            {
                                var packet = new PerfectionRankProto();
                                packet.Page = Info.Page;
                                packet.RegisteredCount = Info.RegisteredCount;
                                packet.Action = Info.Action;
                                packet.Index = Info.Index;
                                packet.items = new EquipProto[1];
                                packet.items[0] = new EquipProto();
                                var itemm = item[i];
                                packet.items[0].Rank = (uint)(i + 1);
                                packet.items[0].RankScore = PerfectionScore.CalculatePerfectionItemPoints(itemm);
                                packet.items[0].ItemID = itemm.ID;
                                packet.items[0].Name = itemm.OwnerName.Replace("/0", "");
                                packet.items[0].PerfectionLevel = itemm.Perfectionlevel;
                                packet.items[0].Position = GetRealyPosition((byte)Info.Index);
                                packet.items[0].Plus = itemm.Plus;
                                packet.items[0].UID = itemm.UID;
                                packet.items[0].UnKnow2 = (uint)Kernel.Random.Next(800, 3000);
                                client.Send(Kernel.FinalizeProtoBuf(packet, 3256));
                                break;
                            }
                        }
                        break;
                    }
                case ActionID.MyRanks:
                    {
                        var packet = new PerfectionRankProto();
                        packet.Action = ActionID.MyRanks;
                        packet.items = new EquipProto[11];
                        for (byte i = 0; i < packet.items.Length; i++)
                        {
                            byte z = GetRealyPosition((byte)(i + 1));
                            var itemmm = AllItemsRanking.Where(p => p.Position == z).OrderByDescending(m => PerfectionScore.CalculatePerfectionItemPoints(m)).ToArray();
                            for (int h = 0; h < itemmm.Length; h++)
                            {
                                if (client.Equipment.IsWearingItemUID(itemmm[h].UID))
                                {
                                    packet.items[i] = new EquipProto();
                                    packet.items[i].ItemID = itemmm[h].ID;
                                    packet.items[i].Name = itemmm[h].OwnerName.Replace("/0", "");
                                    packet.items[i].PerfectionLevel = itemmm[h].Perfectionlevel;
                                    packet.items[i].Plus = itemmm[h].Plus;
                                    packet.items[i].Position = (uint)(i + 1);
                                    if (itemmm[h].Purification.Available)
                                        packet.items[i].PurificationID = itemmm[h].Purification.PurificationItemID;
                                    packet.items[i].Rank = (uint)(h + 1);
                                    packet.items[i].RankScore = PerfectionScore.CalculatePerfectionItemPoints(itemmm[h]);
                                    packet.items[i].UID = itemmm[h].UID;
                                    packet.items[i].UnKnow2 = (uint)Kernel.Random.Next(800, 3000);
                                    break;
                                }
                            }
                        }



                        var proto = Kernel.FinalizeProtoBuf(packet, 3256);
                        client.Send(proto);
                        break;
                    }
                case ActionID.View://ViewItem
                    {
                        if (Database.ConquerItemTable.LoadItem(Info.Index) != null)
                        {
                            var item = Database.ConquerItemTable.LoadItem(Info.Index);
                            item.Mode = Game.Enums.ItemMode.PerfectionView;
                            item.Send(client);
                        }
                        else
                        {
                            client.Send(Kernel.FinalizeProtoBuf(Info, 3256));
                        }
                        break;
                    }
                case ActionID.RankItems://AllRanking
                    {
                        var cnt = AllItemsRanking.Where(p => p.Position == GetRealyPosition((byte)Info.Index)).Count();
                        var packet = new PerfectionRankProto();
                        packet.Action = ActionID.RankItems;
                        packet.RegisteredCount = (uint)Math.Min(cnt, 50);
                        packet.Page = Info.Page;
                        packet.Index = Info.Index;
                        uint sss = (ushort)Math.Min(cnt - (packet.Page * 10), 10);

                        int rank = (int)packet.Page * 10;
                        packet.items = new EquipProto[sss];
                        for (int i = rank; i < rank + sss; i++)
                        {
                            var iteeeem = AllItemsRanking.Where(p => p.Position == GetRealyPosition((byte)Info.Index)).OrderByDescending(x => PerfectionScore.CalculatePerfectionItemPoints(x)).ToArray()[i];
                            packet.items[i] = new EquipProto();
                            if (iteeeem.ID > 0)
                            {
                                packet.items[i].ItemID = iteeeem.ID;
                                packet.items[i].Name = iteeeem.OwnerName.Replace("/0", "");
                                packet.items[i].PerfectionLevel = iteeeem.Perfectionlevel;
                                packet.items[i].Plus = iteeeem.Plus;
                                packet.items[i].Position = GetRealyPosition((byte)Info.Index);
                                if (iteeeem.Purification.Available)
                                    packet.items[i].PurificationID = iteeeem.Purification.PurificationItemID;
                                packet.items[i].Rank = (uint)(i + 1);
                                packet.items[i].RankScore = PerfectionScore.CalculatePerfectionItemPoints(iteeeem);
                                packet.items[i].UID = iteeeem.UID;
                                packet.items[i].UnKnow2 = (uint)Kernel.Random.Next(800, 3000);
                            }
                        }
                        var proto = Kernel.FinalizeProtoBuf(packet, 3256);
                        client.Send(proto);
                        break;
                    }
                case ActionID.MainRank:
                    {


                        var packet = new PerfectionRankProto();
                        packet.Action = ActionID.MainRank;
                        packet.items = new EquipProto[11];
                        for (byte i = 0; i < packet.items.Length; i++)
                        {
                            byte z = GetRealyPosition((byte)(i + 1));
                            ConquerItem itemmm = MainRank[z];
                            if (itemmm == null) continue;
                            packet.items[i] = new EquipProto();
                            packet.items[i].ItemID = itemmm.ID;
                            packet.items[i].Name = itemmm.OwnerName.Replace("/0", "");
                            packet.items[i].PerfectionLevel = itemmm.Perfectionlevel;
                            packet.items[i].Plus = itemmm.Plus;
                            packet.items[i].Position = (uint)(i + 1);
                            if (itemmm.Purification.Available)
                                packet.items[i].PurificationID = itemmm.Purification.PurificationItemID;
                            packet.items[i].Rank = 1;
                            packet.items[i].RankScore = PerfectionScore.CalculatePerfectionItemPoints(itemmm);
                            packet.items[i].UID = itemmm.UID;
                            packet.items[i].UnKnow2 = (uint)Kernel.Random.Next(800, 3000);
                        }



                        var proto = Kernel.FinalizeProtoBuf(packet, 3256);
                        client.Send(proto);
                        break;
                    }
            }
        }
        public bool Read(byte[] packet)
        {
            using (var memoryStream = new MemoryStream(packet))
            {
                Info = Serializer.DeserializeWithLengthPrefix<PerfectionRankProto>(memoryStream, PrefixStyle.Fixed32);
            }
            return true;
        }
    }
    class PrestigeRank
    {
        public struct Data
        {
            public string Name;
            public byte Level;
            public byte Class;
            public uint Prestige;
            public uint UID;
            public uint Mesh;
        }
        public static byte[] S;
        static List<byte> Classs = new List<byte>() { 15, 25, 45, 55, 65, 75, 85, 135, 145, 165 };
        public static byte[] Offline = { 13, 0, 185, 12, 8, 1, 18, 5, 24, 245, 179, 186, 2, 84, 81, 83, 69, 82, 118, 101, 7 };
        [ProtoContract]
        public class Best
        {
            [ProtoMember(1, IsRequired = true)]
            public uint Type;
            [ProtoMember(2, IsRequired = true)]
            public DataBest DB;
        }
        [ProtoContract]
        public class DataBest
        {
            [ProtoMember(1, IsRequired = true)]
            public uint Type;//1
            [ProtoMember(2, IsRequired = true)]
            public uint Rank;//1
            [ProtoMember(3, IsRequired = true)]
            public uint EntityUID;
            [ProtoMember(4, IsRequired = true)]
            public string Name = "";
            [ProtoMember(5, IsRequired = true)]
            public string GuildName = "";
            [ProtoMember(6, IsRequired = true)]
            public uint Mesh;
            [ProtoMember(7, IsRequired = true)]
            public uint HairStyle;
            [ProtoMember(8, IsRequired = true)]
            public uint Head;
            [ProtoMember(9, IsRequired = true)]
            public uint Garment;
            [ProtoMember(10, IsRequired = true)]
            public uint LeftWeapon;
            [ProtoMember(11, IsRequired = true)]
            public uint LeftWeaponAccessory;
            [ProtoMember(12, IsRequired = true)]
            public uint RightWeapon;
            [ProtoMember(13, IsRequired = true)]
            public uint RightWeaponAccessory;
            [ProtoMember(14, IsRequired = true)]
            public uint MountArmor;
            [ProtoMember(15, IsRequired = true)]
            public uint Flag;
            [ProtoMember(16, IsRequired = true)]
            public uint Wing;
            [ProtoMember(17, IsRequired = true)]
            public uint WingPlus;
            [ProtoMember(18, IsRequired = true)]
            public uint Title;
            [ProtoMember(19, IsRequired = true)]
            public uint Flag1;
            [ProtoMember(20, IsRequired = true)]
            public uint Flag2;
        }
        public static Dictionary<byte, List<Data>> SR = new Dictionary<byte, List<Data>>();
        public static Dictionary<byte, Data> Topers = new Dictionary<byte, Data>();
        public static ConquerItem LoadItem(uint OwnerUId, uint Position)
        {
            ConquerItem item = null;
            using (var cn = new MySql.Data.MySqlClient.MySqlConnection(DataHolder.MySqlConnection.ConnectionString))
            using (var cm = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM items WHERE EntityID = @u AND Position =@p", cn))
            {
                cn.Open();
                cm.Parameters.AddWithValue("@u", OwnerUId);
                cm.Parameters.AddWithValue("@p", Position);
                using (MySql.Data.MySqlClient.MySqlDataReader rdr = cm.ExecuteReader())
                    if (rdr.Read())
                        item = deserialzeItem(rdr);
            }
            return item;
        }
        public static ConquerItem deserialzeItem(MySql.Data.MySqlClient.MySqlDataReader reader)
        {
            ConquerItem item = new Network.GamePackets.ConquerItem(true);
            item.ID = reader.GetUInt32("Id");
            item.UID = reader.GetUInt32("Uid");
            item.Perfectionlevel = (byte)reader.GetUInt32("Perfectionlevel");
            item.OwnerName = reader.GetString("OwnerName");
            item.OwnerID = reader.GetUInt32("OwnerID");
            item.PerfectionProgress = reader.GetUInt16("PerfectionProgress");
            item.Signature = reader.GetString("Signature");
            item.Durability = reader.GetUInt16("Durability");
            if (item.ID == 750000)
            {
                item.MaximDurability = reader.GetUInt16("MaximDurability");
            }
            else
            {
                item.MaximDurability = reader.GetUInt16("MaximDurability");
                item.Durability = item.MaximDurability;
            }
            item.Position = reader.GetUInt16("Position");
            item.SocketProgress = reader.GetUInt32("SocketProgress");
            item.PlusProgress = reader.GetUInt32("PlusProgress");
            item.SocketOne = (Game.Enums.Gem)reader.GetUInt16("SocketOne");
            item.SocketTwo = (Game.Enums.Gem)reader.GetUInt16("SocketTwo");
            item.Effect = (Game.Enums.ItemEffect)reader.GetUInt16("Effect");
            item.Mode = Game.Enums.ItemMode.Default;
            item.Plus = (byte)(reader.GetUInt16("Plus"));
            item.Bless = (byte)(reader.GetUInt16("Bless"));
            item.Bound = reader.GetBoolean("Bound");
            item.Enchant = (byte)(reader.GetUInt16("Enchant"));
            item.Lock = (byte)(reader.GetUInt16("Locked"));
            item.UnlockEnd = DateTime.FromBinary(reader.GetInt64("UnlockEnd"));
            item.Suspicious = reader.GetBoolean("Suspicious");
            item.SuspiciousStart = DateTime.FromBinary(reader.GetInt64("SuspiciousStart"));
            item.Color = (Game.Enums.Color)reader.GetUInt32("Color");
            //item.Warehouse = reader.GetUInt16("Warehouse");
            item.StackSize = reader.GetUInt16("StackSize");

            item.SuspiciousStart = DateTime.FromBinary(reader.GetInt64("SuspiciousStart"));
            if (item.StackSize > 10)
                item.StackSize = 0;
            item.RefineItem = reader.GetUInt32("RefineryItem");

            if (item.ID == 300000)
            {
                uint NextSteedColor = reader.GetUInt32("NextSteedColor");
                item.NextGreen = (byte)(NextSteedColor & 0xFF);
                item.NextBlue = (byte)((NextSteedColor >> 8) & 0xFF);
                item.NextRed = (byte)((NextSteedColor >> 16) & 0xFF);
            }
            Int64 rTime = reader.GetInt64("RefineryTime");
            if (item.RefineItem > 0 && rTime != 0)
            {
                item.RefineryTime = DateTime.FromBinary(rTime);
                if (DateTime.Now > item.RefineryTime)
                {
                    item.RefineryTime = new DateTime(0);
                    item.RefineItem = 0;
                }
            }
            if (item.Lock == 2)
                if (DateTime.Now >= item.UnlockEnd)
                    item.Lock = 0;
            return item;
        }
        public static void Load()
        {
            SR.Clear();
            Topers.Clear();
            using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(DataHolder.MySqlConnection.ConnectionString))
            {
                conn.Open();
                uint TopUid = 0;
                uint TopScore = 0;
                foreach (var C in Classs)
                {
                    using (MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand("select TotalPerfectionScore,Body,Name,Class,Level,UID,Face from entities WHERE class =@c order by TotalPerfectionScore desc limit 30 "))
                    {
                        cmd.Parameters.AddWithValue("@c", C);
                        Data D;
                        cmd.Connection = conn;
                        using (MySql.Data.MySqlClient.MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                D = new Data();
                                D.Class = rdr.GetByte("Class");
                                D.Level = rdr.GetByte("Level");
                                D.Mesh = rdr.GetUInt32("Face") * 10000 + rdr.GetUInt16("Body");
                                D.UID = rdr.GetUInt32("UID");
                                D.Prestige = rdr.GetUInt32("TotalPerfectionScore");
                                D.Name = rdr.GetString("Name");
                                if (TopScore == 0 || D.Prestige > TopScore)
                                {
                                    TopScore = D.Prestige;
                                    TopUid = D.UID;
                                }
                                if (!SR.ContainsKey(C))
                                {
                                    SR.Add(C, new List<Data>());
                                    SR[C].Add(D);
                                }
                                else
                                {
                                    SR[C].Add(D);
                                }
                            }
                        }
                    }
                }
                if (TopUid != 0)
                {
                    Best B = new Best();
                    B.Type = 0;
                    B.DB = new DataBest();
                    B.DB.Type = 1;
                    B.DB.Rank = 1;
                    using (MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM entities WHERE UID = @u", conn))
                    {
                        cmd.Parameters.AddWithValue("@u", TopUid);
                        using (MySql.Data.MySqlClient.MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                B.DB.HairStyle = rdr.GetUInt16("HairStyle");
                                B.DB.EntityUID = TopUid;
                                var g = Kernel.Guilds[rdr.GetUInt32("GuildID")];
                                if (g != null)
                                    B.DB.GuildName = g.Name;
                                B.DB.Name = rdr.GetString("Name");
                                B.DB.Mesh = (uint)(rdr.GetUInt16("Face") * 10000 + rdr.GetUInt16("Body"));
                            }
                        }
                    }
                    var it = LoadItem(TopUid, (byte)PacketHandler.Positions.Head);
                    if (it != null) B.DB.Head = it.ID;
                    it = LoadItem(TopUid, (byte)PacketHandler.Positions.Left);
                    if (it != null) B.DB.LeftWeapon = it.ID;
                    it = LoadItem(TopUid, (byte)PacketHandler.Positions.LeftAccessory);
                    if (it != null) B.DB.LeftWeaponAccessory = it.ID;
                    it = LoadItem(TopUid, (byte)PacketHandler.Positions.Right);
                    if (it != null) B.DB.RightWeapon = it.ID;
                    it = LoadItem(TopUid, (byte)PacketHandler.Positions.RightAccessory);
                    if (it != null) B.DB.RightWeaponAccessory = it.ID;
                    it = LoadItem(TopUid, (byte)PacketHandler.Positions.SteedArmor);
                    if (it != null) B.DB.MountArmor = it.ID;
                    if (B.DB.MountArmor == 0)
                    {
                        it = LoadItem(TopUid, (byte)PacketHandler.Positions.Steed);
                        if (it != null) B.DB.MountArmor = it.ID;
                    }
                    it = LoadItem(TopUid, (byte)PacketHandler.Positions.Garment);
                    if (it != null) B.DB.Garment = it.ID;
                    if (B.DB.Garment == 0)
                    {
                        it = LoadItem(TopUid, (byte)PacketHandler.Positions.Armor);
                        if (it != null) B.DB.Garment = it.ID;
                    }
                    if (B.DB.Wing == 0)
                    {
                        it = LoadItem(TopUid, (byte)PacketHandler.Positions.Wing);
                        if (it != null)
                        {
                            B.DB.Wing = it.ID;
                            B.DB.WingPlus = it.Plus;
                        }
                    }
                    using (var MS = new MemoryStream())
                    {
                        Serializer.SerializeWithLengthPrefix(MS, B, PrefixStyle.Fixed32);
                        var pkt = new byte[8 + MS.Length];
                        MS.ToArray().CopyTo(pkt, 0);
                        Writer.WriteUshort((ushort)MS.Length, 0, pkt);
                        Writer.WriteUshort((ushort)3257, 2, pkt);
                        S = pkt;
                    }
                }
            }
            foreach (var top in SR.Keys)
            {
                if (SR[top].Count != 0)
                    Topers.Add(top, SR[top].OrderByDescending(e => e.Prestige).FirstOrDefault());
            }
        }
        public static uint[] R(byte[] Buffer)
        {
            List<uint> ptr2 = new List<uint>();

            for (int i = 0; i < Buffer.Length; )
            {
                if (i + 2 <= Buffer.Length)
                {
                    int tmp = Buffer[i++];

                    if (tmp % 8 == 0)
                        while (true)
                        {
                            if (i + 1 > Buffer.Length) break;
                            tmp = Buffer[i++];
                            if (tmp < 128)
                            {
                                ptr2.Add((uint)tmp);
                                break;
                            }
                            else
                            {
                                int result = tmp & 0x7f;
                                if ((tmp = Buffer[i++]) < 128)
                                {
                                    result |= tmp << 7;
                                    ptr2.Add((uint)result);
                                    break;
                                }
                                else
                                {
                                    result |= (tmp & 0x7f) << 7;
                                    if ((tmp = Buffer[i++]) < 128)
                                    {
                                        result |= tmp << 14;
                                        ptr2.Add((uint)result);
                                        break;
                                    }
                                    else
                                    {
                                        result |= (tmp & 0x7f) << 14;
                                        if ((tmp = Buffer[i++]) < 128)
                                        {
                                            result |= tmp << 21;
                                            ptr2.Add((uint)result);
                                            break;
                                        }
                                        else
                                        {
                                            result |= (tmp & 0x7f) << 21;
                                            result |= (tmp = Buffer[i++]) << 28;
                                            ptr2.Add((uint)result);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                }
                else break;
            }
            return ptr2.ToArray();
        }
    }
}