using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Guild Conductor items that spawn NPCs for guild management.
    /// </summary>
    [ItemHandler(GuildConductor1, GuildConductor2, GuildConductor3, GuildConductor4)]
    public static class GuildConductorHandler {
        private static readonly Dictionary<uint, uint> ConductorToUID = new Dictionary<uint, uint> {
            { GuildConductor1, 9994 },
            { GuildConductor2, 9995 },
            { GuildConductor3, 9996 },
            { GuildConductor4, 9997 }
        };

        public static void Handle(GameState client, ConquerItem item) {
            if (!ConductorToUID.TryGetValue(item.ID, out var uid)) {
                return;
            }

            var getnpc = GuildCondutors.GuildConductors[uid];
            var npc = new NpcRequest(5) {
                NpcID = getnpc.npc.UID,
                Mesh = getnpc.npc.Mesh
            };

            client.Entity.OnMoveNpc = getnpc.npc.UID;
            client.spwansitem = item;
            client.Send(npc.ToArray());
        }
    }
}

