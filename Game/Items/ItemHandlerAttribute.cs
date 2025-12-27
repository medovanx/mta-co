using System;

namespace MTA.Game.Items {
    /// <summary>
    /// Attribute to mark a class as an item handler and specify which item ID(s) it handles.
    /// The handler class must have a static Handle method with signature:
    /// public static void Handle(Client.GameState client, ConquerItem item)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ItemHandlerAttribute : Attribute {
        public uint[] ItemIds { get; }

        /// <summary>
        /// Creates an item handler attribute for a single item ID.
        /// </summary>
        public ItemHandlerAttribute(uint itemId) {
            ItemIds = [itemId];
        }

        /// <summary>
        /// Creates an item handler attribute for multiple item IDs.
        /// </summary>
        public ItemHandlerAttribute(params uint[] itemIds) {
            if (itemIds == null || itemIds.Length == 0)
                throw new ArgumentException("At least one item ID must be specified.", nameof(itemIds));
            ItemIds = itemIds;
        }
    }
}