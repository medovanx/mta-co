using System;
using System.Collections.Generic;
using MTA.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Guilds.Database;

public static class GuildConductors {
    private static readonly List<ushort> AllowMaps =
        [1000, 1002, 1001, 1015, 1020, 1011, 700, 1216, 1214, 1217, 1028, 7007, 8892, 8893, 8894, 1762, 2056];

    public static readonly SafeDictionary<uint, Conductor> GuildConductorsDict = new();


    public static bool MoveNpc(uint uid, ushort mapId, ushort x, ushort y) {
        if (!AllowMaps.Contains(mapId))
            return false;
        if (!GuildConductorsDict.TryGetValue(uid, out var npc)) return false;
        var mapBase = Kernel.Maps[mapId];
        if (!MapAllowThatLocation(mapBase, x, y)) return false;
        var tx = x;
        var ty = y;
        if (!ObtainTeleporter(mapBase, ref tx, ref ty)) return false;
        npc.Npc.X = x;
        npc.Npc.Y = y;
        npc.Npc.MapID = mapId;

        npc.TeleportMapId = mapId;
        npc.TeleportX = tx;
        npc.TeleportY = ty;


        new MySqlCommand(MySqlCommandType.UPDATE)
            .Update("sobnpcs").Set("mapid", npc.Npc.MapID)
            .Set("cellx", npc.Npc.X)
            .Set("celly", npc.Npc.Y).Where("id", uid).Execute();
        return true;
    }

    private static bool MapAllowThatLocation(Map mapBase, ushort x, ushort y) {
        return !mapBase.Floor[x, y, MapObjectType.InvalidCast];
    }

    private static bool ObtainTeleporter(Map map, ref ushort x, ref ushort y) {
        var limy = (ushort)Math.Min(map.Floor.Bounds.Height - 5, y + 5);
        var limx = (ushort)Math.Min(map.Floor.Bounds.Width - 5, x + 5);
        var xStart = (ushort)Math.Max(x - 5, 0);
        var yStart = (ushort)Math.Max(y - 5, 0);

        for (var ay = yStart; ay <= limy; ay++)
        for (var ax = xStart; ax <= limx; ax++)
            if (!map.Floor[ax, ay, MapObjectType.InvalidCast]) {
                x = ax;
                y = ay;
                return true;
            }

        x = 0;
        y = 0;
        return false;
    }

    public class Conductor {
        public required SobNpcSpawn Npc;
        public ushort TeleportMapId;
        public ushort TeleportX;
        public ushort TeleportY;
    }
}