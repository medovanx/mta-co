using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game.Items {
    /// <summary>
    /// Auto-discovers and registers item handlers marked with [ItemHandler] attribute.
    /// </summary>
    public static class ItemHandlerRegistry {
        private static readonly Dictionary<uint, Action<GameState, ConquerItem>> Handlers = new();

        /// <summary>
        /// Scans assembly for [ItemHandler] attributes and registers all handlers.
        /// </summary>
        public static void Initialize() {
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<ItemHandlerAttribute>() != null);

            foreach (var handlerType in handlerTypes) {
                var attribute = handlerType.GetCustomAttribute<ItemHandlerAttribute>();
                var handleMethod = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Static);

                if (handleMethod == null) continue;
                var handler = (Action<GameState, ConquerItem>)
                    Delegate.CreateDelegate(typeof(Action<GameState, ConquerItem>), handleMethod);

                // Register handler for all item IDs specified in the attribute
                foreach (var itemId in attribute!.ItemIds) {
                    if (!Handlers.TryAdd(itemId, handler)) {
                        throw new InvalidOperationException(
                            $"Duplicate item ID {itemId} found. Handler class '{handlerType.Name}' is trying to register an item ID that is already handled by another handler.");
                    }
                }
            }

            Console.WriteLine($"[Item Registry] Registered {Handlers.Count} item handler(s)");
        }

        /// <summary>
        /// Attempts to invoke a registered handler for the given item ID.
        /// </summary>
        /// <returns>True if handler found and executed, false otherwise.</returns>
        public static bool TryHandle(uint itemId, GameState client, ConquerItem item) {
            if (!Handlers.TryGetValue(itemId, out var handler)) return false;
            handler(client, item);
            return true;
        }
    }
}