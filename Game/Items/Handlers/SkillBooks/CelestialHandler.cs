using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Celestial skill book. Scepter skill. Increase attack.
    /// </summary>
    [ItemHandler(Celestial)]
    public static class CelestialHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.Celestial });
        }
    }
}

