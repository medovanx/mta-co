using System;

namespace MTA.Game.Npcs
{
    /// <summary>
    /// Attribute to mark a class as an NPC handler and specify which NPC ID it handles.
    /// The handler class must have a static Handle method with signature:
    /// public static void Handle(Client.GameState client, Network.GamePackets.NpcRequest npcRequest, MTA.Npcs dialog)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NpcHandlerAttribute(uint npcId) : Attribute
    {
        public uint NpcId { get; } = npcId;
    }
}
