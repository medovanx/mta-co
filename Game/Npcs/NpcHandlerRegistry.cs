using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs {
    /// <summary>
    /// Auto-discovers and registers NPC handlers marked with [NpcHandler] attribute.
    /// </summary>
    public static class NpcHandlerRegistry {
        private static readonly Dictionary<uint, Action<GameState, NpcRequest, MTA.Npcs>> Handlers = new();

        /// <summary>
        /// Scans assembly for [NpcHandler] attributes and registers all handlers.
        /// </summary>
        public static void Initialize() {
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<NpcHandlerAttribute>() != null);

            foreach (var handlerType in handlerTypes) {
                var attribute = handlerType.GetCustomAttribute<NpcHandlerAttribute>();
                var handleMethod = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Static);

                if (handleMethod == null) continue;
                var handler = (Action<GameState, NpcRequest, MTA.Npcs>)
                    Delegate.CreateDelegate(typeof(Action<GameState, NpcRequest, MTA.Npcs>), handleMethod);

                // Register handler for all NPC IDs specified in the attribute
                foreach (var npcId in attribute!.NpcIds) {
                    if (!Handlers.TryAdd(npcId, handler)) {
                        throw new InvalidOperationException(
                            $"Duplicate NPC ID {npcId} found. Handler class '{handlerType.Name}' is trying to register an NPC ID that is already handled by another handler.");
                    }
                }
            }

            Console.WriteLine($"[NPC Registry] Registered {Handlers.Count} NPC handler(s)");
        }

        /// <summary>
        /// Attempts to invoke a registered handler for the given NPC ID.
        /// </summary>
        /// <returns>True if handler found and executed, false otherwise.</returns>
        public static bool TryHandle(uint npcId, GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            if (!Handlers.TryGetValue(npcId, out var handler)) return false;
            handler(client, npcRequest, dialog);
            return true;
        }
    }
}