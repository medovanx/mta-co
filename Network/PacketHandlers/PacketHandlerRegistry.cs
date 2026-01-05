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
        private static readonly Dictionary<ushort, List<Func<ushort, byte[], GameState, bool>>> Handlers = new();

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
                    if (!Handlers.TryGetValue(packetId, out var handlerList)) {
                        handlerList = new List<Func<ushort, byte[], GameState, bool>>();
                        Handlers[packetId] = handlerList;
                    }
                    handlerList.Add(handler);
                }
            }

            var totalHandlers = Handlers.Values.Sum(list => list.Count);
            Console.WriteLine($"[Packet Handler Registry] Registered {totalHandlers} packet handler(s) for {Handlers.Count} packet ID(s)");
        }

        /// <summary>
        /// Attempts to invoke registered handlers for the given packet ID in registration order.
        /// </summary>
        /// <returns>True if a handler found and executed (and returned true), false otherwise.</returns>
        public static bool TryHandle(ushort packetId, byte[] packet, GameState client) {
            if (!Handlers.TryGetValue(packetId, out var handlerList)) return false;
            
            // Try each handler in order until one returns true
            foreach (var handler in handlerList) {
                try {
                    if (handler(packetId, packet, client)) {
                        return true; // Handler processed the packet
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"[Packet Handler Error] Exception in handler for packet {packetId}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    // Continue to next handler on error
                }
            }
            
            return false; // No handler processed the packet
        }
    }
}

