using System.Collections.Generic;
using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;
using static MTA.Game.Enums;
using UpdatePacket = MTA.Network.GamePackets.Update;

namespace MTA.Game.Events.GuildWar.Npcs;

/// <summary>
///     Guild War West Gate NPC - Control left gate (open/close/repair)
/// </summary>
[NpcHandler(516074)]
public static class NpcGuildWarWestGate {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        // Only allow gate control during active war and only by current pole keeper
        var gwEvent = GuildWarEvent.GetActiveEvent();
        if (gwEvent is not { IsActive: true } || client.Guild == null || gwEvent.PoleKeeper != client.Guild) return;
        switch (npcRequest.OptionID) {
            case 0:
                dialog.Text("Select the option you want to pursue.");
                var member = client.Guild?.Members.GetValueOrDefault(client.Entity.UID);
                if (member?.Rank is GuildMemberRank.GuildLeader or GuildMemberRank.DeputyLeader) {
                    dialog.Option("Open gate.", 1);
                    dialog.Option("Close gate.", 2);
                }

                dialog.Option("Get inside.", 3);
                dialog.Option("Nothing.", 255);
                dialog.Send();
                break;
            case 1: {
                if (gwEvent.WestGate == null) break;
                gwEvent.WestGate.Mesh = GuildWarConstants.WestGateOpenMesh;

                var upd = new UpdatePacket(true) {
                    UID = gwEvent.WestGate.UID
                };
                upd.Append(UpdatePacket.Mesh, gwEvent.WestGate.Mesh);
                client.SendScreen(upd);
                break;
            }
            case 2: {
                if (gwEvent.WestGate == null) break;
                if (gwEvent.WestGate.Hitpoints == 0) {
                    dialog.Text("The gate is broken and cannot be closed. Please repair it first.");
                    dialog.Option("Okay.", 255);
                    dialog.Send();
                    break;
                }

                gwEvent.WestGate.Mesh = GuildWarConstants.WestGateClosedMesh;
                var upd = new UpdatePacket(true) {
                    UID = gwEvent.WestGate.UID
                };
                upd.Append(UpdatePacket.Mesh, gwEvent.WestGate.Mesh);
                upd.Append(UpdatePacket.Hitpoints, gwEvent.WestGate.Hitpoints);
                client.SendScreen(upd);
                break;
            }
            case 22: {
                if (gwEvent.WestGate == null) break;
                gwEvent.WestGate.Mesh = GuildWarConstants.WestGateClosedMesh;
                if (gwEvent.WestGate.Hitpoints == 0)
                    gwEvent.WestGate.Hitpoints = gwEvent.WestGate.MaxHitpoints;
                var upd = new UpdatePacket(true) {
                    UID = gwEvent.WestGate.UID
                };
                upd.Append(UpdatePacket.Mesh, gwEvent.WestGate.Mesh);
                upd.Append(UpdatePacket.Hitpoints, gwEvent.WestGate.Hitpoints);
                client.SendScreen(upd);
                break;
            }
            case 3: {
                client.Entity.Teleport(1038, 162, 198);
                break;
            }
        }
    }
}