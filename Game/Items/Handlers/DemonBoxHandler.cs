using System;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.Constants.Items.QuestAndOther;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DemonBox items that spawn monsters when used (with 5 minute cooldown).
    /// </summary>
    [ItemHandler(SuperMonsterBox, MonkeyMonsterBox, CrazyGhostBox, GoldenBirdBox, BloodGhostBox, HumanAideBox)]
    public static class DemonBoxHandler {
        private const double CooldownMinutes = 5.0;

        public static void Handle(GameState client, ConquerItem item) {
            if (DateTime.Now < client.matrixtime.AddMinutes(CooldownMinutes)) {
                var remain = client.matrixtime.AddMinutes(CooldownMinutes) - DateTime.Now;
                var message = "Time Till Next Usage :";
                message += string.Format("{0} Minutes : {1} Seconds", remain.Minutes, remain.Seconds);
                client.MessageBox(message);
                return;
            }

            client.matrixtime = DateTime.Now;

            var effectName = "";
            var monsterName = "";
            MonsterInformation monster = null;

            if (item.ID == SuperMonsterBox) {
                effectName = "fathitgtsyd";
                monsterName = "SuperMonster";
                monster = new MonsterInformation {
                    Hitpoints = 7500000,
                    Level = 200,
                    Mesh = 984,
                    Name = monsterName,
                    MaxAttack = 80000,
                    AttackRange = 10,
                    AttackType = 2,
                    SpellID = 10304,
                    AttackSpeed = 1500,
                    ViewRange = 10,
                    MoveSpeed = 500,
                    RunSpeed = 500,
                    MinAttack = 70000
                };
            }
            else if (item.ID == MonkeyMonsterBox) {
                effectName = "fathitgtsyd";
                monsterName = "MonkeyMonster";
                monster = new MonsterInformation {
                    Hitpoints = 7500000,
                    Level = 200,
                    Mesh = 981,
                    Name = monsterName,
                    MaxAttack = 70000,
                    AttackRange = 10,
                    AttackType = 2,
                    SpellID = 10304,
                    AttackSpeed = 1500,
                    ViewRange = 10,
                    MoveSpeed = 500,
                    RunSpeed = 500,
                    MinAttack = 60000
                };
            }
            else if (item.ID == CrazyGhostBox) {
                effectName = "fam_gain_special";
                monsterName = "CrazyGhost";
                monster = new MonsterInformation {
                    Hitpoints = 6000000,
                    Level = 200,
                    Mesh = 982,
                    Name = monsterName,
                    MaxAttack = 60000,
                    AttackRange = 10,
                    AttackType = 2,
                    SpellID = 10304,
                    AttackSpeed = 1000,
                    ViewRange = 5,
                    MoveSpeed = 500,
                    RunSpeed = 500,
                    MinAttack = 50000
                };
            }
            else if (item.ID == GoldenBirdBox) {
                effectName = "fam_exp_special";
                monsterName = "GoldenBird";
                monster = new MonsterInformation {
                    Hitpoints = 3500000,
                    Level = 200,
                    Mesh = 983,
                    Name = monsterName,
                    MaxAttack = 40000,
                    AttackRange = 10,
                    AttackType = 2,
                    SpellID = 10304,
                    AttackSpeed = 1000,
                    ViewRange = 5,
                    MoveSpeed = 500,
                    RunSpeed = 500,
                    MinAttack = 30000
                };
            }
            else if (item.ID == BloodGhostBox) {
                effectName = "fam_gain";
                monsterName = "BloodGhost";
                monster = new MonsterInformation {
                    Hitpoints = 2500000,
                    Level = 200,
                    Mesh = 208,
                    Name = monsterName,
                    MaxAttack = 30000,
                    AttackRange = 10,
                    AttackType = 2,
                    SpellID = 10304,
                    AttackSpeed = 1000,
                    ViewRange = 5,
                    MoveSpeed = 500,
                    RunSpeed = 500,
                    MinAttack = 20000
                };
            }
            else if (item.ID == HumanAideBox) {
                effectName = "fam_exp";
                monsterName = "HumanAide";
                monster = new MonsterInformation {
                    Hitpoints = 1500000,
                    Level = 200,
                    Mesh = 209,
                    Name = monsterName,
                    MaxAttack = 20000,
                    AttackRange = 10,
                    AttackType = 2,
                    SpellID = 10304,
                    AttackSpeed = 1000,
                    ViewRange = 5,
                    MoveSpeed = 500,
                    RunSpeed = 500,
                    MinAttack = 10000
                };
            }

            if (monster == null) return;

            client.Entity.Update(_String.Effect, effectName, true);

            var entity = new Entity(EntityFlag.Monster, false) {
                MapObjType = MapObjectType.Monster,
                MonsterInfo = monster
            };
            entity.MonsterInfo.Owner = entity;
            entity.Name = monsterName;
            entity.MinAttack = monster.MinAttack;
            entity.MaxAttack = entity.MagicAttack = monster.MaxAttack;
            entity.Hitpoints = entity.MaxHitpoints = monster.Hitpoints;
            entity.Body = monster.Mesh;
            entity.Level = monster.Level;
            entity.Defence = 20000;

            // Set position based on monster type
            if (item.ID is CrazyGhostBox or HumanAideBox) {
                entity.X = client.Entity.X;
                entity.Y = client.Entity.Y;
            }
            else {
                entity.X = (ushort)(client.Entity.X - 2);
                entity.Y = (ushort)(client.Entity.Y - 2);
            }

            entity.EntityFlag = EntityFlag.Monster;
            entity.MapID = client.Entity.MapID;
            entity.SendUpdates = true;
            client.Map.RemoveEntity(entity);

            var stringPacket = new _String(true) {
                UID = monster.ID,
                Type = _String.Effect
            };
            stringPacket.Texts.Add("MBStandard");
            client.Map.AddEntity(entity);

            client.Inventory.Remove(item, Enums.ItemUse.Remove);
        }
    }
}
