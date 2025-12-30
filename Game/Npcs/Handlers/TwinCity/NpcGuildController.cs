using MTA.Client;
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
                    dialog.Option("Buy statue.", 3);
                    dialog.Option("Super Guild War area.", 4);
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
                        Program.World.Ctf.SignUp(client);
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
                    var latest = GuildWarHistoryTable.GetLatest();
                    if (client.Guild == null || latest == null || latest.GuildId != client.Guild.ID ||
                        client.Entity.GuildRank != (ushort)Enums.GuildMemberRank.GuildLeader) {
                        dialog.Text("Sorry you need to be guildleader and the winner of the guildwar");
                        dialog.Option("Ahh.", 255);
                        dialog.Send();
                        return;
                    }
                    const uint statuePrice = 25000000;
                    if (client.Inventory.Count <= 1) {
                        if (client.Entity.ConquerPoints >= statuePrice) {
                            client.Entity.ConquerPoints -= statuePrice;
                            client.Inventory.Add(720020, 0, 1);
                        }
                        else {
                            dialog.Text($"Sorry you don't have {statuePrice:N0} CPs");
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
                case 4: {
                    client.Entity.Teleport(Maps.GuildWarMap, 348, 339);
                    break;
                }
            }
        }
    }
}