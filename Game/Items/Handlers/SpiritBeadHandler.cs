using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using _String = MTA.Network.GamePackets._String;
using QuestID = MTA.Network.GamePackets.QuestID;
using static MTA.Game.Constants.Items.QuestAndOther;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SpiritBead items that complete spirit quests when used.
    /// </summary>
    [ItemHandler(NormalSpiritBead, RefinedSpiritBead, UniqueSpiritBead, EliteSpiritBead, SuperSpiritBead)]
    public static class SpiritBeadHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.SpiritBeadQ.CollectedSpirits < client.SpiritBeadQ.Requiredspirits) {
                client.Send(new Message(
                    "Collected spirits : " + client.SpiritBeadQ.CollectedSpirits + ", You need : " +
                    client.SpiritBeadQ.Requiredspirits + ", To finish the task.", Color.Red, Message.TopLeft));
                return;
            }

            client.Send(new Message("Congratulatons, You have finished SpiritTask", Color.Red, Message.TopLeft));
            client.SpiritBeadQ.Reset();

            var str = new _String(true) {
                Type = 10,
                UID = client.Entity.UID
            };
            str.Texts.Add("zf2-e300");

            client.Inventory.Add(ChiToken, 0, 1);

            byte expBalls = item.ID switch {
                NormalSpiritBead => 1,
                RefinedSpiritBead => (byte)1.5,
                UniqueSpiritBead => 2,
                EliteSpiritBead => (byte)2.5,
                SuperSpiritBead => 3,
                _ => 0
            };

            client.IncreaseExperience(client.ExpBalls += expBalls, false);
            client.Quests.FinishQuest(QuestID.Spirit_Beads);
            client.Inventory.Remove(item, Enums.ItemUse.Remove);
        }
    }
}

