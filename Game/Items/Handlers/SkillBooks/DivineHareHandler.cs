using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles DivineHare skill book. Disguise skill. Jump speed increased in the disguise of DivineHare. For Water Taoist of level 54 to learn.
    /// </summary>
    [ItemHandler(DivineHare)]
    public static class DivineHareHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Class is >= 130 and <= 135) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1350 });
            }
        }
    }
}

