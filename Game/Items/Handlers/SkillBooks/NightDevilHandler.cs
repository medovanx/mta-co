using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles NightDevil skill book. Disguise skill. Damage and attack speed increased greatly in the disguise of NightDevil. Learn it at level 70.
    /// </summary>
    [ItemHandler(NightDevil)]
    public static class NightDevilHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 70) {
                client.MessageBox("You need to be at least level 70!");
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.NightDevil });
        }
    }
}