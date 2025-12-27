using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles BombScroll skill book. A spell for Level 80 Fire Wizards.
    /// </summary>
    [ItemHandler(BombScroll)]
    public static class BombScrollHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Class is < 140 or > 145 || client.Entity.Level < 82) return;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 1160 });
        }
    }
}