using System;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles medicine items that restore HP or MP when used.
    /// </summary>
    [ItemHandler(Stancher, Stancher2, Stancher3, AmritaPill, PanaceaPill, GinsengPill, VanillaPill, MilGinsengPill,
        Stancher4, SevenStarOintment, Agrypnotic, Agrypnotic2, Agrypnotic3, RecoveryPillPill, SoulPillPill,
        RefreshingPillPill, ChantPillPill, SerenityPill)]
    public static class MedicineHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var infos = new ConquerItemInformation(item.ID, 0);

            // Check if HP medicines
            var isHpMedicine = item.ID is Stancher or Stancher2 or Stancher3 or AmritaPill or PanaceaPill or GinsengPill or VanillaPill or MilGinsengPill or Stancher4 or SevenStarOintment;

            if (isHpMedicine) {
                if (NoHp.Contains(client.Entity.MapID)) {
                    return;
                }

                if (client.Entity.NoDrugsTime > 0) {
                    if (Time32.Now > client.Entity.NoDrugsStamp.AddSeconds(client.Entity.NoDrugsTime)) {
                        client.Entity.NoDrugsTime = 0;
                    }
                    else {
                        return;
                    }
                }

                if (client.Entity.Hitpoints == client.Entity.MaxHitpoints)
                    return;

                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Entity.Hitpoints = Math.Min(client.Entity.Hitpoints + infos.BaseInformation.ItemHP,
                    client.Entity.MaxHitpoints);
            }
            else {
                // MP medicines
                if (NoHp.Contains(client.Entity.MapID)) {
                    return;
                }

                if (client.Entity.NoDrugsTime > 0) {
                    if (Time32.Now > client.Entity.NoDrugsStamp.AddSeconds(client.Entity.NoDrugsTime)) {
                        client.Entity.NoDrugsTime = 0;
                    }
                    else {
                        return;
                    }
                }

                if (client.Entity.Mana == client.Entity.MaxMana)
                    return;

                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Entity.Mana = (ushort)Math.Min(client.Entity.Mana + infos.BaseInformation.ItemMP,
                    client.Entity.MaxMana);
            }
        }
    }
}

