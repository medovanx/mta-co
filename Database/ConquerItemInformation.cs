using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTA.Database
{
    public class ConquerItemInformation
    {
        public static SafeDictionary<uint, SafeDictionary<byte, ConquerItemPlusInformation>> PlusInformations;
        public static SafeDictionary<uint, ConquerItemBaseInformation> BaseInformations;
        /// <summary>
        /// string - item description
        /// int - grade key (ConquerItemBaseInformation.GradeKey)
        /// </summary>
        public static SafeDictionary<string, SafeDictionary<int, ConquerItemBaseInformation>> GradeInformations;
        public static SafeDictionary<string, SafeDictionary<uint, int>> GradeInformations2;
        public static void Load()
        {
            BaseInformations = new SafeDictionary<uint, ConquerItemBaseInformation>(10000);
            PlusInformations = new SafeDictionary<uint, SafeDictionary<byte, ConquerItemPlusInformation>>(10000);
            GradeInformations = new SafeDictionary<string, SafeDictionary<int, ConquerItemBaseInformation>>(10000);
            GradeInformations2 = new SafeDictionary<string, SafeDictionary<uint, int>>(10000);
            string[] baseText = File.ReadAllLines(Constants.ItemBaseInfosPath);
            //string text = "►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄►◄";
            //int for1prg = baseText.Length / (System.Console.WindowWidth - text.Length);
            int count = 0;
            //System.Console.Write(text);
            //var old1 = System.Console.BackgroundColor;
            //var old2 = System.Console.ForegroundColor;
            //System.Console.BackgroundColor = ConsoleColor.Gray;
            //System.Console.ForegroundColor = ConsoleColor.Gray;
            int gkey = 0;
            int lastlevel = 0;
            string lastgr = "";
            foreach (string line in baseText)
            {
                count++;
                string _item_ = line.Trim();
                if (_item_.Length > 11)
                {
                    if (_item_.IndexOf("//", 0, 2) != 0)
                    {
                        ConquerItemBaseInformation CIBI = new ConquerItemBaseInformation();
                        CIBI.Parse(_item_);

                        var Grades = GradeInformations[CIBI.Description];
                        BaseInformations.Add(CIBI.ID, CIBI);
                        if (GradeInformations.ContainsKey(CIBI.Description) == false)
                        {
                            GradeInformations2.Add(CIBI.Description, new SafeDictionary<uint, int>(1000));
                            GradeInformations2[CIBI.Description].Add((uint)(CIBI.ID / 10), 0);
                            lastlevel = CIBI.Level;
                            GradeInformations.Add(CIBI.Description, new SafeDictionary<int, ConquerItemBaseInformation>(1000));
                            gkey = 0;
                        }
                        else
                        {
                            if (lastgr != CIBI.Description)
                                gkey = GradeInformations2[CIBI.Description].Count - 1;

                            if (GradeInformations2[CIBI.Description].ContainsKey(CIBI.ID / 10) && CIBI.Level == lastlevel)
                            {
                                CIBI.GradeKey = gkey;
                                continue;
                            }
                            else
                            {
                                GradeInformations2[CIBI.Description].Add((uint)(CIBI.ID / 10), 0);
                                lastlevel = CIBI.Level;
                                gkey = gkey + 1;
                            }
                        }
                        lastgr = CIBI.Description;
                        CIBI.GradeKey = gkey;
                        GradeInformations[CIBI.Description].Add(gkey, CIBI);
                    }
                }
            }
            GradeInformations2.Clear();
            baseText = File.ReadAllLines(Constants.ItemPlusInfosPath);

            foreach (string line in baseText)
            {
                try
                {
                    string _item_ = line.Trim();
                    ConquerItemPlusInformation CIPI = new ConquerItemPlusInformation();
                    CIPI.Parse(_item_);
                    SafeDictionary<byte, ConquerItemPlusInformation> info = null;
                    if (PlusInformations.TryGetValue(CIPI.ID, out info))
                    {
                        info.Add(CIPI.Plus, CIPI);
                    }
                    else
                    {
                        PlusInformations.Add(CIPI.ID, new SafeDictionary<byte, ConquerItemPlusInformation>(1000));
                        if (PlusInformations.TryGetValue(CIPI.ID, out info))
                        {
                            info.Add(CIPI.Plus, CIPI);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine(line);
                }
            }
            Console.WriteLine("Item Base and Plus information loaded.");
        }
        public ConquerItemInformation(uint ID, byte Plus)
        {
            _BaseInformation = null;
            if (BaseInformations.TryGetValue(ID, out _BaseInformation))
            {
                getPlusInformation(ID, Plus);
            }
            else
            {
                return;
            }
        }

        public enum ItemSort
        {
            Invalid = -1,
            Expendable = 0,
            Helmet = 1,
            Necklace = 2,
            Armor = 3,
            Weapon1 = 4,
            Weapon2 = 5,
            Shield = 6,
            RingR = 7,
            Shoes = 8,
            Other = 9,
            RingL = 10,
            Overcoat = 11,
            IncreaseDmgArtifact = 12,
            DecreaseDmgArtifact = 13,
            MountAndAccessory = 14
        }
        public enum ItemType
        {
            Invalid = -1,

            // Weapon1
            Blade = 10000,
            Sword = 20000,
            Backsword = 21000,
            Hook = 30000,
            Whip = 40000,
            Axe = 50000,
            Hammer = 60000,
            Club = 80000,
            Scepter = 81000,
            Dagger = 90000,

            // Weapon2
            Bow = 00000,
            Glaive = 10000,
            Poleaxe = 30000,
            Longhammer = 40000,
            Spear = 60000,
            Wand = 61000,
            Pickaxe = 62000,
            Halbert = 80000,

            // Other
            Gem = 00000,
            TaskItem = 10000,
            ActionItem = 20000,
            ComposeItem = 30000,
            MonsterItem = 50000,
            PointCard = 80000,
            DarkHorn = 90000,

            // Expendable
            Physic = 00000,
            PhysicMana = 01000,
            PhysicLife = 02000,
            Franko = 50000,
            Spell = 60000,
            Ore = 70000,
            Special = 80000,
            Silver = 90000,

            // MountAndAccessory
            Mount = 00000,
            Weapon2Accessory = 50000,
            Weapon1Accessory = 60000,
            BowAccessory = 70000,
            ShieldAccessory = 80000,
        }

        public static ItemSort GetItemSort(uint type)
        {
            switch ((type % 10000000) / 100000)
            {
                case 1:
                    {
                        switch ((type % 1000000) / 10000)
                        {
                            case 11: return ItemSort.Helmet;
                            case 12: return ItemSort.Necklace;
                            case 13: return ItemSort.Armor;
                            case 14: return ItemSort.Helmet;
                            case 15: return ItemSort.RingR;
                            case 16: return ItemSort.Shoes;
                            case 17: break;
                            case 18: return ItemSort.Overcoat;
                            case 19: return ItemSort.Overcoat;
                        }
                        break;
                    }
                case 2:
                    {
                        switch ((type % 10000) / 1000)
                        {
                            case 1: return ItemSort.IncreaseDmgArtifact;
                            case 2: return ItemSort.DecreaseDmgArtifact;
                        }
                        break;
                    }
                case 3: return ItemSort.MountAndAccessory;
                case 4: return ItemSort.Weapon1;
                case 5: return ItemSort.Weapon2;
                case 6: return ItemSort.Weapon1;
                case 7: return ItemSort.Other;
                case 9: return ItemSort.Shield;
                case 10: return ItemSort.Expendable;
                default:
                    {
                        break;
                    }
            }

            var sort = ((type % 10000000) / 100000);
            if (sort >= 20 && sort < 30) return ItemSort.RingL;

            return ItemSort.Invalid;
        }
        public static ItemType GetItemType(uint type)
        {
            switch ((type % 10000000) / 100000)
            {
                case 3:
                case 4:
                case 5:
                case 6:
                    {
                        return (ItemType)((type % 100000) / 1000 * 1000);
                    }
                case 7:
                case 10:
                    {
                        return (ItemType)(((type % 100000) / 10000) * 10000);
                    }
            }
            return ItemType.Invalid;
        }

        private void getPlusInformation(uint ID, byte Plus)
        {
            var itemtype = ID;
            var type = GetItemType(ID);
            var sort = GetItemSort(ID);
            if (type == ItemType.Backsword || type == ItemType.Bow)
            {
            }
            else if (sort == ItemSort.Weapon1)
            {
                itemtype = (itemtype / 100000) * 100000 + (itemtype % 1000) + 44000;
            }
            else if (sort == ItemSort.Weapon2)
            {
                itemtype = (itemtype / 100000) * 100000 + (itemtype % 1000) + 55000;
            }
            else if (sort == ItemSort.Helmet || sort == ItemSort.Armor || sort == ItemSort.Shield)
            {
                itemtype = (itemtype / 1000) * 1000 + (itemtype % 1000);
            }

            itemtype = itemtype / 10 * 10;
            ID = (uint)(
                             ID - (ID % 10) // [5] = 0
                         );
            uint orID = ID;
            byte itemType = (byte)(ID / 10000);
            ushort itemType2 = (ushort)(ID / 1000);
            if (itemType == 14 && itemType2 != 143 && itemType2 != 142 && itemType2 != 141)//armors
            {
                ID = (uint)(
                            (((uint)(ID / 1000)) * 1000) + // [3] = 0
                            ((ID % 100))
                        );
            }
            else if (itemType2 == 141 || itemType2 == 142 || itemType2 == 143 || itemType == 13 || itemType == 11 || itemType2 == 123 || itemType == 30 || itemType == 20 || itemType == 12 || itemType == 15 || itemType == 16 || itemType == 50 || itemType2 == 421 || itemType2 == 601 || itemType2 == 610 || itemType == 90)//Necky bow bag
            {
                ID = (uint)(
                            ID - (ID % 10) // [5] = 0
                        );
            }
            else
            {
                byte head = (byte)(ID / 100000);
                ID = (uint)(
                        ((head * 100000) + (head * 10000) + (head * 1000)) + // [1] = [0], [2] = [0]
                        ((ID % 1000) - (ID % 10)) // [5] = 0
                    );
            }

            _PlusInformation = new ConquerItemPlusInformation();
            if (Plus > 0)
            {
                SafeDictionary<byte, ConquerItemPlusInformation> pInfo = null;
                if (!PlusInformations.TryGetValue(itemtype, out pInfo)) if (!PlusInformations.TryGetValue(ID, out pInfo)) PlusInformations.TryGetValue(orID, out pInfo);
                if (pInfo == null)
                {
                    return;
                }
                if (!pInfo.TryGetValue(Plus, out _PlusInformation))
                {
                    _PlusInformation = new ConquerItemPlusInformation();
                }
            }
        }

        /* private void getPlusInformation(uint ID, byte Plus)
         {
             var itemtype = ID;
             var type = GetItemType(ID);
             var sort = GetItemSort(ID);
             if (type == ItemType.Backsword || type == ItemType.Bow)
             {
             }
             else if (sort == ItemSort.Weapon1)
             {
                 itemtype = (itemtype / 100000) * 100000 + (itemtype % 1000) + 44000;
             }
             else if (sort == ItemSort.Weapon2)
             {
                 itemtype = (itemtype / 100000) * 100000 + (itemtype % 1000) + 55000;
             }
             else if (sort == ItemSort.Helmet || sort == ItemSort.Armor || sort == ItemSort.Shield)
             {
                 itemtype = (itemtype / 1000) * 1000 + (itemtype % 1000);
             }

             itemtype = itemtype / 10 * 10;

             /* byte itemType = (byte)(ID / 10000);
              ushort itemType2 = (ushort)(ID / 1000);
              if (itemType == 14 && itemType2 != 143 && itemType2 != 142 && itemType2 != 141)//armors
              {
                  ID = (uint)(
                              (((uint)(ID / 1000)) * 1000) + // [3] = 0
                              ((ID % 100))
                          );
              }
              else if (itemType2 == 141 || itemType2 == 142 || itemType2 == 143 || itemType == 13 || itemType == 11 || itemType2 == 123 || itemType == 30 || itemType == 20 || itemType == 12 || itemType == 15 || itemType == 16 || itemType == 50 || itemType2 == 421 || itemType2 == 601 || itemType2 == 610 || itemType == 90)//Necky bow bag
              {
                  ID = (uint)(
                              ID - (ID % 10) // [5] = 0
                          );
              }
              else
              {
                  byte head = (byte)(ID / 100000);
                  ID = (uint)(
                          ((head * 100000) + (head * 10000) + (head * 1000)) + // [1] = [0], [2] = [0]
                          ((ID % 1000) - (ID % 10)) // [5] = 0
                      );
              }

             _PlusInformation = new ConquerItemPlusInformation();
             if (Plus > 0)
             {
                 SafeDictionary<byte, ConquerItemPlusInformation> pInfo = null;
                 if (!PlusInformations.TryGetValue(itemtype, out pInfo))
                     PlusInformations.TryGetValue(ID, out pInfo);
                 if (pInfo == null)
                     if (!pInfo.TryGetValue(Plus, out _PlusInformation))
                         _PlusInformation = new ConquerItemPlusInformation();
             }
         }
 */
        private ConquerItemPlusInformation _PlusInformation;
        private ConquerItemBaseInformation _BaseInformation;
        public ConquerItemPlusInformation PlusInformation
        {
            get
            {
                if (_PlusInformation == null)
                    return new ConquerItemPlusInformation();
                return _PlusInformation;
            }
        }
        public ConquerItemBaseInformation BaseInformation
        {
            get
            {
                if (_BaseInformation == null)
                    return new ConquerItemBaseInformation();
                return _BaseInformation;
            }
        }
        public uint CalculateUplevel()
        {
            var item = BaseInformations.Values.Where(x => x.Level > this.BaseInformation.Level && (x.ID / 1000) == (this.BaseInformation.ID / 1000) && (x.ID % 10) == (this.BaseInformation.ID % 10)).OrderBy(y => y.Level).FirstOrDefault();
            if (item == null)
                return BaseInformation.ID;
            else
                return item.ID;
        }
        public uint CalculateDownlevel()
        {
            if (BaseInformation.ID / 1000 == 616)
            {
                return (uint)(616010 + BaseInformation.ID % 10);
            }
            var grades = GradeInformations[this.BaseInformation.Description];
            if (grades == null) return BaseInformation.ID;
            if (grades[BaseInformation.GradeKey - 1] == null)
                return BaseInformation.ID;
            else
                return (uint)((grades[BaseInformation.GradeKey - 1].ID / 10) * 10 + BaseInformation.ID % 10);
        }
        public uint LowestID(byte Level)
        {
            if (BaseInformation.ID / 1000 == 616)
            {
                return (uint)(616010 + BaseInformation.ID % 10);
            }
            var grades = GradeInformations[this.BaseInformation.Description];

            if (grades == null) return BaseInformation.ID;
            for (byte gr = 0; gr < grades.Count; gr++)
                if (grades[gr].Level == Level)
                    return (uint)((grades[gr + 1].ID / 10) * 10 + BaseInformation.ID % 10);
            return (uint)((grades[0].ID / 10) * 10 + BaseInformation.ID % 10);
        }
    }
    public class ConquerItemPlusInformation
    {
        public uint ID;
        public byte Plus;
        public ushort ItemHP;
        public uint MinAttack;
        public uint MaxAttack;
        public ushort PhysicalDefence;
        public ushort MagicAttack;
        public ushort MagicDefence;
        public ushort Agility;
        public ushort Vigor { get { return Agility; } }
        public byte Dodge;
        public ushort Accuracy;
        public ushort SpeedPlus { get { return Dodge; } }
        public void Parse(string Line)
        {
            string[] Info = Line.Split(' ');
            ID = uint.Parse(Info[0]);
            Plus = byte.Parse(Info[1]);
            ItemHP = ushort.Parse(Info[2]);
            MinAttack = uint.Parse(Info[3]);
            MaxAttack = uint.Parse(Info[4]);
            PhysicalDefence = ushort.Parse(Info[5]);
            MagicAttack = ushort.Parse(Info[6]);
            MagicDefence = ushort.Parse(Info[7]);
            Agility = ushort.Parse(Info[8]);
            Dodge = byte.Parse(Info[9]);
        }
        public ConquerItemPlusInformation()
        {
            Accuracy = 0;
            ID = MinAttack = MaxAttack = 0;
            Plus = Dodge = 0;
            PhysicalDefence = MagicAttack = MagicDefence = Agility = 0;
        }
    }
    public class ConquerItemBaseInformation
    {
        public string LoweredName;
        public string LowerName;
        public uint ID;
        public string Name;
        public byte Class;
        public byte Proficiency;
        public byte Level;
        public byte Gender;
        public ushort Strength;
        public ushort Agility;
        public uint GoldWorth;
        public ushort MinAttack;
        public ushort MaxAttack;
        public ushort PhysicalDefence;
        public ushort MagicDefence;
        public ushort MagicAttack;
        public byte Dodge;
        public int Accuracy;
        public ushort Frequency;
        public uint ConquerPointsWorth;
        public ushort Durability;
        public ushort StackSize;
        public ushort ItemHP;
        public ushort ItemMP;
        public ushort AttackRange;
        public ushort AttackSpeed;
        public ItemType Type;
        public string Description;
        public int GradeKey;
        public string FinalDescription;

        public uint PurificationLevel;
        public ushort PurificationMeteorNeed;
        public ushort PurificationitemType;
        public ushort PurificationPosition;

        public int EarthResist,
           MetalResist, FireResist, WaterResist, WoodResist, Block, Detoxication,
            CounterAction, Penetration, Immunity, BreakThrough, CriticalStrike, SkillCriticalStrike;
        public uint Weight;
        private ushort auction_class;
        private ushort auction_deposit;
        public uint Time;
        public void Parse(string Line)
        {
            string[] data = Line.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
            ID = Convert.ToUInt32(data[0]);
            Name = data[1].Trim();
            LowerName = Name.ToLower();
            Class = Convert.ToByte(data[2]);
            Proficiency = Convert.ToByte(data[3]);
            Level = Convert.ToByte(data[4]);
            Gender = Convert.ToByte(data[5]);
            Strength = Convert.ToUInt16(data[6]);
            Agility = Convert.ToUInt16(data[7]);
            Type = Convert.ToUInt32(data[10]) == 0 ? ItemType.Dropable : ItemType.Others;
            GoldWorth = Convert.ToUInt32(data[12]);
            MaxAttack = Convert.ToUInt16(data[14]);
            MinAttack = Convert.ToUInt16(data[15]);
            PhysicalDefence = Convert.ToUInt16(data[16]);
            Frequency = Convert.ToUInt16(data[17]);
            Dodge = Convert.ToByte(data[18]);
            ItemHP = Convert.ToUInt16(data[19]);
            ItemMP = Convert.ToUInt16(data[20]);
            Durability = Convert.ToUInt16(data[22]);
            MagicAttack = Convert.ToUInt16(data[30]);
            MagicDefence = Convert.ToUInt16(data[31]);
            AttackRange = Convert.ToUInt16(data[32]);
            ConquerPointsWorth = Convert.ToUInt32(data[37]);
            StackSize = Convert.ToUInt16(data[47]);
            EarthResist = Convert.ToUInt16(data[52]);
            MetalResist = Convert.ToUInt16(data[48]);
            FireResist = Convert.ToUInt16(data[51]);
            WaterResist = Convert.ToUInt16(data[50]);
            WoodResist = Convert.ToUInt16(data[49]);
            Block = Convert.ToUInt16(data[44]);
            //Detoxication = Convert.ToUInt16(data[43]);
            CounterAction = Convert.ToUInt16(data[46]);
            Penetration = Convert.ToUInt16(data[43]);
            Immunity = Convert.ToUInt16(data[42]);
            BreakThrough = Convert.ToUInt16(data[45]);
            CriticalStrike = Convert.ToUInt16(data[40]);
            SkillCriticalStrike = Convert.ToUInt16(data[41]);

            Description = data[53].Replace("`s", "");
            if (Description == "NinjaKatana")
                Description = "NinjaWeapon";
            if (Description == "Earrings")
                Description = "Earring";
            if (Description == "Bow")
                Description = "ArcherBow";
            if (Description == "Backsword")
                Description = "TaoistBackSword";
            Description = Description.ToLower();

            if (data.Length >= 58)
            {
                PurificationLevel = Convert.ToUInt16(data[56]);
                PurificationMeteorNeed = Convert.ToUInt16(data[57]);
            }
            if (data.Length >= 60)
            {
                auction_class = Convert.ToUInt16(data[59]);
                auction_deposit = Convert.ToUInt16(data[60]);
            }
            Time = Convert.ToUInt32(data[39]);

        }
        public enum ItemType : byte
        {
            Dropable = 0,
            Others
        }
    }
}