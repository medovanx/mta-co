using MTA.Client;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Guild Controller - Controls the guild area entry
    /// </summary>
    [NpcHandler(380)]
    public static class NpcGuildController {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text("How can I help you? or where would you like to go?");
                    dialog.Option("Guild War area.", 1);
                    dialog.Option("CTF area.", 2);
                    dialog.Option("Buy Statue Scroll.", 3);
                    dialog.Option("Just passing by.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    client.Entity.Teleport(1038, 348, 339);
                    break;
                }
                case 2: {
                    if (CaptureTheFlag.IsWar) {
                        Program.World?.Ctf.SignUp(client);
                    }
                    else {
                        dialog.Text(
                            "The CTF is not on going at this time. The GuildWar is scheduled to start Sunday at 00:00 and Capture the flag is scheduled to start Saturday at 23:00.");
                        dialog.Option("Oh.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 3: {
                    const uint statuePrice = 50000;
                    if (client.Inventory.Count < 40) {
                        if (client.Entity.Money >= statuePrice) {
                            client.Entity.Money -= statuePrice;
                            client.Inventory.Add(720020, 0, 1);
                        }
                        else {
                            dialog.Text($"Sorry you don't have {statuePrice:N0} gold.");
                            dialog.Option("Ahh.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("Please make more space in you inventory");
                        dialog.Option("Ahh.", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}