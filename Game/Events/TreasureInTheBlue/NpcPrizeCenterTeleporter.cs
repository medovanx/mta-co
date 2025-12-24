using System;
using System.Collections.Generic;
using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
/// Entry point for the Prize Center of the Treasure in the Blue event
/// <event>Treasure in the Blue</event>
/// <npc>Prize Center Teleporter</npc>
/// <description>Teleports players to the Prize Center with a 3-minute cooldown</description>
/// </summary>
[NpcHandler(14)]
public static class NpcPrizeCenterTeleporter {
    private const int CooldownMinutes = 3;

    // Track last teleport time per player (UID -> last teleport time)
    private static readonly Dictionary<uint, DateTime> LastTeleportTimes = new();

    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                dialog.Text(
                    "Hello! You want to exchange for rewards of the Treasure In The Blue event?\n\n" +
                    "I can send you to the Prize Center. If I just sent you to somewhere, you need to wait 3 minutes to use the teleporter again, since there are too many people.\n\n" +
                    "So, where do you want to go?");

                dialog.Option("The Prize Center", 1);
                dialog.Option("Twin City", 2);
                dialog.Option("Not now.", 255);
                dialog.Send();
                break;
            }

            case 1: {
                var playerId = client.Entity.UID;

                // Check cooldown
                if (LastTeleportTimes.TryGetValue(playerId, out var lastTeleport)) {
                    var timeSinceLastTeleport = DateTime.Now - lastTeleport;
                    if (timeSinceLastTeleport.TotalMinutes < CooldownMinutes) {
                        var remainingMinutes = CooldownMinutes - (int)timeSinceLastTeleport.TotalMinutes;
                        var remainingSeconds = CooldownMinutes * 60 - (int)timeSinceLastTeleport.TotalSeconds;
                        var remainingTime = remainingSeconds < 60
                            ? $"{remainingSeconds} seconds"
                            : $"{remainingMinutes} minute{(remainingMinutes > 1 ? "s" : "")}";

                        dialog.Text(
                            $"You need to wait {remainingTime} before you can use the teleporter again. Please be patient!");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                        break;
                    }
                }

                // Update last teleport time
                LastTeleportTimes[playerId] = DateTime.Now;

                // Teleport to Prize Center
                client.Entity.Teleport(MapConstants.JobCenter, MapConstants.TreasureInTheBlue_PrizeCenter, 57, 49);
                client.Entity.Update(_String.Effect, "accession3", true);
                client.Send("You have been teleported to the Prize Center! Exchange your coins for rewards!");

                break;
            }
            case 2: {
                client.Entity.Teleport(MapConstants.TwinCity, 304, 287);
                break;
            }
        }
    }
}