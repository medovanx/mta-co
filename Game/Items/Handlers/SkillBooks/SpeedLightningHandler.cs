using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;
using static MTA.Game.Constants.EntityClass;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles SpeedLightning skill book. XP skill for Level 70 above Taoists.
    /// </summary>
    [ItemHandler(SpeedLightning)]
    public static class SpeedLightningHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (!IsTaoist(client.Entity.Class)) {
                client.MessageBox("Only Taoists can learn this skill!");
                return;
            }

            if (client.Entity.Level < 70) {
                client.MessageBox("You need to be at least level 70!");
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.SpeedLightning });
        }
    }
}