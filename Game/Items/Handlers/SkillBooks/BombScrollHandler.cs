using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;
using static MTA.Game.Constants.EntityClass;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles BombScroll skill book. A spell for Level 80 Fire Wizards.
    /// </summary>
    [ItemHandler(BombScroll)]
    public static class BombScrollHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (!IsFireTaoist(client.Entity.Class)) {
                client.MessageBox("Only Fire Taoists can learn this skill!");
                return;
            }
            if (client.Entity.Level < 80) {
                client.MessageBox("You need to be at least level 80!");
                return;
            }
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.Bomb });
        }
    }
}