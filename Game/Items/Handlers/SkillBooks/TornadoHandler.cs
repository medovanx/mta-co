using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;
using static MTA.Game.EntityClassConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Tornado skill book. For FireTaoist to learn after Spirit 160 and Fire level 3, calling thunderbolt and gale to hit the target.
    /// </summary>
    [ItemHandler(Tornado)]
    public static class TornadoHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (IsFireTaoist(client.Entity.Class) && client.Entity.Level >= 90) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1002 });
            }
        }
    }
}

