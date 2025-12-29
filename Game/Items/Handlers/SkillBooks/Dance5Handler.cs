using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Dance5 skill book. A book to learn dance at level 40.
    /// </summary>
    [ItemHandler(Dance5)]
    public static class Dance5Handler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 40) {
                client.MessageBox("You need to be at least level 40!");
                return;
            }
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.Dance5 });
        }
    }
}

