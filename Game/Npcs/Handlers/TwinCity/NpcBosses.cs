using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Bosses Teleporter - Teleports players to the boss monsters
    /// </summary>
    [NpcHandler(9297)]
    public static class NpcBosses {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            const uint entryFee = 200;
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text($"Hey, which boss do you want to teleport to? It will cost you {entryFee} gold.");
                    dialog.Option("Banshee Soul.", 1);
                    dialog.Option("Terato Dragon.", 2);
                    dialog.Option("Sword Master.", 3);
                    dialog.Option("Lava Best.", 4);
                    dialog.Option("Thrilling Spook.", 5);
                    dialog.Option("Snow Banshee.", 6);
                    dialog.Option("Thanks.", 255);
                    dialog.Send();

                    break;
                }
                case 1: {
                    if (client.Entity.Money >= entryFee) {
                        client.Entity.Money -= entryFee;
                        client.Entity.Teleport(7007, 530, 426);
                    }
                    else {
                        dialog.Text("You don't have enough money.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 2: {
                    if (client.Entity.Money >= entryFee) {
                        client.Entity.Money -= entryFee;
                        client.Entity.Teleport(2056, 635, 392);
                    }
                    else {
                        dialog.Text("You don't have enough money.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 3: {
                    if (client.Entity.Money >= entryFee) {
                        client.Entity.Money -= entryFee;
                        client.Entity.Teleport(1617, 51, 54);
                    }
                    else {
                        dialog.Text("You don't have enough money.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 4: {
                    if (client.Entity.Money >= entryFee) {
                        client.Entity.Money -= entryFee;
                        client.Entity.Teleport(3842, 255, 250);
                    }
                    else {
                        dialog.Text("You don't have enough money.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 5: {
                    if (client.Entity.Money >= entryFee) {
                        client.Entity.Money -= entryFee;
                        client.Entity.Teleport(1512, 55, 83);
                    }
                    else {
                        dialog.Text("You don't have enough money.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 6: {
                    if (client.Entity.Money >= entryFee) {
                        client.Entity.Money -= entryFee;
                        client.Entity.Teleport(1762, 540, 437);
                    }
                    else {
                        dialog.Text("You don't have enough money.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}