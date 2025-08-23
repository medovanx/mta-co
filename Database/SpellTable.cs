using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Database
{
    public enum LeftToAdd : ushort
    {
        Bless = 9876,
        //----------
        SummonGuard = 4000,
        SummonBat = 4010,
        SummonBatBoss = 4020,
        BloodyBat = 4050,
        FireEvil = 4060,
        Skeleton = 4070
    }
    public class SpellSort
    {
        public const byte
            Damage = 1,
            Heal = 2,
            MultiWeaponSkill = 4,
            Circle = 5,
            XP = 6,
            Revive = 7,
            XPIncrease = 11,
            Dash = 12,
            Linear = 14,
            SingleWeaponSkill = 16,
            Specials = 19,
            ManaAdd = 20,
            Summon = 23,
            HPPercentDecrease = 26,
            Spook = 30,
            WarCry = 31,
            Ride = 32;
    }
    public class SpellTarget
    {
        public const byte
            Magic = 0,
            EntitiesOnly = 1,
            Self = 2,
            AroundCoordonates = 4,
            Sector = 8,//power % 1000 = sector angle
            AutoAttack = 16,
            PlayersOnly = 32;
    }
    public class SpellInformation
    {
        public ushort ID;
        public byte Level;

        public bool CanKill;
        public byte Sort;
        public bool OnlyGround;
        public bool Multi;
        public byte Target;

        public ushort UseMana;
        public byte UseStamina;
        public byte UseFrankos;

        public byte Percent;
        public int Sector;

        public int Duration;

        public ushort Range;
        public ushort Distance;

        public ushort Power;
        public float PowerPercent;

        public ulong Status;

        public uint NeedExperience;
        public byte NeedLevel;
        public byte NeedXP;
        public ushort CPCost;

        public List<ushort> WeaponSubtype;
        public List<ushort> OnlyWithThisWeaponSubtype;
        public ushort NextSpellID;

        public string Name;
    }
    public class SpellTable
    {
        public static SafeDictionary<ushort, SafeDictionary<byte, SpellInformation>> SpellInformations = new SafeDictionary<ushort, SafeDictionary<byte, SpellInformation>>();
        public static SafeDictionary<ushort, List<ushort>> WeaponSpells = new SafeDictionary<ushort, List<ushort>>();
        public static void Load()
        {
            using (var command = new MySqlCommand(MySqlCommandType.SELECT).Select("spells"))
            using (var reader = command.CreateReader())
            {
                while (reader.Read())
                {
                    SpellInformation spell = new SpellInformation();
                    spell.ID = reader.ReadUInt16("type");
                    spell.Name = reader.ReadString("Name");
                    spell.Sort = reader.ReadByte("sort");
                    spell.CanKill = reader.ReadBoolean("crime");
                    spell.OnlyGround = reader.ReadBoolean("ground");
                    spell.Multi = reader.ReadBoolean("multi");
                    spell.Target = reader.ReadByte("target");
                    spell.Level = reader.ReadByte("level");
                    spell.UseMana = reader.ReadUInt16("use_mp");
                    spell.UseStamina = reader.ReadByte("use_ep");
                    spell.UseFrankos = reader.ReadByte("use_item_num");
                    spell.Power = reader.ReadUInt16("power");
                    spell.PowerPercent = ((float)reader.ReadUInt16("power") % 1000) / 100;
                    //  if (spell.Power > 13000) spell.Power = 13000;
                    spell.Percent = reader.ReadByte("percent");
                    spell.Duration = reader.ReadInt32("step_secs");
                    spell.Range = reader.ReadUInt16("range");
                    spell.Sector = spell.Range * 20;
                    spell.Distance = reader.ReadUInt16("distance");
                    if (spell.Distance >= 4) spell.Distance--;
                    spell.Status = reader.ReadUInt64("status");
                    spell.NeedExperience = reader.ReadUInt32("need_exp");
                    spell.NeedLevel = reader.ReadByte("need_level");

                    spell.WeaponSubtype = new List<ushort>();
                    spell.OnlyWithThisWeaponSubtype = new List<ushort>();
                    var WeaponSubtype = reader.ReadUInt32("weapon_subtype");

                    var subtype1 = (ushort)(WeaponSubtype % 1000);
                    var subtype2 = (ushort)((WeaponSubtype / 1000) % 1000);
                    var subtype3 = (ushort)((WeaponSubtype / 1000000) % 1000);
                    if (WeaponSubtype == 60000)
                        subtype1 = 614;
                        //subtype2 = 622;
                    if (subtype1 != 0)
                    {
                        spell.WeaponSubtype.Add(subtype1);
                        spell.OnlyWithThisWeaponSubtype.Add(subtype1);
                    }
                    if (subtype2 != 0)
                    {
                        spell.WeaponSubtype.Add(subtype2);
                        spell.OnlyWithThisWeaponSubtype.Add(subtype2);
                    }
                    if (subtype3 != 0)
                    {
                        spell.WeaponSubtype.Add(subtype3);
                        spell.OnlyWithThisWeaponSubtype.Add(subtype3);
                    }

                    if (WeaponSubtype == 50000)
                        spell.WeaponSubtype = spell.OnlyWithThisWeaponSubtype = new List<ushort>();

                    spell.NextSpellID = reader.ReadUInt16("next_magic");
                    spell.NeedXP = reader.ReadByte("use_xp");
                    //  spell.CPCost = reader.ReadUInt16("cpcost");
                    if (spell.CPCost == 0 && spell.Level == 0)
                        spell.CPCost = 27;
                    if (spell.CPCost == 0 && spell.Level == 1)
                        spell.CPCost = 81;
                    if (spell.CPCost == 0 && spell.Level == 2)
                        spell.CPCost = 122;
                    if (spell.CPCost == 0 && spell.Level == 3)
                        spell.CPCost = 181;

                    if (SpellInformations.ContainsKey(spell.ID))
                    {
                        SpellInformations[spell.ID].Add(spell.Level, spell);
                    }
                    else
                    {
                        SpellInformations.Add(spell.ID, new SafeDictionary<byte, SpellInformation>(10));
                        SpellInformations[spell.ID].Add(spell.Level, spell);
                    }
                    if (spell.Distance > 17)
                        spell.Distance = 17;
                    if (spell.WeaponSubtype.Count != 0)
                    {
                        switch (spell.ID)
                        {
                            case 5010:
                            case 7020:
                            case 1290:
                            case 1260:
                            case 5030:
                            case 5040:
                            case 7000:
                            case 7010:
                            case 7030:
                            case 7040:
                            case 1250:
                            case 5050:
                            case 5020:
                            case 10490:
                            case 11140:
                            case 1300:
                            case 11990:
                            case 12110:
                            case 12240:
                            case 12230:
                            case 12220:
                            case 12210:
                            case 12560:
                            case 12570:
                                //  if (spell.Distance >= 3)
                                //      spell.Distance = 3;
                                //  if (spell.Range > 3)
                                //      spell.Range = 3;
                                for (int i = 0; i < spell.WeaponSubtype.Count; i++)
                                {
                                    var subtype = spell.WeaponSubtype[i];                  
                                    if (!WeaponSpells.ContainsKey(subtype))
                                        WeaponSpells.Add(subtype, new List<ushort>());
                                    if (!WeaponSpells[subtype].Contains(spell.ID))
                                        WeaponSpells[subtype].Add(spell.ID);
                                }
                                break;
                        }

                    }

                }
            }
            Console.WriteLine("Spells information loaded.");
        }
        public static SpellInformation GetSpell(ushort ID, Client.GameState client)
        {
            if (client != null)
                if (client.Spells.ContainsKey(ID))
                    return GetSpell(ID, client.Spells[ID].Level);
            return GetSpell(ID, 0);
        }
        public static SpellInformation GetSpell(ushort ID, byte level)
        {
            if (SpellInformations.ContainsKey(ID))
            {
                var dict = SpellInformations[ID];
                if (dict.ContainsKey(level))
                    return dict[level];
                else if (dict.Count != 0) return dict.Values.First(p => p.ID != 0);
                else return null;

            }
            else return null;
        }
        public static readonly List<ushort> AllowSkillSoul = new List<ushort>()
        {
           1115,
            11980,
            11005,
            1165,
            6001,
            6000,
            11650,
            1006,
            1002,
            10415,
            11110,
            11600,
            10381,
            12170,
            12160,
            12220,
            12350,
            12690,
            11660,
            1046,
            6010,
            11000,
            5010,
            11170,
            1045,
            5030,
            11070,
            12070,
            12080
        };
    }
}
