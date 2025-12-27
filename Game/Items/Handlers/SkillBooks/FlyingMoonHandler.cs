using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles FlyingMoon skill book. An XP skill for level 40+ Warrior. Deals a high damage on a single target from a distance.
    /// </summary>
    [ItemHandler(FlyingMoon)]
    public static class FlyingMoonHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 1320 });
        }
    }
}

