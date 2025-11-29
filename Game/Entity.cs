using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MTA.Network;
using MTA.Network.GamePackets;
using System.Collections;
using MTA.Interfaces;
using MTA.Database;
using System.Collections.Concurrent;
using MTA.Client;
using MTA.Magic;

namespace MTA.Game
{
    public class Entity : Writer, IBaseEntity, IMapObject
    {
        public byte _Windwalker;

        public byte Windwalker
        {
            get
            {
                SpawnPacket[304] = _Windwalker;
                return _Windwalker;
            }
            set
            {
                _Windwalker = value;
                SpawnPacket[304] = value;
                if (value > 0)
                    AddFlag4(Network.GamePackets.Update.Flags4.JusticeChant);
            }
        }

        public bool
            ClaimedElitePk = false,
            ClaimedTeamPK = false,
            ClaimedSTeamPK = false;

        public bool InJail()
        {
            if (MapID == 6000 || MapID == 6001)
                return true;
            return false;
        }

        public int MyAllin { get; set; }
        public uint PokerTableUID = 0;

        public Game.ConquerStructures.PokerTable PokerTable
        {
            get
            {
                if (Database.PokerTables.Tables.ContainsKey(PokerTableUID))
                    return Database.PokerTables.Tables[PokerTableUID];
                else return null;
            }
            set { PokerTableUID = value.UID; }
        }

        public uint BankCps;
        public bool InAutoHunt;
        public uint DeputyLeader = 0;
        public Time32 XpBlueStamp;
        public Time32 BackfireStamp;
        public Time32 ManiacDance;
        public byte XPCountTwist = 0;

        public bool EpicWarrior()
        {
            if (EntityFlag == Game.EntityFlag.Player)
            {
                var weapons = Owner.Weapons;
                if (weapons.Item1 != null)
                    if (weapons.Item1.ID / 1000 == 624)
                        return true;
                    else if (weapons.Item2 != null)
                        if (weapons.Item2.ID / 1000 == 624)
                            return true;
            }

            return false;
        }

        public List<string> BlackList;
        public WardrobeTitles WTitles;
        public Dictionary<uint, ConquerItem> StorageItems;
        private ulong _Stateff4 = 0;

        public ulong StatusFlag4
        {
            get { return _Stateff4; }
            set
            {
                ulong OldV = StatusFlag4;
                if (value != OldV)
                {
                    _Stateff4 = value;
                    WriteUInt64(value, _StatusFlag4, SpawnPacket);
                    UpdateEffects(true);
                }
            }
        }

        public void AddFlag4(ulong flag)
        {
            StatusFlag4 |= flag;
        }

        public void RemoveFlag4(ulong flag)
        {
            if (ContainsFlag4(flag))
            {
                StatusFlag4 &= ~flag;
            }
        }

        public bool ContainsFlag4(ulong flag)
        {
            ulong aux = StatusFlag4;
            aux &= ~flag;
            return !(aux == StatusFlag4);
        }

        public int EquippedTitle
        {
            get { return BitConverter.ToInt32(SpawnPacket, 292); }
            set
            {
                WriteInt32(value, 292, SpawnPacket);
                foreach (var player in Owner.Screen.Objects.Where(p => p.MapObjType == MapObjectType.Player))
                    if (player != null)
                        if ((player as Entity) != null)
                            (player as Entity).Owner.Send(SpawnPacket);
            }
        }

        private uint _ChampionPoints;

        public uint ChampionPoints
        {
            get
            {
                MsgGoldLeaguePoint Champ = new MsgGoldLeaguePoint();
                Champ.Points = _ChampionPoints;
                Champ.Points2 = _ChampionPoints;
                Owner.Send(Champ);
                return _ChampionPoints;
            }
            set
            {
                try
                {
                    _ChampionPoints = value;
                }
                finally
                {
                    MsgGoldLeaguePoint Champ = new MsgGoldLeaguePoint();
                    Champ.Points = _ChampionPoints;
                    Champ.Points2 = _ChampionPoints;
                    Owner.Send(Champ);
                }
            }
        }

        public uint totalperfectionscore_ = 0;

        public uint TotalPerfectionScore
        {
            get
            {
                if (Owner == null) return totalperfectionscore_;
                uint points = 0;
                points += Owner.Equipment.GetFullEquipmentEnumPoints;
                points += Owner.Equipment.GetFullEquipmentSocketPoints;
                points += Owner.Equipment.GetFullEquipmentGemPoints;
                points += Owner.Equipment.GetFullEquipmentPlusPoints;
                points += Owner.Equipment.GetFullEquipmentBlessPoints;
                points += Owner.Equipment.GetFullEquipmentRefinePoints;
                points += Owner.Equipment.GetFullEquipmentSoulPoints;
                points += Owner.Equipment.GetFullEquipmentEnchantPoints;
                points += Owner.Equipment.GetFullEquipmentPerfecetionLevelPoints;
                points += Owner.Equipment.GetFullEquipmentLevelPoints;
                points += (uint)PerfectionScore.CalculatePerfectionChiPoints(Owner);
                points += (uint)((Vitality + Spirit + Strength + Agility + Atributes) * 5);
                points += (uint)(Level < 140 ? Level * 20 : Level * 25);
                points += (uint)((uint)NobilityRank * 1000);
                points += (uint)(Reborn * 1000);
                points += (PerfectionTable.PerfectionPoints(Owner, true));
                points += (PerfectionTable.PerfectionPoints(Owner, true));
                points += (uint)(PerfectionScore.CalculateSubClassPoints(Owner));
                return points;
            }
            set { totalperfectionscore_ = value; }
        }

        public int TitlePoints
        {
            get { return WTitles != null ? WTitles.Points : BitConverter.ToInt32(SpawnPacket, 296); }
            set
            {
                if (value < WTitles.Points)
                    return;
                WriteInt32(value, 296, SpawnPacket);
                Update(89, (uint)value, false);
                foreach (var player in Owner.Screen.Objects.Where(p => p.MapObjType == MapObjectType.Player))
                    if (player != null)
                        if ((player as Entity) != null)
                            (player as Entity).Owner.Send(SpawnPacket);
                if (WTitles != null && WTitles.Points != value)
                {
                    WTitles.Points = value;
                    WTitles.Update();
                }
            }
        }

        public int EquippedWing
        {
            get { return BitConverter.ToInt32(SpawnPacket, 300); }
            set
            {
                WriteInt32(value, 300, SpawnPacket);
                foreach (var player in Owner.Screen.Objects.Where(p => p.MapObjType == MapObjectType.Player))
                    if (player != null)
                        if ((player as Entity) != null)
                            (player as Entity).Owner.Send(SpawnPacket);
            }
        }

        public uint lacb;

        public int PerfectionLevel;
        public uint FlameLotusEnergy;
        public uint AuroraLotusEnergy;
        public Time32 LotusEnergyStamp;
        public Time32 lianhuaranStamp;
        public Time32 ConquerPointsStamp;
        public float lianhuaranPercent;
        public int lianhuaranLeft;

        public bool Lotus(uint LotusEnergy, uint aura = Network.GamePackets.Update.AuroraLotus)
        {
            if (Owner.Weapons != null)
            {
                if (Owner.Weapons.Item1 != null)
                {
                    if (Owner.Weapons.Item1.ID / 1000 != 620)
                        return false;
                    Network.GamePackets.Update upgrade = new Network.GamePackets.Update(true);
                    upgrade.UID = UID;
                    upgrade.AppendFull(Network.GamePackets.Update.StatusFlag, StatusFlag, StatusFlag2, StatusFlag3,
                        StatusFlag4);
                    upgrade.Append(Network.GamePackets.Update.Lotus, aura, 5, LotusEnergy, 0);
                    Owner.SendScreen(upgrade);
                    return true;
                }
            }

            return false;
        }

        public uint SkyScore;
        public bool StartTimer { get; set; }
        public bool GuildBlackLast;
        public bool attributes9 = false;
        public bool attributes8 = false;
        public bool attributes7 = false;
        public bool attributes6 = false;
        public bool attributes5 = false;
        public bool attributes4 = false;
        public bool attributes3 = false;
        public bool attributes2 = false;
        public bool attributes1 = false;
        public bool attributes = false;
        public DateTime attributestime9;
        public DateTime attributestime8;
        public DateTime attributestime7;
        public DateTime attributestime6;
        public DateTime attributestime5;
        public DateTime attributestime4;
        public DateTime attributestime3;
        public DateTime attributestime2;
        public DateTime attributestime1;
        public DateTime attributestime;
        public bool StartedEpicQuest = false;
        public bool FinishedFirstStage = false;
        public bool FinishedSecondStage = false;
        public bool DidntPassFirstStage = false;
        public bool DidntPassSecondStage = false;
        public bool DidntPassThirdStage = false;
        public bool GotTrojanScroll = false;
        public bool GotNinjaScroll = false;
        public uint Weight;
        public byte AutoRev = 0;
        public SafeDictionary<uint, Entity> MyClones = new SafeDictionary<uint, Entity>();

        public void AddClone(string Name, ushort cloneid)
        {
            var Entity = new Entity(EntityFlag.Monster, true);
            Entity.Owner = Owner;
            Entity.MonsterInfo = new MonsterInformation();
            MonsterInformation.MonsterInformations.TryGetValue(9003, out Entity.MonsterInfo);
            Entity.MonsterInfo.Owner = Entity;
            Entity.ClanName = this.Name;
            Entity.Name = Name;
            //  Entity.GuildID = GuildID;
            // Entity.GuildRank = GuildRank;
            Entity.NobilityRank = NobilityRank;
            Entity.HairStyle = HairStyle;
            Entity.HairColor = HairColor;
            Entity.EquipmentColor = (uint)this.BattlePower;
            Entity.StatusFlag = StatusFlag;
            Entity.StatusFlag2 = StatusFlag2;
            Entity.StatusFlag3 = StatusFlag3;
            Entity.AddFlag(Network.GamePackets.Update.Flags.Invisibility);
            Entity.InvisibilityStamp = Time32.Now.AddDays(1);

            #region Equip same as me

            foreach (ConquerItem item in Owner.Equipment.Objects)
            {
                if (item == null) continue;
                if (Owner.Equipment.Free(item.Position)) continue;
                if (!item.IsWorn) continue;
                switch ((ushort)item.Position)
                {
                    case Network.GamePackets.ConquerItem.AlternateHead:
                    case Network.GamePackets.ConquerItem.Head:
                        if (Owner.HeadgearLook != 0)
                        {
                            Network.Writer.WriteUInt32(0, Game.ConquerStructures.Equipment.HeadSoul,
                                Entity.SpawnPacket);
                            Network.Writer.WriteUInt32(Owner.HeadgearLook, Game.ConquerStructures.Equipment.Head,
                                Entity.SpawnPacket);
                        }
                        else
                        {
                            if (item.Purification.Available)
                                Network.Writer.WriteUInt32(item.Purification.PurificationItemID,
                                    Game.ConquerStructures.Equipment.HeadSoul, Entity.SpawnPacket);
                            else
                                Network.Writer.WriteUInt32(0, Game.ConquerStructures.Equipment.HeadSoul,
                                    Entity.SpawnPacket);
                            Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.Head,
                                Entity.SpawnPacket);
                        }

                        Network.Writer.WriteUInt16((byte)item.Color, Game.ConquerStructures.Equipment.HeadColor,
                            Entity.SpawnPacket);
                        break;
                    case Network.GamePackets.ConquerItem.AlternateGarment:
                    case Network.GamePackets.ConquerItem.Garment:
                        Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.Garment,
                            Entity.SpawnPacket);
                        break;
                    case Network.GamePackets.ConquerItem.AlternateArmor:
                    case Network.GamePackets.ConquerItem.Armor:
                        if (Owner.ArmorLook != 0)
                        {
                            Network.Writer.WriteUInt32(0, Game.ConquerStructures.Equipment.ArmorSoul,
                                Entity.SpawnPacket);
                            Network.Writer.WriteUInt32(Owner.ArmorLook, Game.ConquerStructures.Equipment.Armor,
                                Entity.SpawnPacket);
                        }
                        else
                        {
                            if (item.Purification.Available)
                                Network.Writer.WriteUInt32(item.Purification.PurificationItemID,
                                    Game.ConquerStructures.Equipment.ArmorSoul, Entity.SpawnPacket);
                            else
                                Network.Writer.WriteUInt32(0, Game.ConquerStructures.Equipment.ArmorSoul,
                                    Entity.SpawnPacket);
                            Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.Armor,
                                Entity.SpawnPacket);
                        }

                        Network.Writer.WriteUInt16((byte)item.Color, Game.ConquerStructures.Equipment.ArmorColor,
                            Entity.SpawnPacket);
                        break;
                    case Network.GamePackets.ConquerItem.AlternateRightWeapon:
                    case Network.GamePackets.ConquerItem.RightWeapon:
                        if (item.Purification.Available)
                            Network.Writer.WriteUInt32(item.Purification.PurificationItemID,
                                Game.ConquerStructures.Equipment.RightWeaponSoul, Entity.SpawnPacket);
                        else
                            Network.Writer.WriteUInt32(0, Game.ConquerStructures.Equipment.RightWeaponSoul,
                                Entity.SpawnPacket);
                        Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.RightWeapon,
                            Entity.SpawnPacket);
                        if (Network.PacketHandler.IsTwoHand(item.ID))
                        {
                            Network.Writer.WriteUInt32(0, Game.ConquerStructures.Equipment.LeftWeaponSoul,
                                Entity.SpawnPacket);
                            Network.Writer.WriteUInt32(0, Game.ConquerStructures.Equipment.LeftWeapon,
                                Entity.SpawnPacket);
                            Network.Writer.WriteUInt16(0, Game.ConquerStructures.Equipment.LeftWeaponColor,
                                Entity.SpawnPacket);
                        }

                        break;
                    case Network.GamePackets.ConquerItem.RightWeaponAccessory:
                        Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.RightWeaponAccessory,
                            Entity.SpawnPacket);
                        break;
                    case Network.GamePackets.ConquerItem.AlternateLeftWeapon:
                    case Network.GamePackets.ConquerItem.LeftWeapon:
                        if (item.Purification.Available)
                            Network.Writer.WriteUInt32(item.Purification.PurificationItemID,
                                Game.ConquerStructures.Equipment.LeftWeaponSoul, Entity.SpawnPacket);
                        else
                            Network.Writer.WriteUInt32(0, Game.ConquerStructures.Equipment.LeftWeaponSoul,
                                Entity.SpawnPacket);

                        Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.LeftWeapon,
                            Entity.SpawnPacket);
                        Network.Writer.WriteUInt16((byte)item.Color, Game.ConquerStructures.Equipment.LeftWeaponColor,
                            Entity.SpawnPacket);
                        break;
                    case Network.GamePackets.ConquerItem.LeftWeaponAccessory:
                        Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.LeftWeaponAccessory,
                            Entity.SpawnPacket);
                        break;
                    case Network.GamePackets.ConquerItem.Steed:
                        Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.Steed, Entity.SpawnPacket);
                        Network.Writer.WriteUInt16((byte)item.Plus, Game.ConquerStructures.Equipment.SteedPlus,
                            Entity.SpawnPacket);
                        Network.Writer.WriteUInt32(item.SocketProgress, Game.ConquerStructures.Equipment.SteedColor,
                            Entity.SpawnPacket);
                        break;
                    case Network.GamePackets.ConquerItem.SteedArmor:
                        Network.Writer.WriteUInt32(item.ID, Game.ConquerStructures.Equipment.MountArmor,
                            Entity.SpawnPacket);
                        break;
                }
            }

            #endregion Equip same as me

            Entity.MinAttack = MinAttack;
            Entity.MaxAttack = Entity.MagicAttack = Math.Max(MinAttack, MaxAttack);
            Entity.Hitpoints = Entity.MaxHitpoints = Hitpoints;
            Entity.Body = Body;

            Entity.UID = 703400 + Owner.Map.CloneCounter.Next;
            Entity.CUID = Owner.Entity.UID;
            Entity.SpawnPacket[Entity._CUID - 3] = 2;
            Writer.Ushort(cloneid, Entity._CUID - 2, Entity.SpawnPacket);
            Entity.MapID = Owner.Map.ID;
            Entity.SendUpdates = true;
            Entity.X = Owner.Entity.X;
            Entity.Y = Owner.Entity.Y;
            //if (!Owner.Map.Companions.ContainsKey(Entity.UID))
            //    Owner.Map.Companions.Add(Entity.UID, Entity);

            MyClones.Add(Entity.UID, Entity);

            Entity.SendSpawn(Owner);

            WriteUInt64(StatusFlag, _StatusFlag, Entity.SpawnPacket);
            Owner.SendScreenSpawn(Entity, false);

            Network.GamePackets._String stringPacket = new Network.GamePackets._String(true);
            stringPacket.UID = Entity.UID;
            stringPacket.Type = Network.GamePackets._String.Effect;
            stringPacket.Texts.Add("replaceappear");
            Owner.SendScreen(stringPacket);
        }

        public uint CUID
        {
            get
            {
                if (SpawnPacket != null)
                    return BitConverter.ToUInt32(SpawnPacket, _CUID);
                else
                    return _uid;
            }
            set
            {
                _uid = value;
                WriteUInt32(value, _CUID, SpawnPacket);
            }
        }

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                LoweredName = value.ToLower();
                if (EntityFlag == Game.EntityFlag.Player)
                {
                    if (ClanName != "")
                    {
                        SpawnPacket = new byte[8 + _Names + _Name.Length + ClanName.Length + 10];
                        WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
                        WriteUInt16(10014, 2, SpawnPacket);
                        WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
                        WriteStringList(new List<string>() { _Name, "", "", "", "", ClanName }, _Names, SpawnPacket);
                    }
                    else
                    {
                        SpawnPacket = new byte[8 + _Names + _Name.Length + 24];
                        WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
                        WriteUInt16(10014, 2, SpawnPacket);
                        WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
                        WriteStringList(new List<string>() { _Name, "", "", "", "", "" }, _Names, SpawnPacket);
                    }
                }
                else
                {
                    if (ClanName != "")
                    {
                        SpawnPacket = new byte[8 + _Names + _Name.Length + ClanName.Length + 30];
                        WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
                        WriteUInt16(10014, 2, SpawnPacket);
                        WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
                        WriteStringList(new List<string>() { _Name, "", "", ClanName }, _Names, SpawnPacket);
                    }
                    else
                    {
                        SpawnPacket = new byte[8 + _Names + _Name.Length + 30];
                        WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
                        WriteUInt16(10014, 2, SpawnPacket);
                        WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
                        WriteStringList(new List<string>() { _Name, "", "", "" }, _Names, SpawnPacket);
                    }
                }
            }
        }

        public string ClanName
        {
            get { return clan; }
            set
            {
                clan = value;
                WriteStringList(new List<string>() { _Name, "", clan, "" }, _Names, SpawnPacket);
            }
        }

        public string ClanName1
        {
            get { return clan; }
            set
            {
                clan = value;
                if (clan.Length > 15)
                    clan = clan.Substring(0, 15);
                WriteStringList(new List<string>() { _Name, "", "", clan }, _Names, SpawnPacket);
            }
        }

        private uint vipBP;

        public uint VipBattlePower
        {
            get { return vipBP; }
            set
            {
                ExtraBattlePower -= vipBP;
                vipBP = value;
                ExtraBattlePower += value;
                int val = BattlePower + (int)value;
                Update(Network.GamePackets.Update.MentorBattlePower, (uint)Math.Min(val, value), (uint)val);
            }
        }

        public Time32 AutoHuntStamp;
        public uint CountKilling;
        public Time32 OnlineTrainningStamp;
        public uint OnlineTrainning;
        public uint HuntingExp;
        public uint WarScore;
        public uint NobiltyPoleWarScore;
        public bool Tournament_Signed2 = false;
        public bool TeamDeathMatch_Signed2 = false;
        public bool TeamDeathMatch_RedCaptain2 = false;
        public bool TeamDeathMatch_BlackCaptain2 = false;
        public bool TeamDeathMatch_WhiteCaptain2 = false;
        public bool TeamDeathMatch_BlueCaptain2 = false;
        public bool TeamDeathMatch_RedTeam2 = false;
        public bool TeamDeathMatch_BlackTeam2 = false;
        public bool TeamDeathMatch_WhiteTeam2 = false;
        public bool TeamDeathMatch_BlueTeam2 = false;
        public int TeamDeathMatch_Hits2 = 0;

        public int TeamDeathMatchTeamKey2
        {
            get
            {
                if (TeamDeathMatch_BlackTeam2) return 0;
                else if (TeamDeathMatch_BlueTeam2) return 1;
                else if (TeamDeathMatch_RedTeam2) return 2;
                return 3;
            }
        }

        public uint vote;
        public uint MonstersPoints;
        public uint DarkPoints;
        public int ChestDemonkill = 0;

        #region Attack

        public Boolean IsGreen(Entity Entity)
        {
            return (Entity.Level - Level) >= 3;
        }

        public Boolean IsWhite(Entity Entity)
        {
            return (Entity.Level - Level) >= 0 && (Entity.Level - Level) < 3;
        }

        public Boolean IsRed(Entity Entity)
        {
            return (Entity.Level - Level) >= -4 && (Entity.Level - Level) < 0;
        }

        public Boolean IsBlack(Entity Entity)
        {
            return (Entity.Level - Level) < -4;
        }

        public bool IsBowEquipped
        {
            get
            {
                if (EntityFlag == Game.EntityFlag.Player)
                {
                    var right = Owner.Equipment.TryGetItem(ConquerItem.RightWeapon);
                    if (right != null)
                    {
                        return PacketHandler.IsBow(right.ID);
                    }
                }

                return false;
                // return Equipment.Select(ItemPosition.WeaponRight, (x) => x.IsBow); 
            }
        }

        public bool IsShieldEquipped
        {
            get
            {
                if (EntityFlag == Game.EntityFlag.Player)
                {
                    var right = Owner.Equipment.TryGetItem(ConquerItem.LeftWeapon);
                    if (right != null)
                    {
                        return PacketHandler.IsShield(right.ID);
                    }
                }

                return false;
                // return Equipment.Select(ItemPosition.WeaponRight, (x) => x.IsBow); 
            }
        }

        public int Attack
        {
            get { return Strength; }
        }

        public ushort Dexterity;
        public const int DefaultDefense2 = 10000;

        public int AdjustWeaponDamage(Entity target, int damage)
        {
            return MathHelper.MulDiv((int)damage, GetDefense2(target), DefaultDefense2);
        }

        public int GetDefense2(Entity target)
        {
            if (Reborn == 0) return DefaultDefense2;

            var defense2 = (FirstRebornClass % 10) >= 3 ? 7000 : DefaultDefense2;
            if (Reborn < 2)
            {
                return defense2;
            }

            if (target.EntityFlag == Game.EntityFlag.Monster)
            {
                return DefaultDefense2;
            }

            var targetHero = target as Entity;
            if (targetHero != null)
            {
                return targetHero.Reborn < 2 ? 5000 : 7000;
            }

            return defense2;
        }

        public int AdjustMagicDamage(Entity target, int damage)
        {
            return MathHelper.MulDiv(damage, GetDefense2(target), DefaultDefense2);
        }

        public int AdjustData(int data, int adjust, int maxData = 0)
        {
            return MathHelper.AdjustDataEx(data, adjust, maxData);
        }

        public int AdjustAttack(int attack)
        {
            //  var addAttack = 0;
            if (OnIntensify())
                //attack = (int)((double)attack * IntensifyPercent); //PvP Reduction!
                attack = (int)((double)attack * 1.5); //PvP Reduction!
            else if (ContainsFlag(Network.GamePackets.Update.Flags.Stigma))
                attack += (int)((double)attack * 0.3);
            //{
            //    addAttack += Math.Max(0, AdjustData((int)attack, 30)) - (int)attack;
            //    attack = (attack + (addAttack * attack / 100));
            //}             
            if (OnSuperman())
                attack += (int)((double)attack * 0); //PvP Reduction!
            if (OnSuperCyclone())
                attack += (int)((double)attack * 0); //PvP Reduction!

            if (OnOblivion())
                attack += (int)((double)attack * 0); //PvP Reduction!
            if (OnFatalStrike())
                attack += (int)((double)attack * 0); //PvP Reduction!

            //if (TryGetStatus(StatusType.KeepBow, out status))
            //{
            //    addAttack += Math.Max(0, AdjustData(attack, status.Power)) - attack;
            //}

            return attack;
        }

        public int AdjustDefense(int defense)
        {
            // var addDefense = 0;


            if (ContainsFlag(Network.GamePackets.Update.Flags.MagicShield))
                defense += (int)((double)defense * 0.3); //PvP Reduction!

            return defense;
        }

        public int AdjustBowDefense(int defense)
        {
            return defense;
        }

        public int AdjustHitrate(int hitrate)
        {
            var addHitrate = 0;


            if (ContainsFlag(Network.GamePackets.Update.Flags.StarOfAccuracy))
            {
                addHitrate += Math.Max(0, AdjustData(hitrate, 30)) - hitrate;
            }

            return hitrate + addHitrate;
        }

        public uint ArmorId
        {
            get
            {
                if (EntityFlag == Game.EntityFlag.Player)
                {
                    var item = Owner.Equipment.TryGetItem(ConquerItem.Armor);
                    if (item != null)
                        return item.ID;
                }

                return 0;
            }
        }

        public int ReduceDamage
        {
            get { return (int)ItemBless; }
        }

        #endregion

        public void AzureShieldPacket()
        {
            var Remain = AzureShieldStamp.AddSeconds(MagicShieldTime) - DateTime.Now;
            Network.GamePackets.Update aupgrade = new Network.GamePackets.Update(true);
            aupgrade.UID = UID;
            aupgrade.Append(49
                , 93
                , (uint)Remain.TotalSeconds, AzureShieldDefence, AzureShieldLevel);
            Owner.Send(aupgrade);
        }

        public ushort CalculateHitPoint()
        {
            ushort valor = 0;
            switch (Class)
            {
                case 11:
                    valor += (ushort)(Agility * 3.15 + Spirit * 3.15 + Strength * 3.15 + Vitality * 25.2);
                    break;
                case 12:
                    valor += (ushort)(Agility * 3.24 + Spirit * 3.24 + Strength * 3.24 + Vitality * 25.9);
                    break;
                case 13:
                    valor += (ushort)(Agility * 3.30 + Spirit * 3.30 + Strength * 3.30 + Vitality * 26.4);
                    break;
                case 14:
                    valor += (ushort)(Agility * 3.36 + Spirit * 3.36 + Strength * 3.36 + Vitality * 26.8);
                    break;
                case 15:
                    valor += (ushort)(Agility * 3.45 + Spirit * 3.45 + Strength * 3.45 + Vitality * 27.6);
                    break;
                default:
                    valor += (ushort)(Agility * 3 + Spirit * 3 + Strength * 3 + Vitality * 24);
                    break;
            }

            return valor;
        }

        public DateTime RadiantStamp;

        public bool OnAzureShield
        {
            get { return ContainsFlag2(Network.GamePackets.Update.Flags2.AzureShield); }
        }

        public ushort AzureShieldTime = 60;
        public DateTime timerInportChampion = new DateTime();
        public ushort AzureShieldDefence = 0;
        public byte AzureShieldLevel = 0;
        public DateTime AzureShieldStamp = DateTime.Now;
        public List<ushort> MonstersSpells = null;
        public bool AllowToAttack = false;
        public ushort EquipAgility = 0;
        public uint Accurity = 0;
        public uint MAttack;
        public uint MDefense;
        public uint OnMoveNpc = 0;

        private ulong _NobalityDonation;

        public ulong NobalityDonation
        {
            get { return _NobalityDonation; }
            set
            {
                if (value <= 0)
                    value = 0;

                _NobalityDonation = value;
                //    new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE)
                //        .Update("entities").Set("Donation", value).Where("UID", Entity.UID).Execute();
            }
        }

        public Game.Features.Flowers MyFlowers;

        private uint Flower;

        public uint FlowerRank
        {
            get { return this.Flower; }
            set
            {
                this.Flower = value;
                Writer.WriteUInt32(value + 10000, 154, this.SpawnPacket); //146
            }
        }

        #region skill team

        public Features.Tournaments.TeamElitePk.Match SkillTeamWatchingElitePKMatch;
        public bool InSkillPk = false;
        public bool WasSend = false;

        #endregion

        #region offsets

        public static int
            _Mesh = 8,
            _UID = 12,
            _GuildID = 16,
            _GuildRank = 20,
            _StatusFlag = 26,
            _StatusFlag2 = 34,
            _StatusFlag3 = 42,
            _StatusFlag4 = 50,
            _AppearanceType = 50 + 4,
            _Hitpoints = 103 + 4 + 4,
            _MonsterLevel = 109 + 4 + 4,
            _X = 111 + 4 + 4,
            _Y = 113 + 4 + 4,
            _HairStyle = 115 + 4 + 4,
            _Facing = 117 + 4 + 4,
            _Action = 118 + 4 + 4,
            _Reborn = 125 + 4 + 4,
            _Level = 130 + 4,
            _WindowSpawn = 128 + 4 + 4,
            _Away = 129 + 4 + 4,
            _ExtraBattlepower = 130 + 4 + 4,
            _FlowerIcon = 146 + 4 + 4,
            _NobilityRank = 150 + 4 + 4,
            _QuizPoints = 160 + 4 + 4,
            _ClanUID = 186 + 4 + 4,
            _ClanRank = 190 + 4 + 4,
            _Title = 198 + 4 + 4,
            _ShowArenaGlow = 209 + 4 + 4,
            _Boss = 212 + 4 + 4,
            _RaceItem = 214 + 4 + 4,
            _SubClass1 = 220 + 4 + 4,
            _SubClassesActive1 = 226 + 4 + 4,
            _ActiveSubclass = 229 + 4 + 4,
            _FirstRebornClass = 246,
            _SecondRebornClass = 248,
            _Class = 246 + 4,
            _AssassinColor = 241 + 9 + 4,
            _CountryCode = 244 + 4 + 4,
            _BattlePower = 250 + 4 + 4,
            _skillsoul = 253 + 4 + 4,
            _skillsoul2 = 256 + 4 + 4,
            _CUID = 266 + 4 + 4,
            _NameClan = 296 + 4,
            _WingColor = 278 + 4,
            _EquipmentColor = 258,
            _EpicColor = 254 + 4,
            _Names = 317;

        #endregion

        public MaTrix.Pet.PetType pettype;

        #region TopDonation

        public int _TopBlackname = 0, _TopRedname = 0, _TopWhitename = 0;

        public int TopBlackname
        {
            get { return _TopBlackname; }
            set
            {
                _TopBlackname = value;
                if (value >= 1)
                {
                    AddFlag2(Network.GamePackets.Update.Flags2.Top8Archer);
                    //MTA.Database.EntityTable.SaveTopDonation(this.Owner);
                }
            }
        }

        public int TopWhitename //Top3Monk
        {
            get { return _TopWhitename; }
            set
            {
                _TopWhitename = value;
                if (value >= 1)
                {
                    AddFlag2(Network.GamePackets.Update.Flags2.Top8Ninja);
                    //MTA.Database.EntityTable.SaveTopDonation(this.Owner);
                }
            }
        }

        public int TopRedname //Top8Fire
        {
            get { return _TopRedname; }
            set
            {
                _TopRedname = value;
                if (value >= 1)
                {
                    AddFlag2(Network.GamePackets.Update.Flags2.Top8Fire);
                    //MTA.Database.EntityTable.SaveTopDonation(this.Owner);
                }
            }
        }

        #endregion

        public void InsertXorNameToDB(string oldName, string newName)
        {
            new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("entities").Set(oldName, newName)
                .Where("UID", UID).Execute();
        }

        #region Variables

        public uint OnlinePoints;
        public Time32 OnlinePointStamp;
        // public MTA.Game.Features.Flowers.Flowers Flowers;

        public int KillCount = 0, KillCount2 = 0;
        public uint LastXLocation, LastYLocation;
        public bool InSteedRace, Invisable, IsBlackSpotted, IsEagleEyeShooted = false;
        public Database.MonsterInformation MonsterInfo;

        public Time32 DeathStamp,
            VortexAttackStamp,
            AttackStamp,
            StaminaStamp,
            FlashingNameStamp,
            CycloneStamp,
            SupermanStamp,
            TwoFlod,
            FatigueStamp,
            CannonBarrageStamp,
            StigmaStamp,
            InvisibilityStamp,
            StarOfAccuracyStamp,
            MagicShieldStamp,
            DodgeStamp,
            EnlightmentStamp,
            BlackSpotStamp,
            BlackbeardsRageStamp,
            DefensiveStanceStamp,
            AccuracyStamp,
            ShieldStamp,
            FlyStamp,
            NoDrugsStamp,
            ToxicFogStamp,
            FatalStrikeStamp,
            DoubleExpStamp,
            DoubleExpStamp5,
            BladeTempest,
            MagicDefenderStamp,
            ShurikenVortexStamp,
            IntensifyStamp,
            TransformationStamp,
            CounterKillStamp,
            PKPointDecreaseStamp,
            LastPopUPCheck,
            HeavenBlessingStamp,
            OblivionStamp,
            DragonSwingStamp,
            AuraStamp,
            ShackleStamp,
            AzureStamp,
            StunStamp,
            WhilrwindKick,
            GuildRequest,
            Confuse,
            LastTeamLeaderLocationSent = Time32.Now,
            BladeFlurryStamp,
            ShieldBlockStamp,
            SuperCycloneStamp;

        public bool IsDropped = false;
        public bool IsWatching = false;
        public bool HasMagicDefender = false;
        public bool IsDefensiveStance = false;
        public bool MagicDefenderOwner = false;
        public bool KillTheTerrorist_IsTerrorist = false;
        public bool Tournament_Signed = false;
        public bool SpawnProtection = false;
        public bool TeamDeathMatch_Signed = false;
        public bool TeamDeathMatch_RedCaptain = false;
        public bool TeamDeathMatch_BlackCaptain = false;
        public bool TeamDeathMatch_WhiteCaptain = false;
        public bool TeamDeathMatch_BlueCaptain = false;
        public bool TeamDeathMatch_RedTeam = false;
        public bool TeamDeathMatch_BlackTeam = false;
        public bool TeamDeathMatch_WhiteTeam = false;

        public int TeamDeathMatchTeamKey
        {
            get
            {
                if (TeamDeathMatch_BlackTeam) return 0;
                else if (TeamDeathMatch_BlueTeam) return 1;
                else if (TeamDeathMatch_RedTeam) return 2;
                return 3;
            }
        }

        public Achievement MyAchievement;
        public uint InteractionType = 0;
        public uint InteractionWith = 0;
        public bool InteractionInProgress = false;
        public ushort InteractionX = 0;
        public ushort InteractionY = 0;
        public bool InteractionSet = false;
        public int CurrentTreasureBoxes = 0;
        public static int Leadrinmap;

        public uint Points = 0;

        //public uint UID = 0;
        //public ushort Avatar = 0;
        //public ushort Mesh = 0;
        //public string Name = "";
        public ushort Postion = 0;

        public ConcurrentDictionary<TitlePacket.Titles, DateTime> Titles =
            new ConcurrentDictionary<TitlePacket.Titles, DateTime>();

        public struct Halo
        {
            public ulong Flag;
            public byte FlagType;
            public DateTime Time;
        }

        public ConcurrentDictionary<int, DateTime> Halos;

        public bool IsWarTop(ulong Title)
        {
            return Title >= 11 && Title <= 20 || Title >= 23 && Title <= 200;
        }

        public void AddTopStatus(UInt64 Title, byte flagtype, DateTime EndsOn, Boolean Db = true)
        {
            Boolean HasFlag = false;
            if (IsWarTop(Title))
            {
                HasFlag = Titles.ContainsKey((TitlePacket.Titles)Title);
                Titles.TryAdd((TitlePacket.Titles)Title, EndsOn);
            }
            else
            {
                switch (flagtype)
                {
                    case 1:
                        HasFlag = ContainsFlag(Title);
                        AddFlag(Title);
                        break;
                    case 2:
                        HasFlag = ContainsFlag2(Title);
                        AddFlag2(Title);
                        break;
                    case 3:
                        HasFlag = ContainsFlag3(Title);
                        AddFlag3(Title);
                        break;
                }
            }

            if (Db)
            {
                if (HasFlag)
                {
                    MySqlCommand cmd = new MySqlCommand(MySqlCommandType.UPDATE);
                    cmd.Update("status").Set("time", Kernel.ToDateTimeInt(EndsOn))
                        .Where("status", Title).And("flagtype", flagtype).And("entityid", (UInt32)UID);
                    cmd.Execute();
                }
                else
                {
                    MySqlCommand cmd = new MySqlCommand(MySqlCommandType.INSERT);
                    cmd.Insert("status").Insert("entityid", (UInt32)UID).Insert("status", Title)
                        .Insert("flagtype", flagtype).Insert("time", Kernel.ToDateTimeInt(EndsOn));
                    cmd.Execute();
                }
            }
        }

        public void RemoveTopStatus(UInt64 Title, byte flagtype = 0)
        {
            ulong baseFlag = Title; //TopStatusToInt(Title);
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.DELETE);
            cmd.Delete("status", "entityid", UID).And("status", baseFlag).And("flagtype", flagtype).Execute();


            switch (flagtype)
            {
                case 0:
                {
                    var title = (TitlePacket.Titles)baseFlag;
                    if (Titles.ContainsKey(title))
                    {
                        Titles.Remove(title);
                        if (MyTitle == title)
                            MyTitle = Network.GamePackets.TitlePacket.Titles.None;

                        Owner.SendScreenSpawn(this, true);
                    }

                    break;
                }
                case 1:
                    RemoveFlag(baseFlag);
                    break;
                case 2:
                    RemoveFlag2(baseFlag);
                    break;
                case 3:
                    RemoveFlag3(baseFlag);
                    break;
            }
        }

        public void LoadTopStatus()
        {
            using (MySqlCommand Command = new MySqlCommand(MySqlCommandType.SELECT))
            {
                Command.Select("status").Where("entityid", UID).Execute();
                using (MySqlReader Reader = new MySqlReader(Command))
                {
                    while (Reader.Read())
                    {
                        UInt64 Title = Reader.ReadUInt64("status");
                        byte flagtype = Reader.ReadByte("flagtype");
                        DateTime Time = Kernel.FromDateTimeInt(Reader.ReadUInt64("time"));
                        if (DateTime.Now > Time)
                            RemoveTopStatus(Title, flagtype);
                        else
                        {
                            //if (!ContainsFlag(IntToTopStatus(Title)))
                            AddTopStatus(Title, flagtype, Time, false);
                            //else DeleteStatus(Title);
                        }
                    }
                }
            }
        }
        //public UInt64 TopStatusToInt(UInt64 top)
        //{
        //    switch (top)
        //    {
        //        case Network.GamePackets.Update.Flags.TopWaterTaoist: return 1;
        //        case Network.GamePackets.Update.Flags.TopWarrior: return 2;
        //        case Network.GamePackets.Update.Flags.TopTrojan: return 3;
        //        case Network.GamePackets.Update.Flags.TopArcher: return 4;
        //        case Network.GamePackets.Update.Flags.TopNinja: return 5;
        //        case Network.GamePackets.Update.Flags.TopFireTaoist: return 6;
        //        case Network.GamePackets.Update.Flags2.TopMonk: return 7;
        //        case Network.GamePackets.Update.Flags.TopSpouse: return 8;
        //        case Network.GamePackets.Update.Flags.TopGuildLeader: return 9;
        //        case Network.GamePackets.Update.Flags.TopDeputyLeader: return 10;
        //        case Network.GamePackets.Update.Flags.MonthlyPKChampion: return 20;
        //        case Network.GamePackets.Update.Flags.WeeklyPKChampion: return 21;
        //        case Network.GamePackets.Update.Flags2.TopPirate: return 22;
        //        case Network.GamePackets.Update.Flags2.Top2Trojan: return 55;
        //        case Network.GamePackets.Update.Flags2.Top2Warrior: return 56;
        //        case Network.GamePackets.Update.Flags2.Top2Ninja: return 57;
        //        case Network.GamePackets.Update.Flags2.Top2Monk: return 58;
        //        case Network.GamePackets.Update.Flags2.Toppirate2: return 59;
        //        case Network.GamePackets.Update.Flags2.Top3Trojan: return 60;
        //        case Network.GamePackets.Update.Flags2.Top3Warrior: return 61;
        //        case Network.GamePackets.Update.Flags2.Top3Ninja: return 62;
        //        case Network.GamePackets.Update.Flags2.Top3Monk: return 63;
        //        case Network.GamePackets.Update.Flags2.Toppirate3: return 64;
        //        case Network.GamePackets.Update.Flags2.Top2Archer: return 65;

        //        //case Network.GamePackets.Update.Flags2.Top3Archer: return 66;
        //        case Network.GamePackets.Update.Flags2.Top8Trojan: return 67;
        //        case Network.GamePackets.Update.Flags2.Top8Archer: return 68;
        //        case Network.GamePackets.Update.Flags2.Top8Warrior: return 69;
        //        //case Network.GamePackets.Update.Flags2.Top8Monk: return 70;
        //        case Network.GamePackets.Update.Flags2.Top8Ninja: return 71;
        //        case Network.GamePackets.Update.Flags2.Toppirate8: return 72;
        //        case Network.GamePackets.Update.Flags2.Top3Water: return 73;
        //        case Network.GamePackets.Update.Flags2.Top8Water: return 74;
        //        case Network.GamePackets.Update.Flags2.Top3Fire: return 75;
        //        case Network.GamePackets.Update.Flags2.Top8Fire: return 76;


        //        case Network.GamePackets.Update.Flags3.DragonWarriorTop: return 260;
        //        case Network.GamePackets.Update.Flags3.ConuqerSuperUnderBlue: return 230;
        //        case Network.GamePackets.Update.Flags3.ConuqerSuperBlue: return 240;
        //        case Network.GamePackets.Update.Flags3.ConuqerSuperYellow: return 250;
        //    }
        //    return top;
        //}
        //public UInt64 IntToTopStatus(UInt64 top)
        //{
        //    switch (top)
        //    {
        //        case 1: return Network.GamePackets.Update.Flags.TopWaterTaoist;
        //        case 2: return Network.GamePackets.Update.Flags.TopWarrior;
        //        case 3: return Network.GamePackets.Update.Flags.TopTrojan;
        //        case 4: return Network.GamePackets.Update.Flags.TopArcher;
        //        case 5: return Network.GamePackets.Update.Flags.TopNinja;
        //        case 6: return Network.GamePackets.Update.Flags.TopFireTaoist;
        //        case 7: return Network.GamePackets.Update.Flags2.TopMonk;
        //        case 8: return Network.GamePackets.Update.Flags.TopSpouse;
        //        case 9: return Network.GamePackets.Update.Flags.TopGuildLeader;
        //        case 10: return Network.GamePackets.Update.Flags.TopDeputyLeader;
        //        case 20: return Network.GamePackets.Update.Flags.MonthlyPKChampion;
        //        case 21: return Network.GamePackets.Update.Flags.WeeklyPKChampion;
        //        case 22: return Network.GamePackets.Update.Flags2.TopPirate;
        //        case 55: return Network.GamePackets.Update.Flags2.Top2Trojan;
        //        case 56: return Network.GamePackets.Update.Flags2.Top2Warrior;
        //        case 57: return Network.GamePackets.Update.Flags2.Top2Ninja;
        //        case 58: return Network.GamePackets.Update.Flags2.Top2Monk;
        //        case 59: return Network.GamePackets.Update.Flags2.Toppirate2;
        //        case 60: return Network.GamePackets.Update.Flags2.Top3Trojan;
        //        case 61: return Network.GamePackets.Update.Flags2.Top3Warrior;
        //        case 62: return Network.GamePackets.Update.Flags2.Top3Ninja;
        //        case 63: return Network.GamePackets.Update.Flags2.Top3Monk;
        //        case 64: return Network.GamePackets.Update.Flags2.Toppirate3;
        //        case 65: return Network.GamePackets.Update.Flags2.Top2Archer;
        //        //case 66: return Network.GamePackets.Update.Flags2.Top3Archer;

        //        case 67: return Network.GamePackets.Update.Flags2.Top8Trojan;
        //        case 68: return Network.GamePackets.Update.Flags2.Top8Archer;
        //        case 69: return Network.GamePackets.Update.Flags2.Top8Warrior;
        //        //case 70: return Network.GamePackets.Update.Flags2.Top8Monk;
        //        case 71: return Network.GamePackets.Update.Flags2.Top8Ninja;
        //        case 72: return Network.GamePackets.Update.Flags2.Toppirate8;

        //        case 73: return Network.GamePackets.Update.Flags2.Top3Water;
        //        case 74: return Network.GamePackets.Update.Flags2.Top8Water;
        //        case 75: return Network.GamePackets.Update.Flags2.Top3Fire;
        //        case 76: return Network.GamePackets.Update.Flags2.Top8Fire;

        //        case 260: return Network.GamePackets.Update.Flags3.DragonWarriorTop;
        //        case 230: return Network.GamePackets.Update.Flags3.ConuqerSuperUnderBlue;
        //        case 240: return Network.GamePackets.Update.Flags3.ConuqerSuperBlue;
        //        case 250: return Network.GamePackets.Update.Flags3.ConuqerSuperYellow;
        //    }
        //    return top;
        //}

        public UInt32 ActivePOPUP;

        public TitlePacket.Titles MyTitle
        {
            get { return (TitlePacket.Titles)SpawnPacket[_Title]; }
            set
            {
                SpawnPacket[_Title] = (Byte)value;
                if (FullyLoaded)
                {
                    MySqlCommand cmd = new MySqlCommand(MySqlCommandType.UPDATE);
                    cmd.Update("entities").Set("My_Title", (Byte)value).Where("uid", UID).Execute();
                }
            }
        }

        public Updating.Offset1 UpdateOffset1 = Updating.Offset1.None;
        public Updating.Offset2 UpdateOffset2 = Updating.Offset2.None;
        public Updating.Offset3 UpdateOffset3 = Updating.Offset3.None;
        public Updating.Offset4 UpdateOffset4 = Updating.Offset4.None;
        public Updating.Offset5 UpdateOffset5 = Updating.Offset5.None;
        public Updating.Offset6 UpdateOffset6 = Updating.Offset6.None;
        public Updating.Offset7 UpdateOffset7 = Updating.Offset7.None;
        public int DisKO = 0;
        public static byte ScreenDistance = 0;
        public bool DisQuest = false;
        public static bool dis = false;
        public static bool dis2 = false;
        public static byte DisMax1 = 0;
        public static byte DisMax2 = 0;
        public static byte DisMax3 = 0;
        public Client.GameState ClientStats;
        public StatusStatics Statistics = new StatusStatics();
        public double DragonGems;
        public double PhoenixGems;
        public int BlackSpotStepSecs;
        public ushort Detoxication;
        public int Immunity;
        public int FatigueSecs;
        public int Breaktrough;
        public int CriticalStrike;
        public int SkillCStrike;
        public ushort Intensification;
        public int Block;
        public ushort FinalMagicDmgPlus;
        public ushort FinalMagicDmgReduct;
        public ushort FinalDmgPlus;
        public ushort FinalDmgReduct;
        public int Penetration;
        public int Counteraction;
        public int MetalResistance;
        public int WoodResistance;
        public int WaterResistance;
        public int FireResistance;
        public int EarthResistance;
        public bool TeamDeathMatch_BlueTeam = false;
        public int TeamDeathMatch_Hits = 0;
        public byte _SubClass, _SubClassLevel;
        public Game.Subclasses SubClasses = new Game.Subclasses();
        public bool Stunned = false, Confused = false;

        public bool Companion;

        //public bool BodyGuard;
        public bool CauseOfDeathIsMagic = false;

        private DateTime mLastLogin;
        uint f_flower;

        public uint ActualMyTypeFlower
        {
            get { return f_flower; }
            set
            {
                //30010202 orchids
                //30010002 rouse
                //30010102 lilyes
                //30010302 orchids

                f_flower = value;
                WriteUInt32(30010002, _FlowerIcon + 4, SpawnPacket); //91
            }
        }

        private uint flower_R;

        public uint AddFlower
        {
            get { return flower_R; }
            set { flower_R = value; }
        }

        public short KOSpellTime
        {
            get
            {
                if (KOSpell == 1110)
                {
                    if (ContainsFlag(Network.GamePackets.Update.Flags.Cyclone))
                    {
                        return CycloneTime;
                    }
                }
                else if (KOSpell == 1025)
                {
                    if (ContainsFlag(Network.GamePackets.Update.Flags.Superman))
                    {
                        return SupermanTime;
                    }
                }

                return 0;
            }
            set
            {
                if (KOSpell == 1110)
                {
                    if (ContainsFlag(Network.GamePackets.Update.Flags.Cyclone))
                    {
                        int Seconds = CycloneStamp.AddSeconds(value).AllSeconds() - Time32.Now.AllSeconds();
                        if (Seconds >= 20)
                        {
                            CycloneTime = 20;
                            CycloneStamp = Time32.Now;
                        }
                        else
                        {
                            CycloneTime = (short)Seconds;
                            CycloneStamp = Time32.Now;
                        }
                    }
                }

                if (KOSpell == 1025)
                {
                    if (ContainsFlag(Network.GamePackets.Update.Flags.Superman))
                    {
                        int Seconds = SupermanStamp.AddSeconds(value).AllSeconds() - Time32.Now.AllSeconds();
                        if (Seconds >= 20)
                        {
                            SupermanTime = 20;
                            SupermanStamp = Time32.Now;
                        }
                        else
                        {
                            SupermanTime = (short)Seconds;
                            SupermanStamp = Time32.Now;
                        }
                    }
                }
            }
        }

        public short CycloneTime = 0,
            SupermanTime = 0,
            NoDrugsTime = 0,
            FatalStrikeTime = 0,
            ShurikenVortexTime = 0,
            OblivionTime = 0,
            AuraTime = 0,
            ShackleTime = 0,
            AzureTime;

        public ushort KOSpell = 0;
        public int AzureDamage = 0;
        private ushort _enlightenPoints;
        private byte _receivedEnlighenPoints;
        private ushort _enlightmenttime;
        public float ToxicFogPercent, StigmaIncrease, MagicShieldIncrease, DodgeIncrease, ShieldIncrease;

        public byte ToxicFogLeft,
            FlashingNameTime,
            FlyTime,
            StigmaTime,
            InvisibilityTime,
            StarOfAccuracyTime,
            MagicShieldTime,
            DodgeTime,
            AccuracyTime,
            ShieldTime,
            MagicDefenderSecs;

        public ushort KOCount = 0;
        public bool CounterKillSwitch = false;
        public Network.GamePackets.Attack AttackPacket;
        public Network.GamePackets.Attack VortexPacket;
        public byte[] SpawnPacket;
        private string _Name, _Spouse;
        private ushort _MDefence, _MDefencePercent;
        public ushort BaseDefence;
        private Client.GameState _Owner;
        public uint ItemHP = 0;

        public ushort ItemMP = 0,
            PhysicalDamageDecrease = 0,
            PhysicalDamageIncrease = 0,
            MagicDamageDecrease = 0,
            MagicDamageIncrease = 0,
            AttackRange = 1,
            Vigor = 0,
            ExtraVigor = 0;

        public double ItemBless = 1.0;

        // public double[] Gems = new double[10];
        public int[] Gems = new int[GemTypes.Last];

        public int Accuracy;
        private uint _MinAttack, _MaxAttack, _MagicAttack;
        public uint BaseMinAttack, BaseMaxAttack, BaseMagicAttack, BaseMagicDefence;
        private uint _TransMinAttack, _TransMaxAttack, _TransDodge, _TransPhysicalDefence, _TransMagicDefence;
        public bool Killed = false;

        public bool Transformed
        {
            get { return TransformationID != 98 && TransformationID != 99 && TransformationID != 0; }
        }

        public uint TransformationAttackRange = 0;
        public int TransformationTime = 0;
        public uint TransformationMaxHP = 0;
        private byte _Dodge;
        private Enums.PkMode _PKMode;
        private EntityFlag _EntityFlag;
        private MapObjectType _MapObjectType;
        public Enums.Mode Mode;
        private ulong _experience;
        private ulong _quizpoints;
        private uint _heavenblessing, _uid, _hitpoints, _maxhitpoints;
        private ulong _money;
        private uint _conquerpoints, _status, _status2, _status3, _status4, _TreasuerPoints;

        private ushort _doubleexp,
            _body,
            _transformationid,
            _face,
            _strength,
            _agility,
            _spirit,
            _vitality,
            _atributes,
            _mana,
            _maxmana,
            _hairstyle,
            _mapid,
            _previousmapid,
            _x,
            _y,
            _pkpoints;

        private byte _stamina, _class, _reborn, _level;
        byte cls, secls;

        public byte FirstRebornClass
        {
            get { return cls; }
            set
            {
                cls = value;
                SpawnPacket[_FirstRebornClass] = value;
                Update(Network.GamePackets.Update.FirsRebornClass, value, false);
            }
        }

        public byte SecondRebornClass
        {
            get { return secls; }
            set
            {
                secls = value;
                SpawnPacket[_SecondRebornClass] = value;
                Update(Network.GamePackets.Update.SecondRebornClass, value, false);
            }
        }

        public byte FirstRebornLevel, SecondRebornLevel;
        public bool FullyLoaded = false, SendUpdates = false, HandleTiming = false;
        private Network.GamePackets.Update update;

        #endregion

        public Time32 Cursed;

        #region MaTrix

        private uint _BoundCps;

        public uint BoundCps
        {
            get { return _BoundCps; }
            set
            {
                value = (uint)Math.Max(0, (int)value);
                _BoundCps = value;
                Database.EntityTable.UpdatebCps(this.Owner);
                if (EntityFlag == EntityFlag.Player)
                {
                    Update(Network.GamePackets.Update.BoundConquerPoints, (uint)value, false);
                }
            }
        }

        public DateTime LastBossAttack;
        public DateTime AturdidoTimeStamp;
        public int CongeladoTime = 0;
        public DateTime CongeladoTimeStamp;
        public Time32 FreezeStamp;
        public uint LotteryItemID = 0;
        public byte LotteryJadeAdd;
        public uint LotteryItemPlus;
        public uint LotteryItemSocket1;
        public uint LotteryItemSoc2;
        public uint LotteryItemColor;
        public ConquerItem LotteryPrize;
        public byte FreezeTime;

        public void SendSysMesage(string p)
        {
            Send(new Message(p, System.Drawing.Color.Purple, Message.TopLeft));
        }

        public void Send(IPacket buffer)
        {
            Send(buffer.ToArray());
        }

        public void Send(Byte[] Buffer)
        {
            Owner.Send(Buffer);
        }

        uint _ClanSharedBp;

        public uint ClanSharedBp
        {
            get { return _ClanSharedBp; }
            set
            {
                switch (EntityFlag)
                {
                    case EntityFlag.Player:
                        if (FullyLoaded)
                        {
                            Update(Network.GamePackets.Update.ClanShareBp, value, false);
                            //WriteUInt32(value, 38, SpawnPacket);//91
                            // WriteUInt32(value, 56, SpawnPacket);//91
                        }

                        break;
                }

                _ClanSharedBp = value;
            }
        }

        private ulong _autohuntxp;

        public ulong autohuntxp
        {
            get { return _autohuntxp; }
            set { _autohuntxp = value; }
        }

        public bool Auto = false;
        public Dictionary<uint, Game.PkExpeliate> PkExplorerValues = new Dictionary<uint, PkExpeliate>();

        public Enums.Maps Mapa { get; set; }

        //public LotteryTable.LotteryItem LOTOITEM;
        public LotteryTable.LotteryItem LOTOITEM;
        public byte AddJade = 0;
        public uint StrResID;
        public Time32 CpsPointStamp;
        public uint VirtuePoints;
        public Time32 WaitingTimeFB;
        public Time32 WinnerWaiting;
        public bool aWinner = false;

        #endregion

        public bool RebornCheck(byte id)
        {
            if (EntityFlag == Game.EntityFlag.Player)
            {
                if ((FirstRebornClass / 10 != id || SecondRebornClass / 10 != id || Class / 10 != id))
                    return true;
            }

            return false;
        }

        public Action<Entity> OnDeath;

        #region Acessors

        #region Fan/Tower Acessor

        public int getFan(bool Magic)
        {
            if (Owner.Equipment.Free(10))
                return 0;

            ushort magic = 0;
            ushort physical = 0;
            ushort gemVal = 0;

            #region Get

            ConquerItem Item = this.Owner.Equipment.TryGetItem(10);

            if (Item != null)
            {
                if (Item.ID > 0)
                {
                    switch (Item.ID % 10)
                    {
                        case 3:
                        case 4:
                        case 5:
                            physical += 300;
                            magic += 150;
                            break;
                        case 6:
                            physical += 500;
                            magic += 200;
                            break;
                        case 7:
                            physical += 700;
                            magic += 300;
                            break;
                        case 8:
                            physical += 900;
                            magic += 450;
                            break;
                        case 9:
                            physical += 1200;
                            magic += 750;
                            break;
                    }

                    switch (Item.Plus)
                    {
                        case 0: break;
                        case 1:
                            physical += 200;
                            magic += 100;
                            break;
                        case 2:
                            physical += 400;
                            magic += 200;
                            break;
                        case 3:
                            physical += 600;
                            magic += 300;
                            break;
                        case 4:
                            physical += 800;
                            magic += 400;
                            break;
                        case 5:
                            physical += 1000;
                            magic += 500;
                            break;
                        case 6:
                            physical += 1200;
                            magic += 600;
                            break;
                        case 7:
                            physical += 1300;
                            magic += 700;
                            break;
                        case 8:
                            physical += 1400;
                            magic += 800;
                            break;
                        case 9:
                            physical += 1500;
                            magic += 900;
                            break;
                        case 10:
                            physical += 1600;
                            magic += 950;
                            break;
                        case 11:
                            physical += 1700;
                            magic += 1000;
                            break;
                        case 12:
                            physical += 1800;
                            magic += 1050;
                            break;
                    }

                    switch (Item.SocketOne)
                    {
                        case Enums.Gem.NormalThunderGem: gemVal += 100; break;
                        case Enums.Gem.RefinedThunderGem: gemVal += 300; break;
                        case Enums.Gem.SuperThunderGem: gemVal += 500; break;
                    }

                    switch (Item.SocketTwo)
                    {
                        case Enums.Gem.NormalThunderGem: gemVal += 100; break;
                        case Enums.Gem.RefinedThunderGem: gemVal += 300; break;
                        case Enums.Gem.SuperThunderGem: gemVal += 500; break;
                    }
                }
            }

            #endregion

            physical = Math.Max(physical, PhysicalDamageIncrease);
            magic = Math.Max(magic, MagicDamageIncrease);
            magic += gemVal;
            physical += gemVal;

            if (Magic)
                return (int)magic;
            else
                return (int)physical;
        }

        public int getTower(bool Magic)
        {
            if (Owner.Equipment.Free(11))
                return 0;

            ushort magic = 0;
            ushort physical = 0;
            ushort gemVal = 0;

            #region Get

            ConquerItem Item = this.Owner.Equipment.TryGetItem(11);

            if (Item != null)
            {
                if (Item.ID > 0)
                {
                    switch (Item.ID % 10)
                    {
                        case 3:
                        case 4:
                        case 5:
                            physical += 250;
                            magic += 100;
                            break;
                        case 6:
                            physical += 400;
                            magic += 150;
                            break;
                        case 7:
                            physical += 550;
                            magic += 200;
                            break;
                        case 8:
                            physical += 700;
                            magic += 300;
                            break;
                        case 9:
                            physical += 1100;
                            magic += 600;
                            break;
                    }

                    switch (Item.Plus)
                    {
                        case 0: break;
                        case 1:
                            physical += 150;
                            magic += 50;
                            break;
                        case 2:
                            physical += 350;
                            magic += 150;
                            break;
                        case 3:
                            physical += 550;
                            magic += 250;
                            break;
                        case 4:
                            physical += 750;
                            magic += 350;
                            break;
                        case 5:
                            physical += 950;
                            magic += 450;
                            break;
                        case 6:
                            physical += 1100;
                            magic += 550;
                            break;
                        case 7:
                            physical += 1200;
                            magic += 625;
                            break;
                        case 8:
                            physical += 1300;
                            magic += 700;
                            break;
                        case 9:
                            physical += 1400;
                            magic += 750;
                            break;
                        case 10:
                            physical += 1500;
                            magic += 800;
                            break;
                        case 11:
                            physical += 1600;
                            magic += 850;
                            break;
                        case 12:
                            physical += 1700;
                            magic += 900;
                            break;
                    }

                    switch (Item.SocketOne)
                    {
                        case Enums.Gem.NormalGloryGem: gemVal += 100; break;
                        case Enums.Gem.RefinedGloryGem: gemVal += 300; break;
                        case Enums.Gem.SuperGloryGem: gemVal += 500; break;
                    }

                    switch (Item.SocketTwo)
                    {
                        case Enums.Gem.NormalGloryGem: gemVal += 100; break;
                        case Enums.Gem.RefinedGloryGem: gemVal += 300; break;
                        case Enums.Gem.SuperGloryGem: gemVal += 500; break;
                    }
                }
            }

            #endregion

            physical = Math.Max(physical, PhysicalDamageDecrease);
            magic = Math.Max(magic, MagicDamageDecrease);
            magic += gemVal;
            physical += gemVal;

            if (Magic)
                return (int)magic;
            else
                return (int)physical;
        }

        #endregion

        //public Double GemBonus(Byte type)
        //{
        //    Double bonus = 0;
        //    foreach (ConquerItem i in Owner.Equipment.Objects)
        //        if (i != null)
        //            if (i.IsWorn)
        //                bonus += i.GemBonus(type);
        //    if (Class >= 130 && Class <= 135)
        //        if (type == ItemSocket.Tortoise)
        //            bonus = Math.Min(0.8, bonus);
        //    return bonus;
        //}
        public int BattlePower
        {
            get { return BattlePowerCalc(this); }
        }

        public int NMBattlePower
        {
            get { return (int)(BattlePowerCalc(this) - MentorBattlePower); }
        }

        public uint BattlePowerFrom(Entity mentor)
        {
            if (mentor.NMBattlePower < NMBattlePower) return 0;
            uint bp = (uint)((mentor.NMBattlePower - NMBattlePower) / 3.3F);
            if (Level >= 125) bp = (uint)((bp * (135 - Level)) / 10);
            if (bp < 0) bp = 0;
            return bp;
        }

        public DateTime LastLogin
        {
            get { return this.mLastLogin; }
            set { this.mLastLogin = value; }
        }

        public bool WearsGoldPrize = false;
        public string LoweredName;

        public string Spouse
        {
            get { return _Spouse; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    Update(Network.GamePackets._String.Spouse, value, false);
                }

                _Spouse = value;
            }
        }

        public ulong Money
        {
            get { return _money; }
            set
            {
                _money = value;
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Money, value, false);
            }
        }

        private byte _vipLevel;

        public byte VIPLevel
        {
            get { return _vipLevel; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    Update(Network.GamePackets.Update.VIPLevel, value, false);
                }

                _vipLevel = value;
            }
        }

        public byte reinc;

        public byte ReincarnationLev
        {
            get { return reinc; }
            set { reinc = value; }
        }

        public uint ConquerPoints
        {
            get { return _conquerpoints; }
            set
            {
                if (value >= int.MaxValue)
                {
                    if (EntityFlag == EntityFlag.Player)
                    {
                        Owner.MessageBox("Max Allowed ConquerPoints :" + int.MaxValue);
                        return;
                    }
                }

                value = (uint)Math.Max(0, (int)value);
                _conquerpoints = value;
                Database.EntityTable.UpdateCps(this.Owner);
                if (EntityFlag == EntityFlag.Player)
                {
                    Update(Network.GamePackets.Update.ConquerPoints, (uint)value, false);
                }
            }
        }

        long _CpsBank;

        public long CpsBank
        {
            get { return _CpsBank; }
            set
            {
                value = Math.Max(0, value);
                _CpsBank = value;
                Database.EntityTable.UpdateBankCps(this.Owner);
            }
        }

        public uint TreasuerPoints
        {
            get { return _TreasuerPoints; }
            set
            {
                if (value <= 0)
                    value = 0;

                _TreasuerPoints = value;
                Database.EntityTable.UpdateTreasuerPoints(this.Owner);
            }
        }

        public ushort Body
        {
            get { return _body; }
            set
            {
                WriteUInt32((uint)(TransformationID * 10000000 + Face * 10000 + value), _Mesh, SpawnPacket);
                _body = value;
                if (EntityFlag == EntityFlag.Player)
                {
                    if (Owner != null)
                    {
                        if (Owner.ArenaStatistic != null)
                            Owner.ArenaStatistic.Model = (uint)(Face * 10000 + value);
                        Update(Network.GamePackets.Update.Mesh, Mesh, true);
                    }
                }
            }
        }

        public ushort DoubleExperienceTime
        {
            get { return _doubleexp; }
            set
            {
                ushort oldVal = DoubleExperienceTime;
                _doubleexp = value;
                if (FullyLoaded)
                    if (oldVal <= _doubleexp)
                        if (EntityFlag == EntityFlag.Player)
                        {
                            if (Owner != null)
                            {
                                Update(Network.GamePackets.Update.DoubleExpTimer, DoubleExperienceTime, 200, false);
                            }
                        }
            }
        }
        /* public ushort SuperPotion
         {
             get
             {
                 return _superpotion;
             }
             set
             {
                 uint oldVal = SuperPotion;
                 _superpotion = value;
                 if (FullyLoaded)
                     if (oldVal <= _doubleexp)
                         if (EntityFlag == EntityFlag.Player)
                         {
                             if (Owner != null)
                             {
                                 Update(Network.GamePackets.Update.DoubleExpTimer, DoubleExperienceTime, 500, false);
                             }
                         }
             }
         }*/

        public uint HeavenBlessing
        {
            get { return _heavenblessing; }
            set
            {
                uint oldVal = HeavenBlessing;
                _heavenblessing = value;
                if (FullyLoaded)
                    if (value > 0)
                        if (!ContainsFlag(Network.GamePackets.Update.Flags.HeavenBlessing) || oldVal <= _heavenblessing)
                        {
                            AddFlag(Network.GamePackets.Update.Flags.HeavenBlessing);
                            Update(Network.GamePackets.Update.HeavensBlessing, HeavenBlessing, false);
                            Update(Network.GamePackets._String.Effect, "bless", true);
                        }
            }
        }

        public byte Stamina
        {
            get { return _stamina; }
            set
            {
                _stamina = value;
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Stamina, value, false);
            }
        }

        public ushort TransformationID
        {
            get { return _transformationid; }
            set
            {
                _transformationid = value;
                WriteUInt32((uint)(value * 10000000 + Face * 10000 + Body), _Mesh, SpawnPacket);
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Mesh, Mesh, true);
            }
        }

        public ushort Face
        {
            get { return _face; }
            set
            {
                WriteUInt32((uint)(TransformationID * 10000000 + value * 10000 + Body), _Mesh, SpawnPacket);
                _face = value;
                if (EntityFlag == EntityFlag.Player)
                {
                    if (Owner != null)
                    {
                        if (Owner.ArenaStatistic != null)
                            Owner.ArenaStatistic.Model = (uint)(value * 10000 + Body);
                        Update(Network.GamePackets.Update.Mesh, Mesh, true);
                    }
                }
            }
        }

        public uint Mesh
        {
            get { return BitConverter.ToUInt32(SpawnPacket, _Mesh); }
        }

        public byte Class
        {
            get { return _class; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    if (Owner != null)
                    {
                        if (Owner.ArenaStatistic != null)
                            Owner.ArenaStatistic.Class = value;
                        Update(Network.GamePackets.Update.Class, value, false);
                    }
                }

                _class = value;
                SpawnPacket[_Class] = value;
                //SpawnPacket[209] = value;
                //SpawnPacket[214] = value;
                //SpawnPacket[217] = value;
                //SpawnPacket[218] = value;
            }
        }

        public byte Reborn
        {
            get
            {
                //  SpawnPacket[_Reborn] = _reborn;
                return _reborn;
            }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    Update(Network.GamePackets.Update.Reborn, value, true);
                }

                _reborn = value;
                SpawnPacket[_Reborn] = value;
            }
        }

        public byte Level
        {
            get
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    SpawnPacket[_Level] = _level;
                    return _level;
                }
                else
                {
                    SpawnPacket[_MonsterLevel] = _level;
                    return _level;
                }
            }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    Update((byte)Network.GamePackets.Update.Level, value, true);
                    Data update = new Data(true);
                    update.UID = UID;
                    update.ID = Data.Leveled;
                    update.Data24_Uint = value;
                    if (Owner != null)
                    {
                        (Owner as Client.GameState).SendScreen(update, true);
                        Owner.ArenaStatistic.Level = value;
                        Owner.ArenaStatistic.ArenaPoints = 1000;
                    }

                    if (Owner != null)
                    {
                        if (Owner.AsMember != null)
                        {
                            Owner.AsMember.Level = value;
                        }
                    }

                    SpawnPacket[_Level] = value;
                    //if (FullyLoaded)
                    UpdateDatabase("Level", value);
                }
                else
                {
                    SpawnPacket[_MonsterLevel] = value;
                }

                _level = value;
            }
        }

        private uint mentorBP;

        public uint MentorBattlePower
        {
            get { return mentorBP; }
            set
            {
                if ((int)value < 0)
                    value = 0;
                if (Owner.Mentor != null)
                {
                    if (Owner.Mentor.IsOnline)
                    {
                        ExtraBattlePower -= mentorBP;
                        mentorBP = value;
                        ExtraBattlePower += value;
                        int val = Owner.Mentor.Client.Entity.BattlePower;
                        Update(Network.GamePackets.Update.MentorBattlePower, (uint)Math.Min(val, value), (uint)val);
                    }
                    else
                    {
                        ExtraBattlePower -= mentorBP;
                        mentorBP = 0;
                        Update(Network.GamePackets.Update.MentorBattlePower, (uint)0, (uint)0);
                    }
                }
                else
                {
                    ExtraBattlePower -= mentorBP;
                    mentorBP = 0;
                    Update(Network.GamePackets.Update.MentorBattlePower, (uint)0, (uint)0);
                }
            }
        }

        public uint vipextra = 0;

        public uint ExtraBattlePower
        {
            get { return BitConverter.ToUInt32(SpawnPacket, _ExtraBattlepower); }
            set
            {
                if (value > 200) value = 0;
                //if ((Owner.Account.State == AccountTable.AccountState.GM || Owner.Account.State == AccountTable.AccountState.GM))
                //{
                //    value -= vipextra;
                //    value += (ConquerStructures.NobilityRank.King - NobilityRank);
                //    vipextra = (ConquerStructures.NobilityRank.King - NobilityRank);
                //    Send(new Message(vipextra + " GM Extra BP.", System.Drawing.Color.BurlyWood, Message.TopLeft));
                //}
                WriteUInt32(value, _ExtraBattlepower, SpawnPacket);
            }
        }

        public bool awayTeleported = false;

        public void SetAway(bool isAway)
        {
            if (!isAway && Away == 1)
            {
                Away = 0;
                Owner.SendScreen(SpawnPacket, false);

                if (awayTeleported)
                {
                    awayTeleported = false;
                    Teleport(PreviousMapID, PrevX, PrevY);
                }
            }
            else if (isAway && Away == 0)
            {
                if (!Constants.PKFreeMaps.Contains(MapID))
                {
                    if (!(MapID == 1036 || Owner.Mining) || Owner.Booth != null)
                    {
                        PreviousMapID = MapID;
                        PrevX = X;
                        PrevY = Y;
                        Teleport(1036, 100, 100);
                        awayTeleported = true;
                    }
                }
            }

            Away = isAway ? (byte)1 : (byte)0;
        }

        public byte Away
        {
            get { return SpawnPacket[_Away]; }
            set { SpawnPacket[_Away] = value; }
        }

        public byte Boss
        {
            get { return SpawnPacket[_Boss]; }
            set { SpawnPacket[_Boss] = 1; }
        }

        public uint UID
        {
            get
            {
                if (SpawnPacket != null)
                    return BitConverter.ToUInt32(SpawnPacket, _UID);
                else
                    return _uid;
            }
            set
            {
                _uid = value;
                WriteUInt32(value, _UID, SpawnPacket);
            }
        }

        public ushort GuildID
        {
            get { return BitConverter.ToUInt16(SpawnPacket, _GuildID); }
            set { WriteUInt32(value, _GuildID, SpawnPacket); }
        }

        public ushort GuildRank
        {
            get { return BitConverter.ToUInt16(SpawnPacket, _GuildRank); }
            set { WriteUInt16(value, _GuildRank, SpawnPacket); }
        }

        public ushort Strength
        {
            get { return _strength; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    Update(Network.GamePackets.Update.Strength, value, false);
                }

                _strength = value;
            }
        }

        public ushort Agility
        {
            get
            {
                if (OnCyclone())
                    return (ushort)(_agility);
                return _agility;
            }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Agility, value, false);
                _agility = value;
            }
        }

        public ushort Spirit
        {
            get { return _spirit; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Spirit, value, false);
                _spirit = value;
            }
        }

        public ushort Vitality
        {
            get { return _vitality; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Vitality, value, false);
                _vitality = value;
            }
        }

        public ushort Atributes
        {
            get { return _atributes; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Atributes, value, false);
                _atributes = value;
            }
        }

        public uint Hitpoints
        {
            get { return _hitpoints; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update((byte)Network.GamePackets.Update.Hitpoints, value, false);
                else if (EntityFlag == EntityFlag.Monster)
                {
                    //    if (Owner != null)
                    {
                        var update = new Update(true);
                        update.UID = UID;
                        update.Append(Network.GamePackets.Update.Hitpoints, value);
                        MonsterInfo.SendScreen(update);
                    }
                }

                _hitpoints = value;
                if (Boss > 0)
                {
                    uint key = (uint)(MaxHitpoints / 10000);
                    if (key != 0)
                        WriteUInt16((ushort)(value / key), _Hitpoints, SpawnPacket);
                    else
                        WriteUInt16((ushort)(value * MaxHitpoints / 1000 / 1.09), _Hitpoints, SpawnPacket);
                }
                else WriteUInt16((ushort)value, _Hitpoints, SpawnPacket);

                if (EntityFlag == EntityFlag.Player)
                    if (Owner != null)
                    {
                        if (Owner.Team != null)
                        {
                            foreach (var Team in Owner.Team.Temates)
                            {
                                AddToTeam addme = new AddToTeam();
                                addme.UID = Owner.Entity.UID;
                                addme.Hitpoints = (ushort)Owner.Entity.Hitpoints;
                                addme.Mesh = Owner.Entity.Mesh;
                                addme.Name = Owner.Entity.Name;
                                addme.MaxHitpoints = (ushort)Owner.Entity.MaxHitpoints;
                                Team.entry.Send(addme.ToArray());
                            }
                        }
                    }
            }
        }

        public ushort Mana
        {
            get { return _mana; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Mana, value, false);
                _mana = value;
            }
        }

        public ushort MaxMana
        {
            get { return _maxmana; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.MaxMana, value, false);
                _maxmana = value;
            }
        }

        public ushort HairStyle
        {
            get { return _hairstyle; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    Update(Network.GamePackets.Update.HairStyle, value, true);
                }

                _hairstyle = value;
                WriteUInt16(value, _HairStyle, SpawnPacket);
            }
        }

        public byte SubClassesActive
        {
            get { return SpawnPacket[238]; }
            set { SpawnPacket[238] = value; }
        }

        public byte SubClass
        {
            get { return this._SubClass; }
            set
            {
                this._SubClass = value;
                this.SpawnPacket[237] = (this.EntityFlag != MTA.Game.EntityFlag.Monster) ? this._SubClass : ((byte)0);
                if (SubClasses != null)
                    WriteUInt32(SubClasses.GetHashPoint(), _ActiveSubclass + 1, SpawnPacket);
                if (EntityFlag == Game.EntityFlag.Player)
                {
                    if (FullyLoaded)
                    {
                        UpdateDatabase("SubClass", _SubClass);
                    }
                }
            }
        }

        public byte SubClassLevel
        {
            get { return this._SubClassLevel; }
            set
            {
                this._SubClassLevel = value;
                switch (EntityFlag)
                {
                    case EntityFlag.Player:
                        if (FullyLoaded)
                        {
                            UpdateDatabase("SubClassLevel", value);
                        }

                        break;
                }
            }
        }

        public ConquerStructures.NobilityRank NobilityRank_;

        public ConquerStructures.NobilityRank NobilityRank
        {
            get { return NobilityRank_; }
            set
            {
                NobilityRank_ = value;
                SpawnPacket[_NobilityRank] = (byte)value;
                if (Owner != null)
                {
                    if (Owner.AsMember != null)
                    {
                        Owner.AsMember.NobilityRank = value;
                    }
                }
            }
        }

        public byte HairColor
        {
            get { return (byte)(HairStyle / 100); }
            set { HairStyle = (ushort)((value * 100) + (HairStyle % 100)); }
        }

        public ushort MapID
        {
            get { return _mapid; }
            set { _mapid = value; }
        }

        public uint Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public uint Status2
        {
            get { return _status2; }
            set { _status2 = value; }
        }

        public uint Status3
        {
            get { return _status3; }
            set { _status3 = value; }
        }

        public uint Status4
        {
            get { return _status4; }
            set { _status4 = value; }
        }

        public ushort PreviousMapID
        {
            get { return _previousmapid; }
            set { _previousmapid = value; }
        }

        private uint _Quest;

        public uint Quest
        {
            get { return this._Quest; }
            set { this._Quest = value; }
        }

        public ushort X
        {
            get { return _x; }
            set
            {
                _x = value;
                WriteUInt16(value, _X, SpawnPacket);
            }
        }

        public ushort Y
        {
            get { return _y; }
            set
            {
                _y = value;
                WriteUInt16(value, _Y, SpawnPacket);
            }
        }

        public ushort PX { get; set; }
        public ushort PY { get; set; }

        public bool Dead
        {
            get { return Hitpoints < 1; }
            set { throw new NotImplementedException(); }
        }

        public ushort Defence
        {
            get
            {
                if (Time32.Now < ShieldStamp.AddSeconds(ShieldTime) &&
                    ContainsFlag(Network.GamePackets.Update.Flags.MagicShield))
                    if (ShieldIncrease > 0)
                        return (ushort)Math.Min(65535, (int)(BaseDefence * ShieldIncrease));
                if (SuperItemBless > 0)
                    return (ushort)(BaseDefence + (float)BaseDefence / 100 * SuperItemBless);
                return BaseDefence;
            }
            set { BaseDefence = value; }
        }

        public ushort TransformationDefence
        {
            get
            {
                if (ContainsFlag(Network.GamePackets.Update.Flags.MagicShield))
                {
                    if (ShieldTime > 0)
                        return (ushort)(_TransPhysicalDefence * ShieldIncrease);
                    else
                        return (ushort)(_TransPhysicalDefence * MagicShieldIncrease);
                }

                return (ushort)_TransPhysicalDefence;
            }
            set { _TransPhysicalDefence = value; }
        }

        public ushort MagicDefencePercent
        {
            get { return _MDefencePercent; }
            set { _MDefencePercent = value; }
        }

        public ushort TransformationMagicDefence
        {
            get { return (ushort)_TransMagicDefence; }
            set { _TransMagicDefence = value; }
        }

        public ushort MagicDefence
        {
            get { return _MDefence; }
            set { _MDefence = value; }
        }

        public Client.GameState Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }

        public uint TransformationMinAttack
        {
            get
            {
                if (ContainsFlag(Network.GamePackets.Update.Flags.Stigma))
                    return (uint)(_TransMinAttack * StigmaIncrease);
                return _TransMinAttack;
            }
            set { _TransMinAttack = value; }
        }

        public uint TransformationMaxAttack
        {
            get
            {
                if (ContainsFlag(Network.GamePackets.Update.Flags.Stigma))
                    return (uint)(_TransMaxAttack * StigmaIncrease);
                return _TransMaxAttack;
            }
            set { _TransMaxAttack = value; }
        }

        public uint MinAttack
        {
            get { return _MinAttack; }
            set { _MinAttack = value; }
        }

        public uint MaxAttack
        {
            get { return _MaxAttack; }
            set { _MaxAttack = value; }
        }

        public uint MaxHitpoints
        {
            get { return _maxhitpoints; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    if (TransformationID != 0 && TransformationID != 98)
                        Update(Network.GamePackets.Update.MaxHitpoints, value, true);
                _maxhitpoints = value;
            }
        }

        public uint MagicAttack
        {
            get { return _MagicAttack; }
            set { _MagicAttack = value; }
        }

        public byte Dodge
        {
            get
            {
                if (ContainsFlag(Network.GamePackets.Update.Flags.Dodge))
                {
                    Console.WriteLine("Calc Dodge =" + (_Dodge * DodgeIncrease).ToString());
                    return (byte)(_Dodge * DodgeIncrease);
                }

                return _Dodge;
            }
            set { _Dodge = value; }
        }

        public byte TransformationDodge
        {
            get
            {
                if (ContainsFlag(Network.GamePackets.Update.Flags.Dodge))
                    return (byte)(_TransDodge * DodgeIncrease);
                return (byte)_TransDodge;
            }
            set { _TransDodge = value; }
        }

        public MapObjectType MapObjType
        {
            get { return _MapObjectType; }
            set { _MapObjectType = value; }
        }

        public EntityFlag EntityFlag
        {
            get { return _EntityFlag; }
            set { _EntityFlag = value; }
        }

        public ulong Experience
        {
            get { return _experience; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.Experience, value, false);
                _experience = value;
            }
        }

        public ushort EnlightenPoints
        {
            get { return _enlightenPoints; }
            set
            {
                _enlightenPoints = value;
                Update(Network.GamePackets.Update.EnlightPoints, value, true);
                WriteUInt16(value, 171, SpawnPacket);
            }
        }

        public byte ReceivedEnlightenPoints
        {
            get { return _receivedEnlighenPoints; }
            set { _receivedEnlighenPoints = value; }
        }

        public ushort EnlightmentTime
        {
            get { return _enlightmenttime; }
            set { _enlightmenttime = value; }
        }

        public ushort PKPoints
        {
            get { return _pkpoints; }
            set
            {
                _pkpoints = value;
                if (EntityFlag == EntityFlag.Player)
                {
                    Update(Network.GamePackets.Update.PKPoints, value, false);
                    if (PKPoints > 99)
                    {
                        RemoveFlag(Network.GamePackets.Update.Flags.RedName);
                        AddFlag(Network.GamePackets.Update.Flags.BlackName);
                    }
                    else if (PKPoints > 29)
                    {
                        AddFlag(Network.GamePackets.Update.Flags.RedName);
                        RemoveFlag(Network.GamePackets.Update.Flags.BlackName);
                    }
                    else if (PKPoints < 30)
                    {
                        RemoveFlag(Network.GamePackets.Update.Flags.RedName);
                        RemoveFlag(Network.GamePackets.Update.Flags.BlackName);
                    }
                }
            }
        }

        public ulong QuizPoints
        {
            get { return _quizpoints; }
            set
            {
                if (EntityFlag == EntityFlag.Player)
                    Update(Network.GamePackets.Update.QuizPoints, value, true);
                _quizpoints = value;
                WriteUInt64(value, _QuizPoints, SpawnPacket);
                UpdateDatabase("QuizPoints", _quizpoints);
            }
        }

        public UInt32 ClanId
        {
            get { return BitConverter.ToUInt32(SpawnPacket, _ClanUID); }
            set { WriteUInt32((UInt32)value, _ClanUID, SpawnPacket); }
        }

        public Clan Myclan;

        public Clan.Ranks ClanRank
        {
            get { return (Clan.Ranks)SpawnPacket[_ClanRank]; }
            set { SpawnPacket[_ClanRank] = (Byte)value; }
        }

        public ClanArena CLanArenaBattle, CLanArenaBattleFight;
        public Guildarena GuildArenaBattle, GuildArenaBattleFight;

        public Clan GetClan
        {
            get
            {
                Clan cl;
                Kernel.Clans.TryGetValue(ClanId, out cl);
                return cl;
            }
        }

        string clan = "";

        private UInt32 mClanJoinTarget;

        public UInt32 ClanJoinTarget
        {
            get { return this.mClanJoinTarget; }
            set { this.mClanJoinTarget = value; }
        }

        public Enums.PkMode PKMode
        {
            get { return _PKMode; }
            set { _PKMode = value; }
        }

        public ushort Action
        {
            get { return BitConverter.ToUInt16(SpawnPacket, _Action); }
            set { WriteUInt16(value, _Action, SpawnPacket); }
        }

        public Enums.ConquerAngle Facing
        {
            get { return (Enums.ConquerAngle)SpawnPacket[_Facing]; }
            set { SpawnPacket[_Facing] = (byte)value; }
        }

        public ulong StatusFlag
        {
            get { return BitConverter.ToUInt64(SpawnPacket, _StatusFlag); }
            set
            {
                ulong OldV = StatusFlag;
                if (value != OldV)
                {
                    WriteUInt64(value, _StatusFlag, SpawnPacket);
                    //Update(Network.GamePackets.Update.StatusFlag, value, !ContainsFlag(Network.GamePackets.Update.Flags.XPList));
                    UpdateEffects(true);
                }
            }
        }

        private ulong _Stateff2 = 0;

        public ulong StatusFlag2
        {
            get { return _Stateff2; }
            set
            {
                ulong OldV = StatusFlag2;
                if (value != OldV)
                {
                    _Stateff2 = value;
                    WriteUInt64(value, _StatusFlag2, SpawnPacket);

                    UpdateEffects(true);
                    // Update2(Network.GamePackets.Update.StatusFlag, value, true);// !ContainsFlag(Network.GamePackets.Update.Flags.XPList));//you need to update the SECOND value of stateff
                }
            }
        }

        private ulong _Stateff3 = 0;

        public ulong StatusFlag3
        {
            get { return _Stateff3; }
            set
            {
                ulong OldV = StatusFlag3;
                if (value != OldV)
                {
                    _Stateff3 = value;
                    WriteUInt64(value, _StatusFlag3, SpawnPacket);

                    UpdateEffects(true);
                    // Update2(Network.GamePackets.Update.StatusFlag, value, true);// !ContainsFlag(Network.GamePackets.Update.Flags.XPList));//you need to update the SECOND value of stateff
                }
            }
        }

        public void Save(String row, String value)
        {
            MySqlCommand Command = new MySqlCommand(MySqlCommandType.UPDATE);
            Command.Update("entities")
                .Set(row, value)
                .Where("uid", UID)
                .Execute();
        }

        public void Save(String row, UInt16 value)
        {
            MySqlCommand Command = new MySqlCommand(MySqlCommandType.UPDATE);
            Command.Update("entities")
                .Set(row, value)
                .Where("uid", UID)
                .Execute();
        }

        public void Save(String row, Boolean value)
        {
            MySqlCommand Command = new MySqlCommand(MySqlCommandType.UPDATE);
            Command.Update("entities")
                .Set(row, value)
                .Where("uid", UID)
                .Execute();
        }

        public void Save(String row, UInt32 value)
        {
            MySqlCommand Command = new MySqlCommand(MySqlCommandType.UPDATE);
            Command.Update("entities")
                .Set(row, value)
                .Where("uid", UID)
                .Execute();
        }

        #endregion

        #region Send Screen Acessor

        public void SendScreen(Interfaces.IPacket Data)
        {
            Client.GameState[] Chars = new Client.GameState[Kernel.GamePool.Count];
            Kernel.GamePool.Values.CopyTo(Chars, 0);
            foreach (Client.GameState C in Chars)
                if (C != null)
                    if (C.Entity != null)
                        if (Game.Calculations.PointDistance(X, Y, C.Entity.X, C.Entity.Y) <= 20)
                            C.Send(Data);
            Chars = null;
        }

        #endregion

        public void DieString()
        {
            _String str = new _String(true);
            str.UID = this.UID;
            str.Type = _String.Effect;
            str.Texts.Add("ghost");
            str.Texts.Add("1ghost");
            str.TextsCount = 1;
            if (EntityFlag == Game.EntityFlag.Player)
            {
                this.SendScreen(str);
            }
        }

        #region Functions

        public UInt16 BattlePowerCalc(Entity e)
        {
            UInt16 BP = (ushort)(e.Level + ExtraBattlePower);

            if (e == null) return 0;
            if (e.Owner == null) return 0;
            var weapons = e.Owner.Weapons;
            foreach (ConquerItem i in e.Owner.Equipment.Objects)
            {
                if (i == null) continue;
                int pos = i.Position;
                if (pos > 20) pos -= 20;
                if (pos != ConquerItem.Bottle &&
                    pos != ConquerItem.Garment && pos != ConquerItem.RightWeaponAccessory &&
                    pos != ConquerItem.LeftWeaponAccessory && pos != ConquerItem.SteedArmor)
                {
                    if (!i.IsWorn) continue;
                    if (pos == ConquerItem.RightWeapon || pos == ConquerItem.LeftWeapon)
                        continue;
                    BP += ItemBatlePower(i);
                }
                //else
                //{
                //    if (i.SocketOne != 0)
                //    {
                //        BP += 1;
                //        if ((Byte)i.SocketOne % 10 == 3)
                //            BP +=1;
                //        if (i.SocketTwo != 0)
                //        {
                //            BP += 1;
                //            if ((Byte)i.SocketTwo % 10 == 3)
                //                BP += 1;
                //        }
                //    }
                //    BP += i.Plus;
                //}
            }

            if (weapons.Item1 != null)
            {
                var i = weapons.Item1;
                Byte Multiplier = 1;
                if (i.IsTwoHander())
                    Multiplier = weapons.Item2 == null ? (Byte)2 : (Byte)1;
                BP += (ushort)(ItemBatlePower(i) * Multiplier);
            }

            if (weapons.Item2 != null)
                BP += ItemBatlePower(weapons.Item2);
            if (EntityFlag == Game.EntityFlag.Player)
            {
                if (Owner.DoChampStats)
                    BP += (Byte)(Math.Min((byte)e.NobilityRank,
                        Owner.ChampionAllowedStats[Owner.ChampionStats.Grade][8]));
                else
                    BP += (Byte)e.NobilityRank;
            }

            BP += (Byte)(e.Reborn * 5);
            EquipmentColor = BP;
            EquipmentColor = (uint)EquipmentColor;
            return BP;
        }

        private ushort ItemBatlePower(ConquerItem i)
        {
            Byte Multiplier = 1;
            Byte quality = (Byte)(i.ID % 10);
            int BP = 0;
            if (quality >= 6)
            {
                BP += (Byte)((quality - 5) * Multiplier);
            }

            if (i.SocketOne != 0)
            {
                BP += (Byte)(1 * Multiplier);
                if ((Byte)i.SocketOne % 10 == 3)
                    BP += (Byte)(1 * Multiplier);
                if (i.SocketTwo != 0)
                {
                    BP += (Byte)(1 * Multiplier);
                    if ((Byte)i.SocketTwo % 10 == 3)
                        BP += (Byte)(1 * Multiplier);
                }
            }

            BP += (Byte)(i.Plus * Multiplier);
            return (ushort)BP;
        }

        public Entity(EntityFlag Flag, bool companion)
        {
            Companion = companion;
            this.EntityFlag = Flag;
            Mode = Enums.Mode.None;
            update = new MTA.Network.GamePackets.Update(true);
            update.UID = UID;
            switch (Flag)
            {
                case EntityFlag.Player:
                    MapObjType = Game.MapObjectType.Player;
                    break;
                case EntityFlag.Monster: MapObjType = Game.MapObjectType.Monster; break;
            }

            SpawnPacket = new byte[0];
        }

        public void Ressurect()
        {
            if (EntityFlag == EntityFlag.Player)
                Owner.Send(new MapStatus()
                {
                    BaseID = Owner.Map.BaseID, ID = Owner.Map.ID,
                    Status = Database.MapsTable.MapInformations[Owner.Map.ID].Status,
                    Weather = Database.MapsTable.MapInformations[Owner.Map.ID].Weather
                });
        }

        public void BringToLife()
        {
            Hitpoints = MaxHitpoints;
            TransformationID = 0;
            Stamina = 100;
            FlashingNameTime = 0;
            FlashingNameStamp = Time32.Now;
            RemoveFlag(Network.GamePackets.Update.Flags.FlashingName);
            RemoveFlag(Network.GamePackets.Update.Flags.Dead | Network.GamePackets.Update.Flags.Ghost);
            if (EntityFlag == EntityFlag.Player)
                Owner.Send(new MapStatus()
                {
                    BaseID = Owner.Map.BaseID, ID = Owner.Map.ID,
                    Status = Database.MapsTable.MapInformations[Owner.Map.ID].Status
                });
            if (EntityFlag == Game.EntityFlag.Player)
            {
                Owner.ReviveStamp = Time32.Now;
                Owner.Attackable = false;
            }
        }

        public void DropRandomStuff(Entity KillerName)
        {
            if (Money > 100)
            {
                int amount = (int)(Money / 2);
                amount = Kernel.Random.Next(amount);
                if (Kernel.Rate(40))
                {
                    uint ItemID = Network.PacketHandler.MoneyItemID((uint)amount);
                    ushort x = X, y = Y;
                    Game.Map Map = Kernel.Maps[MapID];
                    if (Map.SelectCoordonates(ref x, ref y))
                    {
                        Money -= (ulong)amount;
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Money;
                        floorItem.Value = (uint)amount;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = x;
                        floorItem.Y = y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        Owner.SendScreenSpawn(floorItem, true);
                    }
                }
            }

            if (Owner.Inventory.Count > 0)
            {
                var array = Owner.Inventory.Objects.ToArray();
                uint count = (uint)(array.Length / 4);
                byte startfrom = (byte)Kernel.Random.Next((int)count);
                for (int c = 0; c < count; c++)
                {
                    int index = c + startfrom;
                    if (array[index] != null)
                    {
                        var infos = Database.ConquerItemInformation.BaseInformations[(uint)array[index].ID];
                        if (infos.Type == ConquerItemBaseInformation.ItemType.Dropable)
                        {
                            if (array[index].Lock == 0)
                            {
                                {
                                    if (!array[index].Bound && !array[index].Inscribed && array[index].ID != 723753)
                                    {
                                        if (!array[index].Suspicious && array[index].Lock != 1 &&
                                            array[index].ID != 723755 && array[index].ID != 723767 &&
                                            array[index].ID != 723772)
                                        {
                                            if (Kernel.Rate(140) && array[index].ID != 723774 &&
                                                array[index].ID != 723776)
                                            {
                                                var Item = array[index];
                                                if (Item.ID >= 729960 && Item.ID <= 729970)
                                                    return;
                                                Item.Lock = 0;
                                                ushort x = X, y = Y;
                                                Game.Map Map = Kernel.Maps[MapID];
                                                if (Map.SelectCoordonates(ref x, ref y))
                                                {
                                                    Network.GamePackets.FloorItem floorItem =
                                                        new Network.GamePackets.FloorItem(true);
                                                    Owner.Inventory.Remove(Item, Enums.ItemUse.Remove);
                                                    floorItem.Item = Item;
                                                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType
                                                        .Item;
                                                    floorItem.ItemID = (uint)Item.ID;
                                                    floorItem.MapID = MapID;
                                                    floorItem.MapObjType = Game.MapObjectType.Item;
                                                    floorItem.X = x;
                                                    floorItem.Y = y;
                                                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                                                    floorItem.OnFloor = Time32.Now;
                                                    floorItem.ItemColor = floorItem.Item.Color;
                                                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                                    while (Map.Npcs.ContainsKey(floorItem.UID))
                                                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                                    Map.AddFloorItem(floorItem);
                                                    Owner.SendScreenSpawn(floorItem, true);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (PKPoints >= 30 && Killer != null && Killer.Owner != null)
            {
                // foreach (var item in Owner.Equipment.Objects)
                for (int i = 0; i < 9; i++)
                {
                    var rnd = Kernel.Random.Next(19);
                    if (Owner.AlternateEquipment)
                        rnd = Kernel.Random.Next(10, 29);
                    var item = Owner.Equipment.TryGetItem((byte)rnd);
                    var Item = item;
                    if (Item != null)
                    {
                        byte dwp = 20;
                        if (!Owner.AlternateEquipment)
                        {
                            dwp = 0;
                            if (Item.Position >= 20)
                                continue;
                        }

                        if (Item.Position == 4 + dwp)
                        {
                            if (!Owner.Equipment.Free((byte)(5 + dwp)))
                            {
                                Item = Owner.Equipment.TryGetItem((byte)(5 + dwp));
                            }
                        }

                        //5 = LeftHand, 9 = Garment, 12 = Horse
                        if (Item.Position == 9 + dwp || Item.Position == 12)
                            continue;
                        if (Item.Position == 5 + dwp)
                            if (Item.ID.ToString().StartsWith("105"))
                                continue;
                        if (Kernel.Rate(25 + (int)(PKPoints > 30 ? 75 : 0)))
                        {
                            ushort x = X, y = Y;
                            Game.Map Map = Kernel.Maps[MapID];
                            if (Map.SelectCoordonates(ref x, ref y))
                            {
                                Owner.Equipment.RemoveToGround(Item.Position);
                                var infos = Database.ConquerItemInformation.BaseInformations[(uint)Item.ID];

                                Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                                floorItem.Item = Item;
                                floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                                floorItem.ItemID = (uint)Item.ID;
                                floorItem.MapID = MapID;
                                floorItem.MapObjType = Game.MapObjectType.Item;
                                floorItem.X = x;
                                floorItem.Y = y;
                                floorItem.Type = Network.GamePackets.FloorItem.DropDetain;
                                floorItem.OnFloor = Time32.Now;
                                floorItem.ItemColor = floorItem.Item.Color;
                                floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                while (Map.Npcs.ContainsKey(floorItem.UID))
                                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                Owner.SendScreenSpawn(floorItem, true);

                                Database.DetainedItemTable.DetainItem(Item, Owner, Killer.Owner);
                                Owner.Equipment.UpdateEntityPacket();
                                ClientEquip eq = new ClientEquip(Owner);
                                eq.DoEquips(Owner);
                                Owner.Send(eq);

                                Owner.LoadItemStats();
                                break;
                            }
                        }
                    }
                }
            }
            //if (PKPoints >= 30 && Killer != null && Killer.Owner != null)
            //{
            //    foreach (var Item in Owner.Equipment.Objects)
            //    {
            //        if (Item != null)
            //        {
            //            //5 = LeftHand, 9 = Garment, 12 = Horse
            //            if (Item.Position == 9 || Item.Position == 12)
            //                return;
            //            if (Item.Position == 5)
            //                if (Item.ID.ToString().StartsWith("105"))
            //                    return;
            //            if (Kernel.Rate(35 + (int)(PKPoints > 30 ? 75 : 0)))
            //            {
            //                ushort x = X, y = Y;
            //                Game.Map Map = Kernel.Maps[MapID];
            //                if (Map.SelectCoordonates(ref x, ref y))
            //                {
            //                    Owner.Equipment.RemoveToGround(Item.Position);
            //                    var infos = Database.ConquerItemInformation.BaseInformations[(uint)Item.ID];

            //                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
            //                    floorItem.Item = Item;
            //                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
            //                    floorItem.ItemID = (uint)Item.ID;
            //                    floorItem.MapID = MapID;
            //                    floorItem.MapObjType = Game.MapObjectType.Item;
            //                    floorItem.X = x;
            //                    floorItem.Y = y;
            //                    floorItem.Type = Network.GamePackets.FloorItem.DropDetain;
            //                    floorItem.OnFloor = Time32.Now;
            //                    floorItem.ItemColor = floorItem.Item.Color;
            //                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
            //                    while (Map.Npcs.ContainsKey(floorItem.UID))
            //                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
            //                    Owner.SendScreenSpawn(floorItem, true);

            //                    Database.DetainedItemTable.DetainItem(Item, Owner, Killer.Owner);
            //                    Owner.Equipment.UpdateEntityPacket();
            //                    ClientEquip eq = new ClientEquip(Owner);
            //                    eq.DoEquips(Owner);
            //                    Owner.Send(eq);
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}
            if (PKPoints > 99)
            {
                if (KillerName.EntityFlag == EntityFlag.Player)
                {
                    Kernel.SendWorldMessage(
                        new Network.GamePackets.Message(
                            Name + " has been captured by " + KillerName.Name +
                            " and sent in jail! The world is now safer!", System.Drawing.Color.Red, Message.Talk),
                        Program.Values);
                    Teleport(6000, 50, 50);
                }
                else
                {
                    Kernel.SendWorldMessage(
                        new Network.GamePackets.Message(
                            Name + " has been captured and sent in jail! The world is now safer!",
                            System.Drawing.Color.Red, Message.Talk), Program.Values);
                    Teleport(6000, 50, 50);
                }
            }
        }

        public static double GetAngle(ushort x, ushort y, ushort x2, ushort y2)
        {
            double xf1 = x, xf2 = x2, yf1 = y, yf2 = y2;
            double result = 90 - Math.Atan((xf1 - xf2) / (yf1 - yf2)) * 180 / Math.PI;
            if (xf1 - xf2 < 0 && yf1 - yf2 < 0)
                result += 180;
            else if (xf1 - xf2 == 0 && yf1 - yf2 < 0)
                result += 180;
            else if (yf1 - yf2 < 0 && xf1 - xf2 > 0)
                result -= 180;
            return result;
        }

        public class Vector
        {
            public ushort X, Y;
        }

        public static Vector GetBorderCoords(ushort old_x, ushort old_y, ushort Target_x, ushort Target_y)
        {
            double Θ = GetAngle(old_x, old_y, Target_x, Target_y);
            double w, h;
            Vector v = new Vector();
            byte quadrant = 1;
            if (Θ < 0)
                Θ += 360;
            else if (Θ == 360)
                Θ = 0;
            while (Θ >= 90)
            {
                Θ -= 90;
                quadrant++;
            }

            double screendistance = ScreenDistance;
            if (quadrant == 1)
            {
                screendistance = ScreenDistance / (Math.Cos(Θ * Math.PI / 180));
                if (screendistance > 25)
                    screendistance = ScreenDistance / (Math.Sin(Θ * Math.PI / 180));
                else if (Θ != 0)
                    v.Y++;
                h = screendistance * (Math.Sin(Θ * Math.PI / 180));
                w = screendistance * (Math.Cos(Θ * Math.PI / 180));
                v.X += (ushort)(Target_x + Math.Round(w));
                if (Θ == 90)
                    v.Y += (ushort)(Target_y - Math.Round(h));
                else
                    v.Y += (ushort)(Target_y + Math.Round(h));
            }
            else if (quadrant == 2)
            {
                screendistance = ScreenDistance / (Math.Cos(Θ * Math.PI / 180));
                if (screendistance > 25)
                {
                    screendistance = ScreenDistance / (Math.Sin(Θ * Math.PI / 180));
                    v.Y++;
                }

                w = screendistance * (Math.Sin(Θ * Math.PI / 180));
                h = screendistance * (Math.Cos(Θ * Math.PI / 180));
                v.X += (ushort)(Target_x - w);
                v.Y += (ushort)(Target_y + h);
            }
            else if (quadrant == 3)
            {
                screendistance = ScreenDistance / (Math.Cos(Θ * Math.PI / 180));
                if (screendistance > 25)
                    screendistance = ScreenDistance / (Math.Sin(Θ * Math.PI / 180));
                h = screendistance * (Math.Sin(Θ * Math.PI / 180));
                w = screendistance * (Math.Cos(Θ * Math.PI / 180));
                v.X += (ushort)(Target_x - w);
                v.Y += (ushort)(Target_y - h);
            }
            else if (quadrant == 4)
            {
                screendistance = ScreenDistance / (Math.Cos(Θ * Math.PI / 180));
                if (screendistance > 25)
                    screendistance = ScreenDistance / (Math.Sin(Θ * Math.PI / 180));
                else if (Θ > 0)
                    v.X++;
                w = screendistance * (Math.Sin(Θ * Math.PI / 180));
                h = screendistance * (Math.Cos(Θ * Math.PI / 180));
                v.X += (ushort)(Target_x + w);
                v.Y += (ushort)(Target_y - h);
            }

            return v;
        }

        public void Die(UInt32 killer) //replace this one too for die delay
        {
            if (EntityFlag == EntityFlag.Player)
            {
                Owner.XPCount = 0;
                if (Owner.Booth != null)
                {
                    Owner.Booth.Remove();
                    Owner.Booth = null;
                }
            }

            Killed = true;
            Hitpoints = 0;
            DeathStamp = Time32.Now;
            ToxicFogLeft = 0;
            if (Companion)
            {
                AddFlag(Network.GamePackets.Update.Flags.Ghost | Network.GamePackets.Update.Flags.Dead |
                        Network.GamePackets.Update.Flags.FadeAway);
                Network.GamePackets.Attack attack = new Network.GamePackets.Attack(true);
                attack.Attacked = UID;
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                MonsterInfo.SendScreen(attack);
                if (Owner.Entity.MyClones.ContainsKey(UID))
                {
                    Owner.Entity.MyClones.Remove(UID);
                    return;
                }

                Owner.Pet.RemovePet(pettype);
            }

            if (EntityFlag == Game.EntityFlag.Player)
            {
                Owner.Pet.ClearAll();
                if (MyClones.Count > 0)
                {
                    foreach (var item in MyClones.Values)
                    {
                        Data data = new Data(true);
                        data.UID = item.UID;
                        data.ID = Network.GamePackets.Data.RemoveEntity;
                        item.MonsterInfo.SendScreen(data);
                    }

                    MyClones.Clear();
                }
            }

            if (EntityFlag == EntityFlag.Player)
            {
                if (Constants.PKFreeMaps.Contains(MapID))
                    goto Over;

                //DropRandomStuff(Killer);

                Over:
                AddFlag(Network.GamePackets.Update.Flags.Dead);
                RemoveFlag(Network.GamePackets.Update.Flags.Fly);
                RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                RemoveFlag(Network.GamePackets.Update.Flags.Cyclone);
                RemoveFlag(Network.GamePackets.Update.Flags.Superman);
                RemoveFlag(Network.GamePackets.Update.Flags.FatalStrike);
                RemoveFlag(Network.GamePackets.Update.Flags.FlashingName);
                RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                RemoveFlag2(Network.GamePackets.Update.Flags2.Oblivion);
                RemoveFlag2(Network.GamePackets.Update.Flags2.AzureShield);
                RemoveFlag2(Network.GamePackets.Update.Flags2.CarryingFlag);
                RemoveFlag(Network.GamePackets.Update.Flags.CastPray);
                RemoveFlag(Network.GamePackets.Update.Flags.Praying);
                RemoveFlag3(Network.GamePackets.Update.Flags3.SuperCyclone);
                RemoveFlag3(Network.GamePackets.Update.Flags3.DragonCyclone);
                RemoveFlag3(Network.GamePackets.Update.Flags3.DragonFlow);
                RemoveFlag3(Network.GamePackets.Update.Flags3.DragonSwing);
                RemoveFlag2(Network.GamePackets.Update.Flags2.ChainBoltActive);
                RemoveFlag(Network.GamePackets.Update.Flags.Stigma);
                RemoveFlag(Network.GamePackets.Update.Flags.MagicShield);
                RemoveFlag2(Network.GamePackets.Update.Flags2.EffectBall);
                RemoveFlag2(Network.GamePackets.Update.Flags2.CannonBarrage);
                RemoveFlag2(Network.GamePackets.Update.Flags2.BlackbeardsRage);
                RemoveFlag(Network.GamePackets.Update.Flags.StarOfAccuracy);
                RemoveFlag3(Network.GamePackets.Update.Flags3.MagicDefender);
                RemoveFlag3(Network.GamePackets.Update.Flags3.KineticSpark);
                RemoveFlag3(Network.GamePackets.Update.Flags3.BladeFlurry);
                RemoveFlag2(Network.GamePackets.Update.Flags2.Fatigue);
                RemoveFlag3(Network.GamePackets.Update.Flags3.lianhuaran01);
                RemoveFlag3(Network.GamePackets.Update.Flags3.lianhuaran02);
                RemoveFlag3(Network.GamePackets.Update.Flags3.lianhuaran03);
                RemoveFlag3(Network.GamePackets.Update.Flags3.lianhuaran04);
                if (ContainsFlag3(Network.GamePackets.Update.Flags3.AuroraLotus))
                {
                    AuroraLotusEnergy = 0;
                    Lotus(AuroraLotusEnergy, Network.GamePackets.Update.AuroraLotus);
                }

                if (ContainsFlag3(Network.GamePackets.Update.Flags3.FlameLotus))
                {
                    FlameLotusEnergy = 0;
                    Lotus(FlameLotusEnergy, Network.GamePackets.Update.FlameLotus);
                }

                Network.GamePackets.Attack attack = new Attack(true);
                attack.AttackType = Network.GamePackets.Attack.Kill;
                attack.X = X;
                attack.Y = Y;
                attack.Attacked = UID;
                attack.Attacker = killer;
                attack.Damage = 0;
                Owner.SendScreen(attack, true);

                //  if (Body % 10 < 3)
                //     TransformationID = 99;
                // else
                //    TransformationID = 98;

                Owner.Send(new MapStatus()
                {
                    BaseID = Owner.Map.BaseID, ID = Owner.Map.ID,
                    Status = Database.MapsTable.MapInformations[Owner.Map.ID].Status,
                    Weather = Database.MapsTable.MapInformations[Owner.Map.ID].Weather
                });
                if (CLanArenaBattleFight != null)
                    CLanArenaBattleFight.CheakToEnd(Owner);
                if (GuildArenaBattleFight != null)
                    GuildArenaBattleFight.CheakToEnd(Owner);
                Owner.EndQualifier();
            }
            else
            {
                Kernel.Maps[MapID].Floor[X, Y, MapObjType, this] = true;
            }

            if (EntityFlag == EntityFlag.Player)
                if (OnDeath != null)
                    OnDeath(this);
        }

        public Entity Killer;


        public void Die(Entity killer) //just replace this whole void for die delay
        {
            if (killer.EntityFlag == Game.EntityFlag.Player)
            {
                if (ContainsFlag3((ulong)1UL << 53))
                    RemoveFlag3((ulong)1UL << 53);
                if (killer.MapID == 1234)
                {
                    if (ConquerPoints >= 12000000)
                    {
                        ConquerPoints -= 12000000;
                        killer.ConquerPoints += 12000000;
                    }
                }

                if (killer.MapID == 1235)
                {
                    if (ConquerPoints >= 8000000)
                    {
                        ConquerPoints -= 8000000;
                        killer.ConquerPoints += 8000000;
                    }
                }

                if (killer.MapID == 1236)
                {
                    if (ConquerPoints >= 6000000)
                    {
                        ConquerPoints -= 6000000;
                        killer.ConquerPoints += 6000000;
                    }
                }

                if (killer.MapID == 1237)
                {
                    if (ConquerPoints >= 4000000)
                    {
                        ConquerPoints -= 4000000;
                        killer.ConquerPoints += 4000000;
                    }
                }

                if (killer.MapID == 1238)
                {
                    if (ConquerPoints >= 10000000)
                    {
                        ConquerPoints -= 10000000;
                        killer.ConquerPoints += 10000000;
                    }
                }

                killer.CountKilling++;
                if (killer.CountKilling >= 20)
                {
                    killer.CountKilling = 0;
                    killer.HuntingExp += 2;
                }

                if (killer.MapID == 2014)
                {
                    if (killer.MapID == 2014)
                    {
                        killer.Owner.uniquepoints += 1;
                        if (killer.Owner.uniquepoints >= 20)
                        {
                            Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6,
                                "Congratulations, You Have Now " + killer.Owner.uniquepoints +
                                "  Points you can claim your prize now!");
                            npc.OptionID = 255;
                            killer.Owner.Send(npc.ToArray());
                        }
                        else
                        {
                            Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6,
                                "You Have Now " + killer.Owner.uniquepoints + "  Points Congratz you still need " +
                                (20 - killer.Owner.uniquepoints) + " more!");
                            npc.OptionID = 255;
                            killer.Owner.Send(npc.ToArray());
                        }
                    }
                }

                #region LastTeam

                if (killer.MapID == 16414)
                {
                    foreach (GameState clients in Kernel.GamePool.Values)
                    {
                        if (clients.Entity.MapID == 16414)
                        {
                            clients.Entity.SendScoreLAstTeam(clients);
                        }
                    }
                }

                #endregion

                #region Killer Points

                if (EntityFlag == EntityFlag.Player)
                {
                    if (killer.EntityFlag == EntityFlag.Player)
                    {
                        if (killer.MapID == 5451)
                        {
                            killer.ConquerPoints += 25000;
                            NpcReply reply;
                            reply = new NpcReply(6,
                                string.Concat(new object[]
                                    { "You Have Killed ", Owner.Entity.Name, " and get from him 25,000 CPs" }))
                            {
                                OptionID = 0xff
                            };
                            killer.Owner.Send(reply.ToArray());
                        }

                        if (Owner.Entity.MapID == 5451)
                        {
                            Owner.Entity.ConquerPoints -= 25000;
                            Owner.Entity.Teleport(1002, 438, 382);
                        }
                    }
                }

                #endregion

                #region Die Guild System

                if (killer.EntityFlag == EntityFlag.Player && EntityFlag == EntityFlag.Player)
                {
                    if (Owner.Guild != null && killer.Owner.Guild != null && Owner.Map.ID == 1015)
                    {
                        Owner.Guild.pkp_donation += 2;
                        Owner.Guild.pkp_donation -= 2;
                        killer.Money += 20;
                        Kernel.SendWorldMessage(
                            new Message(
                                "The " + killer.Owner.AsMember.Rank + " " + killer.Name + " of the Guild " +
                                killer.Owner.Guild.Name + " has killed the " + killer.Owner.AsMember.Rank + " " + Name +
                                " of the Guild " + Owner.Guild.Name + " at BirdIsland!", System.Drawing.Color.Yellow,
                                Network.GamePackets.Message.Guild), Program.Values);
                    }

                    if (Owner.Guild != null && killer.Owner.Guild != null && Owner.Map.ID == 1020)
                    {
                        Owner.Guild.pkp_donation += 2;
                        Owner.Guild.pkp_donation -= 2;
                        killer.Money += 20;
                        Kernel.SendWorldMessage(
                            new Message(
                                "The " + killer.Owner.AsMember.Rank + " " + killer.Name + " of the Guild " +
                                killer.Owner.Guild.Name + " has killed the " + killer.Owner.AsMember.Rank + " " + Name +
                                " of the Guild " + Owner.Guild.Name + " at ApeCity!", System.Drawing.Color.Yellow,
                                Network.GamePackets.Message.Guild), Program.Values);
                    }

                    if (Owner.Guild != null && killer.Owner.Guild != null && Owner.Map.ID == 1011)
                    {
                        Owner.Guild.pkp_donation += 2;
                        Owner.Guild.pkp_donation -= 2;
                        killer.Money += 20;
                        Kernel.SendWorldMessage(
                            new Message(
                                "The " + killer.Owner.AsMember.Rank + " " + killer.Name + " of the Guild " +
                                killer.Owner.Guild.Name + " has killed the " + killer.Owner.AsMember.Rank + " " + Name +
                                " of the Guild " + Owner.Guild.Name + " at PhoenixCastle!", System.Drawing.Color.Yellow,
                                Network.GamePackets.Message.Guild), Program.Values);
                    }

                    if (Owner.Guild != null && killer.Owner.Guild != null && Owner.Map.ID == 1000)
                    {
                        Owner.Guild.pkp_donation += 2;
                        Owner.Guild.pkp_donation -= 2;
                        killer.Money += 20;
                        Kernel.SendWorldMessage(
                            new Message(
                                "The " + killer.Owner.AsMember.Rank + " " + killer.Name + " of the Guild " +
                                killer.Owner.Guild.Name + " has killed the " + killer.Owner.AsMember.Rank + " " + Name +
                                " of the Guild " + Owner.Guild.Name + " at DesertCity!", System.Drawing.Color.Yellow,
                                Network.GamePackets.Message.Guild), Program.Values);
                    }

                    if (Owner.Guild != null && killer.Owner.Guild != null && Owner.Map.ID == 1001)
                    {
                        Owner.Guild.pkp_donation += 2;
                        Owner.Guild.pkp_donation -= 2;
                        killer.Money += 20;
                        Kernel.SendWorldMessage(
                            new Message(
                                "The " + killer.Owner.AsMember.Rank + " " + killer.Name + " of the Guild " +
                                killer.Owner.Guild.Name + " has killed the " + killer.Owner.AsMember.Rank + " " + Name +
                                " of the Guild " + Owner.Guild.Name + " at MysticCastle!", System.Drawing.Color.Yellow,
                                Network.GamePackets.Message.Guild), Program.Values);
                    }

                    if (Owner.Guild != null && killer.Owner.Guild != null && Owner.Map.ID == 1762)
                    {
                        Owner.Guild.pkp_donation += 2;
                        Owner.Guild.pkp_donation -= 2;
                        killer.Money += 20;
                        Kernel.SendWorldMessage(
                            new Message(
                                "The " + killer.Owner.AsMember.Rank + " " + killer.Name + " of the Guild " +
                                killer.Owner.Guild.Name + " has killed the " + killer.Owner.AsMember.Rank + " " + Name +
                                " of the Guild " + Owner.Guild.Name + " at FrozenGrotto 2!",
                                System.Drawing.Color.Yellow, Network.GamePackets.Message.Guild), Program.Values);
                    }

                    if (Owner.Guild != null && killer.Owner.Guild != null && Owner.Map.ID == 2056)
                    {
                        Owner.Guild.pkp_donation += 2;
                        Owner.Guild.pkp_donation -= 2;
                        killer.Money += 20;
                        Kernel.SendWorldMessage(
                            new Message(
                                "The " + killer.Owner.AsMember.Rank + " " + killer.Name + " of the Guild " +
                                killer.Owner.Guild.Name + " has killed the " + killer.Owner.AsMember.Rank + " " + Name +
                                " of the Guild " + Owner.Guild.Name + " at FrozenGrotto 6!",
                                System.Drawing.Color.Yellow, Network.GamePackets.Message.Guild), Program.Values);
                    }
                }

                #endregion

                #region CaptureTheFlag

                if (killer.GuildID != 0 && killer.MapID == CaptureTheFlag.MapID && CaptureTheFlag.IsWar)
                {
                    if (GuildID != 0)
                    {
                        if (killer.Owner.Guild.Enemy.ContainsKey(GuildID))
                            killer.Owner.Guild.CTFPoints += 1;
                        else if (killer.Owner.Guild.Ally.ContainsKey(GuildID))
                            killer.Owner.Guild.CTFPoints += 1;
                    }

                    if (ContainsFlag2(Network.GamePackets.Update.Flags2.CarryingFlag))
                    {
                        StaticEntity entity = new StaticEntity((uint)(X * 1000 + Y), X, Y, MapID);
                        entity.DoFlag();
                        Owner.Map.AddStaticEntity(entity);
                        RemoveFlag2(Network.GamePackets.Update.Flags2.CarryingFlag);
                        Owner.Send(Program.World.CTF.generateTimer(0));
                        Owner.Send(Program.World.CTF.generateEffect(Owner));
                        if (killer.GuildID != GuildID)
                        {
                            Killer.AddFlag2(Network.GamePackets.Update.Flags2.CarryingFlag);
                            Time32 end = FlagStamp.AddSeconds(60) - Time32.Now;
                            killer.FlagStamp = end;
                            killer.Owner.Send(Program.World.CTF.generateTimer((uint)end.Value));
                            killer.Owner.Send(Program.World.CTF.generateEffect(killer.Owner));
                            killer.Owner.Guild.CTFPoints += 3;
                        }
                    }
                }

                #endregion

                //SkyWar.AddScore(killer, this);
                //if (killer.MapID == Hunt_Thief.Map.ID)
                //    Hunt_Thief.AddScore(killer.Owner);
            }

            if (EntityFlag == Game.EntityFlag.Player && Owner.Fake && Owner.Booth != null)
                return;
            if (killer.MapID == 7777)
                killer.Owner.elitepoints += 1;
            if (killer.MapID == 3072)
                killer.Owner.KillerPoints += 1;

            if (EntityFlag == EntityFlag.Player)
            {
                Owner.XPCount = 0;
                if (Owner.Booth != null)
                {
                    Owner.Booth.Remove();
                    Owner.Booth = null;
                }
            }

            killer.KillCount++;
            killer.KillCount2++;
            Killer = killer;
            Hitpoints = 0;
            DeathStamp = Time32.Now;

            //DieString();
            ToxicFogLeft = 0;
            if (Companion)
            {
                AddFlag(Network.GamePackets.Update.Flags.Ghost | Network.GamePackets.Update.Flags.Dead |
                        Network.GamePackets.Update.Flags.FadeAway);
                Network.GamePackets.Attack zattack = new Network.GamePackets.Attack(true);
                zattack.Attacked = UID;
                zattack.AttackType = Network.GamePackets.Attack.Kill;
                zattack.X = X;
                zattack.Y = Y;
                MonsterInfo.SendScreen(zattack);
                Owner.Map.RemoveEntity(this);
                if (Owner.Entity.MyClones.ContainsKey(UID))
                {
                    Owner.Entity.MyClones.Remove(UID);
                    return;
                }

                Owner.Pet.RemovePet(pettype);
            }

            if (EntityFlag == Game.EntityFlag.Player)
            {
                Owner.Pet.ClearAll();
                if (MyClones.Count > 0)
                {
                    foreach (var item in MyClones.Values)
                    {
                        Data data = new Data(true);
                        data.UID = item.UID;
                        data.ID = Network.GamePackets.Data.RemoveEntity;
                        item.MonsterInfo.SendScreen(data);
                    }

                    MyClones.Clear();
                }
            }

            if (EntityFlag == EntityFlag.Player)
            {
                if (killer.EntityFlag == EntityFlag.Player)
                {
                    #region Cheack Pk Map

                    if (Owner.Entity.MapID == 2555 || Owner.Entity.MapID == 5530 || Owner.Entity.MapID == 5531 ||
                        Owner.Entity.MapID == 5532 || Owner.Entity.MapID == 1452 || Owner.Entity.MapID == 5560 ||
                        Owner.Entity.MapID == 5570 || Owner.Entity.MapID == 5580 || Owner.Entity.MapID == 1707 ||
                        Owner.Entity.MapID == 3070 || Owner.Entity.MapID == 3071)
                    {
                        Owner.Entity.Teleport(1002, 438, 382);
                        // MTA.Console.WriteLine("Done");
                    }

                    #endregion Cheack Pk Map

                    if (Constants.PKFreeMaps.Contains(killer.MapID))
                        goto Over;
                    if (Constants.Damage1Map.Contains(killer.MapID))
                        goto Over;
                    if (killer.Owner.Map.BaseID == 700)
                        goto Over;
                    if (killer.Owner.Challenge != null)
                        goto Over;


                    if (!ContainsFlag(Network.GamePackets.Update.Flags.FlashingName) &&
                        !ContainsFlag(Network.GamePackets.Update.Flags.BlackName))
                    {
                        killer.AddFlag(Network.GamePackets.Update.Flags.FlashingName);
                        killer.FlashingNameStamp = Time32.Now;
                        killer.FlashingNameTime = 60;
                        if (killer.GuildID != 0)
                        {
                            if (killer.Owner.Guild.Enemy.ContainsKey(GuildID))
                            {
                                killer.PKPoints += 3;
                                if (killer.Owner.AsMember != null)
                                    killer.Owner.AsMember.PkDonation += 3;
                            }
                            else
                            {
                                if (!killer.Owner.Enemy.ContainsKey(UID))
                                {
                                    killer.PKPoints += 10;
                                    if (killer.Owner.AsMember != null)
                                        killer.Owner.AsMember.PkDonation += 10;
                                }
                                else
                                {
                                    killer.PKPoints += 5;
                                    if (killer.Owner.AsMember != null)
                                        killer.Owner.AsMember.PkDonation += 5;
                                }
                            }
                        }
                        else
                        {
                            if (!killer.Owner.Enemy.ContainsKey(UID))
                                killer.PKPoints += 10;
                            else
                                killer.PKPoints += 5;
                        }

                        if (HeavenBlessing > 0)
                        {
                            if (killer.HeavenBlessing == 0)
                            {
                                PacketHandler.Cursed(500, killer.Owner);
                            }
                        }

                        Network.PacketHandler.AddEnemy(this.Owner, killer.Owner);
                    }


                    if (killer.EntityFlag == Game.EntityFlag.Player)
                    {
                        if (EntityFlag == Game.EntityFlag.Player)
                        {
                            Game.PkExpeliate pk = new Game.PkExpeliate();
                            if (!killer.PkExplorerValues.ContainsKey(UID))
                            {
                                pk.UID = killer.UID;
                                pk.killedUID = UID;
                                pk.Name = Name;
                                pk.KilledAt = GetMapName(MapID);
                                pk.LostExp = 0;
                                pk.Times = 1;
                                pk.Potency = (uint)BattlePower;
                                pk.Level = Level;
                                PkExpelTable.PkExploitAdd(killer.Owner, pk);
                            }
                            else
                            {
                                pk.UID = killer.UID;
                                pk.killedUID = UID;
                                pk.Name = Name;
                                pk.KilledAt = GetMapName(MapID);
                                pk.LostExp = 0;
                                killer.PkExplorerValues[UID].Times += 1;
                                pk.Times = killer.PkExplorerValues[UID].Times;
                                pk.Potency = (uint)BattlePower;
                                pk.Level = Level;
                                PkExpelTable.Update(killer.Owner, pk);
                            }
                        }
                    }

                    uint ran = (uint)MTA.Kernel.Random.Next(1, 10);
                    if (killer.EntityFlag == MTA.Game.EntityFlag.Player)
                    {
                        if (ran > 5)
                        {
                            DropRandomStuff(Killer);
                            killer.Owner.Send("If you have any problem in your item, relogin");
                        }
                    }
                }
            }

            RemoveFlag(Network.GamePackets.Update.Flags.FlashingName);
            Over:

            Network.GamePackets.Attack attack = new Attack(true);
            attack.Attacker = killer.UID;
            attack.Attacked = UID;
            attack.AttackType = Network.GamePackets.Attack.Kill;
            attack.X = X;
            attack.Y = Y;

            if (EntityFlag == EntityFlag.Player)
            {
                AddFlag(Network.GamePackets.Update.Flags.Ghost);
                AddFlag(Network.GamePackets.Update.Flags.Dead);
                RemoveFlag(Network.GamePackets.Update.Flags.Fly);
                RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                RemoveFlag(Network.GamePackets.Update.Flags.Cyclone);
                RemoveFlag(Network.GamePackets.Update.Flags.Superman);
                RemoveFlag(Network.GamePackets.Update.Flags.FatalStrike);
                RemoveFlag(Network.GamePackets.Update.Flags.FlashingName);
                RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                RemoveFlag2(Network.GamePackets.Update.Flags2.Oblivion);
                if (Body % 10 < 3) TransformationID = 99;
                else TransformationID = 98;
                RemoveFlag2(Network.GamePackets.Update.Flags2.AzureShield);
                RemoveFlag2(Network.GamePackets.Update.Flags2.CarryingFlag);
                RemoveFlag(Network.GamePackets.Update.Flags.CastPray);
                RemoveFlag(Network.GamePackets.Update.Flags.Praying);
                RemoveFlag3(Network.GamePackets.Update.Flags3.SuperCyclone);
                RemoveFlag3(Network.GamePackets.Update.Flags3.DragonCyclone);
                RemoveFlag3(Network.GamePackets.Update.Flags3.DragonFlow);
                RemoveFlag3(Network.GamePackets.Update.Flags3.DragonSwing);
                RemoveFlag2(Network.GamePackets.Update.Flags2.ChainBoltActive);
                RemoveFlag2(Network.GamePackets.Update.Flags2.EffectBall);
                RemoveFlag2(Network.GamePackets.Update.Flags2.CannonBarrage);
                RemoveFlag2(Network.GamePackets.Update.Flags2.BlackbeardsRage);
                RemoveFlag(Network.GamePackets.Update.Flags.Stigma);
                RemoveFlag(Network.GamePackets.Update.Flags.MagicShield);
                RemoveFlag(Network.GamePackets.Update.Flags.StarOfAccuracy);
                RemoveFlag3(Network.GamePackets.Update.Flags3.KineticSpark);
                RemoveFlag3(Network.GamePackets.Update.Flags3.MagicDefender);
                RemoveFlag2(Network.GamePackets.Update.Flags2.Fatigue);
                RemoveFlag3(Network.GamePackets.Update.Flags3.BladeFlurry);
                RemoveFlag3(Network.GamePackets.Update.Flags3.lianhuaran01);
                RemoveFlag3(Network.GamePackets.Update.Flags3.lianhuaran02);
                RemoveFlag3(Network.GamePackets.Update.Flags3.lianhuaran03);
                RemoveFlag3(Network.GamePackets.Update.Flags3.lianhuaran04);
                //  if (Body % 10 < 3)
                //    TransformationID = 99;
                //else
                //  TransformationID = 98;

                if (ContainsFlag3(Network.GamePackets.Update.Flags3.AuroraLotus))
                {
                    AuroraLotusEnergy = 0;
                    Lotus(AuroraLotusEnergy, Network.GamePackets.Update.AuroraLotus);
                }

                if (ContainsFlag3(Network.GamePackets.Update.Flags3.FlameLotus))
                {
                    FlameLotusEnergy = 0;
                    Lotus(FlameLotusEnergy, Network.GamePackets.Update.FlameLotus);
                }

                Owner.SendScreen(attack, true);
                Owner.Send(new MapStatus()
                {
                    BaseID = Owner.Map.BaseID, ID = Owner.Map.ID,
                    Status = Database.MapsTable.MapInformations[Owner.Map.ID].Status,
                    Weather = Database.MapsTable.MapInformations[Owner.Map.ID].Weather
                });
                //if (CLanArenaBattleFight != null)
                //    CLanArenaBattleFight.CheakToEnd(Owner);
                Owner.EndQualifier();
            }
            else
            {
                if (!Companion && !IsDropped && MonsterInfo != null)
                    MonsterInfo.Drop(killer);
                Kernel.Maps[MapID].Floor[X, Y, MapObjType, this] = true;
                if (killer.EntityFlag == EntityFlag.Player)
                {
                    if (Name != "SwordMaster" && Name != "ThrillingSpook" && Name != "LavaBeast" &&
                        Name != "SnowBanshee" && Name != "SnowBansheeSoul" && Name != "TeratoDragon")
                    {
                        killer.Owner.IncreaseExperience(MaxHitpoints, true);
                    }

                    if (killer.Owner.Team != null)
                    {
                        foreach (Client.GameState teammate in killer.Owner.Team.Teammates)
                        {
                            if (teammate == null)
                                continue;
                            if (Kernel.GetDistance(killer.X, killer.Y, teammate.Entity.X, teammate.Entity.Y) <=
                                Constants.pScreenDistance)
                            {
                                if (killer.UID != teammate.Entity.UID)
                                {
                                    uint extraExperience = MaxHitpoints / 2;
                                    if (killer.Spouse == teammate.Entity.Name)
                                        extraExperience = MaxHitpoints * 2;
                                    byte TLevelN = teammate.Entity.Level;
                                    if (killer.Owner.Team.CanGetNoobExperience(teammate))
                                    {
                                        if (teammate.Entity.Level < 137)
                                        {
                                            extraExperience *= 2;
                                            teammate.IncreaseExperience(extraExperience, false);
                                            teammate.Send(Constants.NoobTeamExperience(extraExperience));
                                        }
                                    }
                                    else
                                    {
                                        if (teammate.Entity.Level < 137)
                                        {
                                            teammate.IncreaseExperience(extraExperience, false);
                                            teammate.Send(Constants.TeamExperience(extraExperience));
                                        }
                                    }

                                    byte TLevelNn = teammate.Entity.Level;
                                    byte newLevel = (byte)(TLevelNn - TLevelN);
                                    if (newLevel != 0)
                                    {
                                        if (TLevelN < 70)
                                        {
                                            for (int i = TLevelN; i < TLevelNn; i++)
                                            {
                                                teammate.Team.Teammates[0].VirtuePoints += (uint)(i * 3.83F);
                                                teammate.Team.SendMessage(new Message(
                                                    "The leader, " + teammate.Team.Teammates[0].Entity.Name +
                                                    ", has gained " + (uint)(i * 7.7F) +
                                                    " virtue points for power leveling the rookies.",
                                                    System.Drawing.Color.Red, Message.Team));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (killer.Level < 138)
                    {
                        uint extraExp = MaxHitpoints;
                        extraExp *= Constants.ExtraExperienceRate;
                        extraExp += (uint)(extraExp * killer.Gems[3] / 100);
                        extraExp += (uint)(extraExp * ((float)killer.BattlePower / 100));

                        if (killer.HeavenBlessing > 0)
                            extraExp += (uint)(extraExp * 20 / 100);
                        if (killer.Reborn >= 2)
                            extraExp /= 3;
                        killer.Owner.Send(Constants.ExtraExperience(extraExp));
                    }
                    else if (killer.Level >= 138 && killer.Level < 140)
                    {
                        uint extraExp = MaxHitpoints / 2;
                        extraExp *= Constants.ExtraExperienceRate / 2;
                        extraExp += (uint)(extraExp * killer.Gems[3] / 100);
                        extraExp += (uint)(extraExp * ((float)killer.BattlePower / 100));
                        if (killer.HeavenBlessing > 0)
                            extraExp += (uint)(extraExp * 10 / 100);
                        if (killer.Reborn >= 2)
                            extraExp /= 4;
                        killer.Owner.Send(Constants.ExtraExperience(extraExp));
                    }

                    killer.Owner.XPCount++;
                    if (killer.OnKOSpell())
                        killer.KOSpellTime++;
                }
            }

            if (EntityFlag == EntityFlag.Player)
                if (OnDeath != null)
                    OnDeath(this);
        }

        public void RemoveMagicDefender()
        {
            if (MagicDefenderOwner && HasMagicDefender)
            {
                if (Owner.Team != null && HasMagicDefender && MagicDefenderOwner)
                {
                    foreach (var mate in Owner.Team.Teammates)
                    {
                        mate.Entity.HasMagicDefender = false;
                        mate.Entity.MagicDefenderSecs = 0;
                        mate.Entity.RemoveFlag3(MTA.Network.GamePackets.Update.Flags3.MagicDefender);
                        mate.Entity.Update(mate.Entity.StatusFlag, mate.Entity.StatusFlag2, mate.Entity.StatusFlag3,
                            MTA.Network.GamePackets.Update.MagicDefenderIcone, 0x80, 0, 0, false);
                    }
                }

                MagicDefenderOwner = false;
            }

            RemoveFlag3(MTA.Network.GamePackets.Update.Flags3.MagicDefender);
            Update(StatusFlag, StatusFlag2, StatusFlag3, MTA.Network.GamePackets.Update.MagicDefenderIcone, 0x80, 0, 0,
                false);
            HasMagicDefender = false;
        }

        public void Update(ulong val1, ulong val2, ulong val3, uint val4, uint val5, uint val6, uint val7, bool screen)
        {
            if (!SendUpdates)
                return;
            if (this.Owner == null)
                return;
            update = new Update(true);
            update.UID = UID;
            update.Append(val1, val2, val3, val4, val5, val6, val7);

            if (!screen)
                update.Send(Owner);
            else
                Owner.SendScreen(update, true);
        }

        public void Update(byte type, uint value, uint secondvalue)
        {
            Network.GamePackets.Update upd = new Update(true);
            upd.Append(type, value);
            upd.Append(type, secondvalue);
            upd.UID = UID;
            Owner.Send(upd);
        }

        public void Update(byte type, byte value, bool screen)
        {
            if (!SendUpdates)
                return;
            if (this.Owner == null)
                return;
            update = new Update(true);
            update.UID = UID;
            update.Append(type, value, (byte)UpdateOffset1, (byte)UpdateOffset2, (byte)UpdateOffset3,
                (byte)UpdateOffset4, (byte)UpdateOffset5, (byte)UpdateOffset6, (byte)UpdateOffset7);
            if (!screen)
                update.Send(Owner);
            else
                Owner.SendScreen(update, true);
        }

        public void Update(byte type, ushort value, bool screen)
        {
            if (!SendUpdates)
                return;
            update = new Update(true);
            update.UID = UID;
            update.Append(type, value);
            if (!screen)
                update.Send(Owner as Client.GameState);
            else
                (Owner as Client.GameState).SendScreen(update, true);
        }

        public void Update(byte type, uint value, bool screen)
        {
            if (!SendUpdates)
                return;
            update = new Update(true);
            update.UID = UID;
            update.Append(type, value);
            if (!screen)
                update.Send(Owner as Client.GameState);
            else
                (Owner as Client.GameState).SendScreen(update, true);
        }

        public void Update(byte type, ulong value, bool screen)
        {
            if (!SendUpdates)
                return;
            update = new Update(true);
            update.UID = UID;
            update.Append(type, value);
            if (EntityFlag == EntityFlag.Player)
            {
                if (!screen)
                    update.Send(Owner as Client.GameState);
                else
                    (Owner as Client.GameState).SendScreen(update, true);
            }
            else
            {
                MonsterInfo.SendScreen(update);
            }
        }

        public void Update(byte type, ulong value, ulong value2, bool screen)
        {
            if (!SendUpdates)
                return;
            update = new Update(true);
            update.UID = UID;
            update.PoPAppend(type, value, value2);
            if (!screen)
                update.Send(Owner as Client.GameState);
            else
                (Owner as Client.GameState).SendScreen(update, true);
        }

        public void UpdateEffects(bool screen)
        {
            if (!SendUpdates)
                return;
            update = new Update(true);
            update.UID = UID;
            update.AppendFull(0x19, StatusFlag, StatusFlag2, StatusFlag3, StatusFlag4);
            if (EntityFlag == EntityFlag.Player)
            {
                if (!screen)
                    update.Send(Owner as Client.GameState);
                else
                    (Owner as Client.GameState).SendScreen(update, true);
            }
            else
            {
                MonsterInfo.SendScreen(update);
            }
        }

        public void Update2(byte type, ulong value, bool screen)
        {
            if (!SendUpdates)
                return;
            update = new Update(true);
            update.UID = UID;
            update.Append2(type, value);
            if (EntityFlag == EntityFlag.Player)
            {
                if (!screen)
                    update.Send(Owner as Client.GameState);
                else
                    (Owner as Client.GameState).SendScreen(update, true);
            }
            else
            {
                MonsterInfo.SendScreen(update);
            }
        }

        public void Update(byte type, string value, bool screen)
        {
            if (!SendUpdates)
                return;
            Network.GamePackets._String update = new _String(true);
            update.UID = this.UID;
            update.Type = type;
            update.TextsCount = 1;
            update.Texts.Add(value);
            if (EntityFlag == EntityFlag.Player)
            {
                if (!screen)
                    update.Send(Owner as Client.GameState);
                else
                    (Owner as Client.GameState).SendScreen(update, true);
            }
            else
            {
                MonsterInfo.SendScreen(update);
            }
        }

        private void UpdateDatabase(string column, byte value)
        {
            new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE).Update("entities").Set(column, value)
                .Where("UID", UID).Execute();
        }

        private void UpdateDatabase(string column, long value)
        {
            new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE).Update("entities").Set(column, value)
                .Where("UID", UID).Execute();
        }

        private void UpdateDatabase(string column, ulong value)
        {
            new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE).Update("entities").Set(column, value)
                .Where("UID", UID).Execute();
        }

        private void UpdateDatabase(string column, bool value)
        {
            new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE).Update("entities").Set(column, value)
                .Where("UID", UID).Execute();
        }

        private void UpdateDatabase(string column, string value)
        {
            new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE).Update("entities").Set(column, value)
                .Where("UID", UID).Execute();
        }

        public static sbyte[] XDir = new sbyte[] { 0, -1, -1, -1, 0, 1, 1, 1 };
        public static sbyte[] YDir = new sbyte[] { 1, 1, 0, -1, -1, -1, 0, 1 };

        public static sbyte[] XDir2 = new sbyte[]
            { 0, -2, -2, -2, 0, 2, 2, 2, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1 };

        public static sbyte[] YDir2 = new sbyte[]
            { 2, 2, 0, -2, -2, -2, 0, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2 };

        public bool Move(Enums.ConquerAngle Direction, int teleport = 1)
        {
            ushort _X = X, _Y = Y;
            Facing = Direction;
            int dir = ((int)Direction) % XDir.Length;
            sbyte xi = XDir[dir], yi = YDir[dir];
            _X = (ushort)(X + xi);
            _Y = (ushort)(Y + yi);
            Game.Map Map = null;
            if (Kernel.Maps.TryGetValue(MapID, out Map))
            {
                var objType = MapObjType;
                if (Map.Floor[_X, _Y, objType])
                {
                    if (objType == MapObjectType.Monster)
                    {
                        Map.Floor[_X, _Y, MapObjType] = false;
                        Map.Floor[X, Y, MapObjType] = true;
                    }

                    X = _X;
                    Y = _Y;
                    return true;
                }
                else
                {
                    if (Mode == Enums.Mode.None)
                        if (EntityFlag != EntityFlag.Monster)
                            if (teleport == 1)
                                Teleport(MapID, X, Y);
                    return false;
                }
            }
            else
            {
                if (EntityFlag != EntityFlag.Monster)
                    Teleport(MapID, X, Y);
                else
                    return false;
            }

            return true;
        }

        public bool Move(Enums.ConquerAngle Direction, bool slide)
        {
            ushort _X = X, _Y = Y;
            if (!slide)
                return Move((Enums.ConquerAngle)((byte)Direction % 8));

            int dir = ((int)Direction) % XDir2.Length;
            Facing = Direction;
            sbyte xi = XDir2[dir], yi = YDir2[dir];
            _X = (ushort)(X + xi);
            _Y = (ushort)(Y + yi);

            Game.Map Map = null;

            if (Kernel.Maps.TryGetValue(MapID, out Map))
            {
                if (Map.Floor[_X, _Y, MapObjType])
                {
                    if (MapObjType == MapObjectType.Monster)
                    {
                        Map.Floor[_X, _Y, MapObjType] = false;
                        Map.Floor[X, Y, MapObjType] = true;
                    }

                    X = _X;
                    Y = _Y;
                    return true;
                }
                else
                {
                    if (Mode == Enums.Mode.None)
                    {
                        if (EntityFlag != EntityFlag.Monster)
                            Teleport(MapID, X, Y);
                        else
                            return false;
                    }
                }
            }
            else
            {
                if (EntityFlag != EntityFlag.Monster)
                    Teleport(MapID, X, Y);
                else
                    return false;
            }

            return true;
        }

        public void SendSpawn(Client.GameState client)
        {
            SendSpawn(client, true);
        }

        //public string _Effect;
        //public string Effect
        //{
        //    get { return _Effect; }
        //    set
        //    {
        //        _Effect = value;
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            if (EntityFlag == Game.EntityFlag.Player)
        //            {
        //                this.Owner.SendScreen(new _String(true)
        //                {
        //                    UID = UID,
        //                    TextsCount = 1,
        //                    Type = 10,
        //                    Texts = { value }
        //                }, true);
        //            }
        //        }
        //    }
        //}
        public void SendSpawn(Client.GameState client, bool checkScreen = true)
        {
            if (!this.Invisable)
            {
                if (client.Screen.Add(this) || !checkScreen)
                {
                    if (EntityFlag == Game.EntityFlag.Player)
                    {
                        if (client.InQualifier() && this.Owner.IsWatching() ||
                            (this.SkillTeamWatchingElitePKMatch != null || this.Owner.WatchingElitePKMatch != null) ||
                            this.Invisable)
                            return;
                        if (Owner.WatchingElitePKMatch != null)
                            return;
                        if (Invisable) return;
                        if (this.Owner.IsFairy)
                        {
                            FairySpawn FS = new FairySpawn(true);
                            FS.SType = this.Owner.SType;
                            FS.FairyType = this.Owner.FairyType;
                            FS.UID = this.UID;
                            client.Send(FS);
                        }

                        Data generalData = new Data(true);
                        generalData.UID = client.Entity.UID;
                        generalData.ID = Data.AppearanceType;
                        generalData.dwParam = (uint)client.Entity.Appearance;
                        client.SendScreen(generalData, true);
                        client.Send(SpawnPacket);
                    }
                    else
                    {
                        //if (client.Language != Languages.English)
                        //{

                        //    if (MonsterInfo.Name2 != null && MonsterInfo.Name2 != "" && MonsterInfo.Name2 != " ")
                        //    {
                        //        if (MonsterInfo.Name2.Length > MonsterInfo.Name.Length)
                        //            MonsterInfo.Name2 = MonsterInfo.Name2.Substring(0, MonsterInfo.Name.Length);
                        //        WriteStringList(new List<string>() { MonsterInfo.Name2, "" }, _Names, SpawnPacket);
                        //    }
                        //}
                        //else
                        //{
                        //    WriteStringList(new List<string>() { MonsterInfo.Name, "" }, _Names, SpawnPacket);
                        //}
                        client.Send(SpawnPacket);
                    }

                    if (EntityFlag == EntityFlag.Player)
                    {
                        if (Owner.Booth != null)
                        {
                            client.Send(Owner.Booth);
                            if (Owner.Booth.HawkMessage != null)
                                client.Send(Owner.Booth.HawkMessage);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// New Flag System by abdoumatrix 
        /// credits to its owner
        /// </summary>
        private BitVector32 BitVector32 = new BitVector32(32 * 6);

        public void nAddFlag(int flag)
        {
            BitVector32.Clear();
            BitVector32.Add(flag);
            for (byte x = 0; x < BitVector32.bits.Length; x++)
                WriteUInt32(BitVector32.bits[x], (ushort)((byte)26 + x * 4), SpawnPacket);
            nUpdateEffects(true);
        }

        public bool nContainsFlag(int flag)
        {
            return BitVector32.Contain(flag);
        }

        public void nRemoveFlag(int flag)
        {
            if (nContainsFlag(flag))
            {
                BitVector32.Remove(flag);

                for (byte x = 0; x < BitVector32.bits.Length; x++)
                    WriteUInt32(BitVector32.bits[x], (ushort)((byte)26 + x * 4), SpawnPacket);

                nUpdateEffects(true);
            }
        }

        public void nUpdateEffects(bool screen)
        {
            if (!SendUpdates)
                return;
            update = new Update(true);
            update.UID = UID;
            update.Append(Network.GamePackets.Update.StatusFlag, BitVector32.bits);
            if (EntityFlag == EntityFlag.Player)
            {
                if (EntityFlag == EntityFlag.Player)
                {
                    if (!screen)
                        update.Send(Owner as Client.GameState);
                    else
                        (Owner as Client.GameState).SendScreen(update, true);
                }
                else
                {
                    MonsterInfo.SendScreen(update);
                }
            }
        }

        /// <summary>
        /// Old Flag
        /// </summary>
        /// <param name="flag"></param>
        ///         
        public void AddFlag(ulong flag)
        {
            //if (!ContainsFlag(Network.GamePackets.Update.Flags.Dead) && !ContainsFlag(Network.GamePackets.Update.Flags.Ghost))
            StatusFlag |= flag;
        }

        public bool ContainsFlag(ulong flag)
        {
            ulong aux = StatusFlag;
            aux &= ~flag;
            return !(aux == StatusFlag);
        }

        public void RemoveFlag(ulong flag)
        {
            if (ContainsFlag(flag))
            {
                StatusFlag &= ~flag;
            }
        }

        public void AddFlag2(ulong flag)
        {
            if (flag == Network.GamePackets.Update.Flags2.SoulShackle)
            {
                StatusFlag2 |= flag;
                return;
            }

            if (!ContainsFlag(Network.GamePackets.Update.Flags.Dead) &&
                !ContainsFlag(Network.GamePackets.Update.Flags.Ghost))
                StatusFlag2 |= flag;
        }

        public bool ContainsFlag2(ulong flag)
        {
            ulong aux = StatusFlag2;
            aux &= ~flag;
            return !(aux == StatusFlag2);
        }

        public void RemoveFlag2(ulong flag)
        {
            if (ContainsFlag2(flag))
            {
                StatusFlag2 &= ~flag;
            }
        }

        public void AddFlag3(ulong flag)
        {
            StatusFlag3 |= flag;
        }

        public bool ContainsFlag3(ulong flag)
        {
            ulong aux = StatusFlag3;
            aux &= ~flag;
            return !(aux == StatusFlag3);
        }

        public void RemoveFlag3(ulong flag)
        {
            if (ContainsFlag3(flag))
            {
                StatusFlag3 &= ~flag;
            }
        }

        public void Shift(ushort X, ushort Y, uint mapID, Interfaces.IPacket shift = null)
        {
            if (_mapid != mapID) return;

            //if (EntityFlag == EntityFlag.Player)
            //{
            //    if (!Database.MapsTable.MapInformations.ContainsKey(MapID))
            //        return;
            //}
            this.X = X;
            this.Y = Y;
            if (shift == null)
            {
                shift = new Network.GamePackets.Data(true)
                {
                    UID = UID,
                    ID = Network.GamePackets.Data.FlashStep,
                    dwParam = MapID,
                    wParam1 = X,
                    wParam2 = Y
                };
            }

            if (EntityFlag == EntityFlag.Player)
            {
                Owner.SendScreen(shift, true);
                Owner.Screen.Reload(shift);
            }
        }

        public void Shift(ushort X, ushort Y)
        {
            if (EntityFlag == EntityFlag.Player)
            {
                if (!Database.MapsTable.MapInformations.ContainsKey(MapID))
                    return;
                this.X = X;
                this.Y = Y;
                Network.GamePackets.Data Data = new Network.GamePackets.Data(true);
                Data.UID = UID;
                Data.ID = Network.GamePackets.Data.FlashStep;
                Data.dwParam = MapID;
                Data.wParam1 = X;
                Data.wParam2 = Y;
                Owner.SendScreen(Data, true);
                Owner.Screen.Reload(null);
            }
        }

        public AppearanceType Appearance
        {
            get { return (AppearanceType)BitConverter.ToUInt16(SpawnPacket, _AppearanceType); }
            set { WriteUInt16((ushort)value, _AppearanceType, SpawnPacket); }
        }

        public bool fMove(Enums.ConquerAngle Direction, ref ushort _X, ref ushort _Y)
        {
            Facing = Direction;
            sbyte xi = 0, yi = 0;
            switch (Direction)
            {
                case Enums.ConquerAngle.North:
                    xi = -1;
                    yi = -1;
                    break;
                case Enums.ConquerAngle.South:
                    xi = 1;
                    yi = 1;
                    break;
                case Enums.ConquerAngle.East:
                    xi = 1;
                    yi = -1;
                    break;
                case Enums.ConquerAngle.West:
                    xi = -1;
                    yi = 1;
                    break;
                case Enums.ConquerAngle.NorthWest: xi = -1; break;
                case Enums.ConquerAngle.SouthWest: yi = 1; break;
                case Enums.ConquerAngle.NorthEast: yi = -1; break;
                case Enums.ConquerAngle.SouthEast: xi = 1; break;
            }

            _X = (ushort)(_X + xi);
            _Y = (ushort)(_Y + yi);
            if (EntityFlag == Game.EntityFlag.Player)
            {
                if (Owner.Map.Floor[_X, _Y, MapObjType, null])
                    return true;
                else
                    return false;
            }
            else
            {
                Game.Map Map = null;
                if (Kernel.Maps.TryGetValue(MapID, out Map))
                {
                    if (Map.Floor[_X, _Y, MapObjType, null])
                        return true;
                    else
                        return false;
                }

                return true;
            }
        }

        public void Teleport(ushort X, ushort Y)
        {
            if (EntityFlag == EntityFlag.Player)
            {
                AdvancedTeleport(true);
                if (!Database.MapsTable.MapInformations.ContainsKey(MapID) && !Kernel.Maps.ContainsKey(MapID) &&
                    !Owner.InQualifier())
                {
                    MapID = 1002;
                    X = 432;
                    Y = 378;
                }

                if (Owner.Team != null)
                    Owner.Team.GetClanShareBp(Owner);
                if (Owner.ProgressBar != null)
                    Owner.ProgressBar.End(Owner);
                this.X = X;
                this.Y = Y;
                Network.GamePackets.Data Data = new Network.GamePackets.Data(true);
                Data.UID = UID;
                Data.ID = Network.GamePackets.Data.Teleport;
                Data.dwParam = Database.MapsTable.MapInformations[MapID].BaseID;
                Data.wParam1 = X;
                Data.wParam2 = Y;
                Owner.Send(Data);
                Owner.Screen.FullWipe();
                Owner.Screen.Reload(null);
                Owner.Send(new MapStatus()
                {
                    BaseID = Owner.Map.BaseID, ID = Owner.Map.ID,
                    Status = Database.MapsTable.MapInformations[Owner.Map.ID].Status,
                    Weather = Database.MapsTable.MapInformations[Owner.Map.ID].Weather
                });
                AdvancedTeleport();
            }
        }


        public void SetLocation(ushort MapID, ushort X, ushort Y)
        {
            if (EntityFlag == EntityFlag.Player)
            {
                this.X = X;
                this.Y = Y;
                this.MapID = MapID;
            }
        }

        public void TeleportHouse(ushort MapID, ushort X, ushort Y)
        {
            if (EntityFlag == EntityFlag.Player)
            {
                AdvancedTeleport(true);
                if (!Database.MapsTable.MapInformations.ContainsKey(MapID) && Owner.QualifierGroup == null)
                {
                    MapID = 1002;
                    X = 432;
                    Y = 378;
                }

                if (EntityFlag == EntityFlag.Player)
                {
                    if (Owner.InQualifier())
                        if (MapID != 700 && MapID < 11000)
                            Owner.EndQualifier();
                }

                if (MapID == this.MapID)
                {
                    Teleport(X, Y);
                    return;
                }

                this.X = X;
                this.Y = Y;
                PX = 0;
                PY = 0;
                this.PreviousMapID = this.MapID;
                this.MapID = MapID;

                Network.GamePackets.Data Data = new Network.GamePackets.Data(true);
                Data.UID = UID;
                Data.ID = Network.GamePackets.Data.Teleport;
                Data.dwParam = Database.MapsTable.MapInformations[MapID].BaseID;
                Data.wParam1 = X;
                Data.wParam2 = Y;
                Owner.Send(Data);
                Owner.Send(new MapStatus()
                {
                    BaseID = Owner.Map.BaseID, ID = Owner.Map.ID,
                    Status = Database.MapsTable.MapInformations[Owner.Map.ID].Status,
                    Weather = Database.MapsTable.MapInformations[Owner.Map.ID].Weather
                });
                if (!Owner.Equipment.Free(12))
                    if (Owner.Map.ID == 1036 && Owner.Equipment.TryGetItem((byte)12).Plus < 6)
                        RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                AdvancedTeleport();
            }
        }

        public Map SpookMap;

        public void Teleport(ushort MapID, ushort X, ushort Y)
        {
            if (EntityFlag == EntityFlag.Player)
            {
                AdvancedTeleport(true);
                if (this.SpookMap != null && this.Owner.Team == null &&
                    this.Owner.Map.Entities.ContainsKey(501177 + (uint)this.MapID))
                {
                    Entity entity = this.Owner.Map.Entities[501177 + (uint)this.MapID];
                    this.Owner.Map.RemoveEntity(entity);
                    this.SpookMap.Dispose();
                }

                ushort baseID = 0;
                if (!Kernel.Maps.ContainsKey(MapID))
                {
                    if (!Database.MapsTable.MapInformations.ContainsKey(MapID) && Owner.QualifierGroup == null)
                    {
                        baseID = MapID = 1002;
                        X = 302;
                        Y = 278;
                    }
                    else
                    {
                        baseID = Database.MapsTable.MapInformations[MapID].BaseID;
                    }
                }
                else
                {
                    baseID = Kernel.Maps[MapID].BaseID;
                }

                if (EntityFlag == EntityFlag.Player)
                {
                    if (Owner.InQualifier())
                        if (MapID != 700 && MapID < 11000)
                            Owner.EndQualifier();
                }

                if (MapID == this.MapID)
                {
                    Teleport(X, Y);
                    return;
                }

                this.X = X;
                this.Y = Y;
                this.PreviousMapID = this.MapID;
                if (this.PreviousMapID == 2057)
                {
                    byte[] buffer = new byte[68];
                    Writer.WriteUshort(60, 0, buffer);
                    Writer.WriteUshort(2224, 2, buffer);
                    Writer.WriteUint(9, 4, buffer);
                    Owner.Entity.RemoveFlag2((ulong)Network.GamePackets.Update.Flags2.CarryingFlag);
                    Owner.Send(buffer);
                }

                this.MapID = MapID;
                Data Data = new Data(true);
                Data.UID = UID;
                Data.ID = Data.Teleport;
                Data.dwParam = baseID;
                Data.wParam1 = X;
                Data.wParam2 = Y;
                Owner.Send(Data);
                Owner.Screen.Reload(Data);
                Owner.Screen.FullWipe();
                Owner.Screen.Reload(null);
                Owner.Send(new MapStatus()
                {
                    BaseID = Owner.Map.BaseID, ID = Owner.Map.ID,
                    Status = Database.MapsTable.MapInformations[Owner.Map.ID].Status,
                    Weather = Database.MapsTable.MapInformations[Owner.Map.ID].Weather
                });
                Owner.Entity.Action = Enums.ConquerAction.None;
                Owner.ReviveStamp = Time32.Now;
                Owner.Attackable = false;
                if (!Owner.Equipment.Free(12))
                    if (Owner.Map.ID == 1036 && Owner.Equipment.TryGetItem((byte)12).Plus < 6)
                        RemoveFlag((ulong)Network.GamePackets.Update.Flags.Ride);
                if (ContainsFlag((ulong)Network.GamePackets.Update.Flags.Ride))
                {
                    if (Constants.RideForbiddenMaps.Contains(MapID))
                        RemoveFlag((ulong)Network.GamePackets.Update.Flags.Ride);
                }

                AdvancedTeleport();
            }
        }


        public ushort PrevX, PrevY;

        public void Teleport(ushort BaseID, ushort DynamicID, ushort X, ushort Y)
        {
            if (EntityFlag == EntityFlag.Player)
            {
                if (!Database.DMaps.MapPaths.ContainsKey(BaseID))
                    return;
                if (Owner.ProgressBar != null)
                    Owner.ProgressBar.End(Owner);

                if (Owner.InQualifier())
                    if (BaseID != 700 && BaseID < 11000)
                        Owner.EndQualifier();
                AdvancedTeleport(true);
                if (!Kernel.Maps.ContainsKey(DynamicID)) new Map(DynamicID, BaseID, Database.DMaps.MapPaths[BaseID]);
                this.PrevX = this.X;
                this.PrevY = this.Y;
                this.X = X;
                this.Y = Y;
                this.PreviousMapID = this.MapID;
                this.MapID = DynamicID;
                Network.GamePackets.Data Data = new Network.GamePackets.Data(true);
                Data.UID = UID;
                Data.ID = Network.GamePackets.Data.Teleport;
                Data.dwParam = BaseID;
                Data.wParam1 = X;
                Data.wParam2 = Y;
                Owner.Send(Data);
                Owner.Entity.Action = MTA.Game.Enums.ConquerAction.None;
                Owner.ReviveStamp = Time32.Now;
                Owner.Attackable = false;
                if (Owner.ChampionGroup == null)
                    Owner.Send(new MapStatus()
                    {
                        BaseID = Owner.Map.BaseID, ID = Owner.Map.ID,
                        Status = Database.MapsTable.MapInformations[Owner.Map.BaseID].Status,
                        Weather = Database.MapsTable.MapInformations[Owner.Map.BaseID].Weather
                    });
                if (!Owner.Equipment.Free(12))
                    if (Owner.Map.ID == 1036 && Owner.Equipment.TryGetItem((byte)12).Plus < 6)
                        RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                AdvancedTeleport();
            }
        }

        public void AdvancedTeleport(bool remove = false)
        {
            #region Teleport With Pet & Clones

            if (EntityFlag == EntityFlag.Player)
            {
                if (MyClones.Count > 0)
                {
                    foreach (var clone in MyClones.Values)
                    {
                        if (clone == null) continue;
                        if (remove)
                        {
                            Data data = new Data(true);
                            data.UID = clone.UID;
                            data.ID = Network.GamePackets.Data.RemoveEntity;
                            Owner.SendScreen(data);
                            Owner.RemoveScreenSpawn(clone, true);
                        }
                        else
                        {
                            clone.MapID = this.MapID;
                            clone.X = this.X;
                            clone.Y = this.Y;

                            Network.GamePackets.Data Data = new Network.GamePackets.Data(true);
                            Data.UID = clone.UID;
                            Data.ID = Network.GamePackets.Data.Teleport;
                            Data.dwParam = Database.MapsTable.MapInformations[MapID].BaseID;
                            Data.wParam1 = clone.X;
                            Data.wParam2 = clone.Y;
                            Owner.SendScreen(Data);

                            Owner.SendScreenSpawn(clone, true);
                        }
                    }
                }

                if (Owner.Pet != null)
                {
                    if (Owner.Pet.Pets.Count > 0)
                    {
                        foreach (var pet in Owner.Pet.Pets.Values)
                        {
                            if (pet == null) continue;
                            if (pet.Entity == null) continue;
                            if (remove)
                            {
                                Data data = new Data(true);
                                data.UID = pet.Entity.UID;
                                data.ID = Network.GamePackets.Data.RemoveEntity;
                                Owner.SendScreen(data);
                                Owner.RemoveScreenSpawn(pet.Entity, true);
                            }
                            else
                            {
                                pet.Entity.MapID = this.MapID;
                                pet.Entity.X = this.X;
                                pet.Entity.Y = this.Y;

                                Owner.SendScreenSpawn(pet.Entity, true);
                            }
                        }
                    }
                }

                if (remove)
                    Owner.RemoveScreenSpawn(Owner.Entity, false);
            }

            #endregion Teleport With Pet & Clones
        }

        public bool OnIntensify()
        {
            return ContainsFlag(Network.GamePackets.Update.Flags.Intensify);
        }

        public bool OnKOSpell()
        {
            return OnCyclone() || OnSuperman();
        }

        public bool OnOblivion()
        {
            return ContainsFlag2(Network.GamePackets.Update.Flags2.Oblivion);
        }

        public bool OnCyclone()
        {
            return ContainsFlag(Network.GamePackets.Update.Flags.Cyclone);
        }

        public bool OnSuperman()
        {
            return ContainsFlag(Network.GamePackets.Update.Flags.Superman);
        }

        public bool OnFatalStrike()
        {
            return ContainsFlag(Network.GamePackets.Update.Flags.FatalStrike);
        }

        public bool OnSuperCyclone()
        {
            return ContainsFlag3((uint)Network.GamePackets.Update.Flags3.SuperCyclone);
        }

        public String ToHex(byte[] buf)
        {
            var builder = new StringBuilder();
            foreach (var b in buf)
                builder.Append(b.ToString("X2") + " ");
            return builder.ToString();
        }

        public void Untransform()
        {
            if (MapID == 1036 && TransformationTime == 3600)
                return;
            this.TransformationID = 0;

            double maxHP = TransformationMaxHP;
            double HP = Hitpoints;
            double point = HP / maxHP;

            Hitpoints = (uint)(MaxHitpoints * point);
            Update(Network.GamePackets.Update.MaxHitpoints, MaxHitpoints, false);
        }

        public byte[] WindowSpawn()
        {
            byte[] buffer = new byte[SpawnPacket.Length];
            SpawnPacket.CopyTo(buffer, 0);
            buffer[_WindowSpawn] = 1;
            return buffer;
        }

        #endregion

        public void SetVisible()
        {
            SpawnPacket[_WindowSpawn] = 0;
        }

        public MTA.Game.Enums.CountryID CountryID
        {
            get { return (MTA.Game.Enums.CountryID)BitConverter.ToUInt16(SpawnPacket, _CountryCode); }
            set { WriteUInt16((ushort)value, _CountryCode, SpawnPacket); }
        }

        public uint EquipmentColor
        {
            get { return BitConverter.ToUInt32(this.SpawnPacket, _EquipmentColor); }
            set { WriteUInt32(value, _EquipmentColor, this.SpawnPacket); }
        }

        private uint guildBP = 0;

        public bool Archer()
        {
            if (EntityFlag == Game.EntityFlag.Player)
            {
                var weapons = Owner.Weapons;
                if (weapons.Item1 != null)
                    if (weapons.Item1.ID / 1000 == 500)
                        return true;
                    else if (weapons.Item2 != null)
                        if (weapons.Item2.ID / 1000 == 500)
                            return true;
            }

            return false;
        }

        public bool Assassin()
        {
            if (EntityFlag == Game.EntityFlag.Player)
            {
                if (Class >= 40 && Class <= 45)
                {
                    var weapons = Owner.Weapons;
                    if (weapons.Item1 != null)
                        if (weapons.Item1.ID / 1000 == 500)
                            return false;
                    if (weapons.Item1 != null)
                        if (weapons.Item1.ID / 1000 == 613)
                            return true;
                        else if (weapons.Item2 != null)
                            if (weapons.Item2.ID / 1000 == 613)
                                return true;
                }
            }

            return false;
        }

        public uint LotteryItemSoc1;
        public bool UseItem = false;

        public static string GetMapName(uint MapID)
        {
            string mapName = "Unknown";
            switch (MapID)
            {
                case 0x259:
                    mapName = "OfflineTG";
                    break;

                case 700:
                    mapName = "LotteryMap";
                    break;

                case 0x3e8:
                    mapName = "Desert";
                    break;

                case 0x3e9:
                    mapName = "MysticCastle";
                    break;

                case 0x3ea:
                    mapName = "CentralPlain";
                    break;

                case 0x3eb:
                    mapName = "MineCave";
                    break;

                case 0x3ec:
                    mapName = "JobCenter";
                    break;

                case 0x3ed:
                    mapName = "Arena";
                    break;

                case 0x3ee:
                    mapName = "Stable";
                    break;

                case 0x3ef:
                    mapName = "Blachsmith";
                    break;

                case 0x3f0:
                    mapName = "Grocery";
                    break;

                case 0x3f1:
                    mapName = "ArmorStore";
                    break;

                case 0x3f2:
                    mapName = "BirthVillage";
                    break;

                case 0x3f3:
                    mapName = "Forest";
                    break;

                case 0x3f4:
                    mapName = "Dreamland";
                    break;

                case 0x3f5:
                    mapName = "TigerCave";
                    break;

                case 0x3f6:
                    mapName = "DragonPool";
                    break;

                case 0x3f7:
                    mapName = "Island";
                    break;

                case 0x3f8:
                    mapName = "KylinCave";
                    break;

                case 0x3fa:
                    mapName = "Arena";
                    break;

                case 0x3fc:
                    mapName = "Canyon";
                    break;

                case 0x3fd:
                    mapName = "CopperMine";
                    break;

                case 0x401:
                    mapName = "IronMine";
                    break;

                case 0x402:
                    mapName = "CopperMine";
                    break;

                case 0x403:
                    mapName = "SilverMine";
                    break;

                case 0x404:
                    mapName = "GoldMine";
                    break;

                case 0x40c:
                    mapName = "Market";
                    break;

                case 0x40e:
                    mapName = "GuildArea";
                    break;

                case 0x40f:
                    mapName = "TrainingGround";
                    break;

                case 0x410:
                    mapName = "SkyCityPass";
                    break;

                case 0x411:
                    mapName = "PrizeClaimingMa";
                    break;

                case 0x412:
                    mapName = "PassPortal";
                    break;

                case 0x413:
                    mapName = "Peace";
                    break;

                case 0x414:
                    mapName = "Chaos";
                    break;

                case 0x415:
                    mapName = "Deserted";
                    break;

                case 0x416:
                    mapName = "Prosperous";
                    break;

                case 0x417:
                    mapName = "Disturbed";
                    break;

                case 0x418:
                    mapName = "Calmed";
                    break;

                case 0x419:
                    mapName = "Death";
                    break;

                case 0x41a:
                    mapName = "Life";
                    break;

                case 0x41b:
                    mapName = "MysticIsland";
                    break;

                case 0x41c:
                    mapName = "TestIsland";
                    break;

                case 0x424:
                    mapName = "Maze1";
                    break;

                case 0x425:
                    mapName = "Maze2";
                    break;

                case 0x426:
                    mapName = "Maze3";
                    break;

                case 0x427:
                    mapName = "AdventureIsland";
                    break;

                case 0x42e:
                    mapName = "SnakeDen";
                    break;

                case 0x430:
                    mapName = "CityArena4";
                    break;

                case 0x431:
                    mapName = "Arena1";
                    break;

                case 0x432:
                    mapName = "Arena2";
                    break;

                case 0x433:
                    mapName = "NewCanyon";
                    break;

                case 0x434:
                    mapName = "NewForest";
                    break;

                case 0x435:
                    mapName = "NewDesert";
                    break;

                case 0x436:
                    mapName = "NewIsland";
                    break;

                case 0x438:
                    mapName = "Arena2";
                    break;

                case 0x439:
                    mapName = "Arena3";
                    break;

                case 0x442:
                    mapName = "Arena1";
                    break;

                case 0x443:
                    mapName = "Arena2";
                    break;

                case 0x444:
                    mapName = "Arena1";
                    break;

                case 0x445:
                    mapName = "Arena2";
                    break;

                case 0x446:
                    mapName = "Arena1";
                    break;

                case 0x447:
                    mapName = "Arena2";
                    break;

                case 0x44c:
                    mapName = "MoonPlatform";
                    break;

                case 0x44d:
                    mapName = "MoonPlatform";
                    break;

                case 0x44e:
                    mapName = "MoonPlatform";
                    break;

                case 0x44f:
                    mapName = "MoonPlatform";
                    break;

                case 0x450:
                    mapName = "MoonPlatform";
                    break;

                case 0x451:
                    mapName = "MoonPlatform";
                    break;

                case 0x452:
                    mapName = "MoonPlatform";
                    break;

                case 0x453:
                    mapName = "MoonPlatform";
                    break;

                case 0x454:
                    mapName = "MoonPlatform";
                    break;

                case 0x455:
                    mapName = "MoonPlatform";
                    break;

                case 0x4b1:
                    mapName = "GlobeQuest1";
                    break;

                case 0x4b2:
                    mapName = "GlobeQuest2";
                    break;

                case 0x4b4:
                    mapName = "GlobeQuest4";
                    break;

                case 0x4b5:
                    mapName = "GlobeQuest5";
                    break;

                case 0x4b7:
                    mapName = "GlobeQuest7";
                    break;

                case 0x4b8:
                    mapName = "GlobeQuest8";
                    break;

                case 0x4ba:
                    mapName = "GlobeQuest10";
                    break;

                case 0x4bb:
                    mapName = "GlobeQuest11";
                    break;

                case 0x4bc:
                    mapName = "GlobeIsland";
                    break;

                case 0x4bd:
                    mapName = "GlobeDesert";
                    break;

                case 0x4be:
                    mapName = "GlobeCanyon";
                    break;

                case 0x4bf:
                    mapName = "GlobeForest";
                    break;

                case 0x4c0:
                    mapName = "GlobePlain";
                    break;

                case 0x4c1:
                    mapName = "JointCanyon";
                    break;

                case 0x4c2:
                    mapName = "IronMine1";
                    break;

                case 0x4c3:
                    mapName = "GlobeExit";
                    break;

                case 0x514:
                    mapName = "MysticCave";
                    break;

                case 0x547:
                    mapName = "Labyrinth";
                    break;

                case 0x548:
                    mapName = "Labyrinth";
                    break;

                case 0x549:
                    mapName = "Labyrinth";
                    break;

                case 0x54a:
                    mapName = "Labyrinth";
                    break;

                case 0x5ab:
                    mapName = "MeteorArena";
                    break;

                case 0x5dc:
                    mapName = "ClassPKArena1";
                    break;

                case 0x5dd:
                    mapName = "ClassPKArena2";
                    break;

                case 0x5de:
                    mapName = "ClassPKArena3";
                    break;

                case 0x5e1:
                    mapName = "CityArena1";
                    break;

                case 0x5e2:
                    mapName = "CityArena2";
                    break;

                case 0x5e4:
                    mapName = "CityArena4";
                    break;

                case 0x5e7:
                    mapName = "FurnitureStore";
                    break;

                case 0x5eb:
                    mapName = "CityArena1";
                    break;

                case 0x5ec:
                    mapName = "CityArena2";
                    break;

                case 0x5ee:
                    mapName = "CityArena4";
                    break;

                case 0x5f5:
                    mapName = "CityArena1";
                    break;

                case 0x5f6:
                    mapName = "CityArena2";
                    break;

                case 0x5f8:
                    mapName = "CityArena4";
                    break;

                case 0x60e:
                    mapName = "HalloweenCity1";
                    break;

                case 0x60f:
                    mapName = "HalloweenCity1";
                    break;

                case 0x6a4:
                    mapName = "EvilAbyss";
                    break;

                case 0x6e3:
                    mapName = "Dreamland";
                    break;

                case 0x6e4:
                    mapName = "Dreamland";
                    break;

                case 0x6e5:
                    mapName = "Hall";
                    break;

                case 0x6e8:
                    mapName = "KunLun";
                    break;

                case 0x6e9:
                    mapName = "Garden";
                    break;

                case 0x6ea:
                    mapName = "ArenaStage1";
                    break;

                case 0x6eb:
                    mapName = "ArenaStage2";
                    break;

                case 0x6ec:
                    mapName = "ArenaStage3";
                    break;

                case 0x6ed:
                    mapName = "ArenaStage4";
                    break;

                case 0x6ee:
                    mapName = "ArenaStage5";
                    break;

                case 0x6ef:
                    mapName = "ArenaStage6";
                    break;

                case 0x6f1:
                    mapName = "ArenaStage7";
                    break;

                case 0x6f2:
                    mapName = "DangerCave";
                    break;

                case 0x6f3:
                    mapName = "GhostCity";
                    break;

                case 0x6f4:
                    mapName = "DarkCity";
                    break;

                case 0x6f6:
                    mapName = "TreasureHouse";
                    break;

                case 0x6f7:
                    mapName = "TreasureHouse1";
                    break;

                case 0x6f8:
                    mapName = "Hut";
                    break;

                case 0x6f9:
                    mapName = "Dungeon1F";
                    break;

                case 0x6fa:
                    mapName = "Dungeon2F";
                    break;

                case 0x6fb:
                    mapName = "Dungeon3F";
                    break;

                case 0x6ff:
                    mapName = "RoseGarden";
                    break;

                case 0x700:
                    mapName = "SwanLake";
                    break;

                case 0x702:
                    mapName = "ViperCave";
                    break;

                case 0x709:
                    mapName = "Crypt";
                    break;

                case 0x70e:
                    mapName = "OrchidGarden";
                    break;

                case 0x70f:
                    mapName = "LockerRoomA";
                    break;

                case 0x710:
                    mapName = "MalePKArena";
                    break;

                case 0x711:
                    mapName = "LockerRoomB";
                    break;

                case 0x712:
                    mapName = "FemalePKArena";
                    break;

                case 0x714:
                    mapName = "ExtremePKArena";
                    break;

                case 0x71a:
                    mapName = "BanditChamer";
                    break;

                case 0x72d:
                    mapName = "ClassPKArena4";
                    break;

                case 0x72e:
                    mapName = "ClassPKArena5";
                    break;

                case 0x72f:
                    mapName = "ClassPKArena6";
                    break;

                case 0x742:
                    mapName = "PokerRoom";
                    break;

                case 0x744:
                    mapName = "VIPPokerRoom";
                    break;

                case 0x747:
                    mapName = "TwinCityArena";
                    break;

                case 0x748:
                    mapName = "WindPlainArena";
                    break;

                case 0x74c:
                    mapName = "PhoenixCastleArena";
                    break;

                case 0x74d:
                    mapName = "MapleForestArena";
                    break;

                case 0x751:
                    mapName = "ApeCityArena";
                    break;

                case 0x752:
                    mapName = "LoveCanyonArena";
                    break;

                case 0x756:
                    mapName = "BirdIslandArena";
                    break;

                case 0x757:
                    mapName = "BirdIslandArena";
                    break;

                case 0x75b:
                    mapName = "DesertCityArena";
                    break;

                case 0x75c:
                    mapName = "DesertArena";
                    break;

                case 0x760:
                    mapName = "LotteryHouse";
                    break;

                case 0x761:
                    mapName = "CouplesPKGround";
                    break;

                case 0x786:
                    mapName = "FrozenGrotto1";
                    break;

                case 0x787:
                    mapName = "FrozenGrotto2";
                    break;

                case 0x788:
                    mapName = "ClassPKArena10";
                    break;

                case 0x79a:
                    mapName = "ClassPKArena7";
                    break;

                case 0x79b:
                    mapName = "ClassPKArena8";
                    break;

                case 0x79c:
                    mapName = "ClassPKArena9";
                    break;

                case 0x79e:
                    mapName = "HorseRacing";
                    break;

                case 0x79f:
                    mapName = "RockMonsterDen";
                    break;

                case 0x7a9:
                    mapName = "Mausoleum";
                    break;

                case 0x7aa:
                    mapName = "Mausoleum";
                    break;

                case 0x7ab:
                    mapName = "Mausoleum";
                    break;

                case 0x7ac:
                    mapName = "Mausoleum";
                    break;

                case 0x7ad:
                    mapName = "Mausoleum";
                    break;

                case 0x7ae:
                    mapName = "Mausoleum";
                    break;

                case 0x7af:
                    mapName = "Mausoleum";
                    break;

                case 0x7b0:
                    mapName = "Mausoleum";
                    break;

                case 0x7b1:
                    mapName = "Mausoleum";
                    break;

                case 0x7b3:
                    mapName = "Mausoleum";
                    break;

                case 0x7b4:
                    mapName = "Mausoleum";
                    break;

                case 0x7b5:
                    mapName = "Mausoleum";
                    break;

                case 0x7b6:
                    mapName = "Mausoleum";
                    break;

                case 0x7b7:
                    mapName = "Mausoleum";
                    break;

                case 0x7b8:
                    mapName = "Mausoleum";
                    break;

                case 0x7b9:
                    mapName = "Mausoleum";
                    break;

                case 0x7ba:
                    mapName = "Mausoleum";
                    break;

                case 0x7bb:
                    mapName = "Mausoleum";
                    break;

                case 0x7bd:
                    mapName = "Mausoleum";
                    break;

                case 0x7be:
                    mapName = "Mausoleum";
                    break;

                case 0x7bf:
                    mapName = "Mausoleum";
                    break;

                case 0x7c0:
                    mapName = "Mausoleum";
                    break;

                case 0x7c1:
                    mapName = "Mausoleum";
                    break;

                case 0x7c2:
                    mapName = "Mausoleum";
                    break;

                case 0x7c3:
                    mapName = "Mausoleum";
                    break;

                case 0x7c4:
                    mapName = "Mausoleum";
                    break;

                case 0x7c5:
                    mapName = "Mausoleum";
                    break;

                case 0x7cf:
                    mapName = "FrozenGrotto3";
                    break;

                case 0x7d0:
                    mapName = "IronMine2";
                    break;

                case 0x7d1:
                    mapName = "IronMine2F";
                    break;

                case 0x7d2:
                    mapName = "IronMine3F";
                    break;

                case 0x7d3:
                    mapName = "IronMine3F";
                    break;

                case 0x7d4:
                    mapName = "IronMine3F";
                    break;

                case 0x7d5:
                    mapName = "IronMine3F";
                    break;

                case 0x7d6:
                    mapName = "IronMine4F";
                    break;

                case 0x7d7:
                    mapName = "IronMine4F";
                    break;

                case 0x7d8:
                    mapName = "IronMine4F";
                    break;

                case 0x7d9:
                    mapName = "IronMine4F";
                    break;

                case 0x7da:
                    mapName = "IronMine4F";
                    break;

                case 0x7db:
                    mapName = "IronMine4F";
                    break;

                case 0x7dc:
                    mapName = "IronMine4F";
                    break;

                case 0x7dd:
                    mapName = "IronMine4F";
                    break;

                case 0x7e4:
                    mapName = "CopperMine2F";
                    break;

                case 0x7e5:
                    mapName = "CopperMine2F";
                    break;

                case 0x7e6:
                    mapName = "CopperMine3F";
                    break;

                case 0x7e7:
                    mapName = "CopperMine3F";
                    break;

                case 0x7e8:
                    mapName = "CopperMine3F";
                    break;

                case 0x7e9:
                    mapName = "CopperMine3F";
                    break;

                case 0x7ea:
                    mapName = "CopperMine4F";
                    break;

                case 0x7eb:
                    mapName = "CopperMine4F";
                    break;

                case 0x7ec:
                    mapName = "CopperMine4F";
                    break;

                case 0x7ed:
                    mapName = "CopperMine4F";
                    break;

                case 0x7ee:
                    mapName = "CopperMine4F";
                    break;

                case 0x7ef:
                    mapName = "CopperMine4F";
                    break;

                case 0x7f0:
                    mapName = "CopperMine4F";
                    break;

                case 0x7f1:
                    mapName = "CopperMine4F";
                    break;

                case 0x7f8:
                    mapName = "SilverMine2F";
                    break;

                case 0x7f9:
                    mapName = "SilverMine2F";
                    break;

                case 0x7fa:
                    mapName = "SilverMine3F";
                    break;

                case 0x7fb:
                    mapName = "SilverMine3F";
                    break;

                case 0x7fc:
                    mapName = "SilverMine3F";
                    break;

                case 0x7fd:
                    mapName = "SilverMine3F";
                    break;

                case 0x7fe:
                    mapName = "SilverMine4F";
                    break;

                case 0x7ff:
                    mapName = "SilverMine4F";
                    break;

                case 0x800:
                    mapName = "SilverMine4F";
                    break;

                case 0x801:
                    mapName = "SilverMine4F";
                    break;

                case 0x802:
                    mapName = "SilverMine4F";
                    break;

                case 0x803:
                    mapName = "SilverMine4F";
                    break;

                case 0x804:
                    mapName = "SilverMine4F";
                    break;

                case 0x805:
                    mapName = "SilverMine4F";
                    break;

                case 0x806:
                    mapName = "FrozenGrotto4";
                    break;

                case 0x807:
                    mapName = "FrozenGrotto5";
                    break;

                case 0x808:
                    mapName = "FrozenGrotto6";
                    break;

                case 0x80c:
                    mapName = "GuildContest";
                    break;

                case 0x817:
                    mapName = "GuildPKTournamentMap";
                    break;

                case 0x818:
                    mapName = "GuildPKTournamentMap";
                    break;

                case 0x819:
                    mapName = "GuildPKTournamentMap";
                    break;

                case 0x81a:
                    mapName = "GuildPKTournamentMap";
                    break;

                case 0x81b:
                    mapName = "ElitePKWaitingArea1";
                    break;

                case 0x81c:
                    mapName = "ElitePKWaitingArea2";
                    break;

                case 0x81d:
                    mapName = "ElitePKWaitingArea3";
                    break;

                case 0x81e:
                    mapName = "ElitePKWaitingArea4";
                    break;

                case 0xfb5:
                    mapName = "HellGate";
                    break;

                case 0xfb6:
                    mapName = "HellHall";
                    break;

                case 0xfb7:
                    mapName = "LeftCloister";
                    break;

                case 0xfb8:
                    mapName = "RightCloister";
                    break;

                case 0xfb9:
                    mapName = "BattleFormation";
                    break;

                case 0x1388:
                    mapName = "NPCJail";
                    break;

                case 0x138a:
                    mapName = "دفعه";
                    break;

                case 0x1770:
                    mapName = "PKerJail";
                    break;

                case 0x1771:
                    mapName = "Jail";
                    break;

                case 0x1772:
                    mapName = "MacroJail";
                    break;

                case 0x1773:
                    mapName = "BotJail";
                    break;

                case 0x177a:
                    mapName = "AFKerJail";
                    break;

                case 0x2276:
                    mapName = "FiendLairFloor1";
                    break;

                case 0x2277:
                    mapName = "FiendLairFloor2";
                    break;

                case 0x2278:
                    mapName = "FiendLairFloor3";
                    break;

                case 0x2279:
                    mapName = "FiendLairFloor4";
                    break;

                case 0x227a:
                    mapName = "FiendLairFloor5";
                    break;

                case 0x227b:
                    mapName = "FiendLairFloor6";
                    break;

                case 0x227c:
                    mapName = "FiendLairFloor7";
                    break;

                case 0x227d:
                    mapName = "FiendLairFloor8";
                    break;

                case 0x227e:
                    mapName = "FiendLairFloor9";
                    break;

                case 0x227f:
                    mapName = "FiendLairFloor10";
                    break;

                case 0x2280:
                    mapName = "FiendLairFloor11";
                    break;

                case 0x2281:
                    mapName = "FiendLairFloor12";
                    break;

                case 0x2282:
                    mapName = "FiendLairFloor13";
                    break;

                case 0x2283:
                    mapName = "FiendLairFloor14";
                    break;

                case 0x2284:
                    mapName = "FiendLairFloor15";
                    break;

                case 0x2285:
                    mapName = "FiendLairFloor16";
                    break;

                case 0x2286:
                    mapName = "FiendLairFloor17";
                    break;

                case 0x2287:
                    mapName = "FiendLairFloor18";
                    break;

                case 0xdbba0:
                    mapName = "PlayersArena";
                    break;

                case 0xde2b0:
                    mapName = "ElitePKTournament";
                    break;

                case 0xf4240:
                    mapName = "ClanQualifier";
                    break;

                case 0xbef:
                    mapName = "GaleShallow";
                    break;

                case 0xbf0:
                    mapName = "SeaOfDeath";
                    break;

                default:
                    mapName = "OtherMap";
                    break;
            }

            return mapName;
        }

        public bool Ninja()
        {
            if (EntityFlag == Game.EntityFlag.Player)
            {
                if (Class >= 50 && Class <= 55)
                {
                    var weapons = Owner.Weapons;
                    if (weapons.Item1 != null && weapons.Item2 != null)
                        if (weapons.Item1.ID / 1000 == 601 || weapons.Item2.ID / 1000 == 601)
                            return true;
                        else
                            return false;
                }
            }

            return false;
        }

        public bool Fire()
        {
            if (EntityFlag == Game.EntityFlag.Player)
            {
                if (Class >= 140 && Class >= 145)
                    return true;
                else
                    return false;
            }

            return false;
        }

        public bool Water()
        {
            if (EntityFlag == Game.EntityFlag.Player)
            {
                if (Class >= 130 && Class >= 135)
                    return true;
                else
                    return false;
            }

            return false;
        }
        //public byte spiritFocusEffectuse = 0;
        //public Time32 spiritFocusEffect;
        // private Time32 spiritFocusStamp;
        //private bool spiritFocus;
        //public bool SpiritFocus
        //{
        //    get
        //    {
        //        if (!spiritFocus) return false;
        //        if (Time32.Now > spiritFocusStamp.AddSeconds(5))
        //            return true;
        //        return false;
        //    }
        //    set
        //    {
        //        if (value == true)
        //            AddFlag(Network.GamePackets.Update.Flags.Intensify);
        //        else
        //            RemoveFlag(Network.GamePackets.Update.Flags.Intensify);

        //        spiritFocus = value; spiritFocusStamp = Time32.Now;
        //    }
        //}
        public int SuperItemBless;
        public float IntensifyPercent;
        public Time32 EagleEyeStamp;
        public Time32 MortalWoundStamp;
        public Time32 Vortex;
        public bool Aura_isActive;
        public ulong Aura_actType;
        public ulong Aura_actType2;
        public uint Aura_actPower;
        public uint Aura_actLevel;
        public Time32 SpellStamp;
        public Time32 LastExpSave;
        public bool JustCreated = false;
        public Time32 ChainboltStamp;
        public int ChainboltTime;
        public Time32 FrozenStamp;
        public int FrozenTime;
        public object FlusterTime;
        public bool FrozenD;
        public AppearanceType AppearanceBkp;
        public Enums.PkMode PrevPKMode;
        public Time32 FlagStamp;
        private uint assassinBP;
        public static int Memberrinmap;
        public int TiempoAturdido;

        #region Cyclone War

        public void SendScoreLAstTeam(GameState client)
        {
            for (uint x = 1; x < 3; x++)
            {
                string Mesage = "";
                Leadrinmap = 0;
                Memberrinmap = 0;
                foreach (GameState clients in Kernel.GamePool.Values)
                {
                    if (clients.Entity.MapID == 16414)
                    {
                        if (clients.Entity.ContainsFlag(Network.GamePackets.Update.Flags.TeamLeader) &&
                            !clients.Entity.Dead)
                            Leadrinmap++;
                        else if (!clients.Entity.ContainsFlag(Network.GamePackets.Update.Flags.TeamLeader) &&
                                 !clients.Entity.Dead && clients.Team != null)
                            Memberrinmap++;
                    }
                }

                if (x == 1)

                    Mesage = "Team Leader Alive .: " + Leadrinmap + "";
                else
                    Mesage = "all Player Alive: " + Memberrinmap + "";
                Message msg = new Message(Mesage, System.Drawing.Color.Red,
                    x == 1 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                client.Send(msg);
            }
        }

        public uint ClanArenaCps = 0;
        public uint GuildArenaCps = 0;
        private uint _race;
        public static ushort Speed;
        public int DemonQuest;
        public DateTime LastDonate;
        public bool OnDragonSwing;
        public ushort DragonSwingPower;
        public int DragonFuryTime;
        public Time32 DragonFuryStamp;
        public Time32 DragonFlowStamp2;
        public Time32 DragonFlowStamp;
        public Time32 DragonCycloneStamp;
        public bool IsShieldBlock;
        public ushort ShieldBlockPercent;
        public uint killerpoints;
        public int TransferPoints;
        public bool checkingserver;
        public Time32 ShockStamp;
        public int Shock;
        public int Fright;
        public int SelectedStage;
        public int SelectedAttribute;

        public string QuestFrom
        {
            get
            {
                if (EntityFlag == Game.EntityFlag.Player)
                    return Owner["QuestFrom"];
                return "";
            }
            set
            {
                if (EntityFlag == Game.EntityFlag.Player)
                    Owner["QuestFrom"] = value;
            }
        }

        public string QuestMob
        {
            get
            {
                if (EntityFlag == Game.EntityFlag.Player)
                    return Owner["QuestMob"];
                return "";
            }
            set
            {
                if (EntityFlag == Game.EntityFlag.Player)
                    Owner["QuestMob"] = value;
            }
        }

        public uint QuestKO
        {
            get
            {
                if (EntityFlag == Game.EntityFlag.Player)
                    return Owner["QuestKO"];
                return 0;
            }
            set
            {
                if (EntityFlag == Game.EntityFlag.Player)
                    Owner["QuestKO"] = value;
            }
        }


        public uint race
        {
            get { return this._race; }
            set { this._race = value; }
        }

        #endregion

        public uint GuildBattlePower
        {
            get { return guildBP; }
            set
            {
                ExtraBattlePower -= guildBP;
                guildBP = value;
                Update(Network.GamePackets.Update.GuildBattlepower, guildBP, false);
                ExtraBattlePower += guildBP;
            }
        }

        internal void PreviousTeleport()
        {
            Teleport(PreviousMapID, PrevX, PrevY);
            BringToLife();
        }

        public bool IsThisLeftGate(int X, int Y)
        {
            if (Game.GuildWar.RightGate == null)
                return false;
            if (MapID == 1038)
            {
                if ((X == 223 || X == 222) && (Y >= 175 && Y <= 185))
                {
                    if (Game.GuildWar.RightGate.Mesh / 10 == 27)
                    {
                        return true;
                    }
                }
            }

            if (Game.SuperGuildWar.RightGate == null)
                return false;
            if (MapID == 10380)
            {
                if ((X == 223 || X == 222) && (Y >= 175 && Y <= 185))
                {
                    if (Game.SuperGuildWar.RightGate.Mesh / 10 == 27)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsThisRightGate(int X, int Y)
        {
            if (Game.GuildWar.LeftGate == null)
                return false;
            if (MapID == 1038)
            {
                if ((Y == 210 || Y == 209) && (X >= 154 && X <= 166))
                {
                    if (Game.GuildWar.LeftGate.Mesh / 10 == 24)
                    {
                        return true;
                    }
                }
            }

            if (Game.SuperGuildWar.LeftGate == null)
                return false;
            if (MapID == 10380)
            {
                if ((Y == 210 || Y == 209) && (X >= 154 && X <= 166))
                {
                    if (Game.SuperGuildWar.LeftGate.Mesh / 10 == 24)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ThroughGate(int X, int Y)
        {
            return IsThisLeftGate(X, Y) || IsThisRightGate(X, Y);
        }

        public uint AssassinBP
        {
            get { return assassinBP; }
            set
            {
                assassinBP = value;
                Writer.WriteUInt32(value, 233 + 4 + 4 + 9 + 4, SpawnPacket);
            }
        }

        public uint TrojanBP
        {
            get { return BitConverter.ToUInt32(this.SpawnPacket, 237 + 4 + 9); }
            set { Writer.WriteUInt32(value, 237 + 4 + 9 + 4, SpawnPacket); }
        }

        public bool EpicMonk()
        {
            if (EntityFlag == Game.EntityFlag.Player)
            {
                var weapons = Owner.Weapons;
                if (weapons.Item1 != null)
                    if (weapons.Item1.ID / 1000 == 622)
                        return true;
                    else if (weapons.Item2 != null)
                        if (weapons.Item2.ID / 1000 == 622)
                            return true;
            }

            return false;
        }

        public bool WrathoftheEmperor { get; set; }
        public DateTime WrathoftheEmperorStamp { get; set; }

        public byte ServerID
        {
            get { return SpawnPacket[254 + 4]; }
            set { SpawnPacket[254 + 4] = value; }
        }

        public int ChampionPointsToday { get; set; }
    }
}