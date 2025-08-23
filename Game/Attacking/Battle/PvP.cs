// * Created by CptSky
// * Copyright © 2011
// * COPS v6 Emulator - Project

using System;
using CO2_CORE_DLL.IO;
using MTA.Game;
using MTA.Network.GamePackets;
using MTA;
using MTA.Database;
using MTA.Game.Attacking;
using System.Collections.Generic;

namespace MTA
{
    public partial class MyMath
    {
        public static Int32 GetDamageEntity2Entity(Entity Attacker, Entity Target, uint AtkType, ref Attack Packet)
        {
            Double Damage = 0;

            Double Reborn = 1.00;
            if (Target.Reborn == 1)
                Reborn -= 0.30; //30%
            else if (Target.Reborn >= 2)
                Reborn -= 0.50; //50%

            Double Dodge = 1.00;
            Dodge -= (Double)Target.Dodge / 1000;
            Dodge += (Double)Target.Weight / 100;

            switch (AtkType)
            {
                case Attack.Melee:
                    {
                        if (!Attacker.Transformed)
                            Damage += MyMath.Generate(Attacker.MinAttack, Attacker.MaxAttack);
                        else
                            Damage = MyMath.Generate(Attacker.TransformationMinAttack, Attacker.TransformationMaxAttack);
                        MTA.Game.Attacking.Calculate.Refinary(Attacker, Target, ref Damage, ref Packet, false);
                        //       Damage += Attacker.getFan(false);

                        var bonus = Attacker.Gems[GemTypes.Dragon];
                        if (bonus > 0)
                            Damage += MathHelper.MulDiv((int)Damage, bonus, 100);


                        Damage = Attacker.AdjustAttack((int)Damage);


                        if (!Attacker.Transformed)
                            Damage -= Target.AdjustDefense(Target.Defence);
                        else
                            Damage -= Target.AdjustDefense(Target.TransformationDefence);


                        //   Damage -= Target.getTower(false);
                        break;
                    }
                case Attack.Magic:
                    {


                        Damage = Attacker.MagicAttack;
                        MTA.Game.Attacking.Calculate.Refinary(Attacker, Target, ref Damage, ref Packet, true);

                        //Damage *= ((Double)(100 - Target.MagicDefence) / 100);
                        //Damage -= Target.Block;
                        //Damage *= 0.75;

                        Damage -= Target.MagicDefence;
                        Damage -= Target.Block;
                        //    Damage *= 0.65;

                        var effect = Attacker.Gems[GemTypes.Phoenix];
                        Damage += Damage * effect / 100;
                        if (effect >= 180)
                            Damage += Damage / 20;

                        break;
                    }
                case Attack.Ranged:
                    {
                        Damage = MyMath.Generate(Attacker.MinAttack, Attacker.MaxAttack);
                        MTA.Game.Attacking.Calculate.Refinary(Attacker, Target, ref Damage, ref Packet, false);
                        //      Damage += Attacker.getFan(false);

                        var bonus = Attacker.Gems[GemTypes.Dragon];
                        if (bonus > 0)
                            Damage += MathHelper.MulDiv((int)Damage, bonus, 100);

                        Damage = Attacker.AdjustAttack((int)Damage);


                        Damage -= Target.AdjustBowDefense(Target.Defence);
                        //  Damage -= Target.getTower(false);
                        Damage *= Dodge;
                        // Damage *= 0.10;
                        //Damage *= 0.12;
                        break;
                    }
            }

            if (AtkType != Attack.Magic)
            {
                if (Attacker.Gems[GemTypes.Dragon] >= 210)
                    Damage += Damage * 50 / 100;
            }

            Damage *= Reborn;
            Damage *= Target.ItemBless;
            double torist = (double)(Target.Gems[GemTypes.Tortoise] / 100d);
            torist = (double)(1 - torist);
            torist = Math.Max(torist, 0.6);
            Damage *= torist;

            Double BattlePower = Attacker.BattlePower - Target.BattlePower;
            BattlePower = Math.Pow(2, BattlePower / 12.0);
            BattlePower = Math.Min(BattlePower, 100.0);
            bool bypass = false;
            if (Target.BattlePower > Attacker.BattlePower)
            {
                if (Attacker.Breaktrough > Target.Counteraction)
                {
                    double Power = (double)(Attacker.Breaktrough - Target.Counteraction);
                    Power = (double)(Power / 10);
                    if (MyMath.Success(Power))
                    {
                        bypass = true;
                        Packet.Effect1 |= Attack.AttackEffects1.Break;
                    }
                }
            }
            if (!bypass)
                Damage *= BattlePower;

            Damage += Attacker.getFan(AtkType == Attack.Magic);
            if (Target.EntityFlag == EntityFlag.Player)
                Damage -= Target.getTower(AtkType == Attack.Magic);
            if (Damage < 1)
                Damage = 1;



            //if (Attacker.EntityFlag == EntityFlag.Player)
            //{
            //    if (Attacker.Owner.BlessTime > 0 && MyMath.Success(5))
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
            //if (Target.EntityFlag == EntityFlag.Player)
            //{
            //    if (Target.Owner.BlessTime > 0 && MyMath.Success(5))
            //    {
            //        Damage = 1;
            //        _String str = new _String(true);
            //        str.UID = Target.UID;
            //        str.TextsCount = 1;
            //        str.Type = _String.Effect;
            //        str.Texts.Add("LuckyGuy");
            //        Target.Owner.SendScreen(str, true);
            //    }
            //}
            return (Int32)Math.Round(Damage, 0);
        }

        public static Int32 GetDamageEntity2Entity(Entity Attacker, Entity Target, ushort MagicType, Byte MagicLevel, ref Attack Packet)
        {
            var Info = SpellTable.GetSpell(MagicType, MagicLevel);


            Double Damage = 0;

            Double Reborn = 1.00;
            if (Target.Reborn == 1)
                Reborn -= 0.30; //30%
            else if (Target.Reborn >= 2)
                Reborn -= 0.50; //50%

            Double Dodge = 1.00;
            Dodge -= (Double)Target.Dodge / 1000;
            Dodge += (Double)Target.Weight / 100;

            List<ushort> spells = new List<ushort>()
            {
                11190,
                11070,
                11030,
                1115,
                6010,
                10315
            };
            bool magic = false;
            if ((spells.Contains(Info.ID) || (Info.WeaponSubtype.Count > 0)) && !Info.WeaponSubtype.Contains(500) && !Info.WeaponSubtype.Contains(613))
            {
                Damage = MyMath.Generate(Attacker.MinAttack, Attacker.MaxAttack);
                MTA.Game.Attacking.Calculate.Refinary(Attacker, Target, ref Damage, ref Packet, false);
                //    Damage += Attacker.getFan(false);                
                double bonus = Attacker.Gems[GemTypes.Dragon];
                if (bonus > 0)
                    Damage += MathHelper.MulDiv((int)Damage, (int)bonus, 100);
                Damage = Attacker.AdjustAttack((int)Damage);

                if (Info.Power > 30000)
                    Damage *= (Double)(Info.Power - 30000) / 100;
                else
                    Damage += Info.Power;






                Damage -= Target.AdjustDefense(Target.Defence);
                //  Damage -= Target.getTower(false);

                if (Attacker.Gems[GemTypes.Dragon] >= 210)
                    Damage += Damage * 50 / 100;


            }
            else if (Info.WeaponSubtype.Contains(500) || Info.WeaponSubtype.Contains(613))
            {
                Damage = MyMath.Generate(Attacker.MinAttack, Attacker.MaxAttack);
                MTA.Game.Attacking.Calculate.Refinary(Attacker, Target, ref Damage, ref Packet, false);

                //    Damage += Attacker.getFan(false);
                if (Info.Power > 30000)
                    Damage *= (Double)(Info.Power - 30000) / 100;
                else
                    Damage += Info.Power;


                Damage *= Dodge;
                Damage *= 0.10;


                //  Damage -= Target.getTower(false);

                Damage = Attacker.AdjustAttack((int)Damage);
                var bonus = Attacker.Gems[GemTypes.Dragon];
                if (bonus > 0)
                    Damage += MathHelper.MulDiv((int)Damage, bonus, 100);

                if (Attacker.Gems[GemTypes.Dragon] >= 210)
                    Damage += Damage * 50 / 100;
                //Damage *= 0.12;
            }
            else
            {
                magic = true;
                Damage = Attacker.MagicAttack;
                MTA.Game.Attacking.Calculate.Refinary(Attacker, Target, ref Damage, ref Packet, true);

                //        Damage += Attacker.getFan(true);

                var effect = Attacker.Gems[GemTypes.Phoenix];
                Damage += Damage * effect / 100;

                if (Info.Power > 30000)
                    Damage *= (Double)(Info.Power - 30000) / 100;
                else
                    Damage += Info.Power;

                //Damage *= ((Double)(100 - Target.MagicDefence) / 100);
                //Damage -= Target.Block;
                //Damage *= 0.75;

                // Damage -= ((Double)(100 - Math.Min((int)Target.MagicDefence, 95)) / 100);
                Damage -= Target.MagicDefence;
                Damage -= Target.Block;
                Damage *= 0.65;


                // Damage -= Target.getTower(true);

                if (effect >= 180)
                    Damage += Damage / 20;
            }


            Damage *= Reborn;
            Damage *= Target.ItemBless;
            double torist = (double)(Target.Gems[GemTypes.Tortoise] / 100d);
            torist = (double)(1 - torist);
            torist = Math.Max(torist, 0.6);
            Damage *= torist;


            Double BattlePower = Attacker.BattlePower - Target.BattlePower;
            BattlePower = Math.Pow(2, BattlePower / 12.0);
            BattlePower = Math.Min(BattlePower, 100.0);
            bool bypass = false;
            if (Target.BattlePower > Attacker.BattlePower)
            {
                if (Attacker.Breaktrough > Target.Counteraction)
                {
                    double Power = (double)(Attacker.Breaktrough - Target.Counteraction);
                    Power = (double)(Power / 10);
                    if (MyMath.Success(Power))
                    {
                        bypass = true;
                        Packet.Effect1 |= Attack.AttackEffects1.Break;
                    }
                }
            }
            if (!bypass)
                Damage *= BattlePower;
            Damage += Attacker.getFan(magic);
            if (Target.EntityFlag == EntityFlag.Player)
                Damage -= Target.getTower(magic);
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
            //if (Target.EntityFlag == EntityFlag.Player)
            //{
            //    if (Target.Owner.BlessTime > 0 && MyMath.Success(10))
            //    {
            //        Damage = 1;
            //        _String str = new _String(true);
            //        str.UID = Target.UID;
            //        str.TextsCount = 1;
            //        str.Type = _String.Effect;
            //        str.Texts.Add("LuckyGuy");
            //        Target.Owner.SendScreen(str, true);
            //    }
            //}
            return (Int32)Math.Round(Damage, 0);
        }
    }
}
