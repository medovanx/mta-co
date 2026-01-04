using System;
using System.Linq;
using MTA.Game.Constants;

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
        /// Creates a packet handler attribute for a single packet ID using the PacketIds enum.
        /// </summary>
        public PacketHandlerAttribute(Packets packet) {
            PacketIds = [(ushort)packet];
        }

        /// <summary>
        /// Creates a packet handler attribute for multiple packet IDs using the PacketIds enum.
        /// </summary>
        public PacketHandlerAttribute(params Packets[] packetIds) {
            if (packetIds == null || packetIds.Length == 0)
                throw new ArgumentException("At least one packet ID must be specified.", nameof(packetIds));
            PacketIds = packetIds.Select(p => (ushort)p).ToArray();
        }
    }
}

