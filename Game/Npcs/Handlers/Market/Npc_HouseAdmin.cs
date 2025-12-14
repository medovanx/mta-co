using System;
using System.Collections.Generic;
using System.Linq;
using static MTA.Game.Enums;
using MTA.Network;
using MTA.Network.GamePackets;
using MTA.MrNiTro.Systems.House;

namespace MTA.Game.Npcs.Handlers.Market
{
    /// <summary>
    /// House Admin NPC - Provides House related services
    /// </summary>
    [NpcHandler(115522007)]
    public static class Npc_HouseAdmin
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            const uint HousePermitId = 721170;
            const uint HousePermitCostCPs = 99;
            const uint HouseBuildingCostGold = 300000;
            const uint ItemBoxId = 721189;
            const uint ItemBoxCostGold = 100000;
            const uint UpgradeCertificateLv2Id = 721174;
            const uint UpgradeCertificateLv2CostCPs = 199;
            const uint UpgradeCertificateLv3Id = 729200;
            const uint UpgradeCertificateLv3CostCPs = 259;
            const uint UpgradeCertificateLv4Id = 729201;
            const uint UpgradeCertificateLv4CostCPs = 299;
            const uint UpgradeCertificateLv5Id = 3001548;
            const uint UpgradeCertificateLv5CostCPs = 9999;
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        if (!House.Houses.ContainsKey(client.Entity.UID))
                        {
                            dialog.Text("Good day, my friend, How may I help you");
                            dialog.Option("I want to buy a house.", 1);
                            dialog.Option("Enter Spouse House", 4);
                            dialog.Option("Buy house certificate.", 6);
                            dialog.Option("Just Passing By!.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("Good day, my friend, How may I help you");
                            dialog.Option("Enter my house", 3);
                            dialog.Option("Enter my spouse's house", 4);
                            dialog.Option("Upgrade my house", 5);
                            dialog.Option("Buy house certificate.", 6);
                            dialog.Option("Just Passing By!.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                // Give a House Permit and build a house
                case 1:
                    {
                        dialog.Text("So, you've decided to build your own house?\n");
                        dialog.Text($"You can give me a House Permit and {HouseBuildingCostGold} silver and get a new house.");
                        dialog.Option($"Here you go!", 2);
                        dialog.Option("I'm not interested.", 255);
                        dialog.Send();
                        break;
                    }
                // Build a house
                case 2:
                    {
                        if (client.Entity.Money >= HouseBuildingCostGold && client.Inventory.Contains(HousePermitId, 1))
                        {
                            client.Entity.Money -= HouseBuildingCostGold;
                            client.Inventory.Remove(HousePermitId, 1);
                            House.createhouse(client);
                            dialog.Text("Congratulations you got a new house!");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("Sorry, you don't have enough money or a House Permit.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                // Enter my house
                case 3:
                    {
                        var info = House.Houses[client.Entity.UID];

                        // If the player is a team leader, teleport all team members to the house
                        if ((client.Team != null) && client.Team.TeamLeader)
                        {
                            foreach (Client.GameState state2 in client.Team.Teammates)
                            {
                                House.TelePort(state2, info);
                            }
                        }
                        // If the player is not a team leader, only teleport the player to the house
                        else
                        {
                            House.TelePort(client, info);
                        }
                        break;
                    }
                // Enter my spouse's house
                case 4:
                    {
                        if (House.SpouseHouse(client.Entity.Spouse) != null)
                        {
                            var spouse = House.SpouseHouse(client.Entity.Spouse);
                            House.TelePort(client, spouse);
                        }
                        else
                        {
                            dialog.Text("Sorry you're not married or your spouse doesn't have a house.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                // Upgrade my house
                case 5:
                    {
                        var currentLevel = House.Houses[client.Entity.UID].level;

                        // Check if house is already at max level
                        if (currentLevel >= 5)
                        {
                            dialog.Text("You have already upgraded your house to the maximum level.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                            break;
                        }

                        uint requiredCertificateId = 0;
                        string certificateName = "";

                        // Determine which certificate is needed based on current level
                        if (currentLevel == 1)
                        {
                            requiredCertificateId = UpgradeCertificateLv2Id;
                            certificateName = "Upgrade Certificate (Lv.2)";
                        }
                        else if (currentLevel == 2)
                        {
                            requiredCertificateId = UpgradeCertificateLv3Id;
                            certificateName = "Upgrade Certificate (Lv.3)";
                        }
                        else if (currentLevel == 3)
                        {
                            requiredCertificateId = UpgradeCertificateLv4Id;
                            certificateName = "Upgrade Certificate (Lv.4)";
                        }
                        else if (currentLevel == 4)
                        {
                            requiredCertificateId = UpgradeCertificateLv5Id;
                            certificateName = "Upgrade Certificate (Lv.5)";
                        }

                        // Verify certificate is required and player has it
                        if (requiredCertificateId > 0 && client.Inventory.Contains(requiredCertificateId, 1))
                        {
                            client.Inventory.Remove(requiredCertificateId, (byte)1);
                            House.UpgradeHouse(client, (byte)currentLevel);
                            dialog.Text($"Congratulations! Your house has been upgraded to level {currentLevel + 1}");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text($"Sorry, you don't have a {certificateName}. Please come back later.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 6:
                    {
                        dialog.Text("What would you like to buy?");
                        dialog.Option($"House Permit: {HousePermitCostCPs} CPs", 7);
                        dialog.Option($"Upgrade Certificate (Lv.2): {UpgradeCertificateLv2CostCPs} CPs.", 8);
                        dialog.Option($"Upgrade Certificate (Lv.3): {UpgradeCertificateLv3CostCPs} CPs.", 9);
                        dialog.Option($"Upgrade Certificate (Lv.4): {UpgradeCertificateLv4CostCPs} CPs.", 10);
                        dialog.Option($"Upgrade Certificate (Lv.5): {UpgradeCertificateLv5CostCPs} CPs.", 11);
                        dialog.Option($"Item Box: {ItemBoxCostGold} gold", 12);
                        dialog.Option("Let me think about it.", 255);
                        dialog.Send();
                        break;
                    }
                // Buy a house permit
                case 7:
                    {
                        if (client.Entity.ConquerPoints < HousePermitCostCPs)
                        {
                            dialog.Text("Sorry, you don't have enough CPs to buy a house permit.");
                            dialog.Option("Oh Sorry!.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            client.Entity.ConquerPoints -= HousePermitCostCPs;
                            client.Inventory.Add(HousePermitId, 0, 1);
                            dialog.Text("Here is your House Permit!");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        break;
                    }
                // Buy an upgrade certificate (Lv.2)
                case 8:
                    {
                        if (client.Entity.ConquerPoints < UpgradeCertificateLv2CostCPs)
                        {
                            dialog.Text("Sorry, you don't have enough CPs to buy an upgrade certificate (Lv.2).");
                            dialog.Option("Oh Sorry!.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            client.Entity.ConquerPoints -= UpgradeCertificateLv2CostCPs;
                            client.Inventory.Add(UpgradeCertificateLv2Id, 0, 1);
                            dialog.Text("Here is your Upgrade Certificate (Lv.2)!");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        break;
                    }
                // Buy an upgrade certificate (Lv.3)
                case 9:
                    {
                        if (client.Entity.ConquerPoints < UpgradeCertificateLv3CostCPs)
                        {
                            dialog.Text("Sorry, you don't have enough CPs to buy an upgrade certificate (Lv.3).");
                            dialog.Option("Oh Sorry!.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            client.Entity.ConquerPoints -= UpgradeCertificateLv3CostCPs;
                            client.Inventory.Add(UpgradeCertificateLv3Id, 0, 1);
                            dialog.Text("Here is your Upgrade Certificate (Lv.3)!");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        break;
                    }
                // Buy an upgrade certificate (Lv.4)
                case 10:
                    {
                        if (client.Entity.ConquerPoints < UpgradeCertificateLv4CostCPs)
                        {
                            dialog.Text("Sorry, you don't have enough CPs to buy an upgrade certificate (Lv.4).");
                            dialog.Option("Oh Sorry!.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            client.Entity.ConquerPoints -= UpgradeCertificateLv4CostCPs;
                            client.Inventory.Add(UpgradeCertificateLv4Id, 0, 1);
                            dialog.Text("Here is your Upgrade Certificate (Lv.4)!");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        break;
                    }
                // Buy an upgrade certificate (Lv.5)
                case 11:
                    {
                        if (client.Entity.ConquerPoints < UpgradeCertificateLv5CostCPs)
                        {
                            dialog.Text("Sorry, you don't have enough CPs to buy an upgrade certificate (Lv.5).");
                            dialog.Option("Oh Sorry!.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            client.Entity.ConquerPoints -= UpgradeCertificateLv5CostCPs;
                            client.Inventory.Add(UpgradeCertificateLv5Id, 0, 1);
                            dialog.Text("Here is your Upgrade Certificate (Lv.5)!");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        break;
                    }
                // Buy an item box
                case 12:
                    {
                        if (client.Entity.Money < ItemBoxCostGold)
                        {
                            dialog.Text("Sorry, you don't have enough money to buy an item box.");
                            dialog.Option("Oh Sorry!.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            client.Entity.Money -= ItemBoxCostGold;
                            client.Inventory.Add(ItemBoxId, 0, 1);
                            dialog.Text("Here is your Item Box!");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        break;
                    }
            }
        }
    }
}
