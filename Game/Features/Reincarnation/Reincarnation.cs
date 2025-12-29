using System.Linq;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Reincarnation {
    public class ReincarnateInfo {
        public uint Uid;
        public byte Level;
        public ulong Experience;
    }

    public class Reincarnation {
        public Reincarnation(Client.GameState client, byte newClass) {
            if (client.Entity.Level < 110)
                return;
            if (Kernel.ReincarnatedCharacters.ContainsKey(client.Entity.UID))
                return;
            ReincarnationTable.NewReincarnated(client.Entity);
            var info = new ReincarnateInfo {
                Uid = client.Entity.UID,
                Level = client.Entity.Level,
                Experience = client.Entity.Experience
            };
            Kernel.ReincarnatedCharacters.Add(info.Uid, info);
            client.Entity.FirstRebornClass = client.Entity.SecondRebornClass;
            client.Entity.SecondRebornClass = client.Entity.Class;
            client.Entity.Class = newClass;
            client.Entity.SecondRebornLevel = client.Entity.Level;
            client.Entity.ReincarnationLev = client.Entity.Level;
            client.Entity.Level = 15;
            client.Entity.Experience = 0;
            client.Entity.Strength = 0;
            client.Entity.Vitality = 0;
            client.Entity.Agility = 0;
            client.Entity.Spirit = 0;
            client.Entity.Atributes =
                (ushort)
                (client.ExtraAtributePoints(client.Entity.FirstRebornClass, client.Entity.FirstRebornLevel) +
                 client.ExtraAtributePoints(client.Entity.SecondRebornClass, client.Entity.SecondRebornLevel) +
                 62);
            DataHolder.GetStats(client.Entity.Class, client.Entity.Level, client);
            client.CalculateStatBonus();
            client.CalculateHPBonus();
            client.GemAlgorithm();

            #region RemoveAllSpells

            var spells = client.Spells.Values.ToArray();
            foreach (var spell in spells) {
                if (!GameConstants.AvaibleSpells.Contains(spell.ID)) {
                    client.RemoveSpell(spell);
                    SkillTable.DeleteSpell(client, spell.ID);
                }
            }

            client.Proficiencies.Clear();
            SkillTable.removeAllProfs(client);

            #region Archer2

            #region Arch-Arch

            switch (client.Entity) {
                case {
                    FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.Archer_Master_5
                }: {
                    if (client.Entity.Class == EntityClass.Archer_Eagle_2) {
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.StarArrow });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                    }
                    else {
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                    }

                    break;
                }
                case { FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.Fire_Saint_5 }: {
                    switch (client.Entity.Class) {
                        case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                            or EntityClass.Archer_Eagle_2 or EntityClass.Pirate_Gunner_2
                            or EntityClass.DragonWarrior_Expert_2:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                            client.AddSpell(new Spell(true) { ID = Spells.Fire });
                            client.AddSpell(new Spell(true) { ID = Spells.Cure });
                            client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                            client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                            break;
                        case EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                            client.AddSpell(new Spell(true) { ID = Spells.Fire });
                            client.AddSpell(new Spell(true) { ID = Spells.Cure });
                            client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                            client.AddSpell(new Spell(true) { ID = Spells.Poison });
                            break;
                        case EntityClass.Water_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                            client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                            break;
                        case EntityClass.Fire_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                            client.AddSpell(new Spell(true) { ID = Spells.Dodge });
                            break;
                    }

                    break;
                }
            }

            #endregion

            #region Arch-Tro

            if (client.Entity is
                { FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.Trojan_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Fire_Wizard_3 or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt });
                        break;
                }
            }

            #endregion

            #region Arch-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Fire_Wizard_3 or EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        break;
                    default: {
                        switch (client.Entity.Class) {
                            case EntityClass.Ninja_Middle_2:
                                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                                client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                                client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                                break;
                            case EntityClass.Warrior_Brass_2:
                                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                                client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                                client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                                break;
                            case EntityClass.Water_Wizard_3:
                                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                                client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                                client.AddSpell(new Spell(true) { ID = Spells.Shield });
                                client.AddSpell(new Spell(true) { ID = Spells.Roar });
                                client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                                client.AddSpell(new Spell(true) { ID = Spells.Superman });
                                break;
                        }

                        break;
                    }
                }
            }

            #endregion

            #region Arch-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade });
                        break;
                    case EntityClass.Ninja_Middle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Poison });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                    case EntityClass.Pirate_Gunner_2:
                    case EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                }
            }

            #endregion

            #region Arch-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Water_Wizard_3 or EntityClass.Fire_Wizard_3 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        break;
                    case EntityClass.Ninja_Middle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.TwofoldBlades });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonStar });
                        client.AddSpell(new Spell(true) { ID = Spells.ArcherBane });
                        client.AddSpell(new Spell(true) { ID = Spells.FatalStrike });
                        client.AddSpell(new Spell(true) { ID = Spells.ShurikenVortex });
                        break;
                }
            }

            #endregion

            #region Arch-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Fire });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Archer_Eagle_2
                        or EntityClass.Water_Wizard_3 or EntityClass.Fire_Wizard_3 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                }
            }

            #endregion

            #region Arch-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Water_Wizard_3 or EntityClass.Fire_Wizard_3
                        or EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                }
            }

            #endregion

            #region Arch-Brucelee

            if (client.Entity is {
                    FirstRebornClass: EntityClass.Archer_Master_5, SecondRebornClass: EntityClass.DragonWarrior_King_5
                }) {
                switch (client.Entity.Class) {
                    case EntityClass.Water_Wizard_3 or EntityClass.Fire_Wizard_3
                        or EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                }
            }

            #endregion

            #endregion

            #region Trojan2

            #region Tro-Arch

            if (client.Entity is
                { FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.Archer_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                        break;
                    case EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        break;
                    default:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                }
            }

            #endregion

            #region Tro-Fire

            if (client.Entity is
                { FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.Fire_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Fire });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Fire });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Dodge });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                        break;
                }
            }

            #endregion

            #region Tro-Tro

            if (client.Entity is
                { FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.Trojan_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade });
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade });
                        client.AddSpell(new Spell(true) { ID = Spells.DragonWhirl });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        break;
                }
            }

            #endregion

            #region Tro-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Superman });
                        break;
                }
            }

            #endregion

            #region Tro-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive });
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        break;
                    case EntityClass.Warrior_Brass_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade });
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                }
            }

            #endregion

            #region Tro-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                if (client.Entity.Class == EntityClass.Ninja_Middle_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                    client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                    client.AddSpell(new Spell(true) { ID = Spells.Golem });
                    client.AddSpell(new Spell(true) { ID = Spells.TwofoldBlades });
                    client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                    client.AddSpell(new Spell(true) { ID = Spells.PoisonStar });
                    client.AddSpell(new Spell(true) { ID = Spells.ArcherBane });
                    client.AddSpell(new Spell(true) { ID = Spells.FatalStrike });
                    client.AddSpell(new Spell(true) { ID = Spells.ShurikenVortex });
                }
                else {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                    client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                    client.AddSpell(new Spell(true) { ID = Spells.Golem });
                    client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                }
            }

            #endregion

            #region Tro-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                client.AddSpell(new Spell(true) { ID = Spells.Golem });
                client.AddSpell(new Spell(true) { ID = Spells.Serenity });
            }

            #endregion

            #region Tro-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                client.AddSpell(new Spell(true) { ID = Spells.Golem });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
            }

            #endregion

            #region Tro-Brucelee

            if (client.Entity is {
                    FirstRebornClass: EntityClass.Trojan_Master_5, SecondRebornClass: EntityClass.DragonWarrior_King_5
                }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                client.AddSpell(new Spell(true) { ID = Spells.Golem });
            }

            #endregion

            #endregion

            #region Ninja2

            #region Nin-Arch

            if (client.Entity is
                { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.Archer_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                        break;
                    case EntityClass.Ninja_Middle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Fire_Wizard_3 or EntityClass.Water_Wizard_3 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                }
            }

            #endregion

            #region Nin-Fire

            {
                if (client.Entity is
                    { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.Fire_Saint_5 }) {
                    switch (client.Entity.Class) {
                        case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                            or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2
                            or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                            client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                            client.AddSpell(new Spell(true) { ID = Spells.Fire });
                            client.AddSpell(new Spell(true) { ID = Spells.Cure });
                            client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                            break;
                        case EntityClass.Fire_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                            client.AddSpell(new Spell(true) { ID = Spells.Dodge });
                            client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                            break;
                        case EntityClass.Water_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                            client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                            break;
                    }
                }
            }

            #endregion

            #region Nin-Tro

            if (client.Entity is
                { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.Trojan_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Water_Wizard_3 or EntityClass.Fire_Wizard_3 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt });
                        break;
                }
            }

            #endregion

            #region Nin-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Superman });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        break;
                }
            }

            #endregion

            #region Nin-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive });
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade });
                        break;
                }
            }

            #endregion

            #region Nin-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                if (client.Entity.Class == EntityClass.Ninja_Middle_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.TwofoldBlades }); //
                    client.AddSpell(new Spell(true) { ID = Spells.ToxicFog }); //
                    client.AddSpell(new Spell(true) { ID = Spells.PoisonStar }); //
                    client.AddSpell(new Spell(true) { ID = Spells.TwofoldBlades_L3 }); //
                    client.AddSpell(new Spell(true) { ID = Spells.ArcherBane }); //
                    client.AddSpell(new Spell(true) { ID = Spells.ShurikenVortex }); //
                    client.AddSpell(new Spell(true) { ID = Spells.FatalStrike }); //
                }
                else {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                }
            }

            #endregion

            #region Nin-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
            }

            #endregion

            #region Nin-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
            }

            #endregion

            #region Nin-Brucelee

            if (client.Entity is
                { FirstRebornClass: EntityClass.Ninja_Master_5, SecondRebornClass: EntityClass.DragonWarrior_King_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
            }

            #endregion

            #endregion

            #region Fire2

            #region Fire-Arch

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.Archer_Master_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                client.AddSpell(new Spell(true) { ID = Spells.Fire });
                client.AddSpell(new Spell(true) { ID = Spells.Cure });
                client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
            }

            #endregion

            #region Fire-Fire

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.Fire_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Trojan_Veteran_2
                        or EntityClass.Ninja_Middle_2 or EntityClass.Warrior_Brass_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Dodge });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.HeavenBlade });
                        client.AddSpell(new Spell(true) { ID = Spells.Dodge });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Dodge }); //
                        client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                }
            }

            #endregion

            #region Fire-Tro

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.Trojan_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3
                        or EntityClass.Water_Wizard_3 or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Golem }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone }); //
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing }); //
                        break;
                    case EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone }); //
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Golem }); //
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone }); //
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing }); //
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade }); //
                        break;
                }
            }

            #endregion

            #region Fire-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Shield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Superman }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Shield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        break;
                }
            }

            #endregion

            #region Fire-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive }); //
                        client.AddSpell(new Spell(true) { ID = Spells.HealingRain }); //
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Ninja_Middle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade }); //
                        client.AddSpell(new Spell(true) { ID = Spells.FireCircle }); //
                        break;
                    case EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                }
            }

            #endregion

            #region Fire-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                if (client.Entity.Class is EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2
                    or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.Dodge }); //
                    client.AddSpell(new Spell(true) { ID = Spells.ToxicFog }); //
                    client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                    client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                    client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                    client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                }
                else {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.ToxicFog }); //
                    client.AddSpell(new Spell(true) { ID = Spells.Thunder }); //
                    client.AddSpell(new Spell(true) { ID = Spells.Fire }); //
                    client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                    client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                }
            }

            #endregion

            #region Fire-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Water_Wizard_3 or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                    case EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Dodge });
                        break;
                }
            }

            #endregion

            #region Fire-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                client.AddSpell(new Spell(true) { ID = Spells.Cure });
                client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
            }

            #endregion

            #region Fire-Brucelee

            if (client.Entity is
                { FirstRebornClass: EntityClass.Fire_Saint_5, SecondRebornClass: EntityClass.DragonWarrior_King_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                client.AddSpell(new Spell(true) { ID = Spells.Cure });
                client.AddSpell(new Spell(true) { ID = Spells.Meditation });
            }

            #endregion

            #endregion

            #region War2

            #region War-Arch

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.Archer_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                        break;
                    case EntityClass.Water_Wizard_3 or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                }
            }

            #endregion

            #region War-Fire

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.Fire_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Fire });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Dodge });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Fire });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                    case EntityClass.Warrior_King_5:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                        client.AddSpell(new Spell(true) { ID = Spells.Fire });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                        break;
                }
            }

            #endregion

            #region War-Tro

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.Trojan_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3
                        or EntityClass.Water_Wizard_3 or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        break;
                }
            }

            #endregion

            #region War-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Perseverance });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Superman });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                }
            }

            #endregion

            #region War-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                if (client.Entity.Class == EntityClass.Archer_Eagle_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.Shield });
                    client.AddSpell(new Spell(true) { ID = Spells.Roar });
                    client.AddSpell(new Spell(true) { ID = Spells.Cure });
                    client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                    client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                    client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                    client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                    client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                    client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                }

                switch (client.Entity.Class) {
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive });
                        client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Superman });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                    case EntityClass.Ninja_Middle_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                }
            }

            #endregion

            #region War-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                if (client.Entity.Class == EntityClass.Ninja_Middle_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                    client.AddSpell(new Spell(true) { ID = Spells.Roar });
                }
                else {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                    client.AddSpell(new Spell(true) { ID = Spells.Roar });
                    client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                }
            }

            #endregion

            #region War-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Warrior_Brass_2 or EntityClass.Archer_Eagle_2
                        or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2 or EntityClass.Fire_Wizard_3
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Dodge }); // Dodge
                        break;
                }
            }

            #endregion

            #region War-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Roar });
                client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                client.AddSpell(new Spell(true) { ID = Spells.Reflect });
            }

            #endregion

            #region War-Brucelee

            if (client.Entity is
                { FirstRebornClass: EntityClass.Warrior_King_5, SecondRebornClass: EntityClass.DragonWarrior_King_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Roar });
                client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                client.AddSpell(new Spell(true) { ID = Spells.Reflect });
            }

            #endregion

            #endregion

            #region Water2

            #region Water-Arch

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.Archer_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                    case EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                    default:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.FreezingArrow });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                        break;
                }
            }

            #endregion

            #region Water-Fire

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.Fire_Saint_5 }) {
                if (client.Entity.Class == EntityClass.Trojan_Veteran_2 ||
                    client.Entity.Class == EntityClass.Warrior_Brass_2 |
                    client.Entity.Class == EntityClass.Archer_Eagle_2 ||
                    client.Entity.Class is EntityClass.Ninja_Middle_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.Revive });
                    client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                    client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                    client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                    client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                    client.AddSpell(new Spell(true) { ID = Spells.Fire });
                    client.AddSpell(new Spell(true) { ID = Spells.Cure });
                    client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                }
                else
                    switch (client.Entity.Class) {
                        case EntityClass.Monk_Dhyana_2:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.Revive });
                            client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                            client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                            client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                            client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                            client.AddSpell(new Spell(true) { ID = Spells.Fire });
                            client.AddSpell(new Spell(true) { ID = Spells.Cure });
                            client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                            client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                            client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                            break;
                        case EntityClass.Water_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.Revive });
                            client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                            client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                            client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                            client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                            break;
                        case EntityClass.Fire_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.Revive });
                            client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                            client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                            client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                            client.AddSpell(new Spell(true) { ID = Spells.Dodge });
                            break;
                    }
            }

            #endregion

            #region Water-Tro

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.Trojan_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3
                        or EntityClass.Water_Wizard_3 or EntityClass.Ninja_Middle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                    case EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure });
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                        client.AddSpell(new Spell(true) { ID = Spells.Golem });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Cyclone }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Golem }); //
                        client.AddSpell(new Spell(true) { ID = Spells.IronShirt }); //
                        break;
                    case EntityClass.Trojan_Veteran_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.CruelShade }); //
                        break;
                }
            }

            #endregion

            #region Water-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Shield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare }); //
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf }); //
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Shield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon }); //
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon }); //
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Shield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Roar }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Superman }); //
                        break;
                }
            }

            #endregion

            #region Water-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade }); //
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                        client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade }); //
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade }); //
                        client.AddSpell(new Spell(true) { ID = Spells.AzureShield });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.HealingRain }); //
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure }); //
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade }); //
                        break;
                }
            }

            #endregion

            #region Water-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Cure }); //
                client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy }); //
                client.AddSpell(new Spell(true) { ID = Spells.MagicShield }); //
                client.AddSpell(new Spell(true) { ID = Spells.Stigma }); //
                client.AddSpell(new Spell(true) { ID = Spells.Meditation }); //
                client.AddSpell(new Spell(true) { ID = Spells.ToxicFog }); //
            }

            #endregion

            #region Water-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Cure });
                client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                client.AddSpell(new Spell(true) { ID = Spells.Serenity });
            }

            #endregion

            #region Water-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Cure });
                client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
            }

            #endregion

            #region Water-Brucelee

            if (client.Entity is
                { FirstRebornClass: EntityClass.Water_Saint_5, SecondRebornClass: EntityClass.DragonWarrior_King_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Cure });
                client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                client.AddSpell(new Spell(true) { ID = Spells.Meditation });
            }

            #endregion

            #endregion

            #region Monk2

            #region Monk-Arch

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.Archer_Master_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
                client.AddSpell(new Spell(true) { ID = Spells.Serenity });
            }

            #endregion

            #region Monk-Fire

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.Fire_Saint_5 }) {
                if (client.Entity.Class == EntityClass.Trojan_Veteran_2 ||
                    client.Entity.Class == EntityClass.Warrior_Brass_2 |
                    client.Entity.Class == EntityClass.Archer_Eagle_2 ||
                    client.Entity.Class is EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                    client.AddSpell(new Spell(true) { ID = Spells.Fire });
                    client.AddSpell(new Spell(true) { ID = Spells.Cure });
                    client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                    client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                }
                else
                    switch (client.Entity.Class) {
                        case EntityClass.Fire_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                            break;
                        case EntityClass.Water_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                            client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                            break;
                        case EntityClass.Ninja_Middle_2:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.StarofAccuracy });
                            client.AddSpell(new Spell(true) { ID = Spells.MagicShield });
                            client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                            client.AddSpell(new Spell(true) { ID = Spells.Cure });
                            client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                            client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                            break;
                    }
            }

            #endregion

            #region Monk-Tro

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.Trojan_Master_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                client.AddSpell(new Spell(true) { ID = Spells.Golem });
            }

            #endregion

            #region Monk-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.Superman });
                        break;
                }
            }

            #endregion

            #region Monk-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                    case EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        break;
                }
            }

            #endregion

            #region Monk-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Ninja_Middle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonStar });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Warrior_Brass_2 or EntityClass.Archer_Eagle_2 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Fire_Wizard_3 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        break;
                }
            }

            #endregion

            #region Monk-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Fire_Wizard_3 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SoulShackle });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                }
            }

            #endregion

            #region Monk-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
            }

            #endregion

            #region Monk-Brucelee

            if (client.Entity is
                { FirstRebornClass: EntityClass.Monk_Nirvana_5, SecondRebornClass: EntityClass.DragonWarrior_King_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.Serenity });
            }

            #endregion

            #endregion

            #region Pirate2

            #region Pirate-Arch

            if (client.Entity is
                { FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.Archer_Master_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
            }

            #endregion

            #region Pirate-Fire

            if (client.Entity is
                { FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.Fire_Saint_5 }) {
                if (client.Entity.Class == EntityClass.Trojan_Veteran_2 ||
                    client.Entity.Class == EntityClass.Warrior_Brass_2 |
                    client.Entity.Class == EntityClass.Archer_Eagle_2 ||
                    client.Entity.Class is EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.Ninja_Middle_2 or EntityClass.DragonWarrior_Expert_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                    client.AddSpell(new Spell(true) { ID = Spells.Fire });
                    client.AddSpell(new Spell(true) { ID = Spells.Cure });
                    client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                    client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                }
                else
                    switch (client.Entity.Class) {
                        case EntityClass.Fire_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                            break;
                        case EntityClass.Water_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                            client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                            break;
                    }
            }

            #endregion

            #region Pirate-Tro

            if (client.Entity is
                { FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.Trojan_Master_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                client.AddSpell(new Spell(true) { ID = Spells.Golem });
            }

            #endregion

            #region Pirate-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.Superman });
                        break;
                }
            }

            #endregion

            #region Pirate-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                }
            }

            #endregion

            #region Pirate-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Ninja_Middle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonStar });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Warrior_Brass_2 or EntityClass.Archer_Eagle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        break;
                }
            }

            #endregion

            #region Pirate-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SoulShackle });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                    case EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                }
            }

            #endregion

            #region Pirate-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Fire_Wizard_3 or EntityClass.Monk_Dhyana_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        break;
                    case EntityClass.Pirate_Gunner_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
                        client.AddSpell(new Spell(true) { ID = Spells.ScurvyBomb });
                        break;
                }
            }

            #endregion

            #region Pirate-Brucelee

            if (client.Entity is {
                    FirstRebornClass: EntityClass.Pirate_Lord_5, SecondRebornClass: EntityClass.DragonWarrior_King_5,
                    Class: EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2 or EntityClass.Archer_Eagle_2
                    or EntityClass.Ninja_Middle_2 or EntityClass.Water_Wizard_3 or EntityClass.Fire_Wizard_3
                    or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
            }

            #endregion

            #endregion

            #region Brucelee2

            #region Brucelee-Arch

            if (client.Entity is {
                    FirstRebornClass: EntityClass.DragonWarrior_King_5, SecondRebornClass: EntityClass.Archer_Master_5
                }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.PoisonousArrows });
            }

            #endregion

            #region Brucelee-Fire

            if (client.Entity is
                { FirstRebornClass: EntityClass.DragonWarrior_King_5, SecondRebornClass: EntityClass.Fire_Saint_5 }) {
                if (client.Entity.Class == EntityClass.Trojan_Veteran_2 ||
                    client.Entity.Class == EntityClass.Warrior_Brass_2 |
                    client.Entity.Class == EntityClass.Archer_Eagle_2 ||
                    client.Entity.Class is EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.Ninja_Middle_2 or EntityClass.DragonWarrior_Expert_2) {
                    client.AddSpell(new Spell(true) { ID = Spells.Bless });
                    client.AddSpell(new Spell(true) { ID = Spells.Thunder });
                    client.AddSpell(new Spell(true) { ID = Spells.Fire });
                    client.AddSpell(new Spell(true) { ID = Spells.Cure });
                    client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                }
                else
                    switch (client.Entity.Class) {
                        case EntityClass.Fire_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            break;
                        case EntityClass.Water_Wizard_3:
                            client.AddSpell(new Spell(true) { ID = Spells.Bless });
                            client.AddSpell(new Spell(true) { ID = Spells.FireCircle });
                            break;
                    }
            }

            #endregion

            #region Brucelee-Tro

            if (client.Entity is {
                    FirstRebornClass: EntityClass.DragonWarrior_King_5, SecondRebornClass: EntityClass.Trojan_Master_5
                }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.SpiritHealing });
                client.AddSpell(new Spell(true) { ID = Spells.Cyclone });
                client.AddSpell(new Spell(true) { ID = Spells.Golem });
            }

            #endregion

            #region Pirate-War

            if (client.Entity is
                { FirstRebornClass: EntityClass.DragonWarrior_King_5, SecondRebornClass: EntityClass.Warrior_King_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Archer_Eagle_2 or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Ninja_Middle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        break;
                    case EntityClass.Warrior_Brass_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.Reflect });
                        client.AddSpell(new Spell(true) { ID = Spells.Superman });
                        break;
                }
            }

            #endregion

            #region Brucelee-Water

            if (client.Entity is
                { FirstRebornClass: EntityClass.DragonWarrior_King_5, SecondRebornClass: EntityClass.Water_Saint_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Monk_Dhyana_2
                        or EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Shield });
                        client.AddSpell(new Spell(true) { ID = Spells.Roar });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.Meditation });
                        client.AddSpell(new Spell(true) { ID = Spells.Accuracy });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        client.AddSpell(new Spell(true) { ID = Spells.Stigma });
                        break;
                    case EntityClass.Water_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Pervade });
                        break;
                    case EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.WaterElf });
                        client.AddSpell(new Spell(true) { ID = Spells.AdvancedCure });
                        client.AddSpell(new Spell(true) { ID = Spells.Invisibility });
                        client.AddSpell(new Spell(true) { ID = Spells.HealingRain });
                        client.AddSpell(new Spell(true) { ID = Spells.Revive });
                        client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
                        break;
                }
            }

            #endregion

            #region Brucelee-Nin

            if (client.Entity is
                { FirstRebornClass: EntityClass.DragonWarrior_King_5, SecondRebornClass: EntityClass.Ninja_Master_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Ninja_Middle_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.PoisonStar });
                        break;
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Warrior_Brass_2 or EntityClass.Archer_Eagle_2
                        or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2
                        or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.ToxicFog });
                        break;
                }
            }

            #endregion

            #region Brucelee-Monk

            if (client.Entity is
                { FirstRebornClass: EntityClass.DragonWarrior_King_5, SecondRebornClass: EntityClass.Monk_Nirvana_5 }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Fire_Wizard_3:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        break;
                    case EntityClass.Monk_Dhyana_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.SoulShackle });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                    case EntityClass.Pirate_Gunner_2 or EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.Serenity });
                        break;
                }
            }

            #endregion

            #region Brucelee-Pirate

            if (client.Entity is
                { FirstRebornClass: EntityClass.DragonWarrior_King_5, SecondRebornClass: EntityClass.Pirate_Lord_5 }) {
                client.AddSpell(new Spell(true) { ID = Spells.Bless });
                client.AddSpell(new Spell(true) { ID = Spells.GaleBomb });
            }

            #endregion

            #region Brucelee-Brucelee

            if (client.Entity is {
                    FirstRebornClass: EntityClass.DragonWarrior_King_5,
                    SecondRebornClass: EntityClass.DragonWarrior_King_5
                }) {
                switch (client.Entity.Class) {
                    case EntityClass.Trojan_Veteran_2 or EntityClass.Warrior_Brass_2
                        or EntityClass.Archer_Eagle_2 or EntityClass.Ninja_Middle_2 or EntityClass.Water_Wizard_3
                        or EntityClass.Fire_Wizard_3 or EntityClass.Monk_Dhyana_2 or EntityClass.Pirate_Gunner_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        break;
                    case EntityClass.DragonWarrior_Expert_2:
                        client.AddSpell(new Spell(true) { ID = Spells.Bless });
                        client.AddSpell(new Spell(true) { ID = Spells.DragonFury });
                        break;
                }
            }

            #endregion

            #endregion

            #endregion

            #region Low level items

            for (byte i = 1; i < 9; i++) {
                if (i == 7) continue;
                var item = client.Equipment.TryGetItem(i);
                if (item is not { ID: not 0 }) continue;
                try {
                    var cii = new ConquerItemInformation(item.ID, item.Plus);
                    item.ID = cii.LowestID(
                        Network.PacketHandler.ItemMinLevel(Network.PacketHandler.ItemPosition(item.ID)));
                    item.Mode = Enums.ItemMode.Update;
                    item.Send(client);
                    client.LoadItemStats();
                    ConquerItemTable.UpdateItemID(item, client);
                }
                catch {
                    Console.WriteLine("Reborn item problem: " + item.ID);
                }
            }

            var hand = client.Equipment.TryGetItem(5);
            if (!client.Equipment.Remove(5)) {
                if (client.Warehouses[ConquerStructures.Warehouse.WarehouseID.Market].Count < 20)
                    client.Warehouses[ConquerStructures.Warehouse.WarehouseID.Market].Add(hand);
                ConquerItemTable.UpdatePosition(hand);
                var equips = new ClientEquip();
                equips.DoEquips(client);
                client.Send(equips);
            }

            client.CalculateStatBonus();
            client.CalculateHPBonus();

            hand = client.Equipment.TryGetItem(25);
            if (!client.Equipment.Remove(25)) {
                if (client.Warehouses[ConquerStructures.Warehouse.WarehouseID.Market].Count < 20)
                    client.Warehouses[ConquerStructures.Warehouse.WarehouseID.Market].Add(hand);
                ConquerItemTable.UpdatePosition(hand);
                var equips = new ClientEquip();
                equips.DoEquips(client);
                client.Send(equips);
            }

            client.CalculateStatBonus();
            client.CalculateHPBonus();

            client.LoadItemStats();
            client.SendScreen(client.Entity.SpawnPacket, false);

            #endregion

            DataHolder.GetStats(client.Entity.Class, client.Entity.Level, client);
            client.CalculateStatBonus();
            client.CalculateHPBonus();
            client.GemAlgorithm();

            Network.PacketHandler.WorldMessage($"{client.Entity.Name} has been reincarnated, congratulations!");
        }
    }
}