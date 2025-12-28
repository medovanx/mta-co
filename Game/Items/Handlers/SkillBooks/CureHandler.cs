using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Cure skill book. Elementary magic learned after Spirit 30, healing hit points.
    /// </summary>
    [ItemHandler(Cure)]
    public static class CureHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Spirit >= 30) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1005 });
            }
        }
    }
}

