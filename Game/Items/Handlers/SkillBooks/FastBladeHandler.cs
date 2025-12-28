using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.SkillBooks;

namespace MTA.Game.Items.Handlers.SkillBooks {
    /// <summary>
    /// Handles FastBlade skill book. Blade skill. Attack multiple targets in a straight line.
    /// </summary>
    [ItemHandler(FastBlade)]
    public static class FastBladeHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 40) return;
            if (SpellTable.SpellInformations[1045][0].WeaponSubtype
                .Where(prof => client.Proficiencies.ContainsKey(prof))
                .Any(prof => client.Proficiencies[prof].Level >= 5)) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.AddSpell(new Spell(true) { ID = 1045 });
                return;
            }

            client.Send(new Message("You need level 5 at blade proficiency!", Color.Tan, Message.TopLeft));
        }
    }
}