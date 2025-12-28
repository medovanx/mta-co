using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Lightning skill book. Elementary XP skill learned after Spirit 25, calling a lighting to hit the surrounding targets.
    /// </summary>
    [ItemHandler(Lightning)]
    public static class LightningHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Class is >= 130 and <= 135 ||
                client.Entity.Class is >= 140 and <= 145 && client.Entity.Level >= 15 ||
                client.Entity.Class == 100 || client.Entity.Class == 101) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1010 });
            }
        }
    }
}

