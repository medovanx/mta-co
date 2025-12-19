using System;
using System.Collections.Generic;
using System.IO;
using MTA.Client;
using MTA.Database;
using MTA.Game;
using MTA.Network.GamePackets;
using BoothItem = MTA.Game.ConquerStructures.BoothItem;

namespace MTA.MrNiTro.Systems {
    public class Booths {
        public enum BoothType {
            Npc = 0,
            Entity = 1
        }

        public static SafeDictionary<uint, Booth> Boooths = new SafeDictionary<uint, Booth>();

        public static void Load() {
            string[] text = File.ReadAllLines(Constants.BoothsPath);
            Booth booth = new();
            for (int x = 0; x < text.Length; x++) {
                string line = text[x];
                string[] split = line.Split('=');
                if (split[0] == "ID") {
                    if (booth.UID == 0)
                        booth.UID = uint.Parse(split[1]);
                    else {
                        if (!Boooths.ContainsKey(booth.UID)) {
                            Boooths.Add(booth.UID, booth);
                            booth = new Booth {
                                UID = uint.Parse(split[1])
                            };
                        }
                    }
                }
                else if (split[0] == "Type") {
                    booth.Type = (BoothType)byte.Parse(split[1]);
                }
                else if (split[0] == "Name") {
                    booth.Name = split[1];
                }
                else if (split[0] == "BotMessage") {
                    booth.BotMessage = split[1];
                }
                else if (split[0] == "Garment") {
                    booth.Garment = uint.Parse(split[1]);
                }
                else if (split[0] == "Head") {
                    booth.Head = uint.Parse(split[1]);
                }
                else if (split[0] == "WeaponR") {
                    booth.WeaponR = uint.Parse(split[1]);
                }
                else if (split[0] == "WeaponL") {
                    booth.WeaponL = uint.Parse(split[1]);
                }
                else if (split[0] == "Armor") {
                    booth.Armor = uint.Parse(split[1]);
                }
                else if (split[0] == "Mesh") {
                    booth.Mesh = ushort.Parse(split[1]);
                }
                else if (split[0] == "Map") {
                    booth.Map = ushort.Parse(split[1]);
                }
                else if (split[0] == "X") {
                    booth.X = ushort.Parse(split[1]);
                }
                else if (split[0] == "Y") {
                    booth.Y = ushort.Parse(split[1]);
                }
                else if (split[0] == "ItemAmount") {
                    booth.Items = new List<string>(ushort.Parse(split[1]));
                }
                else if (split[0].Contains("Item") && split[0] != "ItemAmount") {
                    string name = split[1];
                    booth.Items.Add(name);
                }
            }

            if (!Boooths.ContainsKey(booth.UID))
                Boooths.Add(booth.UID, booth);
            CreateBooths();
        }

        public static void UpdateCoordonatesForAngle(ref ushort X, ref ushort Y, Enums.ConquerAngle angle) {
            sbyte xi = 0, yi = 0;
            switch (angle) {
                case Enums.ConquerAngle.North:
                    xi = 1;
                    yi = 1;
                    break;
                case Enums.ConquerAngle.South:
                    xi = -1;
                    yi = -1;
                    break;
                case Enums.ConquerAngle.East:
                    xi = -1;
                    yi = 1;
                    break;
                case Enums.ConquerAngle.West:
                    xi = 1;
                    yi = -1;
                    break;
                case Enums.ConquerAngle.NorthWest: xi = 1; break;
                case Enums.ConquerAngle.SouthWest: yi = -1; break;
                case Enums.ConquerAngle.NorthEast: yi = 1; break;
                case Enums.ConquerAngle.SouthEast: xi = -1; break;
            }

            X = (ushort)(X + xi);
            Y = (ushort)(Y + yi);
        }

        public static void CreateBooths() {
            foreach (var bo in Boooths.Values) {
                Game.ConquerStructures.Booth booth = new Game.ConquerStructures.Booth();

                SobNpcSpawn Base = new SobNpcSpawn {
                    UID = bo.UID
                };


                if (Game.ConquerStructures.Booth.Booths2.ContainsKey(Base.UID))
                    Game.ConquerStructures.Booth.Booths2.Remove(Base.UID);
                Game.ConquerStructures.Booth.Booths2.Add(Base.UID, booth);
                Base.Mesh = bo.Mesh;
                Base.Type = Enums.NpcType.Booth;
                Base.ShowName = true;
                Base.Name = bo.Name;

                Base.MapID = bo.Map;
                Base.X = bo.X;
                Base.Y = bo.Y;
                booth.Base = Base;

                if (bo.Type == BoothType.Entity) {
                    var c = new GameState(null);
                    c.FakeLoad2(bo.UID, bo.Name);

                    uint WeaponR = bo.WeaponR;
                    uint WeaponL = bo.WeaponL;
                    uint Armor = bo.Armor;
                    uint Head = bo.Head;
                    uint Garment = bo.Garment;

                    ConquerItem? item7 = null;
                    ClientEquip? equip = null;
                    if (WeaponR > 0) {
                        ConquerItemBaseInformation CIBI = ConquerItemInformation.BaseInformations[WeaponR];
                        if (CIBI == null) return;
                        item7 = new ConquerItem(true) {
                            ID = WeaponR,
                            UID = Program.NextItemID,
                            //Program.NextItemID++;
                            Position = 4,
                            Durability = CIBI.Durability,
                            MaximDurability = CIBI.Durability
                        };
                        c.Equipment.Remove(4);
                        if (c.Equipment.Objects[3] != null) {
                            c.Equipment.Objects[3] = null;
                        }

                        c.Equipment.Add(item7);
                        item7.Mode = Enums.ItemMode.Update;
                        item7.Send(c);
                        equip = new ClientEquip();
                        equip.DoEquips(c);
                        c.Send(equip);
                        c.Equipment.UpdateEntityPacket();
                    }

                    if (WeaponL > 0) {
                        ConquerItemBaseInformation CIBI = ConquerItemInformation.BaseInformations[WeaponL];
                        if (CIBI == null) return;
                        item7 = new ConquerItem(true) {
                            ID = WeaponL,
                            UID = Program.NextItemID,
                            Position = 5,
                            Durability = CIBI.Durability,
                            MaximDurability = CIBI.Durability
                        };
                        c.Equipment.Remove(5);
                        if (c.Equipment.Objects[4] != null) {
                            c.Equipment.Objects[4] = null;
                        }

                        c.Equipment.Add(item7);
                        item7.Mode = Enums.ItemMode.Update;
                        item7.Send(c);
                        equip = new ClientEquip();
                        equip.DoEquips(c);
                        c.Send(equip);
                        c.Equipment.UpdateEntityPacket();
                    }

                    if (Armor > 0) {
                        ConquerItemBaseInformation CIBI = ConquerItemInformation.BaseInformations[Armor];
                        if (CIBI == null) return;
                        item7 = new ConquerItem(true) {
                            ID = Armor,
                            UID = Program.NextItemID,
                            //Program.NextItemID++;
                            Position = 3,
                            Durability = CIBI.Durability,
                            MaximDurability = CIBI.Durability
                        };
                        c.Equipment.Remove(3);
                        if (c.Equipment.Objects[2] != null) {
                            c.Equipment.Objects[2] = null;
                        }

                        c.Equipment.Add(item7);
                        item7.Mode = Enums.ItemMode.Update;
                        item7.Send(c);
                        equip = new ClientEquip();
                        equip.DoEquips(c);
                        c.Send(equip);
                        c.Equipment.UpdateEntityPacket();
                    }

                    if (Head > 0) {
                        ConquerItemBaseInformation CIBI = ConquerItemInformation.BaseInformations[Head];
                        if (CIBI == null) return;
                        item7 = new ConquerItem(true) {
                            ID = Head,
                            UID = Program.NextItemID,
                            //Program.NextItemID++;
                            Position = 1,
                            Durability = CIBI.Durability,
                            MaximDurability = CIBI.Durability
                        };
                        c.Equipment.Remove(1);
                        if (c.Equipment.Objects[0] != null) {
                            c.Equipment.Objects[0] = null;
                        }

                        c.Equipment.Add(item7);
                        item7.Mode = Enums.ItemMode.Update;
                        item7.Send(c);
                        equip = new ClientEquip();
                        equip.DoEquips(c);
                        c.Send(equip);
                        c.Equipment.UpdateEntityPacket();
                    }

                    if (Garment > 0) {
                        ConquerItemBaseInformation CIBI = ConquerItemInformation.BaseInformations[Garment];
                        if (CIBI == null) return;
                        item7 = new ConquerItem(true) {
                            ID = Garment,
                            UID = Program.NextItemID,
                            //Program.NextItemID++;
                            Position = 9,
                            Durability = CIBI.Durability,
                            MaximDurability = CIBI.Durability
                        };
                        c.Equipment.Remove(9);
                        if (c.Equipment.Objects[8] != null) {
                            c.Equipment.Objects[8] = null;
                        }

                        c.Equipment.Add(item7);
                        item7.Mode = Enums.ItemMode.Update;
                        item7.Send(c);
                        equip = new ClientEquip();
                        equip.DoEquips(c);
                        c.Send(equip);
                        c.Equipment.UpdateEntityPacket();
                    }

                    c.Entity.Facing = (Enums.ConquerAngle)(bo.Mesh % 10);
                    UpdateCoordonatesForAngle(ref bo.X, ref bo.Y, c.Entity.Facing);
                    c.Entity.X = bo.X;
                    c.Entity.Y = bo.Y;
                    c.Entity.MapID = bo.Map;
                    c.Booth = booth;
                    c.Booth.HawkMessage = new Message(bo.BotMessage, Message.HawkMessage);
                    c.Entity.Action = Enums.ConquerAction.Sit;
                    c.Send(new Data(true) { ID = Data.ChangeAction, UID = c.Entity.UID, dwParam = 0 });
                    var data = new Data(true) {
                        UID = c.Entity.UID,
                        dwParam = Base.UID,
                        wParam1 = Base.X,
                        wParam2 = Base.Y,
                        ID = Data.OwnBooth
                    };
                    c.Send(data);
                    Base.Owner = c;
                }
                else {
                    if (!Kernel.Maps.ContainsKey(bo.Map)) {
                        if (DMaps.MapPaths.TryGetValue(bo.Map, out string? value))
                            _ = new Map(bo.Map, value);
                        else
                            _ = new Map(bo.Map, "");
                    }

                    if (Kernel.Maps[bo.Map].Npcs.ContainsKey(Base.UID))
                        Kernel.Maps[bo.Map].Npcs.Remove(Base.UID);
                    Kernel.Maps[bo.Map].Npcs.Add(Base.UID, Base);
                }

                for (int i = 0; i < bo.Items.Count; i++) {
                    var line = bo.Items[i].Split(["@@", "@"], StringSplitOptions.RemoveEmptyEntries);

                    #region booth

                    BoothItem item = new();

                    Booth booth1 = new();
                    item.Item = new ConquerItem(true) {
                        UID = Program.NextItemID,

                        //Program.NextItemID++;
                        ID = uint.Parse(line[0])
                    };
                    if (line.Length >= 2)
                        item.Cost = uint.Parse(line[1]);
                    if (line.Length >= 3)
                        item.Item.Plus = byte.Parse(line[2]);
                    if (line.Length >= 4)
                        item.Item.Enchant = byte.Parse(line[3]);
                    if (line.Length >= 5)
                        item.Item.Bless = byte.Parse(line[4]);
                    if (line.Length >= 6)
                        item.Item.SocketOne = (Enums.Gem)byte.Parse(line[5]);
                    if (line.Length >= 7)
                        item.Item.SocketTwo = (Enums.Gem)byte.Parse(line[6]);
                    if (line.Length >= 8)
                        item.Item.StackSize = ushort.Parse(line[7]);


                    if (line.Length >= 19)
                        item.Item.Bound = true;


                    ConquerItemBaseInformation? CIBI = null;
                    CIBI = ConquerItemInformation.BaseInformations[item.Item.ID];
                    if (CIBI == null)
                        break;
                    item.Item.Durability = CIBI.Durability;
                    item.Item.MaximDurability = CIBI.Durability;
                    item.Cost_Type = BoothItem.CostType.ConquerPoints;
                    booth.ItemList.Add(item.Item.UID, item);

                    #endregion
                }
            }

            Console.WriteLine("" + Game.ConquerStructures.Booth.Booths2.Count + " New Booths Loaded.");
        }

        public class Booth {
            public uint Armor = 135259;
            public string BotMessage = "Selling Items.[Boothing AI]";
            public uint Garment = 194300;
            public uint Head = 112259;
            public List<string>? Items;
            public ushort Map;
            public ushort Mesh = 100;
            public string? Name;
            public BoothType Type;
            public uint UID;
            public uint WeaponL = 601439;
            public uint WeaponR = 601439;
            public ushort X;
            public ushort Y;
        }
    }
}