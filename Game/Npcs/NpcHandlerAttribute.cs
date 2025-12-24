using System;

namespace MTA.Game.Npcs {
    /// <summary>
    /// Attribute to mark a class as an NPC handler and specify which NPC ID(s) it handles.
    /// The handler class must have a static Handle method with signature:
    /// public static void Handle(Client.GameState client, Network.GamePackets.NpcRequest npcRequest, MTA.Npcs dialog)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NpcHandlerAttribute : Attribute {
        public uint[] NpcIds { get; }

        /// <summary>
        /// Creates an NPC handler attribute for a single NPC ID.
        /// </summary>
        public NpcHandlerAttribute(uint npcId) {
            NpcIds = [npcId];
        }

        /// <summary>
        /// Creates an NPC handler attribute for multiple NPC IDs.
        /// </summary>
        public NpcHandlerAttribute(params uint[] npcIds) {
            if (npcIds == null || npcIds.Length == 0)
                throw new ArgumentException("At least one NPC ID must be specified.", nameof(npcIds));
            NpcIds = npcIds;
        }
    }
}