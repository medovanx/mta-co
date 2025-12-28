using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.QuestAndOther;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles random CP item that grants a random amount of Conquer Points.
    /// </summary>
    [ItemHandler(RandomCPItem)]
    public static class RandomCPHandler {
        private const int MinCP = 10;
        private const int MaxCP = 50;

        public static void Handle(GameState client, ConquerItem item) {
            var cps = Kernel.Random.Next(MinCP, MaxCP);
            client.Entity.ConquerPoints += (uint)cps;
            client.Send(new Message("Congratulations you got Conquer Point Points keep going", Color.Red,
                Message.Whisper));
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}

