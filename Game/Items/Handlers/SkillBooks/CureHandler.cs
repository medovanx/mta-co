using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Cure skill book. Elementary magic learned after Spirit 30, healing hit points.
    /// </summary>
    [ItemHandler(Cure)]
    public static class CureHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Spirit < 30) {
                client.MessageBox("You need at least 30 spirit!");
                return;
            }
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.Cure });
        }
    }
}