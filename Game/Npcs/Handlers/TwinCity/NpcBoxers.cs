using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Boxer - Exit NPC from training ground
    /// </summary>
    [NpcHandler(97619)]
    public static class NpcBoxerExit {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        "If you want to leave, just tell me when you're ready. It's free.\nYou will be teleported back to the city you were in before coming here.");
                    dialog.Option("I'm ready.", 1);
                    dialog.Option("Wait a minute.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    int prevMap = client.Entity.PreviousMapID;
                    switch (prevMap) {
                        default: {
                            client.Entity.Teleport(1002, 303, 278);
                            break;
                        }
                        case 1000: {
                            client.Entity.Teleport(1000, 500, 650);
                            break;
                        }
                        case 1020: {
                            client.Entity.Teleport(1020, 565, 562);
                            break;
                        }
                        case 1011: {
                            client.Entity.Teleport(1011, 188, 264);
                            break;
                        }
                        case 1015: {
                            client.Entity.Teleport(1015, 717, 571);
                            break;
                        }
                    }

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Boxer - Entry NPCs to training ground
    /// Order: Boxer Huang, Boxer Zhang, Boxer Li, Boxer Zhao, Boxer Wang
    /// </summary>
    [NpcHandler(97614, 97615, 97616, 97617, 97618)]
    public static class NpcBoxerEntry {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            const uint entryFee = 1000;
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        $"Hello, if your level is 20+, I can send you to a training ground in exchange for {entryFee} silvers. Keep in mind that you cannot attack dumes with levels higher than yours.");
                    dialog.Option("Alright, let me in.", 1);
                    dialog.Option("Never mind", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    if (client.Entity.Level >= 20) {
                        if (client.Entity.Money >= entryFee) {
                            client.Entity.Money -= entryFee;
                            client.Entity.Teleport(Maps.TrainingGround, 208, 221);
                        }
                        else {
                            dialog.Text($"You don't have enough silvers. You need {entryFee} silvers to enter.");
                            dialog.Option("Aww!", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            "Your level is not high enough. You must be at least level 20 to enter the training ground.");
                        dialog.Option("Aww!", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}