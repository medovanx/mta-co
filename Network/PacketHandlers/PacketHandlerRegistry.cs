using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MTA.Client;

namespace MTA.Network.PacketHandlers {
    /// <summary>
    /// Auto-discovers and registers packet handlers marked with [PacketHandler] attribute.
    /// </summary>
    public static class PacketHandlerRegistry {
        private static readonly Dictionary<ushort, Func<ushort, byte[], GameState, bool>> Handlers = new();

        /// <summary>
        /// Scans assembly for [PacketHandler] attributes and registers all handlers.
        /// </summary>
        public static void Initialize() {
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<PacketHandlerAttribute>() != null);

            foreach (var handlerType in handlerTypes) {
                var attribute = handlerType.GetCustomAttribute<PacketHandlerAttribute>();
                var handleMethod = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Static);

                if (handleMethod == null) continue;
                var handler = (Func<ushort, byte[], GameState, bool>)
                    Delegate.CreateDelegate(typeof(Func<ushort, byte[], GameState, bool>), handleMethod);

                // Register handler for all packet IDs specified in the attribute
                foreach (var packetId in attribute!.PacketIds) {
                    if (!Handlers.TryAdd(packetId, handler)) {
                        throw new InvalidOperationException(
                            $"Duplicate packet ID {packetId} found. Handler class '{handlerType.Name}' is trying to register a packet ID that is already handled by another handler.");
                    }
                }
            }

            Console.WriteLine($"[Packet Handler Registry] Registered {Handlers.Count} packet handler(s)");
        }

        /// <summary>
        /// Attempts to invoke a registered handler for the given packet ID.
        /// </summary>
        /// <returns>True if handler found and executed (and returned true), false otherwise.</returns>
        public static bool TryHandle(ushort packetId, byte[] packet, GameState client) {
            if (!Handlers.TryGetValue(packetId, out var handler)) return false;
            try {
                return handler(packetId, packet, client);
            }
            catch (Exception ex) {
                Console.WriteLine($"[Packet Handler Error] Exception in handler for packet {packetId}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}

