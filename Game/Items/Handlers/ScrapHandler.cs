using MTA.Client;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles scrap and crafting material items that can be combined to create packs.
    /// </summary>
    [ItemHandler(P7WeaponSoulPackScrap, P7EquipmentSoulScrap, SacredRefineryScrap, AncientStone, SoulScrolls)]
    public static class ScrapHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.ID == P7WeaponSoulPackScrap) {
                if (item.StackSize >= 15) {
                    client.Inventory.Add(P7WeaponSoulPack2, 0, 1);
                    item.StackSize -= 15;
                    client.Inventory.Remove(item, Enums.ItemUse.Remove);
                }
                else {
                    client.Send("You need 15 Scraps to Continue ");
                }
            }
            else if (item.ID == P7EquipmentSoulScrap) {
                if (item.StackSize >= 15) {
                    client.Inventory.Add(P7EquipmentSoulPack, 0, 1);
                    item.StackSize -= 15;
                    client.Inventory.Remove(item, Enums.ItemUse.Remove);
                }
                else {
                    client.Send("You need 15 Scraps to Continue ");
                }
            }
            else if (item.ID == SacredRefineryScrap) {
                if (item.StackSize >= 15) {
                    client.Inventory.Add(SacredRefineryPack, 0, 1);
                    item.StackSize -= 15;
                    client.Inventory.Remove(item, Enums.ItemUse.Remove);
                }
                else {
                    client.Send("You need 15 Scraps to Continue ");
                }
            }
            else if (item.ID == AncientStone) {
                if (item.StackSize >= 200) {
                    client.Inventory.Add(P7WeaponSoulPack2, 0, 1);
                    client.Inventory.Add(P7EquipmentSoulPack, 0, 1);
                    client.Inventory.Add(SacredRefineryPack, 0, 1);
                    item.StackSize -= 200;
                    client.Inventory.Remove(item, Enums.ItemUse.Remove);
                }
                else {
                    client.Send("You need 200 AncientStone to Continue ");
                }
            }
            else if (item.ID == SoulScrolls) {
                if (item.StackSize >= 7) {
                    client.Inventory.Add(P7EquipmentSoulPack, 0, 1);
                    item.StackSize -= 7;
                    client.Inventory.Remove(item, Enums.ItemUse.Remove);
                }
                else {
                    client.Send("You need 7 SoulScrolls to Continue ");
                }
            }
        }
    }
}

