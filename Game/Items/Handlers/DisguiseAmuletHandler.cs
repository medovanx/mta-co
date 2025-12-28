using System;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DisguiseAmulet item that transforms the player into a random monster.
    /// </summary>
    [ItemHandler(DisguiseAmulet)]
    public static class DisguiseAmuletHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var disguise = Kernel.Random.Next(DataHolder.Disguises.Length);
            var selected = DataHolder.Disguises[disguise];

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);

            var wasTransformated = client.Entity.Transformed;
            if (wasTransformated) {
                client.Entity.Hitpoints = client.Entity.MaxHitpoints;
                client.Entity.TransformationID = 0;
                client.Entity.TransformationStamp = Time32.Now;
            }

            client.Entity.TransformationID = selected;
            client.Entity.TransformationStamp = Time32.Now;
            client.Entity.TransformationTime = 110;
            var spellUse = new SpellUse(true) {
                Attacker = client.Entity.UID,
                SpellID = 1360,
                SpellLevel = 4,
                X = client.Entity.X,
                Y = client.Entity.Y
            };
            spellUse.AddTarget(client.Entity, (uint)0, null);
            client.Send(spellUse);
            client.Entity.TransformationMaxHP = 3000;
            double maxHp = client.Entity.MaxHitpoints;
            double hp = client.Entity.Hitpoints;
            var point = hp / maxHp;

            client.Entity.Hitpoints = (uint)(client.Entity.TransformationMaxHP * point);
            client.Entity.Update(Update.MaxHitpoints, client.Entity.TransformationMaxHP, false);
        }
    }
}
