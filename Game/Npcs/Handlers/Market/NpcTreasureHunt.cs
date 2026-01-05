using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.QuestAndOther;
using static MTA.Game.Constants.Items.DragonSouls.P7;
using static MTA.Game.Constants.Items.RefineryPacks;

namespace MTA.Game.Npcs.Handlers.Market {
    /// <summary>
    /// P7 Sacred Treasures Envoy - Allows players to explore treasures using Savage Bones or CPs
    /// </summary>
    [NpcHandler(30)]
    public static class NpcTreasureHunt {
        private const uint CheapExplorationCost = 37;
        private const uint ExpensiveExplorationCost = 370;

        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            dialog.Avatar(0);
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        "There's a mysterious treasure sealed since ancient times. With a Savage Bone, you can explore and have a chance to get Sacred refinery materials, rare weapon accessories, and even the legendary P7 Dragon Soul.");
                    dialog.Option("I want to explore.", 1);
                    //dialog.Option("Special exploration.", 2);
                    dialog.Option("I see.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    dialog.Text(
                        "The Nemesis Tyrant in the Bloodshed Sea has the Savage Bones. You can use one to explore each time.");
                    dialog.Option("Use a Savage Bone.", 3);
                    if (client.Entity.Level >= 120) {
                        dialog.Option("Send me there.", 4);
                    }

                    dialog.Option("I see.", 255);
                    dialog.Send();
                    break;
                }
                case 2: {
                    dialog.Text(
                        $"If you don't have a Savage Bone, you can also pay {CheapExplorationCost} or {ExpensiveExplorationCost} CPs to explore. Of course, the more you pay, the better you get. How much would you like to pay?");
                    dialog.Option($"{CheapExplorationCost} CPs.", 5);
                    dialog.Option($"{ExpensiveExplorationCost} CPs.", 6);
                    dialog.Option("I see.", 255);
                    dialog.Send();
                    break;
                }
                case 3: {
                    if (client.Inventory.Contains(SavageBone, 1)) {
                        client.Inventory.Remove(SavageBone, 1);
                        var r = Kernel.Random.Next(1, 4);
                        switch (r) {
                            case 1:
                                client.Inventory.Add(P7WeaponSoulPack2, 0, 1);
                                client.MessageBox("You received a P7 Weapon Soul Pack.");
                                break;
                            default:
                                client.Inventory.Add(P7EquipmentSoulPack, 0, 1);
                                client.MessageBox("You received a P7 Equipment Soul Pack.");
                                break;
                        }
                    }
                    else {
                        dialog.Text("Sorry, you don't have a Savage Bone.");
                        dialog.Option("My bad.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 4: {
                    client.Entity.Teleport(3055, 61, 90);
                    break;
                }
                case 5: {
                    dialog.Text(
                        $"Are you sure you want to use {CheapExplorationCost} CPs to explore the P7 Sacred Treasures?");
                    dialog.Option("Yes.", 7);
                    dialog.Option("No.", 255);
                    dialog.Send();
                    break;
                }
                case 6: {
                    dialog.Text(
                        $"Are you sure you want to use {ExpensiveExplorationCost} CPs to explore the P7 Sacred Treasures?");
                    dialog.Option("Yes.", 8);
                    dialog.Option("No.", 255);
                    dialog.Send();
                    break;
                }
                case 7: {
                    if (client.Entity.ConquerPoints >= CheapExplorationCost) {
                        client.Entity.ConquerPoints -= CheapExplorationCost;
                        client.Inventory.Add(SacredRefineryPack, 0, 1);
                        client.MessageBox("You received a Sacred Refinery Pack.");
                    }
                    else {
                        dialog.Text("Sorry, you don't have enough CPs.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 8: {
                    if (client.Entity.ConquerPoints >= ExpensiveExplorationCost) {
                        client.Entity.ConquerPoints -= ExpensiveExplorationCost;
                        var r = Kernel.Random.Next(1, 4);
                        switch (r) {
                            case 1:
                                client.Inventory.Add(P7WeaponSoulPack2, 0, 1);
                                client.MessageBox("You received a P7 Weapon Soul Pack.");
                                break;
                            default:
                                client.Inventory.Add(P7EquipmentSoulPack, 0, 1);
                                client.MessageBox("You received a P7 Equipment Soul Pack.");
                                break;
                        }
                    }
                    else {
                        dialog.Text("Sorry, you don't have enough CPs.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}