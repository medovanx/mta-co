using System;
using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.GuildWar.Npcs;

/// <summary>
///     Guild War Prsion NPC - Allows players to teleport out during pardon time
/// </summary>
[NpcHandler(140)]
public static class NpcGuildWarPrison {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                var gwEvent = GuildWarEvent.GetActiveEvent();
                if (gwEvent?.IsActive == true) {
                    if (DateTime.Now.Minute >= 0 && DateTime.Now.Minute <= 59) {
                        dialog.Text("My friend, you may leave if you want.");
                        dialog.Option("Yes please.", 1);
                        dialog.Option("No need...", 255);
                    }
                    else {
                        dialog.Text(
                            "You lost your chance. Now wait for the next pardon btw xx:00 to xx:05 and xx:30 to xx:35!");
                        dialog.Option("No!!!", 255);
                    }
                }
                else {
                    dialog.Text("My friend, you may leave if you want.");
                    dialog.Option("Yes please.", 1);
                    dialog.Option("No need...", 255);
                }

                dialog.Send();

                break;
            }
            case 1: {
                var gwEvent = GuildWarEvent.GetActiveEvent();
                if (gwEvent?.IsActive == true) {
                    if (DateTime.Now.Minute >= 0 && DateTime.Now.Minute <= 59) {
                        client.Entity.Teleport(1002, 430, 380);
                    }
                    else {
                        dialog.Text("You lost your chance. Now wait for the next pardon!");
                        dialog.Option("No!!!", 255);
                        dialog.Send();
                    }
                }
                else {
                    client.Entity.Teleport(1002, 430, 380);
                }

                break;
            }
        }
    }
}