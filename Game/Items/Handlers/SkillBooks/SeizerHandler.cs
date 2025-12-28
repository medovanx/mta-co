using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Seizer skill book. Hook skill. Enhance attack.
    /// </summary>
    [ItemHandler(Seizer)]
    public static class SeizerHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 7000 });
        }
    }
}

