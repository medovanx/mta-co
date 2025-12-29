using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Thunder skill book. Elementary magic learned after Spirit 20, calling a lighting to hit the target.
    /// </summary>
    [ItemHandler(Thunder)]
    public static class ThunderHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Spirit >= 20) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = Spells.Thunder });
            }
            else {
                client.MessageBox("You need at least 20 spirit!");
            }
        }
    }
}

