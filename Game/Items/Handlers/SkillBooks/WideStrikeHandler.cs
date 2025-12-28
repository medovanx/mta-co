using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles WideStrike skill book. Glaive skill. A passive skill, once activated, will cause a spread attack in frount of you.
    /// </summary>
    [ItemHandler(WideStrike)]
    public static class WideStrikeHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 1250 });
        }
    }
}

