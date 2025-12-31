using System;

namespace MTA.Network.PacketHandlers {
    /// <summary>
    /// Attribute to mark a class as a packet handler and specify which packet ID(s) it handles.
    /// The handler class must have a static Handle method with signature:
    /// public static bool Handle(ushort packetId, byte[] packet, Client.GameState client)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PacketHandlerAttribute : Attribute {
        public ushort[] PacketIds { get; }

        /// <summary>
        /// Creates a packet handler attribute for a single packet ID.
        /// </summary>
        public PacketHandlerAttribute(ushort packetId) {
            PacketIds = [packetId];
        }

        /// <summary>
        /// Creates a packet handler attribute for multiple packet IDs.
        /// </summary>
        public PacketHandlerAttribute(params ushort[] packetIds) {
            if (packetIds == null || packetIds.Length == 0)
                throw new ArgumentException("At least one packet ID must be specified.", nameof(packetIds));
            PacketIds = packetIds;
        }
    }
}

