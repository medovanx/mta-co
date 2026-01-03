using MTA.Client;
using MTA.Game.Events.GuildWar;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Services;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Handles guild conductor NPC spawn/move requests (packet 2030 when mesh / 10 == 147).
/// </summary>
[PacketHandler(2030)]
public static class GuildConductorSpawnHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        // Only handle if client action is 2 (required for packet 2030)
        if (client.Action != 2)
            return false;

        var spawn = new NpcSpawn(false);
        spawn.Deserialize(packet);

        // Only handle guild conductor spawns (mesh / 10 == 147)
        if (spawn.Mesh / 10 != 147)
            return false;

        // This is a guild conductor spawn - handle it
        var uid = client.Entity.OnMoveNpc;
        if (client.Guild != null) {
            if (client.AsMember is { Rank: MemberRank.GuildLeader }) {
                if (client.Guild?.Name is not null and not "") {
                    var gwEvent6 = GuildWarEvent.GetActiveEvent();
                    if (client.Guild.Name == (gwEvent6?.Pole?.Name ?? "")) {
                        var getNpc = GuildConductors.GuildConductorsDict[uid];
                        var oldMap = getNpc.Npc.MapID;

                        if (!GuildConductors.MoveNpc(getNpc.Npc.UID, client.Entity.MapID, spawn.X,
                                spawn.Y)) {
                            client.Entity.SendSysMesage(
                                "Invalid New location ! or Invalid map, try again ");
                        }
                        else {
                            var removeOldNpc = new Data(true) { UID = uid, ID = Data.RemoveEntity };

                            Kernel.SendWorldMessage(removeOldNpc, Program.Values, oldMap);
                            client.Map.RemoveNpc(getNpc.Npc, true);

                            var newMap = Kernel.Maps[client.Entity.MapID];
                            newMap.AddNpc(getNpc.Npc, true);
                            client.SendScreen(getNpc.Npc.ToArray());
                        }
                    }
                    else {
                        client.Entity.SendSysMesage("Your guild is not the one dominating the Guild War area!");
                    }
                }
                else {
                    client.Entity.SendSysMesage("Your guild is not the one dominating the Guild War area!");
                }
            }
            else {
                client.Entity.SendSysMesage("Your guild is not the one dominating the Guild War area!");
            }
        }
        else {
            client.Entity.SendSysMesage("Your guild is not the one dominating the Guild War area!");
        }

        return true; // Handled (even if validation failed, we processed it)
    }
}