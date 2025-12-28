using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles DragonTail skill book. Wand skill. Attack multiple targets in a straight line.
    /// </summary>
    [ItemHandler(DragonTail)]
    public static class DragonTailHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 40) return;
            foreach (var prof in SpellTable.SpellInformations[11000][0].WeaponSubtype
                         .Where(prof => client.Proficiencies.ContainsKey(prof))
                         .Where(prof => client.Proficiencies[prof].Level >= 5)) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 11000 });
            }

            client.Send(new Message("You need level 5 at Wand proficiency!", Color.Tan, Message.TopLeft));
        }
    }
}