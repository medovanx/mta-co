using System.Linq;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.Market {
    /// <summary>
    /// Divorce Manager - Helps players divorce their spouses
    /// </summary>
    [NpcHandler(600055)]
    public static class Npc_DivorceManager {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            const uint meteorTearId = 1088002;
            switch (npcRequest.OptionID) {
                case 0: {
                    if (client.Entity.Spouse == "None") {
                        dialog.Text(
                            "Hey there! You're not married yet, which is great!\nBut please, don't get married - I don't want to see you here again!");
                        dialog.Option("Haha, I'll stay single then.", 255);
                    }
                    else {
                        dialog.Text(
                            "Hey there! Listen, I can help you divorce your spouse. You'll need a Meteor Tear to proceed. Are you ready?");
                        dialog.Option("Yes, divorce me.", 1);
                        dialog.Option("I want to buy a Meteor Tear.", 2);
                        dialog.Option("Nothing, thank you.", 255);
                    }

                    dialog.Send();

                    break;
                }
                case 1: {
                    if (!client.Inventory.Contains(meteorTearId, 1)) {
                        dialog.Text("Sorry, you need a Meteor Tear to divorce your spouse.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }
                    else {
                        client.Inventory.Remove(meteorTearId, 1);
                        var spouseName = client.Entity.Spouse;

                        // Divorce the player
                        client.Entity.Spouse = "None";

                        // Find and divorce the spouse
                        var spouse = Program.Values.FirstOrDefault(p => p.Entity.Name == spouseName);
                        if (spouse != null) {
                            spouse.Entity.Spouse = "None";
                            spouse.Send(new Message($"You have been divorced from {client.Entity.Name}.",
                                System.Drawing.Color.Yellow, Message.Service));
                        }

                        dialog.Text("You are free now. I hope you will find a good person for you.");
                        dialog.Option("Alright, that's what I'll do.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 2: {
                    const uint meteorTearCostCPs = 2500;
                    if (client.Entity.ConquerPoints < meteorTearCostCPs) {
                        dialog.Text(
                            $"Sorry, you don't have enough CPs to buy a Meteor Tear. You need {meteorTearCostCPs} CPs.");
                        dialog.Option("I understand.", 255);
                    }
                    else {
                        client.Entity.ConquerPoints -= meteorTearCostCPs;
                        client.Inventory.Add(meteorTearId, 0, 1);
                        dialog.Text("Here is your Meteor Tear! Now you can proceed with the divorce if you wish.");
                        dialog.Option("Thank you!", 255);
                    }

                    dialog.Send();
                    break;
                }
            }
        }
    }
}