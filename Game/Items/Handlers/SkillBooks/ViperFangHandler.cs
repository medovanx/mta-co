using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles ViperFang skill book. Spear skill. Attack multiple targets in a straight line.
    /// </summary>
    [ItemHandler(ViperFang)]
    public static class ViperFangHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 40) {
                client.MessageBox("You need to be at least level 40!");
                return;
            }

            if (SpellTable.SpellInformations[Spells.ViperFang][0].WeaponSubtype
                .Where(prof => client.Proficiencies.ContainsKey(prof))
                .Any(prof => client.Proficiencies[prof].Level >= 5)) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = Spells.ViperFang });
                return;
            }

            client.MessageBox("You need level 5 at Spear proficiency!");
        }
    }
}