using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Phoenix skill book. Sword skill. A passive skill, once activated, will enhance your attack greatly.
    /// </summary>
    [ItemHandler(Phoenix)]
    public static class PhoenixHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.Phoenix });
        }
    }
}