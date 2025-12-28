using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles FireofHell skill book. A spell for Level 82 Fire Wizards.
    /// </summary>
    [ItemHandler(FireofHell)]
    public static class FireofHellHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Class is >= 140 and <= 145 && client.Entity.Level >= 84) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1165 });
            }
        }
    }
}

