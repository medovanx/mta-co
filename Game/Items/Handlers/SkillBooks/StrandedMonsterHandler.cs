using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles StrandedMonster skill book. Halbert skill. Ranged attack.
    /// </summary>
    [ItemHandler(StrandedMonster)]
    public static class StrandedMonsterHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 5020 });
        }
    }
}