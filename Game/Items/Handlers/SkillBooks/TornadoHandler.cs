using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;
using static MTA.Game.Constants.EntityClass;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Tornado skill book. For FireTaoist to learn after Spirit 160 and Fire level 3, calling thunderbolt and gale to hit the target.
    /// </summary>
    [ItemHandler(Tornado)]
    public static class TornadoHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (!IsFireTaoist(client.Entity.Class)) {
                client.MessageBox("Only Fire Taoists can learn this skill!");
                return;
            }
            if (client.Entity.Level < 90) {
                client.MessageBox("You need to be at least level 90!");
                return;
            }
            if (client.Entity.Spirit < 160) {
                client.MessageBox("You need at least 160 spirit!");
                return;
            }
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddSpell(new Spell(true) { ID = Spells.Tornado });
        }
    }
}

