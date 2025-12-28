using System;
using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using MTA.Game.ConquerStructures.House;
using MTA.Network.GamePackets;
using Update = MTA.Network.GamePackets.Update;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles House Mob Pack items that spawn monsters in player houses.
    /// </summary>
    [ItemHandler(HouseMobPack1, HouseMobPack2)]
    public static class HouseMobHandler {
        private const double CooldownHours = 3.0;
        private static readonly Dictionary<uint, uint> PackToMonsterID = new Dictionary<uint, uint> {
            { HouseMobPack1, 41710 },
            { HouseMobPack2, 4170 }
        };

        public static void Handle(GameState client, ConquerItem item) {
            if (!PackToMonsterID.TryGetValue(item.ID, out var monsterID)) {
                return;
            }

            DateTime itemtime = client[item.ID.ToString()];
            if (DateTime.Now < itemtime.AddHours(CooldownHours)) {
                var remain = itemtime.AddHours(CooldownHours) - DateTime.Now;
                var message = "Time Till Next Usage :";
                message += string.Format("{1} Minutes : {2} Seconds", remain.Hours, remain.Minutes, remain.Seconds);
                client.MessageBox(message);
                return;
            }

            if (!House.Houses.ContainsKey(client.Entity.UID)) {
                client.MessageBox("only in house.");
                return;
            }

            var myhouse = House.Houses[client.Entity.UID];
            if (client.Entity.MapID != myhouse.ID) {
                client.MessageBox("only in your house.");
                return;
            }

            if (!MonsterInformation.MonsterInformations.ContainsKey(monsterID)) {
                return;
            }

            var mt = MonsterInformation.MonsterInformations[monsterID];
            mt.BoundX = client.Entity.X;
            mt.BoundY = client.Entity.Y;
            mt.RespawnTime = 36000;

            var entity = new Entity(EntityFlag.Monster, false) {
                MapObjType = MapObjectType.Monster,
                MonsterInfo = mt.Copy()
            };
            entity.MonsterInfo.Owner = entity;
            entity.Name = mt.Name;
            entity.MinAttack = mt.MinAttack;
            entity.MaxAttack = entity.MagicAttack = mt.MaxAttack;
            entity.Hitpoints = entity.MaxHitpoints = mt.Hitpoints;
            entity.Defence = mt.Defence;
            entity.Body = mt.Mesh;
            entity.Level = mt.Level;
            entity.UID = client.Map.EntityUIDCounter.Next;
            entity.MapID = client.Entity.MapID;
            entity.SendUpdates = true;
            entity.X = client.Entity.X;
            entity.Y = client.Entity.Y;

            client.Map.AddEntity(entity);
            client.SendScreenSpawn(entity, true);

            if (entity.MaxHitpoints > 65535) {
                var upd = new Update(true) { UID = entity.UID };
                upd.Append(Update.Hitpoints, entity.Hitpoints);
                client.SendScreen(upd);
            }

            client[item.ID.ToString()] = DateTime.Now;
            client.Inventory.Remove(item, Enums.ItemUse.Remove);
        }
    }
}

