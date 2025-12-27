using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles ViperFang skill book. Spear skill. Attack multiple targets in a straight line.
    /// </summary>
    [ItemHandler(ViperFang)]
    public static class ViperFangHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 40) return;
            if (SpellTable.SpellInformations[11005][0].WeaponSubtype
                .Where(prof => client.Proficiencies.ContainsKey(prof))
                .Any(prof => client.Proficiencies[prof].Level >= 5)) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 11005 });
                return;
            }

            client.Send(new Message("You need level 5 at Spear proficiency!", Color.Tan, Message.TopLeft));
        }
    }
}