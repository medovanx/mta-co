using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Spells;

namespace MTA.Game.Npcs.Handlers.Market {
    /// <summary>
    /// Skill Soul Master - Allows players to upgrade their skill soul levels
    /// </summary>
    [NpcHandler(29)]
    public static class NpcSkillSoul {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            dialog.Avatar(85);
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        "Hello friend. I am the Skill Soul master! Have you ever imagined if you could change your skills' appearance?");
                    dialog.Option("Okay, go ahead.", 1);
                    dialog.Option("Sorry.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    dialog.Text("Choose its new style.");
                    dialog.Option("Upgrade My Skill Souls", 25);
                    dialog.Option("I'll think about it.", 255);
                    dialog.Send();
                    break;
                }

                case 25: {
                    dialog.Text("Choose the skill you want.");
                    foreach (var spell in client.Spells.Values) {
                        if (!SpellTable.AllowSkillSoul.Contains(spell.ID)) continue;
                        var id = (byte)(SpellTable.AllowSkillSoul.IndexOf(spell.ID) + 2);


                        {
                            switch (spell.ID) {
                                default: {
                                    if (spell.Souls < Spell.Soul_Level.Level_Two) {
                                        if (SpellTable.SpellInformations.ContainsKey(spell.ID)) {
                                            if (SpellTable.SpellInformations[spell.ID].ContainsKey(spell.Level)) {
                                                dialog.Option(
                                                    SpellTable.SpellInformations[spell.ID][spell.Level].Name != ""
                                                        ? $"{SpellTable.SpellInformations[spell.ID][spell.Level].Name}, [Skill Soul Level]: {spell.Souls}"
                                                        : $"{SpellTable.SpellInformations[spell.ID][spell.Level].ID}, [Skill Soul Level]: {spell.Souls}",
                                                    id);
                                            }
                                            else {
                                                dialog.Option(
                                                    SpellTable.SpellInformations[spell.ID][0].Name != ""
                                                        ? $"{SpellTable.SpellInformations[spell.ID][0].Name}, [Skill Soul Level]: {spell.Souls}"
                                                        : $"{SpellTable.SpellInformations[spell.ID][0].ID}, [Skill Soul Level]: {spell.Souls}",
                                                    id);
                                            }
                                        }
                                    }

                                    break;
                                }
                                case RadiantPalm: {
                                    if (spell.Souls < Spell.Soul_Level.Level_Four) {
                                        if (SpellTable.SpellInformations.ContainsKey(spell.ID)) {
                                            if (SpellTable.SpellInformations[spell.ID].ContainsKey(spell.Level)) {
                                                dialog.Option(
                                                    SpellTable.SpellInformations[spell.ID][spell.Level].Name != ""
                                                        ? $"{SpellTable.SpellInformations[spell.ID][spell.Level].Name}, [Skill Soul Level]: {spell.Souls}"
                                                        : $"{SpellTable.SpellInformations[spell.ID][spell.Level].ID}, [Skill Soul Level]: {spell.Souls}",
                                                    id);
                                            }
                                            else {
                                                dialog.Option(
                                                    SpellTable.SpellInformations[spell.ID][0].Name != ""
                                                        ? $"{SpellTable.SpellInformations[spell.ID][0].Name}, [Skill Soul Level]: {spell.Souls}"
                                                        : $"{SpellTable.SpellInformations[spell.ID][0].ID}, [Skill Soul Level]: {spell.Souls}",
                                                    id);
                                            }
                                        }
                                    }

                                    break;
                                }
                                case ToxicFog:
                                case Fire: {
                                    if (spell.Souls < Spell.Soul_Level.Level_One) {
                                        if (SpellTable.SpellInformations.ContainsKey(spell.ID)) {
                                            if (SpellTable.SpellInformations[spell.ID].ContainsKey(spell.Level)) {
                                                dialog.Option(
                                                    SpellTable.SpellInformations[spell.ID][spell.Level].Name != ""
                                                        ? $"{SpellTable.SpellInformations[spell.ID][spell.Level].Name}, [Skill Soul Level]: {spell.Souls}"
                                                        : $"{SpellTable.SpellInformations[spell.ID][spell.Level].ID}, [Skill Soul Level]: {spell.Souls}",
                                                    id);
                                            }
                                            else {
                                                dialog.Option(
                                                    SpellTable.SpellInformations[spell.ID][0].Name != ""
                                                        ? $"{SpellTable.SpellInformations[spell.ID][0].Name}, [Skill Soul Level]: {spell.Souls}"
                                                        : $"{SpellTable.SpellInformations[spell.ID][0].ID}, [Skill Soul Level]: {spell.Souls}",
                                                    id);
                                            }
                                        }
                                    }

                                    break;
                                }
                                case ScentSword:
                                case FastBlade2:
                                case ChargingVortex:
                                case WhirlwindKick:
                                case Tornado: {
                                    if (spell.Souls < Spell.Soul_Level.Level_Three) {
                                        if (SpellTable.SpellInformations.ContainsKey(spell.ID)) {
                                            if (SpellTable.SpellInformations[spell.ID].ContainsKey(spell.Level)) {
                                                dialog.Option(
                                                    SpellTable.SpellInformations[spell.ID][spell.Level].Name != ""
                                                        ? $"{SpellTable.SpellInformations[spell.ID][spell.Level].Name}, [Skill Soul Level]: {spell.Souls}"
                                                        : $"{SpellTable.SpellInformations[spell.ID][spell.Level].ID}, [Skill Soul Level]: {spell.Souls}",
                                                    id);
                                            }
                                            else {
                                                dialog.Option(
                                                    SpellTable.SpellInformations[spell.ID][0].Name != ""
                                                        ? $"{SpellTable.SpellInformations[spell.ID][0].Name}, [Skill Soul Level]: {spell.Souls}"
                                                        : $"{SpellTable.SpellInformations[spell.ID][0].ID}, [Skill Soul Level]: {spell.Souls}",
                                                    id);
                                            }
                                        }
                                    }
                                }
                                    break;
                            }
                        }
                    }

                    dialog.Option("Nevermind.", 255);
                    dialog.Send();
                    break;
                }
                default: {
                    var i = (byte)(npcRequest.OptionID - 2);
                    var skill = SpellTable.AllowSkillSoul[i];
                    if (client.Spells.TryGetValue(skill, out var value)) {
                        {
                            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                            switch (value.Souls) {
                                case Spell.Soul_Level.Default:
                                case Spell.Soul_Level.Level_One:
                                case Spell.Soul_Level.Level_Two:
                                case Spell.Soul_Level.Level_Three: {
                                    var lvl = Spell.SkillSoul_values.IndexOf(value.Souls);
                                    lvl++;
                                    var cost = (uint)(lvl * 1000);
                                    if (client.Entity.ConquerPoints >= cost) {
                                        client.Entity.ConquerPoints -= cost;
                                        client.Spells[skill].Souls = Spell.SkillSoul_values[lvl];
                                    }
                                    else {
                                        dialog.Text($"You don't have {cost} CPs. Come back when you have it.");
                                        dialog.Option("Sorry.", 255);
                                        dialog.Send();
                                        return;
                                    }
                                }
                                    break;
                            }

                            var data = new Data(true) {
                                UID = client.Entity.UID,
                                dwParam = value.ID,
                                ID = 109
                            };
                            client.Send(data);
                            client.Send(new Spell(true) {
                                ID = value.ID,
                                Level = value.Level,
                                PreviousLevel = value.PreviousLevel,
                                Experience = 0,
                                Souls = value.Souls,
                                Available = true
                            }.ToArray());
                            value.Send(client);

                            SkillTable.SaveSpells(client);
                            dialog.Text($"You have upgraded your skill {value.ID} to Skill Soul level {value.Souls}.");
                            dialog.Option("Good job.", 255);
                            dialog.Send();
                        }
                    }
                }
                    break;
            }
        }
    }
}