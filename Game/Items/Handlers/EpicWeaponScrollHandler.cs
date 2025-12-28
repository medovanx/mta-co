using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Epic Weapon Scroll items that show quest information dialogs.
    /// </summary>
    [ItemHandler(NinjaScroll, TrojanScroll)]
    public static class EpicWeaponScrollHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.ID == NinjaScroll) {
                var dialog = new MTA.Npcs(client);
                dialog.Text(
                    "Welcome Ninja , The ninja EpicWeapon (Nabuganas Claws ) Was Forged Carfully by the Ancient FrankoNinja . Which mastered the NinjaClass and the Franko . Only the True ninjas who have Mastered Those Ancient Skills . are qualified to get that Legendary EpicWeapon . He/She will be Qualified to Take on the Extraordinary EpicQuest . To Take on the quest talk to DivineFranko in the Middle of Twincity's Square . GoodLuck Ninja .");
                dialog.Option("Iam on it!", 255);
                dialog.Send();
            }
            else if (item.ID == TrojanScroll) {
                var dialog = new MTA.Npcs(client);
                dialog.Text(
                    "Welcome Trojan . The TrojanEpicWeapon was Forged with the power of Light . Was Forged Carfully and Professionally by the Most Spectacular Trojans of the Ancient Times . the Weapon Was Stolen and Lost by the FlameDevastators Army . The EpicWeapon is a Saber . Which have an amazing Attack and Agillity . It's the Weapon which all the professional Trojan will be dreaming with RightNow ! . Only the Profesional Trojans Can Be Qualified to Take on this Legendary Hard Quest . Talk to TrojanEpicQuest NPC At Twincity Square for all the information . GoodLuck Trojan");
                dialog.Option("Iam on it!", 255);
                dialog.Send();
            }
        }
    }
}

