using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Reincarnation
{
    public class Handle
    {
        public static void Hash(Client.GameState client)
        {
            if (Kernel.ReincarnatedCharacters.ContainsKey(client.Entity.UID))
            {
                if (client.Entity.Level >= 110 && client.Entity.Reborn >= 2)
                {
                    ushort stats = 0;
                    uint lev1 = client.Entity.Level;
                    ReincarnateInfo info = Kernel.ReincarnatedCharacters[client.Entity.UID];
                    client.Entity.Level = info.Level;
                    client.Entity.Experience = info.Experience;
                    Kernel.ReincarnatedCharacters.Remove(info.UID);
                    Database.ReincarnationTable.RemoveReincarnated(client.Entity);
                    stats = (ushort)(((client.Entity.Level - lev1) * 3) - 3);
                    client.Entity.Atributes += stats;
                }
            }
        }
    }

    #region Reincarnation

    public class Reincarnation
    {
        private Client.GameState _client;
        private SafeDictionary<ushort, MTA.Interfaces.ISkill> RemoveSkill = null;
        private SafeDictionary<ushort, MTA.Interfaces.ISkill> Addskill = null;

        public Reincarnation(Client.GameState client, byte new_class)
        {
            if (client.Entity.Level < 130)
                return;
            _client = client;
            RemoveSkill = new SafeDictionary<ushort, MTA.Interfaces.ISkill>(500);
            Addskill = new SafeDictionary<ushort, MTA.Interfaces.ISkill>(500);

            #region Low level items

            for (byte i = 1; i < 9; i++)
            {
                if (i != 7)
                {
                    ConquerItem item = client.Equipment.TryGetItem(i);
                    if (item != null && item.ID != 0)
                    {
                        try
                        {
                            //client.UnloadItemStats(item, false);
                            Database.ConquerItemInformation cii =
                                new MTA.Database.ConquerItemInformation(item.ID, item.Plus);
                            item.ID =
                                cii.LowestID(
                                    Network.PacketHandler.ItemMinLevel(Network.PacketHandler.ItemPosition(item.ID)));
                            item.Mode = MTA.Game.Enums.ItemMode.Update;
                            item.Send(client);
                            client.LoadItemStats();
                            Database.ConquerItemTable.UpdateItemID(item, client);
                        }
                        catch
                        {
                            Console.WriteLine("Reborn item problem: " + item.ID);
                        }
                    }
                }
            }
            ConquerItem hand = client.Equipment.TryGetItem(5);
            if (hand != null)
            {
                client.Equipment.Remove(5);
                client.CalculateStatBonus();
                client.CalculateHPBonus();
                client.SendStatMessage();
            }
            else
                //client.Screen.send(client.Entity.SpawnPacket, false);
            #endregion

            #region Remove Extra Skill

                if (client.Entity.FirstRebornClass == 15 && client.Entity.SecondRebornClass == 15 &&
                    client.Entity.Class == 15)
            {
                WontAdd(MTA.Game.Enums.SkillIDs.DragonWhirl);
            }
            if (client.Entity.FirstRebornClass == 85 && client.Entity.SecondRebornClass == 85 &&
                client.Entity.Class == 85)
            {
                WontAdd(MTA.Game.Enums.SkillIDs.DragonFury);
            }
            if (client.Entity.FirstRebornClass == 25 && client.Entity.SecondRebornClass == 25 &&
                client.Entity.Class == 25)
            {
                WontAdd(MTA.Game.Enums.SkillIDs.Perseverance);
            }
            if (client.Entity.FirstRebornClass == 45 && client.Entity.SecondRebornClass == 45 &&
                client.Entity.Class == 45)
            {
                WontAdd(MTA.Game.Enums.SkillIDs.StarFranko);
            }
            if (client.Entity.FirstRebornClass == 55 && client.Entity.SecondRebornClass == 55 &&
                client.Entity.Class == 55)
            {
                WontAdd(MTA.Game.Enums.SkillIDs.PoisonStar);
            }
            if (client.Entity.FirstRebornClass == 65 && client.Entity.SecondRebornClass == 65 &&
                client.Entity.Class == 65)
            {
                WontAdd(MTA.Game.Enums.SkillIDs.soulshackle);
            }
            if (client.Entity.FirstRebornClass == 135 && client.Entity.SecondRebornClass == 135 &&
                client.Entity.Class == 135)
            {
                WontAdd(MTA.Game.Enums.SkillIDs.AzureShield);
            }
            if (client.Entity.FirstRebornClass == 145 && client.Entity.SecondRebornClass == 145 &&
                client.Entity.Class == 145)
            {
                WontAdd(MTA.Game.Enums.SkillIDs.HeavenBlade);
            }

            #endregion

            Database.ReincarnationTable.NewReincarnated(client.Entity);
            Game.Features.Reincarnation.ReincarnateInfo info = new Game.Features.Reincarnation.ReincarnateInfo();
            info.UID = client.Entity.UID;
            info.Level = client.Entity.Level;
            info.Experience = client.Entity.Experience;
            Kernel.ReincarnatedCharacters.Add(info.UID, info);
            client.Entity.FirstRebornClass = client.Entity.SecondRebornClass;
            client.Entity.SecondRebornClass = client.Entity.Class;
            client.Entity.Class = new_class;
            client.Entity.SecondRebornLevel = client.Entity.Level;
            client.Entity.Level = 15;
            client.Entity.Experience = 0;
            client.Entity.Atributes =
                (ushort)(client.ExtraAtributePoints(client.Entity.FirstRebornClass, client.Entity.FirstRebornLevel) +
                          client.ExtraAtributePoints(client.Entity.SecondRebornClass, client.Entity.SecondRebornLevel) +
                          62);


            client.Spells.Clear();
            client.Spells = new SafeDictionary<ushort, MTA.Interfaces.ISkill>(100);
            switch (client.Entity.FirstRebornClass)
            {
                case 15:
                    {
                        Add(MTA.Game.Enums.SkillIDs.Cyclone);
                        Add(MTA.Game.Enums.SkillIDs.Hercules);
                        Add(MTA.Game.Enums.SkillIDs.SpiritHealing);
                        Add(MTA.Game.Enums.SkillIDs.Robot);
                        Add(MTA.Game.Enums.SkillIDs.SuperCyclone);
                        Add(MTA.Game.Enums.SkillIDs.FatalCross);
                        Add(MTA.Game.Enums.SkillIDs.MortalStrike);
                        Add(MTA.Game.Enums.SkillIDs.BreathFocus);
                        break;
                    }
                case 25:
                    {
                        Add(MTA.Game.Enums.SkillIDs.SuperMan);
                        Add(MTA.Game.Enums.SkillIDs.Dash);
                        Add(MTA.Game.Enums.SkillIDs.Shield);
                        break;
                    }
                case 45:
                    {
                        Add(MTA.Game.Enums.SkillIDs.Intensify);
                        Add(MTA.Game.Enums.SkillIDs.Scatter);
                        Add(MTA.Game.Enums.SkillIDs.RapidFire);
                        Add(MTA.Game.Enums.SkillIDs.XPFly);
                        Add(MTA.Game.Enums.SkillIDs.AdvancedFly);
                        break;
                    }
                case 55:
                    {
                        Add(MTA.Game.Enums.SkillIDs.FatalStrike);
                        Add(MTA.Game.Enums.SkillIDs.ShurikenVortex);
                        Add(MTA.Game.Enums.SkillIDs.ToxicFog);
                        Add(MTA.Game.Enums.SkillIDs.TwofoldBlades);
                        Add(MTA.Game.Enums.SkillIDs.PoisonStar);
                        Add(MTA.Game.Enums.SkillIDs.TwilightDance);
                        Add(MTA.Game.Enums.SkillIDs.SuperTwofoldBlade);
                        Add(MTA.Game.Enums.SkillIDs.FatalSpin);

                        break;
                    }
                case 65:
                    {
                        Add(MTA.Game.Enums.SkillIDs.RadiantPalm);
                        Add(MTA.Game.Enums.SkillIDs.WhirlWindKick);
                        Add(MTA.Game.Enums.SkillIDs.TripleAttack);
                        Add(MTA.Game.Enums.SkillIDs.Oblivion);
                        Add(MTA.Game.Enums.SkillIDs.Serenity);
                        Add(MTA.Game.Enums.SkillIDs.Compassion);
                        Add(MTA.Game.Enums.SkillIDs.TyrantAura);
                        Add(MTA.Game.Enums.SkillIDs.TyrantAura);
                        Add(MTA.Game.Enums.SkillIDs.DeflectionAura);
                        break;
                    }
                case 75:
                    {
                        Add(MTA.Game.Enums.SkillIDs.RadiantPalm);
                        Add(MTA.Game.Enums.SkillIDs.WhirlWindKick);
                        Add(MTA.Game.Enums.SkillIDs.TripleAttack);
                        Add(MTA.Game.Enums.SkillIDs.Oblivion);
                        Add(MTA.Game.Enums.SkillIDs.Serenity);
                        Add(MTA.Game.Enums.SkillIDs.Compassion);
                        Add(MTA.Game.Enums.SkillIDs.TyrantAura);
                        Add(MTA.Game.Enums.SkillIDs.TyrantAura);
                        Add(MTA.Game.Enums.SkillIDs.DeflectionAura);
                        break;
                    }
                case 85:
                    {
                        Add(MTA.Game.Enums.SkillIDs.SpeedKick);
                        Add(MTA.Game.Enums.SkillIDs.ViolentKick);
                        Add(MTA.Game.Enums.SkillIDs.StormKick);
                        Add(MTA.Game.Enums.SkillIDs.CrackingSwip);
                        Add(MTA.Game.Enums.SkillIDs.SplittingSwipe);
                        Add(MTA.Game.Enums.SkillIDs.DragonSwing);
                        Add(MTA.Game.Enums.SkillIDs.DragonPunch);
                        Add(MTA.Game.Enums.SkillIDs.DragonSlash);
                        Add(MTA.Game.Enums.SkillIDs.DragonFlow);
                        Add(MTA.Game.Enums.SkillIDs.DragonRoar);
                        Add(MTA.Game.Enums.SkillIDs.DragonCyclone);
                        Add(MTA.Game.Enums.SkillIDs.AirKick);
                        Add(MTA.Game.Enums.SkillIDs.AirSweep);
                        Add(MTA.Game.Enums.SkillIDs.AirRaid);
                        break;
                    }
                case 135:
                    {
                        Add(MTA.Game.Enums.SkillIDs.Thunder);
                        Add(MTA.Game.Enums.SkillIDs.WaterElf);
                        Add(MTA.Game.Enums.SkillIDs.Cure);
                        Add(MTA.Game.Enums.SkillIDs.Lightning);
                        Add(MTA.Game.Enums.SkillIDs.Volcano);
                        Add(MTA.Game.Enums.SkillIDs.Pray);
                        Add(MTA.Game.Enums.SkillIDs.AdvancedCure);
                        Add(MTA.Game.Enums.SkillIDs.Meditation);
                        Add(MTA.Game.Enums.SkillIDs.Stigma);
                        break;
                    }
                case 140:
                    {
                        Add(MTA.Game.Enums.SkillIDs.Thunder);
                        Add(MTA.Game.Enums.SkillIDs.Cure);
                        Add(MTA.Game.Enums.SkillIDs.Lightning);
                        Add(MTA.Game.Enums.SkillIDs.Tornado);
                        Add(MTA.Game.Enums.SkillIDs.FireCircle);
                        Add(MTA.Game.Enums.SkillIDs.FireMeteor);
                        Add(MTA.Game.Enums.SkillIDs.FireRing);
                        break;
                    }
            }

            byte PreviousClass = client.Entity.FirstRebornClass;
            byte toClass = (byte)(client.Entity.SecondRebornClass - 4);

            Interfaces.ISkill[] ADD_spells = this.Addskill.Values.ToArray();
            foreach (Interfaces.ISkill skill in ADD_spells)
            {
                skill.Available = true;
                if (!client.Spells.ContainsKey(skill.ID))
                    client.Spells.Add(skill.ID, skill);
            }

            #region Spells

            Interfaces.ISkill[] spells = client.Spells.Values.ToArray();
            foreach (Interfaces.ISkill spell in spells)
            {
                spell.PreviousLevel = spell.Level;
                spell.Level = 0;
                spell.Experience = 0;

                #region Pirate

                if (PreviousClass == 75)
                {
                    if (client.Entity.Class != 71)
                    {
                        switch (spell.ID)
                        {
                            case 10490:
                            case 10415:
                            case 10381:
                                client.RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion

                #region Monk

                if (PreviousClass == 65)
                {
                    if (client.Entity.Class != 61)
                    {
                        switch (spell.ID)
                        {
                            case 10490:
                            case 10415:
                            case 10381:
                                client.RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion

                #region Warrior

                if (PreviousClass == 25)
                {
                    if (client.Entity.Class != 21)
                    {
                        switch (spell.ID)
                        {
                            case 1025:
                                if (client.Entity.Class != 21 && client.Entity.Class != 132)
                                    client.RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion

                #region Ninja

                if (toClass != 51)
                {
                    switch (spell.ID)
                    {
                        case 6010:
                        case 6000:
                        case 6011:
                            client.RemoveSpell(spell);
                            break;
                    }
                }

                #endregion

                #region Trojan

                if (toClass != 11)
                {
                    switch (spell.ID)
                    {
                        case 1115:
                            client.RemoveSpell(spell);
                            break;
                    }
                }

                #endregion

                #region Archer

                if (toClass != 41)
                {
                    switch (spell.ID)
                    {
                        case 8001:
                        case 8000:
                        case 8003:
                        case 9000:
                        case 8002:
                        case 8030:
                            client.RemoveSpell(spell);
                            break;
                    }
                }

                #endregion

                #region WaterTaoist

                if (PreviousClass == 135)
                {
                    if (toClass != 132)
                    {
                        switch (spell.ID)
                        {
                            case 1000:
                            case 1001:
                            case 1010:
                            case 1125:
                            case 1100:
                            case 8030:
                                client.RemoveSpell(spell);
                                break;
                            case 1050:
                            case 1175:
                            case 1170:
                                if (toClass != 142)
                                    client.RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion

                #region FireTaoist

                if (PreviousClass == 145)
                {
                    if (toClass != 142)
                    {
                        switch (spell.ID)
                        {
                            case 1000:
                            case 1001:
                            case 1150:
                            case 1180:
                            case 1120:
                            case 1002:
                            case 1160:
                            case 1165:
                                client.RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion

                if (client.Spells.ContainsKey(spell.ID))
                    if (spell.ID != (ushort)Game.Enums.SkillIDs.Reflect)
                        spell.Send(client);
            }

            #endregion

            Add(MTA.Game.Enums.SkillIDs.Bless);

            Addskill.Clear();
            Addskill = new SafeDictionary<ushort, MTA.Interfaces.ISkill>(100);

            PreviousClass = client.Entity.SecondRebornClass;
            toClass = client.Entity.Class;
            switch (client.Entity.SecondRebornClass)
            {
                case 15:
                    {
                        Add(MTA.Game.Enums.SkillIDs.Robot);
                        Add(MTA.Game.Enums.SkillIDs.Cyclone);
                        Add(MTA.Game.Enums.SkillIDs.Hercules);
                        Add(MTA.Game.Enums.SkillIDs.SpiritHealing);
                        Add(MTA.Game.Enums.SkillIDs.SuperCyclone);
                        Add(MTA.Game.Enums.SkillIDs.FatalCross);
                        Add(MTA.Game.Enums.SkillIDs.MortalStrike);
                        Add(MTA.Game.Enums.SkillIDs.BreathFocus);
                        break;
                    }
                case 25:
                    {
                        Add(MTA.Game.Enums.SkillIDs.SuperMan);
                        Add(MTA.Game.Enums.SkillIDs.Dash);
                        Add(MTA.Game.Enums.SkillIDs.Shield);
                        break;
                    }
                case 45:
                    {
                        Add(MTA.Game.Enums.SkillIDs.Intensify);
                        Add(MTA.Game.Enums.SkillIDs.Scatter);
                        Add(MTA.Game.Enums.SkillIDs.RapidFire);
                        Add(MTA.Game.Enums.SkillIDs.XPFly);
                        Add(MTA.Game.Enums.SkillIDs.AdvancedFly);
                        break;
                    }
                case 55:
                    {
                        Add(MTA.Game.Enums.SkillIDs.FatalStrike);
                        Add(MTA.Game.Enums.SkillIDs.ShurikenVortex);
                        Add(MTA.Game.Enums.SkillIDs.ToxicFog);
                        Add(MTA.Game.Enums.SkillIDs.TwofoldBlades);
                        Add(MTA.Game.Enums.SkillIDs.TwilightDance);
                        Add(MTA.Game.Enums.SkillIDs.SuperTwofoldBlade);
                        Add(MTA.Game.Enums.SkillIDs.FatalSpin);
                        break;
                    }
                case 65:
                    {
                        Add(MTA.Game.Enums.SkillIDs.RadiantPalm);
                        Add(MTA.Game.Enums.SkillIDs.WhirlWindKick);
                        Add(MTA.Game.Enums.SkillIDs.TripleAttack);
                        Add(MTA.Game.Enums.SkillIDs.Oblivion);
                        Add(MTA.Game.Enums.SkillIDs.Serenity);
                        Add(MTA.Game.Enums.SkillIDs.Compassion);
                        Add(MTA.Game.Enums.SkillIDs.TyrantAura);
                        Add(MTA.Game.Enums.SkillIDs.TyrantAura);
                        Add(MTA.Game.Enums.SkillIDs.DeflectionAura);
                        break;
                    }
                case 75:
                    {
                        Add(MTA.Game.Enums.SkillIDs.RadiantPalm);
                        Add(MTA.Game.Enums.SkillIDs.WhirlWindKick);
                        Add(MTA.Game.Enums.SkillIDs.TripleAttack);
                        Add(MTA.Game.Enums.SkillIDs.Oblivion);
                        Add(MTA.Game.Enums.SkillIDs.Serenity);
                        Add(MTA.Game.Enums.SkillIDs.Compassion);
                        Add(MTA.Game.Enums.SkillIDs.TyrantAura);
                        Add(MTA.Game.Enums.SkillIDs.TyrantAura);
                        Add(MTA.Game.Enums.SkillIDs.DeflectionAura);
                        break;
                    }
                case 85:
                    {
                        Add(MTA.Game.Enums.SkillIDs.SpeedKick);
                        Add(MTA.Game.Enums.SkillIDs.ViolentKick);
                        Add(MTA.Game.Enums.SkillIDs.StormKick);
                        Add(MTA.Game.Enums.SkillIDs.CrackingSwip);
                        Add(MTA.Game.Enums.SkillIDs.SplittingSwipe);
                        Add(MTA.Game.Enums.SkillIDs.DragonSwing);
                        Add(MTA.Game.Enums.SkillIDs.DragonPunch);
                        Add(MTA.Game.Enums.SkillIDs.DragonSlash);
                        Add(MTA.Game.Enums.SkillIDs.DragonFlow);
                        Add(MTA.Game.Enums.SkillIDs.DragonCyclone);
                        Add(MTA.Game.Enums.SkillIDs.AirKick);
                        Add(MTA.Game.Enums.SkillIDs.AirSweep);
                        Add(MTA.Game.Enums.SkillIDs.AirRaid);
                        break;
                    }
                case 135:
                    {
                        Add(MTA.Game.Enums.SkillIDs.Thunder);
                        Add(MTA.Game.Enums.SkillIDs.WaterElf);
                        Add(MTA.Game.Enums.SkillIDs.Cure);
                        Add(MTA.Game.Enums.SkillIDs.Lightning);
                        Add(MTA.Game.Enums.SkillIDs.Volcano);
                        Add(MTA.Game.Enums.SkillIDs.Pray);
                        Add(MTA.Game.Enums.SkillIDs.Stigma);
                        Add(MTA.Game.Enums.SkillIDs.AdvancedCure);
                        Add(MTA.Game.Enums.SkillIDs.Meditation);
                        break;
                    }
                case 140:
                    {
                        Add(MTA.Game.Enums.SkillIDs.Thunder);
                        Add(MTA.Game.Enums.SkillIDs.Cure);
                        Add(MTA.Game.Enums.SkillIDs.Lightning);
                        Add(MTA.Game.Enums.SkillIDs.Tornado);
                        Add(MTA.Game.Enums.SkillIDs.FireCircle);
                        Add(MTA.Game.Enums.SkillIDs.FireMeteor);
                        Add(MTA.Game.Enums.SkillIDs.FireRing);
                        break;
                    }
            }

            //PreviousClass = client.Entity.FirstRebornClass;
            //toClass = client.Entity.SecondRebornClass;
            Add(MTA.Game.Enums.SkillIDs.Bless);

            Interfaces.ISkill[] aADD_spells = this.Addskill.Values.ToArray();
            foreach (Interfaces.ISkill skill in aADD_spells)
            {
                skill.Available = true;
                if (!client.Spells.ContainsKey(skill.ID))
                    client.Spells.Add(skill.ID, skill);
            }

            #region Spells

            Interfaces.ISkill[] aspells = client.Spells.Values.ToArray();
            foreach (Interfaces.ISkill aspell in spells)
            {
                aspell.PreviousLevel = aspell.Level;
                aspell.Level = 0;
                aspell.Experience = 0;

                #region Pirate

                if (PreviousClass == 75)
                {
                    if (client.Entity.Class != 71)
                    {
                        switch (aspell.ID)
                        {
                            case 10490:
                            case 10415:
                            case 10381:
                                client.RemoveSpell(aspell);
                                break;
                        }
                    }
                }

                #endregion

                #region Monk

                if (PreviousClass == 65)
                {
                    if (client.Entity.Class != 61)
                    {
                        switch (aspell.ID)
                        {
                            case 10490:
                            case 10415:
                            case 10381:
                                client.RemoveSpell(aspell);
                                break;
                        }
                    }
                }

                #endregion

                #region Warrior

                if (PreviousClass == 25)
                {
                    if (client.Entity.Class != 21)
                    {
                        switch (aspell.ID)
                        {
                            case 1025:
                                if (client.Entity.Class != 21 && client.Entity.Class != 132)
                                    client.RemoveSpell(aspell);
                                break;
                        }
                    }
                }

                #endregion

                #region Ninja

                if (toClass != 51)
                {
                    switch (aspell.ID)
                    {
                        case 6010:
                        case 6000:
                        case 6011:
                            client.RemoveSpell(aspell);
                            break;
                    }
                }

                #endregion

                #region Trojan

                if (toClass != 11)
                {
                    switch (aspell.ID)
                    {
                        case 1115:
                            client.RemoveSpell(aspell);
                            break;
                    }
                }

                #endregion

                #region Archer

                if (toClass != 41)
                {
                    switch (aspell.ID)
                    {
                        case 8001:
                        case 8000:
                        case 8003:
                        case 9000:
                        case 8002:
                        case 8030:
                            client.RemoveSpell(aspell);
                            break;
                    }
                }

                #endregion

                #region WaterTaoist

                if (PreviousClass == 135)
                {
                    if (toClass != 132)
                    {
                        switch (aspell.ID)
                        {
                            case 1000:
                            case 1001:
                            case 1010:
                            case 1125:
                            case 1100:
                            case 8030:
                                client.RemoveSpell(aspell);
                                break;
                            case 1050:
                            case 1175:
                            case 1170:
                                if (toClass != 142)
                                    client.RemoveSpell(aspell);
                                break;
                        }
                    }
                }

                #endregion

                #region FireTaoist

                if (PreviousClass == 145)
                {
                    if (toClass != 142)
                    {
                        switch (aspell.ID)
                        {
                            case 1000:
                            case 1001:
                            case 1150:
                            case 1180:
                            case 1120:
                            case 1002:
                            case 1160:
                            case 1165:
                                client.RemoveSpell(aspell);
                                break;
                        }
                    }
                }

                #endregion

                if (client.Spells.ContainsKey(aspell.ID))
                    if (aspell.ID != (ushort)Game.Enums.SkillIDs.Reflect)
                        aspell.Send(client);
            }

            #endregion

            Addskill.Clear();
            Addskill = new SafeDictionary<ushort, MTA.Interfaces.ISkill>(20);

            #region Add Extra Skill

            if (client.Entity.FirstRebornClass == 15 && client.Entity.SecondRebornClass == 15 &&
                client.Entity.Class == 11)
            {
                Add(MTA.Game.Enums.SkillIDs.DragonWhirl);
            }
            if (client.Entity.FirstRebornClass == 85 && client.Entity.SecondRebornClass == 85 &&
                client.Entity.Class == 81)
            {
                Add(MTA.Game.Enums.SkillIDs.DragonFury);
            }
            if (client.Entity.FirstRebornClass == 25 && client.Entity.SecondRebornClass == 25 &&
                client.Entity.Class == 21)
            {
                Add(MTA.Game.Enums.SkillIDs.Perseverance);
            }
            if (client.Entity.FirstRebornClass == 45 && client.Entity.SecondRebornClass == 45 &&
                client.Entity.Class == 41)
            {
                Add(MTA.Game.Enums.SkillIDs.StarFranko);
            }
            if (client.Entity.FirstRebornClass == 55 && client.Entity.SecondRebornClass == 55 &&
                client.Entity.Class == 55)
            {
                Add(MTA.Game.Enums.SkillIDs.PoisonStar);
                Add(MTA.Game.Enums.SkillIDs.CounterKill);
            }
            if (client.Entity.FirstRebornClass == 65 && client.Entity.SecondRebornClass == 65 &&
                client.Entity.Class == 61)
            {
                Add(MTA.Game.Enums.SkillIDs.soulshackle);
            }
            if (client.Entity.FirstRebornClass == 135 && client.Entity.SecondRebornClass == 135 &&
                client.Entity.Class == 132)
            {
                Add(MTA.Game.Enums.SkillIDs.AzureShield);
            }
            if (client.Entity.FirstRebornClass == 145 && client.Entity.SecondRebornClass == 145 &&
                client.Entity.Class == 142)
            {
                Add(MTA.Game.Enums.SkillIDs.HeavenBlade);
            }

            #endregion

            Interfaces.ISkill[] aaADD_spells = this.Addskill.Values.ToArray();
            foreach (Interfaces.ISkill skill in aaADD_spells)
            {
                skill.Available = true;
                if (!client.Spells.ContainsKey(skill.ID))
                    client.Spells.Add(skill.ID, skill);
            }

            #region Proficiencies

            foreach (Interfaces.ISkill proficiency in client.Proficiencies.Values)
            {
                proficiency.PreviousLevel = proficiency.Level;
                proficiency.Level = 0;
                proficiency.Experience = 0;
                proficiency.Send(client);
            }

            #endregion

            Database.DataHolder.GetStats(client.Entity.Class, client.Entity.Level, client);
            client.CalculateStatBonus();
            client.CalculateHPBonus();
            client.GemAlgorithm();
            client.SendStatMessage();
            Network.PacketHandler.WorldMessage(client.Entity.Name + " has got Reincarnation! Congratulations!");
        }

        private void Add(MTA.Game.Enums.SkillIDs S)
        {
            Interfaces.ISkill New = new Network.GamePackets.Spell(true);
            New.ID = (ushort)S;
            New.Level = 0;
            New.Experience = 0;
            New.PreviousLevel = 0;
            New.Send(_client);
            Addskill.Add(New.ID, New);
        }

        private void WontAdd(MTA.Game.Enums.SkillIDs S)
        {
            Network.GamePackets.Data data = new Data(true);
            data.UID = _client.Entity.UID;
            data.dwParam = (byte)S;
            data.ID = 109;
            data.Send(_client);

            Interfaces.ISkill New = new Network.GamePackets.Spell(true);
            New.ID = (ushort)S;
            New.Level = 0;
            New.Experience = 0;
            New.PreviousLevel = 0;
            RemoveSkill.Add(New.ID, New);
        }
    }

    #endregion

    public class Reincarnate
    {
        public Game.ConquerStructures.Inventory Inventory;
        public Game.ConquerStructures.Equipment Equipment;
        public Entity Entity;
        public byte Class;
        public byte First;
        public byte Second;
        private Interfaces.ISkill[] Skills;
        private Interfaces.IProf[] Profs;
        private Dictionary<ushort, Interfaces.ISkill> Learn = null;
        private Dictionary<ushort, Interfaces.ISkill> WontLearn = null;
        //Client.GameState[] States;

        public Reincarnate(Entity _Entity, byte _class)
        {
            Entity = _Entity;
            Class = _class;
            First = Entity.SecondRebornClass;
            Second = Entity.Class;
            Learn = new Dictionary<ushort, Interfaces.ISkill>();
            WontLearn = new Dictionary<ushort, Interfaces.ISkill>();
            Skills = new Interfaces.ISkill[Entity.Owner.Spells.Values.Count];
            Entity.Owner.Spells.Values.CopyTo(Skills, 0);
            Profs = new Interfaces.IProf[Entity.Owner.Proficiencies.Values.Count];
            Entity.Owner.Proficiencies.Values.CopyTo(Profs, 0);
            //States = new Client.GameState[Program.SafeReturn().Count];
            //Program.SafeReturn().Values.CopyTo(States, 0);
            doIt();
        }

        private void doIt()
        {
            #region Reincarnate

            Database.ReincarnationTable.NewReincarnated(Entity);
            Game.Features.Reincarnation.ReincarnateInfo info = new Game.Features.Reincarnation.ReincarnateInfo();
            info.UID = Entity.UID;
            info.Level = Entity.Level;
            info.Experience = Entity.Experience;
            Kernel.ReincarnatedCharacters.Add(info.UID, info);
            Entity.FirstRebornClass = First;
            Entity.SecondRebornClass = Second;
            Entity.Class = Class;
            Entity.Atributes = 182;
            Entity.Level = 15;
            Entity.Experience = 0;

            #endregion

            #region Low level items

            for (byte i = 1; i < 9; i++)
            {
                if (i != 7)
                {
                    ConquerItem item = Entity.Owner.Equipment.TryGetItem(i);
                    if (item != null && item.ID != 0)
                    {
                        try
                        {
                            Database.ConquerItemInformation cii =
                                new MTA.Database.ConquerItemInformation(item.ID, item.Plus);
                            item.ID =
                                cii.LowestID(
                                    Network.PacketHandler.ItemMinLevel(Network.PacketHandler.ItemPosition(item.ID)));
                            item.Mode = MTA.Game.Enums.ItemMode.Update;
                            item.Send(Entity.Owner);
                            Database.ConquerItemTable.UpdateItemID(item, Entity.Owner);
                        }
                        catch
                        {
                            Console.WriteLine("Reborn item problem: " + item.ID);
                        }
                    }
                }
            }
            Entity.Owner.LoadItemStats();
            ConquerItem hand = Entity.Owner.Equipment.TryGetItem(5);
            if (hand != null)
            {
                Entity.Owner.Equipment.Remove(5);
                CalculateStatBonus();
                CalculateHPBonus();
                Entity.Owner.Screen.Reload(null);
            }
            else
                Entity.Owner.SendScreen(Entity.Owner.Entity.SpawnPacket, false);

            #endregion

            foreach (Interfaces.ISkill s in Skills)
                Entity.Owner.Spells.Remove(s.ID);

            switch (First)
            {
                #region KungFuKing
                case 85:
                    {
                        switch (Second)
                        {
                            case 81: // ana zh2t we da5el anam lma tegy a3melha anta
                                //Add(Enums.SkillIDs.SpeedKick);
                                //Add(Enums.SkillIDs.ViolentKick);
                                //Add(Enums.SkillIDs.StormKick);
                                //Add(Enums.SkillIDs.CrackingSwip);
                                //Add(Enums.SkillIDs.SplittingSwipe);
                                //Add(Enums.SkillIDs.DragonSwing);
                                //Add(Enums.SkillIDs.DragonPunch);
                                //Add(Enums.SkillIDs.DragonSlash);
                                //Add(Enums.SkillIDs.DragonFlow);
                                Add(Enums.SkillIDs.DragonRoar);
                                //Add(Enums.SkillIDs.DragonCyclone);
                                //Add(Enums.SkillIDs.AirKick);
                                //Add(Enums.SkillIDs.AirSweep);
                                //Add(Enums.SkillIDs.AirRaid);
                                break;
                            default:
                                //WontAdd(Enums.SkillIDs.SpeedKick);
                                //WontAdd(Enums.SkillIDs.ViolentKick);
                                //WontAdd(Enums.SkillIDs.StormKick);
                                //WontAdd(Enums.SkillIDs.CrackingSwip);
                                //WontAdd(Enums.SkillIDs.SplittingSwipe);
                                //WontAdd(Enums.SkillIDs.DragonSwing);
                                //WontAdd(Enums.SkillIDs.DragonPunch);
                                //WontAdd(Enums.SkillIDs.DragonSlash);
                                //WontAdd(Enums.SkillIDs.DragonFlow);
                                ////WontAdd(Enums.SkillIDs.DragonRoar);
                                //WontAdd(Enums.SkillIDs.DragonCyclone);
                                //WontAdd(Enums.SkillIDs.AirKick);
                                //WontAdd(Enums.SkillIDs.AirSweep);
                                //WontAdd(Enums.SkillIDs.AirRaid);
                                break;
                        }
                        break;
                    }
                #endregion
                #region Trojan

                case 15:
                    {
                        switch (Second)
                        {
                            case 11:
                                Add(Enums.SkillIDs.CruelShade);
                                break;
                            default:
                                WontAdd(Enums.SkillIDs.Accuracy);
                                break;
                        }
                        break;
                    }

                #endregion

                #region Warrior

                case 25:
                    {
                        switch (Second)
                        {
                            case 11:
                                Add(Enums.SkillIDs.IronShirt);
                                WontAdd(Enums.SkillIDs.Shield);
                                WontAdd(Enums.SkillIDs.SuperMan);
                                break;
                            case 21:
                                Add(Enums.SkillIDs.Reflect);
                                break;
                            case 132:
                                WontAdd(Enums.SkillIDs.FlyingMoon);
                                WontAdd(Enums.SkillIDs.Shield);
                                break;
                            case 142:
                                WontAdd(Enums.SkillIDs.Accuracy);
                                WontAdd(Enums.SkillIDs.FlyingMoon);
                                WontAdd(Enums.SkillIDs.SuperMan);
                                break;
                            default:
                                WontAdd(Enums.SkillIDs.Accuracy);
                                WontAdd(Enums.SkillIDs.FlyingMoon);
                                WontAdd(Enums.SkillIDs.SuperMan);
                                WontAdd(Enums.SkillIDs.Shield);
                                break;
                        }
                        break;
                    }

                #endregion

                #region Archer

                case 45:
                    {
                        switch (Second)
                        {
                            case 41:
                                break;
                            default:
                                WontAdd(Enums.SkillIDs.Scatter);
                                WontAdd(Enums.SkillIDs.XPFly);
                                WontAdd(Enums.SkillIDs.AdvancedFly);
                                WontAdd(Enums.SkillIDs.FrankoRain);
                                WontAdd(Enums.SkillIDs.Intensify);
                                WontAdd(Enums.SkillIDs.RapidFire);
                                break;
                        }
                        break;
                    }

                #endregion

                #region Ninja

                case 55:
                    {
                        switch (Second)
                        {
                            case 51:
                                break;
                            default:
                                WontAdd(Enums.SkillIDs.PoisonStar);
                                WontAdd(Enums.SkillIDs.ShurikenVortex);
                                WontAdd(Enums.SkillIDs.FatalStrike);
                                WontAdd(Enums.SkillIDs.TwofoldBlades);
                                WontAdd(Enums.SkillIDs.ArcherBane);
                                WontAdd(Enums.SkillIDs.TwilightDance);
                                WontAdd(Enums.SkillIDs.SuperTwofoldBlade);
                                WontAdd(Enums.SkillIDs.FatalSpin);
                                break;
                        }
                        break;
                    }

                #endregion

                #region Monk

                case 65:
                    {
                        switch (Second)
                        {
                            case 61:
                                break;
                            default:
                                WontAdd(Enums.SkillIDs.Oblivion);
                                WontAdd(Enums.SkillIDs.RadiantPalm);
                                WontAdd(Enums.SkillIDs.TyrantAura);
                                WontAdd(Enums.SkillIDs.DeathBlow);
                                WontAdd(Enums.SkillIDs.DeflectionAura);
                                WontAdd(Enums.SkillIDs.TripleAttack);
                                break;
                        }
                        break;
                    }

                #endregion

                #region Water

                case 135:
                    {
                        switch (Second)
                        {
                            case 132:
                                Add(Enums.SkillIDs.Pervade);
                                break;
                            case 142:
                                WontAdd(Enums.SkillIDs.Nectar);
                                WontAdd(Enums.SkillIDs.HealingRain);
                                break;
                            default:
                                WontAdd(Enums.SkillIDs.Nectar);
                                WontAdd(Enums.SkillIDs.Lightning);
                                WontAdd(Enums.SkillIDs.Volcano);
                                WontAdd(Enums.SkillIDs.AdvancedCure);
                                WontAdd(Enums.SkillIDs.SpeedLightning);
                                WontAdd(Enums.SkillIDs.HealingRain);
                                break;
                        }
                        break;
                    }

                #endregion

                #region Fire

                case 145:
                    {
                        switch (Second)
                        {
                            case 142:
                                Add(Enums.SkillIDs.Dodge);
                                break;
                            default:
                                if (Second != 132)
                                    WontAdd(Enums.SkillIDs.FireCircle);

                                WontAdd(Enums.SkillIDs.Tornado);
                                WontAdd(Enums.SkillIDs.FireMeteor);
                                WontAdd(Enums.SkillIDs.FireOfHell);
                                WontAdd(Enums.SkillIDs.FireRing);
                                WontAdd(Enums.SkillIDs.Volcano);
                                WontAdd(Enums.SkillIDs.Lightning);
                                WontAdd(Enums.SkillIDs.SpeedLightning);
                                break;
                        }
                        break;
                    }

                    #endregion
            }


            Add(Enums.SkillIDs.Bless);

            #region Re-Learn Profs

            foreach (Interfaces.IProf Prof in Profs)
            {
                if (Prof == null)
                    continue;

                Prof.Available = false;
                Prof.PreviousLevel = Prof.Level;
                Prof.Level = 0;
                Prof.Experience = 0;
                Entity.Owner.Proficiencies.Add(Prof.ID, Prof);
                Prof.Send(Entity.Owner);
            }

            #endregion

            #region Re-Learn Skills

            foreach (Interfaces.ISkill Skill in Skills)
            {
                if (Skill == null)
                    continue;

                Skill.Available = false;
                Skill.PreviousLevel = Skill.Level;
                Skill.Level = 0;
                Skill.Experience = 0;

                if (!WontLearn.ContainsKey(Skill.ID))
                {
                    Entity.Owner.Spells.Add(Skill.ID, Skill);
                    Skill.Send(Entity.Owner);
                }
            }

            #endregion

            #region Learn Skills

            foreach (Interfaces.ISkill L in Learn.Values)
            {
                if (L == null)
                    continue;

                L.Available = false;
                Entity.Owner.Spells.Add(L.ID, L);
                L.Send(Entity.Owner);
            }

            #endregion

            #region Remove Skills

            foreach (Interfaces.ISkill L in WontLearn.Values)
            {
                if (L == null)
                    continue;

                L.Available = false;
                Entity.Owner.Spells.Remove(L.ID);
                Database.SkillTable.DeleteSpell(Entity.Owner, L.ID);
                L.Send(Entity.Owner);
            }

            #endregion

            Database.DataHolder.GetStats(Entity.Class, Entity.Level, Entity.Owner);
            Entity.Owner.CalculateStatBonus();
            Entity.Owner.CalculateHPBonus();
            //Samak Database.SkillTable.SaveSpells(Entity.Owner);
            //Samak Database.SkillTable.SaveProficiencies(Entity.Owner);
            Kernel.SendWorldMessage(
                new MTA.Network.GamePackets.Message(
                    "Congratulations, " + Entity.Name + " reincarnated!", System.Drawing.Color.White,
                    Network.GamePackets.Message.TopLeft), Program.Values);
        }

        private void Add(Enums.SkillIDs S)
        {
            Interfaces.ISkill New = new Network.GamePackets.Spell(true);
            New.ID = (ushort)S;
            New.Level = 0;
            New.Experience = 0;
            New.PreviousLevel = 0;
            Learn.Add(New.ID, New);
        }

        private void WontAdd(Enums.SkillIDs S)
        {
            Interfaces.ISkill New = new Network.GamePackets.Spell(true);
            New.ID = (ushort)S;
            New.Level = 0;
            New.Experience = 0;
            New.PreviousLevel = 0;
            WontLearn.Add(New.ID, New);
        }

        private void WontAdd2(Enums.SkillIDs S)
        {
            Interfaces.ISkill New = new Network.GamePackets.Spell(true);
            New.ID = (ushort)S;
            New.Level = 0;
            New.Experience = 0;
            New.PreviousLevel = 0;
            WontLearn.Add(New.ID, New);
        }

        private int StatHP;

        public void CalculateHPBonus()
        {
            switch (Entity.Class)
            {
                case 11:
                    Entity.MaxHitpoints = (uint)(StatHP * 1.05F);
                    break;
                case 12:
                    Entity.MaxHitpoints = (uint)(StatHP * 1.08F);
                    break;
                case 13:
                    Entity.MaxHitpoints = (uint)(StatHP * 1.10F);
                    break;
                case 14:
                    Entity.MaxHitpoints = (uint)(StatHP * 1.12F);
                    break;
                case 15:
                    Entity.MaxHitpoints = (uint)(StatHP * 1.15F);
                    break;
                default:
                    Entity.MaxHitpoints = (uint)StatHP;
                    break;
            }
            Entity.MaxHitpoints += Entity.ItemHP;
            Entity.Hitpoints = Math.Min(Entity.Hitpoints, Entity.MaxHitpoints);
        }

        public void CalculateStatBonus()
        {
            byte ManaBoost = 5;
            const byte HitpointBoost = 24;
            sbyte Class = (sbyte)(Entity.Class / 10);
            if (Class == 13 || Class == 14)
                ManaBoost += (byte)(5 * (Entity.Class - (Class * 10)));
            StatHP = (ushort)((Entity.Strength * 3) +
                               (Entity.Agility * 3) +
                               (Entity.Spirit * 3) +
                               (Entity.Vitality * HitpointBoost));
            Entity.MaxMana = (ushort)((Entity.Spirit * ManaBoost) + Entity.ItemMP);
            Entity.Mana = Math.Min(Entity.Mana, Entity.MaxMana);
        }

        public void GemAlgorithm()
        {
            Entity.MaxAttack = Entity.Strength + Entity.BaseMaxAttack;
            Entity.MinAttack = Entity.Strength + Entity.BaseMinAttack;
            Entity.MagicAttack = Entity.BaseMagicAttack;
            if (Entity.Gems[0] != 0)
            {
                Entity.MagicAttack += (uint)Math.Floor(Entity.MagicAttack * (double)(Entity.Gems[0] * 0.01));
            }
            if (Entity.Gems[1] != 0)
            {
                Entity.MaxAttack += (uint)Math.Floor(Entity.MaxAttack * (double)(Entity.Gems[1] * 0.01));
                Entity.MinAttack += (uint)Math.Floor(Entity.MinAttack * (double)(Entity.Gems[1] * 0.01));
            }
        }
    }
}