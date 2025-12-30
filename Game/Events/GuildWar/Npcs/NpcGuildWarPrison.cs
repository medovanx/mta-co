using System;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.GuildWar.Npcs;

/// <summary>
///     Guild War Prison NPC - Allows players to teleport out during pardon time
/// </summary>
[NpcHandler(140)]
public static class NpcGuildWarPrison {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                var gwEvent = GuildWarEvent.GetActiveEvent();
                if (gwEvent?.IsActive == true) {
                    var minute = DateTime.Now.Minute;
                    if (minute is >= 0 and <= 5 or >= 30 and <= 35) {
                        dialog.Text("My friend, you may leave if you want.");
                        dialog.Option("Yes please.", 1);
                        dialog.Option("No need...", 255);
                    }
                    else {
                        dialog.Text(
                            "You lost your chance.\nNow wait for the next pardon between xx:00 to xx:05 and xx:30 to xx:35!");
                        dialog.Option("I will rot here...!", 255);
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
                    var minute = DateTime.Now.Minute;
                    if (minute is >= 0 and <= 5 or >= 30 and <= 35) {
                        client.Entity.Teleport(Maps.TwinCity, 304, 287);
                    }
                    else {
                        dialog.Text("You lost your chance.\nNow wait for the next pardon between xx:00 to xx:05 and xx:30 to xx:35!");
                        dialog.Option("I will rot here...!", 255);
                        dialog.Send();
                    }
                }
                else {
                    client.Entity.Teleport(Maps.TwinCity, 304, 287);
                }

                break;
            }
        }
    }
}