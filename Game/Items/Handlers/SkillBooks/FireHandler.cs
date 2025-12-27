using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Fire skill book. Junior magic learned after Spirit 80, calling a thunderbolt to hit the target.
    /// </summary>
    [ItemHandler(Fire)]
    public static class FireHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Spirit >= 80) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1001 });
            }
            else {
                client.Send(new Message("You need atleast 80 spirit!", Color.Tan, Message.TopLeft));
            }
        }
    }
}

