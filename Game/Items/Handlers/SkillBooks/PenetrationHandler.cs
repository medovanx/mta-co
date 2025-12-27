using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Penetration skill book. Dagger skill. A passive skill, once activated, will enhance your attack greatly.
    /// </summary>
    [ItemHandler(Penetration)]
    public static class PenetrationHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 1290 });
        }
    }
}

