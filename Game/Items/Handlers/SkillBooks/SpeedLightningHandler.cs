using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles SpeedLightning skill book. XP skill for Level 70 above Taoists.
    /// </summary>
    [ItemHandler(SpeedLightning)]
    public static class SpeedLightningHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Class is >= 130 and <= 135 or >= 140 and <= 145)
                client.AddSpell(new Spell(true) { ID = 5001 });
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}

