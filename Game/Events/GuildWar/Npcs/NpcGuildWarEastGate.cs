using System.Collections.Generic;
using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;
using static MTA.Game.Enums;
using UpdatePacket = MTA.Network.GamePackets.Update;

namespace MTA.Game.Events.GuildWar.Npcs;

/// <summary>
///     Guild War East Gate NPC - Control right gate (open/close/repair)
/// </summary>
[NpcHandler(516075)]
public static class NpcGuildWarEastGate {
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
                if (gwEvent?.EastGate == null) break;
                gwEvent.EastGate.Mesh = GuildWarConstants.EastGateOpenMesh;

                var upd = new UpdatePacket(true) {
                    UID = gwEvent.EastGate.UID
                };
                upd.Append(UpdatePacket.Mesh, gwEvent.EastGate.Mesh);
                client.SendScreen(upd);
                break;
            }
            case 2: {
                if (gwEvent?.EastGate == null) break;
                if (gwEvent.EastGate.Hitpoints == 0) {
                    dialog.Text("The gate is broken and cannot be closed. Please repair it first.");
                    dialog.Option("Okay.", 255);
                    dialog.Send();
                    break;
                }
                gwEvent.EastGate.Mesh = GuildWarConstants.EastGateClosedMesh;
                var upd = new UpdatePacket(true) {
                    UID = gwEvent.EastGate.UID
                };
                upd.Append(UpdatePacket.Mesh, gwEvent.EastGate.Mesh);
                upd.Append(UpdatePacket.Hitpoints, gwEvent.EastGate.Hitpoints);
                client.SendScreen(upd);
                break;
            }
            case 22: {
                if (gwEvent?.EastGate == null) break;
                gwEvent.EastGate.Mesh = GuildWarConstants.EastGateClosedMesh;
                if (gwEvent.EastGate.Hitpoints == 0)
                    gwEvent.EastGate.Hitpoints = gwEvent.EastGate.MaxHitpoints;
                var upd = new UpdatePacket(true) {
                    UID = gwEvent.EastGate.UID
                };
                upd.Append(UpdatePacket.Mesh, gwEvent.EastGate.Mesh);
                upd.Append(UpdatePacket.Hitpoints, gwEvent.EastGate.Hitpoints);
                client.SendScreen(upd);
                break;
            }
            case 3: {
                client.Entity.Teleport(1038, 210, 177);
                break;
            }
        }
    }
}