using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Tornado skill book. For FireTaoist to learn after Spirit 160 and Fire level 3, calling thunderbolt and gale to hit the target.
    /// </summary>
    [ItemHandler(Tornado)]
    public static class TornadoHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Class is >= 140 and <= 145 && client.Entity.Level >= 90) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1002 });
            }
        }
    }
}

