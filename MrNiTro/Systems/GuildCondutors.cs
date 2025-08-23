using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Database
{
    public class GuildCondutors
    {
        public static List<ushort> AllowMaps = new List<ushort>()
        {
            1000, 1002, 1001,1015, 1020, 1011, 700,1216,1214,1217,1028, 7007,8892, 8893,8894,1762
            ,2056
        };
        public class Conductor
        {
            public ushort Teleport_MapId = 0;
            public ushort Teleport_X = 0;
            public ushort Teleport_Y = 0;
            public Network.GamePackets.SobNpcSpawn npc;

        }

        public static SafeDictionary<uint, Conductor> GuildConductors = new SafeDictionary<uint, Conductor>();


        public static bool MoveNpc(uint UID, ushort MapID, ushort X, ushort Y)
        {
            Conductor npc = null;
            if (!AllowMaps.Contains(MapID))
                return false;
            if (GuildConductors.TryGetValue(UID, out npc))
            {
                var mapbase = Kernel.Maps[MapID];
                if (MapAllowThatLocation(mapbase, X, Y))
                {
                    ushort tx = X; ushort ty = Y;
                    if (ObtainTeleporter(mapbase, ref tx, ref ty))
                    {
                        npc.npc.X = X;
                        npc.npc.Y = Y;
                        npc.npc.MapID = MapID;

                        npc.Teleport_MapId = MapID;
                        npc.Teleport_X = tx;
                        npc.Teleport_Y = ty;


                        new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE)
                        .Update("sobnpcs").Set("mapid", npc.npc.MapID)
                        .Set("cellx", npc.npc.X)
                        .Set("celly", npc.npc.Y).Where("id", UID).Execute();
                        return true;
                    }
                }
            }
            return false;
        }
        public static Boolean MapAllowThatLocation(Game.Map mapbase, ushort x, ushort y)
        {
            if (!mapbase.Floor[x, y, Game.MapObjectType.InvalidCast, null])
                return true;

            return false;

        }
        public static bool ObtainTeleporter(Game.Map map, ref ushort x, ref ushort y)
        {
            ushort limy = (ushort)Math.Min(map.Floor.Bounds.Height - 5, y + 5);
            ushort limx = (ushort)Math.Min(map.Floor.Bounds.Width - 5, x + 5);
            ushort xstart = (ushort)Math.Max(x - 5, 0);
            ushort ystart = (ushort)Math.Max(y - 5, 0);

            for (ushort ay = ystart; ay <= limy; ay++)
            {
                for (ushort ax = xstart; ax <= limx; ax++)
                {
                    if (!map.Floor[ax, ay, Game.MapObjectType.InvalidCast, null])
                    {
                        x = ax;
                        y = ay;
                        return true;
                    }
                }
            }
            x = 0;
            y = 0;
            return false;
        }
    }
}
