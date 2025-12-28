using MTA.Client;
using MTA.Database;
using MTA.Network;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.QuestAndOther;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Chi Power Max item that maxes out all chi power attributes.
    /// </summary>
    [ItemHandler(ChiPowerMaxItem)]
    public static class ChiPowerMaxHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.ChiPowers.Count == 0) {
                client.MessageBox("You Don't Open chi yet");
                return;
            }

            for (var i = 0; i < client.ChiPowers.Count; i++) {
                var Mode = i + 1;
                for (var ii = 0; ii < 4; ii++) {
                    var pos = ii;
                    var powers = client.ChiPowers[Mode - 1];
                    var attributes = powers.Attributes;
                    attributes[pos].Value = (ushort)Enums.ChiMaxValues(attributes[pos].Type);
                    powers.CalculatePoints();
                    ChiTable.Sort((Enums.ChiPowerType)Mode);
                    powers.Power = (Enums.ChiPowerType)Mode;
                    client.Send(new ChiPowers(true).Query(client));

                    #region update ranking

                    ChiTable.ChiData[] array = null;
                    switch ((Enums.ChiPowerType)Mode) {
                        case Enums.ChiPowerType.Dragon:
                            array = ChiTable.Dragon;
                            break;

                        case Enums.ChiPowerType.Phoenix:
                            array = ChiTable.Phoenix;
                            break;

                        case Enums.ChiPowerType.Tiger:
                            array = ChiTable.Tiger;
                            break;

                        case Enums.ChiPowerType.Turtle:
                            array = ChiTable.Turtle;
                            break;
                    }

                    foreach (var chiData in array) {
                        if (Kernel.GamePool.TryGetValue(chiData.UID, out var pClient)) {
                            if (pClient is not { ChiData: not null }) continue;
                            PacketHandler.SendRankingQuery(new GenericRanking(true) { Mode = GenericRanking.QueryCount },
                                pClient, GenericRanking.Chi + (uint)Mode,
                                pClient.ChiData.SelectRank((Enums.ChiPowerType)Mode),
                                pClient.ChiData.SelectPoints((Enums.ChiPowerType)Mode));
                            if (pClient.Entity.UID == client.Entity.UID ||
                                pClient.ChiData.SelectRank((Enums.ChiPowerType)Mode) < 50)
                                pClient.LoadItemStats();
                        }
                    }

                    #endregion

                    ChiTable.Save(client);
                }
            }

            client.Inventory.Remove(item, Enums.ItemUse.Remove);
        }
    }
}

