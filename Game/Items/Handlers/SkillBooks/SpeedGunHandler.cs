using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles SpeedGun skill book. Spear skill. A passive skill, once activated, will damage your enemies in a straight line.
    /// </summary>
    [ItemHandler(SpeedGun)]
    public static class SpeedGunHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 1260 });
        }
    }
}

