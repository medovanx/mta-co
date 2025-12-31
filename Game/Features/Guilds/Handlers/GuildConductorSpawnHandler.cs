using MTA.Client;
using MTA.Database;
using MTA.Game.Events.GuildWar;
using MTA.Game.Features.Guilds.Database;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Handlers;

/// <summary>
/// Handles guild conductor NPC spawn/move requests (packet 2030 when mesh / 10 == 147).
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
        if ((spawn.Mesh / 10) != 147)
            return false;

        // This is a guild conductor spawn - handle it
        var uid = client.Entity.OnMoveNpc;
        if (client.Guild != null || client.Account.State == AccountTable.AccountState.GM) {
            if (client.AsMember is { Rank: Enums.GuildMemberRank.GuildLeader } ||
                client.Account.State == AccountTable.AccountState.GM) {
                if (client.Guild?.Name is not null and not "") {
                    var gwEvent6 = GuildWarEvent.GetActiveEvent();
                    if (client.Guild.Name == (gwEvent6?.Pole?.Name ?? "") ||
                        client.Account.State == AccountTable.AccountState.GM) {
                        var getnpc = GuildConductors.GuildConductorsDict[uid];
                        var oldmap = getnpc.Npc.MapID;

                        if (!GuildConductors.MoveNpc(getnpc.Npc.UID, client.Entity.MapID, spawn.X,
                                spawn.Y))
                            client.Entity.SendSysMesage(
                                "Invalid New location ! or Invalid map, try again ");
                        else {
                            var removeoldnpc = new Data(true) { UID = uid, ID = Data.RemoveEntity };

                            Kernel.SendWorldMessage(removeoldnpc, Program.Values, oldmap);
                            // var dictionary = Kernel.GamePool.Values.Where((play) => play.Entity.MapID == oldmap && Kernel.GetDDistance(play.Entity.X, play.Entity.Y, oldx, oldy) <= 17);
                            // foreach (var pclient in dictionary)
                            //     pclient.Send(removeoldnpc);

                            client.Map.RemoveNpc(getnpc.Npc, true);

                            var newmap = Kernel.Maps[client.Entity.MapID];
                            newmap.AddNpc(getnpc.Npc, true);
                            client.SendScreen(getnpc.Npc.ToArray());
                        }
                    }
                    else
                        client.Entity.SendSysMesage("Sorry, you guild not dominate the Guild War");
                }
                else
                    client.Entity.SendSysMesage("Sorry, you guild not dominate the Guild War");
            }
            else
                client.Entity.SendSysMesage("Sorry, you guild not dominate the Guild War");
        }
        else
            client.Entity.SendSysMesage("Sorry, you guild not dominate the Guild War");

        return true; // Handled (even if validation failed, we processed it)
    }
}