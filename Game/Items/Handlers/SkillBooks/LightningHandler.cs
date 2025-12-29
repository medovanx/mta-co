using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;
using static MTA.Game.Constants.EntityClass;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles Lightning skill book. Elementary XP skill learned after Spirit 25, calling a lighting to hit the surrounding targets.
    /// </summary>
    [ItemHandler(Lightning)]
    public static class LightningHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Spirit < 25) {
                client.MessageBox("You need at least 25 spirit!");
                return;
            }

            if (IsWaterTaoist(client.Entity.Class) ||
                IsFireTaoist(client.Entity.Class) && client.Entity.Level >= 15) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = Spells.Lightning });
                return;
            }

            if (IsFireTaoist(client.Entity.Class) && client.Entity.Level < 15) {
                client.MessageBox("Fire Taoists need to be at least level 15!");
                return;
            }

            client.MessageBox("Only Taoists can learn this skill!");
        }
    }
}