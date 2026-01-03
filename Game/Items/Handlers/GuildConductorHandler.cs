using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Guilds.Services;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.GuildItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Guild Conductor items that spawn NPCs for guild management.
    /// </summary>
    [ItemHandler(GuildConductor1, GuildConductor2, GuildConductor3, GuildConductor4)]
    public static class GuildConductorHandler {
        private static readonly Dictionary<uint, uint> ConductorToUid = new Dictionary<uint, uint> {
            { GuildConductor1, 9994 },
            { GuildConductor2, 9995 },
            { GuildConductor3, 9996 },
            { GuildConductor4, 9997 }
        };

        public static void Handle(GameState client, ConquerItem item) {
            if (!ConductorToUid.TryGetValue(item.ID, out var uid)) {
                return;
            }

            var getnpc = GuildConductors.GuildConductorsDict[uid];
            var npc = new NpcRequest(5) {
                NpcID = getnpc.Npc.UID,
                Mesh = getnpc.Npc.Mesh
            };

            client.Entity.OnMoveNpc = getnpc.Npc.UID;
            client.spwansitem = item;
            client.Send(npc.ToArray());
        }
    }
}