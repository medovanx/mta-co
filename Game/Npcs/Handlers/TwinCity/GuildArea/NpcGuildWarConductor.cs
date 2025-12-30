using MTA.Client;
using MTA.Database;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity.GuildArea {
    /// <summary>
    ///     Guild War Conductor NPCs - Teleport players to special places for a fee
    /// </summary>
    [NpcHandler(9884, 9885, 9886, 9887)]
    public static class NpcGuildWarConductor {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        "In exchange of a fee of 1000gold, I will teleport you to a special place. If you don't have money, don't bother me.");
                    dialog.Option("Teleport me.", 1);
                    dialog.Option("Buy Conductor Scroll.", 2);
                    dialog.Option("I'm too poor.", 255);
                    dialog.Send();
                    break;
                }
                case 2: {
                    if (client.Entity.Money >= 10000) {
                        if (client.Inventory.Count < 40) {
                            client.Entity.Money -= 10000;
                            switch (client.ActiveNpc) {
                                case 9884: {
                                    client.Inventory.Add(720021, 0, 1);
                                    break;
                                }
                                case 9885: {
                                    client.Inventory.Add(720022, 0, 1);
                                    break;
                                }
                                case 9886: {
                                    client.Inventory.Add(720023, 0, 1);
                                    break;
                                }
                                case 9887: {
                                    client.Inventory.Add(720024, 0, 1);
                                    break;
                                }
                            }
                        }
                        else {
                            dialog.Text("Sorry, you inventory is full");
                            dialog.Option("Ah, ok", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("Sorry, you not have 10.000 silver.");
                        dialog.Option("Ah, ok", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 1: {
                    if (client.Entity.Money >= 1000) {
                        client.Entity.Money -= 1000;

                        GuildCondutors.Conductor cond = null;
                        if (GuildCondutors.GuildConductors.TryGetValue(client.ActiveNpc + 110, out cond)) {
                            client.Entity.Teleport(cond.Teleport_MapId, cond.Teleport_X, cond.Teleport_Y);
                        }

                        break;
                    }
                    else {
                        dialog.Text("Sorry, you not have 1.000 silver.");
                        dialog.Option("Ah, ok", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}

