using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles FortunePack items that grant Conquer Points when used.
    /// </summary>
    [ItemHandler(Class1FortunePack, Class2FortunePack, Class3FortunePack, Class4FortunePack, Class5FortunePack,
        Class6FortunePack)]
    public static class FortunePackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.Remove);

            uint gold = item.ID switch {
                Class1FortunePack => 5000u,
                Class2FortunePack => 10000u,
                Class3FortunePack => 20000u,
                Class4FortunePack => 500000u,
                Class5FortunePack => 100000u,
                Class6FortunePack => 200000u,
                _ => 0u // Default case (should not occur)
            };

            client.Entity.Money += (int)gold;

            // Class6FortunePack also grants study points
            if (item.ID == Class6FortunePack) {
                client.Entity.SubClasses.StudyPoints += 400;
                client.Send(new Message($"Congratulations, you got {gold} Gold and 400 study points!", Color.Red,
                    Message.TopLeft));
            }
            else {
                client.Send(new Message($"Congratulations, you got {gold} Gold!", Color.Red,
                    Message.TopLeft));
            }
        }
    }
}

