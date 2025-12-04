using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs
{
    /// <summary>
    /// Auto-discovers and registers NPC handlers marked with [NpcHandler] attribute.
    /// </summary>
    public static class NpcHandlerRegistry
    {
        private static readonly Dictionary<uint, Action<GameState, NpcRequest, MTA.Npcs>> _handlers
            = new Dictionary<uint, Action<GameState, NpcRequest, MTA.Npcs>>();

        /// <summary>
        /// Scans assembly for [NpcHandler] attributes and registers all handlers.
        /// </summary>
        public static void Initialize()
        {
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<NpcHandlerAttribute>() != null);

            foreach (var handlerType in handlerTypes)
            {
                var attribute = handlerType.GetCustomAttribute<NpcHandlerAttribute>();
                var handleMethod = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Static);

                if (handleMethod != null)
                {
                    var handler = (Action<GameState, NpcRequest, MTA.Npcs>)
                        Delegate.CreateDelegate(typeof(Action<GameState, NpcRequest, MTA.Npcs>), handleMethod);
                    _handlers[attribute.NpcId] = handler;
                }
            }

            Console.WriteLine($"[NPC Registry] Registered {_handlers.Count} NPC handler(s)");
        }

        /// <summary>
        /// Attempts to invoke a registered handler for the given NPC ID.
        /// </summary>
        /// <returns>True if handler found and executed, false otherwise.</returns>
        public static bool TryHandle(uint npcId, GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            if (_handlers.TryGetValue(npcId, out var handler))
            {
                handler(client, npcRequest, dialog);
                return true;
            }
            return false;
        }
    }
}
