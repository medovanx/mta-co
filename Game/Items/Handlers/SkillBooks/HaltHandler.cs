using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Halt skill book. Long hammer skill. A passive skill, once activated, will render the target stop attacking.
    /// </summary>
    [ItemHandler(Halt)]
    public static class HaltHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = 1300 });
        }
    }
}

