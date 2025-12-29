using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;
using static MTA.Game.Constants.EntityClass;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles DivineHare skill book. Disguise skill. Jump speed increased in the disguise of DivineHare. For Water Taoist of level 54 to learn.
    /// </summary>
    [ItemHandler(DivineHare)]
    public static class DivineHareHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (!IsWaterTaoist(client.Entity.Class)) {
                client.MessageBox("Only Water Taoists can learn this skill!");
                return;
            }
            if (client.Entity.Level < 54) {
                client.MessageBox("You need to be at least level 54!");
                return;
            }
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.DivineHare });
        }
    }
}

