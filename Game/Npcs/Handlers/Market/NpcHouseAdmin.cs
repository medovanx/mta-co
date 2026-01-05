using MTA.Client;
using MTA.Game.Features.House;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.Market {
    /// <summary>
    /// House Admin - Provides House related services
    /// </summary>
    [NpcHandler(28)]
    public static class NpcHouseAdmin {
        public static void Handle(GameState player, NpcRequest npcRequest, MTA.Npcs dialog) {
            const uint housePermitId = 721170;
            const uint housePermitCostCPs = 99;
            const uint houseBuildingCostGold = 300000;
            const uint upgradeCertificateLv2Id = 721174;
            const uint upgradeCertificateLv2CostCPs = 199;
            const uint upgradeCertificateLv3Id = 729200;
            const uint upgradeCertificateLv3CostCPs = 259;
            const uint upgradeCertificateLv4Id = 729201;
            const uint upgradeCertificateLv4CostCPs = 299;
            const uint upgradeCertificateLv5Id = 3001548;
            const uint upgradeCertificateLv5CostCPs = 9999;
            switch (npcRequest.OptionID) {
                case 0: {
                    if (!House.Houses.TryGetValue(player.Entity.UID, out var value)) {
                        dialog.Text("Good day, my friend, How may I help you");
                        dialog.Option("I want to buy a house.", 1);
                        dialog.Option("Enter Spouse House", 4);
                        dialog.Option("Buy house certificate.", 6);
                        dialog.Option("Just Passing By!.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("Good day, my friend, How may I help you");
                        dialog.Option($"Enter my house (Lv.{value.Level})", 3);
                        var spouseHouse = House.SpouseHouse(player.Entity.Spouse);
                        if (spouseHouse != null) {
                            dialog.Option($"Enter my spouse's house (Lv.{spouseHouse.Level})", 4);
                        }

                        if (value.Level < 5) {
                            dialog.Option($"Upgrade my house to Lv.{value.Level + 1}", 5);
                        }

                        if (value.Level > 1) {
                            dialog.Option($"Downgrade my house to Lv.{value.Level - 1}", 13);
                        }

                        dialog.Option("Buy house certificate.", 6);
                        dialog.Option("Just Passing By!.", 255);
                        dialog.Send();
                    }

                    break;
                }
                // Give a House Permit and build a house
                case 1: {
                    dialog.Text("So, you've decided to build your own house?\n");
                    dialog.Text(
                        $"You can give me a House Permit and {houseBuildingCostGold} silver and get a new house.");
                    dialog.Option($"Here you go!", 2);
                    dialog.Option("I'm not interested.", 255);
                    dialog.Send();
                    break;
                }
                // Build a house
                case 2: {
                    if (player.Entity.Money >= houseBuildingCostGold && player.Inventory.Contains(housePermitId, 1)) {
                        player.Entity.Money -= houseBuildingCostGold;
                        player.Inventory.Remove(housePermitId, 1);
                        House.CreateHouse(player);
                        dialog.Text("Congratulations you got a new house!");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("Sorry, you don't have enough money or a House Permit.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
                // Enter my house
                case 3: {
                    var info = House.Houses[player.Entity.UID];
                    const ushort maxTeleportRange = 18;

                    // If the player is in a team and is a team leader, teleport all team members to the house
                    if (player.Team?.Leader == player) {
                        foreach (var teammate in player.Team.Teammates) {
                            // Skip the leader (they'll be teleported at the end)
                            if (teammate == player) continue;

                            // Check if team member is on the same map
                            if (teammate.Entity.MapID != player.Entity.MapID) {
                                continue;
                            }

                            var distance = Kernel.GetDistance(
                                player.Entity.X, player.Entity.Y,
                                teammate.Entity.X, teammate.Entity.Y);

                            if (distance <= maxTeleportRange) {
                                House.Teleport(teammate, info);
                            }
                        }
                        // Always teleport the team leader to the house
                    }

                    // If the player is not a team leader, only teleport the player to the house
                    House.Teleport(player, info);
                    break;
                }
                // Enter my spouse's house
                case 4: {
                    if (House.SpouseHouse(player.Entity.Spouse) != null) {
                        var spouseHouse = House.SpouseHouse(player.Entity.Spouse);
                        House.Teleport(player, spouseHouse!);
                    }
                    else {
                        dialog.Text("Sorry you're not married or your spouse doesn't have a house.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
                // Upgrade my house
                case 5: {
                    var currentLevel = House.Houses[player.Entity.UID].Level;

                    // Check if house is already at max level
                    if (currentLevel >= 5) {
                        dialog.Text("You have already upgraded your house to the maximum level.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                        break;
                    }

                    uint requiredCertificateId = 0;
                    var certificateName = "";

                    switch (currentLevel) {
                        // Determine which certificate is needed based on current level
                        case 1:
                            requiredCertificateId = upgradeCertificateLv2Id;
                            certificateName = "Upgrade Certificate (Lv.2)";
                            break;
                        case 2:
                            requiredCertificateId = upgradeCertificateLv3Id;
                            certificateName = "Upgrade Certificate (Lv.3)";
                            break;
                        case 3:
                            requiredCertificateId = upgradeCertificateLv4Id;
                            certificateName = "Upgrade Certificate (Lv.4)";
                            break;
                        case 4:
                            requiredCertificateId = upgradeCertificateLv5Id;
                            certificateName = "Upgrade Certificate (Lv.5)";
                            break;
                    }

                    // Verify certificate is required and player has it
                    if (requiredCertificateId > 0 && player.Inventory.Contains(requiredCertificateId, 1)) {
                        player.Inventory.Remove(requiredCertificateId, 1);
                        House.UpgradeHouse(player, (byte)currentLevel);
                        dialog.Text($"Congratulations! Your house has been upgraded to level {currentLevel + 1}");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text($"Sorry, you don't have an {certificateName}. Please come back later.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 6: {
                    dialog.Text("What would you like to buy?");
                    dialog.Option($"House Permit: {housePermitCostCPs} CPs", 7);
                    dialog.Option($"Upgrade Certificate (Lv.2): {upgradeCertificateLv2CostCPs} CPs.", 8);
                    dialog.Option($"Upgrade Certificate (Lv.3): {upgradeCertificateLv3CostCPs} CPs.", 9);
                    dialog.Option($"Upgrade Certificate (Lv.4): {upgradeCertificateLv4CostCPs} CPs.", 10);
                    dialog.Option($"Upgrade Certificate (Lv.5): {upgradeCertificateLv5CostCPs} CPs.", 11);
                    dialog.Option("Let me think about it.", 255);
                    dialog.Send();
                    break;
                }
                // Buy a house permit
                case 7: {
                    if (player.Entity.ConquerPoints < housePermitCostCPs) {
                        dialog.Text("Sorry, you don't have enough CPs to buy a house permit.");
                        dialog.Option("Oh Sorry!.", 255);
                    }
                    else {
                        player.Entity.ConquerPoints -= housePermitCostCPs;
                        player.Inventory.Add(housePermitId, 0, 1);
                        dialog.Text("Here is your House Permit!");
                        dialog.Option("Thank you!", 255);
                    }

                    dialog.Send();

                    break;
                }
                // Buy an upgrade certificate (Lv.2)
                case 8: {
                    if (player.Entity.ConquerPoints < upgradeCertificateLv2CostCPs) {
                        dialog.Text("Sorry, you don't have enough CPs to buy an upgrade certificate (Lv.2).");
                        dialog.Option("Oh Sorry!.", 255);
                    }
                    else {
                        player.Entity.ConquerPoints -= upgradeCertificateLv2CostCPs;
                        player.Inventory.Add(upgradeCertificateLv2Id, 0, 1);
                        dialog.Text("Here is your Upgrade Certificate (Lv.2)!");
                        dialog.Option("Thank you!", 255);
                    }

                    dialog.Send();

                    break;
                }
                // Buy an upgrade certificate (Lv.3)
                case 9: {
                    if (player.Entity.ConquerPoints < upgradeCertificateLv3CostCPs) {
                        dialog.Text("Sorry, you don't have enough CPs to buy an upgrade certificate (Lv.3).");
                        dialog.Option("Oh Sorry!.", 255);
                    }
                    else {
                        player.Entity.ConquerPoints -= upgradeCertificateLv3CostCPs;
                        player.Inventory.Add(upgradeCertificateLv3Id, 0, 1);
                        dialog.Text("Here is your Upgrade Certificate (Lv.3)!");
                        dialog.Option("Thank you!", 255);
                    }

                    dialog.Send();

                    break;
                }
                // Buy an upgrade certificate (Lv.4)
                case 10: {
                    if (player.Entity.ConquerPoints < upgradeCertificateLv4CostCPs) {
                        dialog.Text("Sorry, you don't have enough CPs to buy an upgrade certificate (Lv.4).");
                        dialog.Option("Oh Sorry!.", 255);
                    }
                    else {
                        player.Entity.ConquerPoints -= upgradeCertificateLv4CostCPs;
                        player.Inventory.Add(upgradeCertificateLv4Id, 0, 1);
                        dialog.Text("Here is your Upgrade Certificate (Lv.4)!");
                        dialog.Option("Thank you!", 255);
                    }

                    dialog.Send();

                    break;
                }
                // Buy an upgrade certificate (Lv.5)
                case 11: {
                    if (player.Entity.ConquerPoints < upgradeCertificateLv5CostCPs) {
                        dialog.Text("Sorry, you don't have enough CPs to buy an upgrade certificate (Lv.5).");
                        dialog.Option("Oh Sorry!.", 255);
                    }
                    else {
                        player.Entity.ConquerPoints -= upgradeCertificateLv5CostCPs;
                        player.Inventory.Add(upgradeCertificateLv5Id, 0, 1);
                        dialog.Text("Here is your Upgrade Certificate (Lv.5)!");
                        dialog.Option("Thank you!", 255);
                    }

                    dialog.Send();

                    break;
                }
                // Downgrade my house
                case 13: {
                    var currentLevel = House.Houses[player.Entity.UID].Level;

                    // Check if house is already at minimum level
                    if (currentLevel <= 1) {
                        dialog.Text("Your house is already at the minimum level and cannot be downgraded further.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                        break;
                    }

                    dialog.Text(
                        $"Are you sure you want to downgrade your house from level {currentLevel} to level {currentLevel - 1}?\nThis action is free but cannot be undone.");
                    dialog.Option("Yes, downgrade my house", 14);
                    dialog.Option("No, I changed my mind", 255);
                    dialog.Send();
                    break;
                }
                // Confirm downgrade
                case 14: {
                    var currentLevel = House.Houses[player.Entity.UID].Level;

                    if (currentLevel <= 1) {
                        dialog.Text("Your house is already at the minimum level and cannot be downgraded further.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                        break;
                    }

                    House.DowngradeHouse(player, (byte)currentLevel);
                    dialog.Text($"Your house has been downgraded to level {currentLevel - 1}.");
                    dialog.Option("Thank you!", 255);
                    dialog.Send();
                    break;
                }
            }
        }
    }
}