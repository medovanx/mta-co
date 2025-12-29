using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles FlyingMoon skill book. An XP skill for level 40+ Warrior. Deals high damage on a single target from a distance.
    /// </summary>
    [ItemHandler(FlyingMoon)]
    public static class FlyingMoonHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 40) {
                client.MessageBox("You need to be at least level 40!");
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.FlyingMoon });
        }
    }
}