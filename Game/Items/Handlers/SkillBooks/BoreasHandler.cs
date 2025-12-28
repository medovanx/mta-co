using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Boreas skill book. Poleaxe skill. Ranged skill.
    /// </summary>
    [ItemHandler(Boreas)]
    public static class BoreasHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 5050 });
        }
    }
}

