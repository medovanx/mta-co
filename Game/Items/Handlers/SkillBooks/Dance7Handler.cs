using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Dance7 skill book. A book to learn dance at level 40.
    /// </summary>
    [ItemHandler(Dance7)]
    public static class Dance7Handler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 40) {
                client.MessageBox("You need to be at least level 40!");
                return;
            }
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.Dance7 });
        }
    }
}

