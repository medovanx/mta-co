using System;
using System.Collections.Generic;
using MTA.Game;
using MTA.Client;

namespace MTA.Network.GamePackets
{
    public class Update : Writer, Interfaces.IPacket
    {
        public struct UpdateStruct
        {
            public uint Type;
            public ulong Value1;
            public ulong Value2;
        }

        public class Flags
        {
            public const ulong
                Normal = 0x0,
                FlashingName = 0x1,
                Stun = 1UL << 58,
                FreezeSmall = 1UL << 54,
                Dizzy = 1UL << 58,
                DivineShield = 1UL << 57,
                Poisoned = 0x2,
                SpeedIncreased = 1UL << 55,
                Confused = 1152921504606846976,
                Invisible = 0x4,
                XPList = 0x10,
                Frightened = 1UL << 54,
                Dead = 0x20,
                TeamLeader = 0x40,
                StarOfAccuracy = 0x80,
                MagicShield = 0x100,
                Stigma = 0x200,
                Ghost = 0x400,
                FadeAway = 0x800,
                RedName = 0x4000,
                BlackName = 0x8000,
                ReflectMelee = 0x20000,
                Superman = 0x40000,
                Ball = 0x80000,
                Ball2 = 0x100000,
                Invisibility = 0x400000,
                Cyclone = 0x800000,
                Dodge = 0x4000000,
                Fly = 0x8000000,
                Intensify = 0x10000000,
                CastPray = 0x40000000,
                Praying = 0x80000000,
                HeavenBlessing = 0x200000000,
                TopGuildLeader = 0x400000000,
                TopDeputyLeader = 0x800000000,
                MonthlyPKChampion = 0x1000000000,
                WeeklyPKChampion = 0x2000000000,
                TopWarrior = 0x4000000000,
                TopTrojan = 0x8000000000,
                TopArcher = 0x10000000000,
                TopWaterTaoist = 0x20000000000,
                TopFireTaoist = 0x40000000000,
                TopNinja = 0x80000000000,
                ShurikenVortex = 0x400000000000,
                FatalStrike = 0x800000000000,
                Flashy = 0x1000000000000,
                Ride = 0x4000000000000,
                TopSpouse = 1UL << 51,
                OrangeSparkles = 1UL << 52,
                PurpleSparkles = 1UL << 53,
                Dazed = 1UL << 54,
                RestoreAura = 1UL << 55,
                MoveSpeedRecovered = 1UL << 56,
                GodlyShield = 1UL << 57,
                ShockDaze = 1UL << 58,
                Cursed = 1UL << 32,
                Freeze = 1UL << 59,
                ChaosCycle = 1UL << 60;
        }

        public class Flags2
        {
            public const ulong
                Aturdido = 1152921504606846976,
                Congelado = 0x400000000000000,
                WeeklyTop8Pk = 0x01,
                TopPirate = 1UL << 58,
                //SoulShackle = 140737488355328,
                CarryingFlag = 0x40000000000000,
                WeeklyTop2PkGold = 0x2,
                WeeklyTop2PkBlue = 0x4,
                MonthlyTop8Pk = 0x8,
                MontlyTop2Pk = 0x10,
                MontlyTop3Pk = 0x20,
                Top8Fire = 0x40,
                Top2Fire = 0x80,
                Top3Fire = 0x100,
                Top8Water = 0x200,
                Top2Water = 0x400,
                Top3Water = 0x800,
                Top8Ninja = 0x1000,
                Top2Ninja = 0x2000,
                Top3Ninja = 0x4000,
                Top8Warrior = 0x8000,
                Top2Warrior = 0x10000,
                Top3Warrior = 0x20000,
                Top8Trojan = 0x40000,
                Top2Trojan = 0x80000,
                Top3Trojan = 0x100000,
                Top8Archer = 0x200000,
                Top2Archer = 0x400000,
                Top3Archer = 0x800000,
                Top3SpouseBlue = 0x1000000,
                Top2SpouseBlue = 0x2000000,
                Top3SpouseYellow = 0x4000000,
                Contestant = 0x8000000,
                ChainBoltActive = 0x10000000,
                AzureShield = 0x20000000,
                AzureShieldFade = 0x40000000,
                CaryingFlag = 2147483648,
                //blank next one?
                /*   TyrantAura = 0x400000000,
                   FendAura = 0x1000000000,
                   MetalAura = 0x4000000000,
                   WoodAura = 0x10000000000,
                   WaterAura = 0x40000000000,
                   FireAura = 17592186044416,
                   EarthAura = 0x400000000000,*/
                TyrantAura = 1UL << 98,
                FendAura = 1UL << 100,
                MetalAura = 1UL << 102,
                WoodAura = 1UL << 104,
                WaterAura = 1UL << 106,
                FireAura = 1UL << 108,
                EffectBall = 1UL << 119,
                EarthAura = 1UL << 110,
                TyrantAura2 = 0x600000000,
                FendAura2 = 0x1800000000,
                MetalAura2 = 0x6000000000,
                WoodAura2 = 0x18000000000,
                WaterAura2 = 0x60000000000,
                FireAura2 = 0x180000000000,
                EarthAura2 = 0x600000000000,
                SoulShackle = 140737488355328,
                Oblivion = 0x1000000000000,
                Top8Monk = 0x8000000000000,
                Top2Pirate = 0x1000000000000000,
                //Code MTA
                Top2Monk = 0x10000000000000,
                LionShield = 0x200000000000000,
                OrangeHaloGlow = 1125899906842624,
                LowVigorUnableToJump = 1125899906842624,
                TopSpouse = 2251799813685248,
                GoldSparkle = 4503599627370496,
                VioletSparkle = 9007199254740992,
                Dazed = 18014398509481984,
                //no movement
                BlueRestoreAura = 36028797018963968,
                MoveSpeedRecovered = 72057594037927936,
                SuperShieldHalo = 144115188075855872,
                HUGEDazed = 288230376151711744,
                //no movement
                IceBlock = 576460752303423488,
                //no movement
                Confused = 1152921504606846976,
                //reverses movement
                Top3Monk = 0x20000000000000,
                CannonBarrage = 1UL << 120,
                BlackbeardsRage = 1UL << 121,
                Fatigue = 1UL << 126,
                ShieldBlock = 1UL << 113,
                Toppirate3 = 1UL << 61,
                TopMonk = 0x4000000000000L;
        }

        public class Flags3
        {
            public const ulong
                MagicDefender = 0x01,
                Assassin = 0x20000,
                PathOfShadow = 1UL << 145,
                BlueBall = (ulong)1UL << 132,
                AutoHunting = (ulong)1UL << 20,
                SuperCyclone = (uint)1UL << 22,
                ConuqerSuperYellow = (ulong)1UL << 23, //GL flag
                ConuqerSuperBlue = (ulong)1UL << 24, //DL flag
                ConuqerSuperUnderBlue = (ulong)1UL << 25, // Memeber Flag 
                MrConquer = 1UL << 166,
                MsConquerHostess = 1UL << 167,
                rygh_hglx = 1UL << 174,
                rygh_syzs = 1UL << 175,
                MRConquerHost = 1UL << 166,
                MSConquerHostess = 1UL << 167,
                Flame1 = (ulong)1UL << 168,
                Flame2 = (ulong)1UL << 169,
                Flame3 = (ulong)1UL << 170,
                Flame4 = (ulong)1UL << 171,
                  GoldBrickNormal = 1UL << 161,
            GoldBrickRefined = 1UL << 162,
            GoldBrickUnique = 1UL << 163,
            GoldBrickElite = 1UL << 164,
            GoldBrickSuper = 1UL << 165,
                FlameLotus = 1UL << 173,
                BladeFlurry = 0x40000,
                KineticSpark = 0x80000,
                ShieldBreak = 1UL << 176,
                DivineGuard = 1UL << 177,
                AuroraLotus = 1UL << 172,
            #region EpicWarrior
 WarriorEpicShield = 1UL << 55,
                ManiacDance = 1UL << 53,
                BackFire = 1UL << 51,
                AngerWarriorEpic = 1UL << 178,
            #endregion
 DragonFlow = (ulong)1UL << 20,
                DragonWarriorTop = (uint)1UL << 26,
                DragonFury = (ulong)1UL << 30,
                DragonCyclone = (ulong)1UL << 31,
                DragonSwing = (ulong)1UL << 32,
                lianhuaran01 = 1UL << 168,
            lianhuaran02 = 1UL << 169,
            lianhuaran03 = 1UL << 170,
            lianhuaran04 = 1UL << 171;
        }

        public class Flags4
        {
            public const ulong
            Omnipotence = 1UL << 192,
            JusticeChant = 1UL << 194,
            BlockFrostGaze = 1UL << 195,
            HealingSnow = 1UL << 196,
            ChillingSnow = 1UL << 197,
            xChillingSnow = 1UL << 198,
            FreezingPelter = 1UL << 200,
            xFreezingPelter = 1UL << 201,
            RevengeTaill = 1UL << 202,
            WindwalkerTop = 1UL << 203,
            ShadowofChaser = 1UL << 204;
        }


        public class nFlags
        {
            public const
                int Normal = 0x0,
                FlashingName = 0,
                Poisoned = 1,
                Invisible = 2,
                XPList = 4,
                Dead = 5,
                TeamLeader = 6,
                StarOfAccuracy = 7,
                MagicShield = 8,
                Stigma = 9,
                Ghost = 10,
                FadeAway = 11,
                RedName = 14,
                BlackName = 15,
                ReflectMelee = 17,
                Superman = 18,
                Ball = 19,
                Ball2 = 20,
                Invisibility = 22,
                Cyclone = 23,
                Dodge = 26,
                Fly = 27,
                Lotus = 78,
                Intensify = 28,
                CastPray = 30,
                Praying = 31,
                HeavenBlessing = 33,
                TopGuildLeader = 34,
                TopDeputyLeader = 35,
                MonthlyPKChampion = 36,
                WeeklyPKChampion = 37,
                TopWarrior = 38,
                TopTrojan = 39,
                TopArcher = 40,
                TopWaterTaoist = 41,
                TopFireTaoist = 42,
                TopNinja = 43,
                ShurikenVortex = 46,
                FatalStrike = 47,
                Flashy = 48,
                Ride = 50,
                AzureShield = 92,
                SoulShackle = 110,
                Oblivion = 111,
                Shield = 128,
                RemoveName = 129,
                PurpleBall = 131,
                BlueBall = 132,
                PathOfShadow = 145,
                AssasinExpSkill = 146,
                KineticSpark = 147;
        }



        public const byte
            Hitpoints = 0,
            MaxHitpoints = 1,
            Mana = 2,
            MaxMana = 3,
            Money = 4,
            Experience = 5,
            PKPoints = 6,
            Class = 7,
            Stamina = 8,
            WHMoney = 9,
            Atributes = 10,
            Mesh = 11,
            Level = 12,
            Spirit = 13,
            Vitality = 14,
            Strength = 15,
            Agility = 16,
            HeavensBlessing = 17,
            DoubleExpTimer = 18,
            CursedTimer = 20,
            Reborn = 22,
            StatusFlag = 25,
            HairStyle = 26,
            RaceShopPoints = 47,
            XPCircle = 27,
            LuckyTimeTimer = 28,
            ConquerPoints = 29,
            OnlineTraining = 31,
            MentorBattlePower = 36,
            ExtraBattlePower = 36,
            Merchant = 38,
            VIPLevel = 39,
            QuizPoints = 40,
            EnlightPoints = 41,
            GuildShareBP = 44,
            BoundConquerPoints = 45,
            FirsRebornClass = 51,
            SecondRebornClass = 50,
            WoodResist = 60,
            WaterResist = 61,
            FireResist = 62,
            ClanSharedBattlePower = 42,
            GuildBattlepower = 44,
            MetalResist = 63,
            EarthResist = 64,
            MagicDefenderIcone = 49,
            DefensiveStance = 56,
            IncreasePStrike = 59,
            IncreaseMStrike = 60,
            IncreaseImunity = 61,
            IncreaseBreack = 62,
            IncreaseAntiBreack = 63,
            IncreaseMaxHp = 64,
            IncreasePAttack = 65,
            IncreaseMAttack = 66,
            IncreaseFinalPDamage = 67,
            IncreaseFinalMDamage = 68,
            IncreaseFinalPAttack = 69,
            IncreaseFinalMAttack = 70,
            Blessed = 66,
            gMagicAtk = 67,
            ExtraInventory = 79,
            AvailableSlots = 80,
            UnionMember = 81,
            MilitaryRank = 82,
            UnionRank = 83,
            MilitaryExploits = 84,
            gAttack = 68,
            FirstCreditGift = 71,
            Angery = 178,
            mantos = 82,
            mantos1 = 1,
            mantos2 = 2,
            mantos3 = 3,
            mantos4 = 4,
            mantos5 = 5,
            mantos6 = 6,
            mantos7 = 7,
            mantos8 = 8,
            mantos9 = 9,
            mantos10 = 10,
            mantos11 = 11,
            mantos12 = 12,
            mantos13 = 13,
            AuroraLotus = 172,
            FlameLotus = 173,
            Sash = 79,
            DragonSwing = 75,
            Lotus = 78,
            SoulShackle = 54,
            Assassin = 57;
        public const byte ClanShareBp = 42;


        byte[] Buffer;
        const byte minBufferSize = 96;
        public Update(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[minBufferSize + 8];
                WriteUInt16(minBufferSize, 0, Buffer);
                WriteUInt16(10017, 2, Buffer);
                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
            }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public uint self
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }
        public uint UpdateCount
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set
            {
                byte[] buffer = new byte[minBufferSize + 8 + 28 * value];
                Buffer.CopyTo(buffer, 0);
                WriteUInt16((ushort)(minBufferSize + 28 * value), 0, buffer);
                Buffer = buffer;
                WriteUInt32(value, 12, Buffer);
            }
        }
        public void Append(byte type, byte value)
        {
            this.UpdateCount++;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            Writer.WriteUInt32(type, offset, Buffer);
            Writer.WriteUInt64((ulong)value, offset + 4, Buffer);
            Writer.WriteUInt64(value, offset + 8, Buffer);
        }
        public void Append(byte type, double value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            Writer.WriteUInt32(type, offset, Buffer);
            Writer.WriteUInt64((ulong)(value * 100), offset + 4, Buffer);

        }
        public void Append(byte type, ushort value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 4, Buffer);
        }
        public static void MsgUpdateActivenessTask(Client.GameState client, byte Task, byte value)
        {
            byte[] Buffer = new byte[12 + 8];
            Writer.WriteByte(12, 0, Buffer);
            Writer.WriteUInt16(2820, 2, Buffer);//Length 
            Writer.WriteByte(3, 4, Buffer);//Unknown 
            Writer.WriteByte(1, 5, Buffer);//AmountTasksWillUpdate 
            Writer.WriteByte(Task, 6, Buffer);//type 
            Writer.WriteByte(0, 10, Buffer);//Completed 
            Writer.WriteByte(value, 11, Buffer);//value 
            client.Send(Buffer);
        }
        public void AppendFull(byte type, ulong val1, ulong val2, ulong val3, ulong val4)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(val1, offset + 4, Buffer);
            WriteUInt64(val2, offset + 12, Buffer);
            WriteUInt64(val3, offset + 20, Buffer);
            WriteUInt64(val4, offset + 28, Buffer);
        }

        public void PoPAppend(byte type, ulong val1, ulong val2)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(val1, offset + 8, Buffer);
            WriteUInt64(val2, offset + 16, Buffer);
        }
        public void Append(byte type, uint value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32)
                );
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 4, Buffer);

        }
        public void Append3(byte type, uint value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 20));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 8, Buffer);
        }
        public void Append4(byte type, uint value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 4, Buffer);
        }
        public void Append(byte type, ulong value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 4, Buffer);
        }
        public void Append2(byte type, byte value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 12, Buffer);
        }
        public void Append2(byte type, ushort value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 12, Buffer);
        }
        public void Append2(byte type, uint value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 12, Buffer);
        }
        public void Append2(byte type, ulong value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + ((UpdateCount - 1) * 32));
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 12, Buffer);
        }
        public void Clear()
        {
            byte[] buffer = new byte[minBufferSize + 8];
            WriteUInt16(minBufferSize, 0, Buffer);
            WriteUInt16(10017, 2, Buffer);
            WriteUInt32(UID, 8, buffer);
            WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
            Buffer = buffer;
        }
        public List<UpdateStruct> Updates
        {
            get
            {
                List<UpdateStruct> structs = new List<UpdateStruct>();
                ushort offset = 16;
                if (UpdateCount > 0)
                {
                    for (int c = 0; c < UpdateCount; c++)
                    {
                        UpdateStruct st = new UpdateStruct();
                        st.Type = BitConverter.ToUInt32(Buffer, offset); offset += 4;
                        st.Value1 = BitConverter.ToUInt64(Buffer, offset); offset += 8;
                        st.Value2 = BitConverter.ToUInt64(Buffer, offset); offset += 8;
                        structs.Add(st);
                    }
                }
                return structs;
            }
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
        public void Append(byte type, byte value, byte second, byte second2, byte second3, byte second4, byte second5, byte second6, byte second7)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(UpdateCount * 16);//12
            WriteUInt32(type, offset, Buffer);
            WriteUInt64(value, offset + 4, Buffer);
            WriteByte(second, offset + 12, Buffer);
            WriteByte(second2, offset + 13, Buffer);
            WriteByte(second3, offset + 14, Buffer);
            WriteByte(second4, offset + 15, Buffer);
            WriteByte(second5, offset + 16, Buffer);
            WriteByte(second6, offset + 17, Buffer);
            WriteByte(second7, offset + 18, Buffer);
        }
        /* public void Append(ulong val1, ulong val2, ulong val3, uint val4, uint val5, uint val6, uint val7)
         {
             UpdateCount = 2;
             WriteUInt32(0x19, 16, Buffer);
             WriteUInt64(val1, 20, Buffer);
             WriteUInt64(val2, 28, Buffer);
             WriteUInt64(val3, 36, Buffer);
             WriteUInt32(val4, 40 + 4, Buffer);
             WriteUInt32(val5, 44 + 4, Buffer);
             WriteUInt32(val6, 48 + 4, Buffer);
             WriteUInt32(val7, 52 + 4, Buffer);
             WriteUInt32(val4, 24, Buffer);
             WriteUInt32(val6, 32, Buffer);
         }*/

        public void Append(ulong val1, ulong val2, ulong val3, uint val4, uint val5, uint val6, uint val7)
        {
            WriteUInt32(val4, 24, Buffer);
            WriteUInt32(val6, 32, Buffer);
        }
        /* public void Append(ulong val1, ulong val2, ulong val3, uint val4, uint val5, uint val6, uint val7)
         {
             UpdateCount = 2;
             WriteUInt32(25, 16, Buffer);
             WriteUInt64(val1, 20, Buffer);
             WriteUInt64(val2, 28, Buffer);
             WriteUInt64(val3, 36, Buffer);
             WriteUInt32(val4, 48, Buffer);
             WriteUInt32(val5, 52, Buffer);
             WriteUInt32(val6, 56, Buffer);
             WriteUInt32(val7, 70, Buffer);
         }*/
        public void Append(byte type, uint[] value)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + (UpdateCount - 1) * 28);
            int position = offset + 4;
            for (int x = 0; x < value.Length; x++)
            {
                WriteUInt32(value[x], (ushort)(position), Buffer);
                position += 4;
            }
            ;
        }
        public byte[] PStrike(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(59, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(150, 52, buffer);
            return buffer;
        }
        public byte[] MStrike(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(60, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(150, 52, buffer);
            return buffer;
        }
        public byte[] Immunity(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(61, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(150, 52, buffer);
            return buffer;
        }
        public byte[] Break(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(62, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(150, 52, buffer);
            return buffer;
        }
        public byte[] Antibreak(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(63, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(150, 52, buffer);
            return buffer;
        }
        public byte[] MaxHP(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(64, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(3000, 52, buffer);
            return buffer;
        }
        public byte[] immunizecontrol(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(65, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(3000, 52, buffer);
            return buffer;
        }
        public byte[] immunizeall(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(66, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(3000, 52, buffer);
            return buffer;
        }
        public byte[] FinalPDamage(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(67, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(3000, 52, buffer);
            return buffer;
        }
        public byte[] FinalMDamage(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(68, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(3000, 52, buffer);
            return buffer;
        }
        public byte[] FinalPAttack(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(69, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(3000, 52, buffer);
            return buffer;
        }
        public byte[] FinalMAttack(Entity attacker)
        {
            byte[] buffer = new byte[96];
            Writer.WriteUInt16(88, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(attacker.UID, 8, buffer);
            Writer.WriteUInt32(2, 12, buffer);
            Writer.WriteInt32(25, 16, buffer);
            Writer.WriteInt32(2, 37, buffer);
            Writer.WriteInt32(70, 40, buffer);
            Writer.WriteInt32(137, 44, buffer);
            Writer.WriteInt32(80, 48, buffer);
            Writer.WriteInt32(3000, 52, buffer);
            return buffer;
        }
        public void Append(byte type, uint Flag, uint Time, uint Dmg, uint Level)
        {
            UpdateCount = UpdateCount + 1;
            ushort offset = (ushort)(16 + (UpdateCount - 1) * 32);
            WriteUInt32(type, offset, Buffer);
            WriteUInt32(Flag, (ushort)(offset + 4), Buffer);
            WriteUInt32(Time, (ushort)(offset + 8), Buffer);
            WriteUInt32(Dmg, (ushort)(offset + 12), Buffer);
            WriteUInt32(Level, (ushort)(offset + 16), Buffer);
        }
        /* public void xAzureShield(Entity Entity, int Dmg, byte SpellLevel, byte Time_)
         {
             byte[] buffer = new byte[84 + 4 + 8];
             Writer.WriteUInt16(84 + 4, 0, buffer);
             Writer.WriteUInt16(10017, 2, buffer);
             Writer.WriteUInt32(Entity.UID, 4 + 4, buffer);
             Writer.WriteUInt32(2, 8 + 4, buffer);
             Writer.WriteInt32(25, 12 + 4, buffer);
             Writer.WriteUInt16(2, 20 + 4, buffer);
             Writer.WriteByte(0x20, 27 + 4, buffer);
             Writer.WriteUInt64(Entity.StatusFlag, 16 + 4, buffer);
             Writer.WriteUInt64(Entity.StatusFlag2, 16 + 4 + 8, buffer);
             Writer.WriteUInt32(0x31, 36 + 4, buffer);
             Writer.WriteUInt32(93, 40 + 4, buffer);
             Writer.WriteInt32(Time_, 44 + 4, buffer);
             Writer.WriteInt32(Dmg, 48 + 4, buffer);
             Writer.WriteUInt32(SpellLevel, 52 + 4, buffer);
             if (Kernel.GamePool.ContainsKey(Entity.UID))
             {
                 Kernel.GamePool[Entity.UID].Send(buffer);
             }
         }*/
        public void xSoulShackle(Entity Entity, int Dmg, byte SpellLevel, byte Time_)
        {
            byte[] buffer = new byte[84 + 4 + 8];
            Writer.WriteUInt16(84 + 4, 0, buffer);
            Writer.WriteUInt16(10017, 2, buffer);
            Writer.WriteUInt32(Entity.UID, 4 + 4, buffer);
            Writer.WriteUInt32(2, 8 + 4, buffer);
            Writer.WriteInt32(25, 12 + 4, buffer);
            Writer.WriteUInt64(Entity.StatusFlag, 16 + 4, buffer);
            Writer.WriteUInt64(Entity.StatusFlag2, 16 + 4 + 8, buffer);
            Writer.WriteUInt32(0x80, 29 + 4, buffer);
            Writer.WriteUInt32(54, 36 + 4, buffer);
            Writer.WriteUInt32(111, 40 + 4, buffer);
            Writer.WriteInt32(Time_, 44 + 4, buffer);
            if (Kernel.GamePool.ContainsKey(Entity.UID))
            {
                Kernel.GamePool[Entity.UID].Send(buffer);
            }
        }
        public enum AuraDataTypes
        {
            Add = 3,
            Remove = 2
        }
        public enum AuraType
        {
            EarthAura = 7,
            FendAura = 2,
            FireAura = 6,
            MagicDefender = 8,
            MetalAura = 3,
            TyrantAura = 1,
            WaterAura = 5,
            WoodAura = 4
        }
        public void Aura(Entity Entity, AuraDataTypes state, AuraType AuraType, Database.SpellInformation spell)
        {
            Aura(Entity, state, AuraType, spell.Level, spell.Power);
        }
        public void Aura(Entity Entity, AuraDataTypes state, AuraType AuraType, uint Level, uint Power)
        {
            //Update update = new Update(true);
            //update.UID = Entity.UID;
            //update.Append(52, 1320);
            //Entity.Owner.Send(update);

            byte[] buffer = new byte[40];
            Writer.WriteUInt16(32, 0, buffer);
            Writer.WriteUInt16(2410, 2, buffer);
            Writer.WriteUInt32((uint)state, 8, buffer);
            Writer.WriteUInt32(Entity.UID, 12, buffer);
            Writer.WriteUInt32((uint)AuraType, 16, buffer);
            Writer.WriteUInt32(Level, 20, buffer);
            Writer.WriteUInt32(Power, 24, buffer);
            Writer.WriteUInt32(Power, 28, buffer);

            if (Kernel.GamePool.ContainsKey(Entity.UID))
            {
                Kernel.GamePool[Entity.UID].Send(buffer);
            }
        }
    }
}