using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles DragonTail skill book. Wand skill. Attack multiple targets in a straight line.
    /// </summary>
    [ItemHandler(DragonTail)]
    public static class DragonTailHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 40) {
                client.MessageBox("You need to be at least level 40!");
                return;
            }

            if (SpellTable.SpellInformations[Spells.DragonTail][0].WeaponSubtype
                .Where(prof => client.Proficiencies.ContainsKey(prof))
                .Any(prof => client.Proficiencies[prof].Level >= 5)) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = Spells.DragonTail });
                return;
            }

            client.MessageBox("You need level 5 at Wand proficiency!");
        }
    }
}