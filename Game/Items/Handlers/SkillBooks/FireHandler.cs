using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Fire skill book. Junior magic learned after Spirit 80, calling a thunderbolt to hit the target.
    /// </summary>
    [ItemHandler(Fire)]
    public static class FireHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Spirit >= 80) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = Spells.Fire });
            }
            else {
                client.MessageBox("You need at least 80 spirit!");
            }
        }
    }
}