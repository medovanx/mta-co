using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.QuestAndOther;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Epic Weapon items that grant epic weapons when used.
    /// </summary>
    [ItemHandler(LifesEye, TrojanEpic, DivinePanacea, EpicWeaponToken)]
    public static class EpicWeaponHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var weaponName = "";
            var message = "";

            if (item.ID == LifesEye) {
                Network.PacketHandler.CheckCommand2("@tegotegatege Nobunaga`sTwistedClaw Super 6 3 245 13 13", client);
                weaponName = "Nobunaga's Claws";
                message = " " + client.Entity.Name +
                          " Got the life'sEye . and Obtained a Super +6 -3 . Nobunaga's Claws ( NinjaEpicWeapon )";
            }
            else if (item.ID == TrojanEpic) {
                Network.PacketHandler.CheckCommand2("@tegotegatege FrankoCrossSaber Super 6 3 245 13 13", client);
                weaponName = "FrankoCrossSaber";
                message = " " + client.Entity.Name +
                          " Got the SolarBlade . and Obtained a Super +6 -3 . FrankoCrossSaber ( TrojanEpicWeapon )";
            }
            else if (item.ID == DivinePanacea) {
                Network.PacketHandler.CheckCommand2("@tegotegatege DivineBacksword Super 6 3 245 3 3", client);
                weaponName = "ImperialBacksword";
                message = " " + client.Entity.Name +
                          " Got the DivinePanacea . and Obtained a Super +6 -3 . ImperialBacksword ( TaoistEpicWeapon )";
            }
            else if (item.ID == EpicWeaponToken) {
                Network.PacketHandler.CheckCommand2("@tegotegatege DesireHossu Super 6 3 245 3 3", client);
                weaponName = "UniverseHossu";
                message = " " + client.Entity.Name +
                          " Got the EpicWeaponToken . and Obtained a Super +6 -3 . UniverseHossu ( TaoistEpicWeapon )";
            }

            client.Inventory.Remove(item, Enums.ItemUse.Remove);
            Kernel.SendWorldMessage(
                new Message(message, Color.Cyan, Message.Center), Program.Values);
        }
    }
}

