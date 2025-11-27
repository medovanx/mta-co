// * Created by CptSky
// * Copyright © 2011
// * COPS v6 Emulator - Project

using System;
using CO2_CORE_DLL.IO;
using MTA.Network.GamePackets;
using MTA.Game;
using MTA.Database;
using MTA.Game.Attacking;
using System.Collections.Generic;

namespace MTA
{
    public partial class MyMath
    {
        public static Int32 GetDamageEntity2Environment(Entity Attacker, SobNpcSpawn sobnpc, uint AtkType, ref Attack Packet)
        {
            Double Damage = 0;

            switch (AtkType)
            {
                case Attack.Melee:
                    {
                        Damage = MyMath.Generate(Attacker.MinAttack, Attacker.MaxAttack);
                        //  Damage += Attacker.getFan(false);
                        //  Damage -= sobnpc.Defence;
                        break;
                    }
                case Attack.Magic:
                    {
                        Damage = Attacker.MagicAttack;
                        //   Damage += Attacker.getFan(true);
                        //   Damage *= ((Double)(100 - sobnpc.mag) / 100);
                        //  Damage -= sobnpc.Block;
                        Damage *= 0.75;
                        break;
                    }
                case Attack.Ranged:
                    {
                        Damage = MyMath.Generate(Attacker.MinAttack, Attacker.MaxAttack);
                        //  Damage += Attacker.getFan(false);
                        break;
                    }
            }
            MTA.Game.Attacking.Calculate.Refinary(Attacker, sobnpc, ref Damage, ref Packet, AtkType == Attack.Melee);
            Damage = Attacker.AdjustAttack((int)Damage) * 14.8;

            Damage += Attacker.getFan(AtkType == Attack.Magic);
            if (Damage < 1)
                Damage = 1;



            //if (Attacker.EntityFlag == EntityFlag.Player)
            //{
            //    if (Attacker.Owner.BlessTime > 0 && MyMath.Success(10))
            //    {
            //        Damage *= 2;
            //        _String str = new _String(true);
            //        str.UID = Attacker.UID;
            //        str.TextsCount = 1;
            //        str.Type = _String.Effect;
            //        str.Texts.Add("LuckyGuy");
            //        Attacker.Owner.SendScreen(str, true);

            //    }
            //}
            return (Int32)Math.Round(Damage, 0);
        }

        public static Int32 GetDamageEntity2Environment(Entity Attacker, SobNpcSpawn sobnpc, ushort MagicType, Byte MagicLevel, ref Attack Packet)
        {
            var Info = SpellTable.GetSpell(MagicType, MagicLevel);

            Double Damage = 0;

            List<ushort> spells = new List<ushort>()
            {
                11190,
                10315
            };
            if (Info.ID == 1115 || spells.Contains(Info.ID) || (Info.WeaponSubtype.Count > 0 && !Info.WeaponSubtype.Contains(500) && !Info.WeaponSubtype.Contains(613)))
            {
                Damage = MyMath.Generate(Attacker.MinAttack, Attacker.MaxAttack);
                //      Damage += Attacker.getFan(false);
                if (Info.Power > 30000)
                    Damage *= (Double)(Info.Power - 30000) / 100;
                else
                    Damage += Info.Power;
                MTA.Game.Attacking.Calculate.Refinary(Attacker, sobnpc, ref Damage, ref Packet, true);
                // Damage -= sobnpc.Defence;
            }
            else if (Info.WeaponSubtype.Contains(500) || Info.WeaponSubtype.Contains(613))
            {
                Damage = MyMath.Generate(Attacker.MinAttack, Attacker.MaxAttack);
                //   Damage += Attacker.getFan(false);
                if (Info.Power > 30000)
                    Damage *= (Double)(Info.Power - 30000) / 100;
                else
                    Damage += Info.Power;
                MTA.Game.Attacking.Calculate.Refinary(Attacker, sobnpc, ref Damage, ref Packet, true);
            }
            else
            {
                Damage = Attacker.MagicAttack;
                //  Damage += Attacker.getFan(true);
                if (Info.Power > 30000)
                    Damage *= (Double)(Info.Power - 30000) / 100;
                else
                    Damage += Info.Power;
                MTA.Game.Attacking.Calculate.Refinary(Attacker, sobnpc, ref Damage, ref Packet, false);
                //  Damage *= ((Double)(100 - sobnpc.MagicDefence) / 100);
                //  Damage -= sobnpc.Block;
            }

            Damage += Attacker.getFan(Packet.AttackType == Attack.Magic);

            if (Damage < 1)
                Damage = 1;



            //if (Attacker.EntityFlag == EntityFlag.Player)
            //{
            //    if (Attacker.Owner.BlessTime > 0 && MyMath.Success(10))
            //    {
            //        Damage *= 2;
            //        _String str = new _String(true);
            //        str.UID = Attacker.UID;
            //        str.TextsCount = 1;
            //        str.Type = _String.Effect;
            //        str.Texts.Add("LuckyGuy");
            //        Attacker.Owner.SendScreen(str, true);

            //    }
            //}

            return (Int32)Math.Round(Damage, 0);
        }
    }
}
