using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Thunder skill book. Elementary magic learned after Spirit 20, calling a lighting to hit the target.
    /// </summary>
    [ItemHandler(Thunder)]
    public static class ThunderHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Spirit >= 20) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1000 });
            }
            else {
                client.Send(new Message("You need atleast 20 spirit!", Color.Tan, Message.TopLeft));
            }
        }
    }
}

