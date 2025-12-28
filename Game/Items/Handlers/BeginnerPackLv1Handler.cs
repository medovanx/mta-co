using System.Drawing;
using MTA.Client;
using MTA.Network;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.BasicItems;
using static MTA.Game.Constants.Items.MedicineAndTeleport;
using static MTA.Game.Constants.Items.SuperEquipment;
using static MTA.Game.EntityClassConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles BeginnerPack items that give rewards based on player level.
    /// </summary>
    [ItemHandler(BeginnerPackLv1, BeginnerPackLv10, BeginnerPackLv70, BeginnerPackLv100, BeginnerPackLv110,
        BeginnerPackLv120)]
    public static class BeginnerPackLv1Handler {
        public static void Handle(GameState client, ConquerItem item) {
            switch (item.ID) {
                case BeginnerPackLv1:
                    if (client.Entity.Level >= 1) {
                        if (client.Inventory.Count < 33) {
                            client.Entity.Money += 5000;
                            client.Entity.ConquerPoints += 10;
                            client.Inventory.Add(Stancher, 0, 3);
                            client.Inventory.Add(Agrypnotic, 0, 3);
                            client.Inventory.Add(BeginnerPackLv10, 0, 1);
                            client.Inventory.Add(TwinCityGate, 0, 1);
                            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                        }
                        else {
                            client.Send(new Message("You need to make atleast 7 free spots in your inventory.",
                                Color.Red, Message.TopLeft));
                        }
                    }
                    else {
                        client.Send(new Message("You must be atleast level 1", Color.Red, Message.TopLeft));
                    }

                    break;

                case BeginnerPackLv10:
                    if (client.Entity.Level >= 10) {
                        if (client.Inventory.Count < 24) {
                            client.Inventory.Add(ExpBall_B, 0, 10);
                            client.Inventory.Add(ExpPotion, 0, 3);
                            client.Inventory.Add(BeginnerPackLv70, 0, 1);
                            client.Entity.Money += 7000;
                            client.Entity.ConquerPoints += 10;
                            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                        }
                        else {
                            client.Send(new Message("You need to make atleast 14 free spots in your inventory.",
                                Color.Red, Message.TopLeft));
                        }
                    }
                    else {
                        client.Send(new Message("You must be atleast level 10", Color.Red, Message.TopLeft));
                    }

                    break;

                case BeginnerPackLv70:
                    if (client.Entity.Level >= 70) {
                        if (client.Inventory.Count < 36) {
                            client.Inventory.Add(ExpPotion, 0, 5);
                            client.Entity.ConquerPoints += 20;
                            client.Inventory.Add(Emerald, 0, 1);
                            client.Inventory.Add(BeginnerPackLv100, 0, 1);
                            client.Inventory.Add(TwinCityGate, 0, 5);
                            client.Entity.Money += 7000;
                            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                        }
                        else {
                            client.Send(new Message("You need to make atleast 7 free spots in your inventory.",
                                Color.Red, Message.TopLeft));
                        }
                    }
                    else {
                        client.Send(new Message("You must be atleast level 70", Color.Red, Message.TopLeft));
                    }

                    break;

                case BeginnerPackLv100:
                    if (client.Entity.Level >= 100) {
                        if (client.Inventory.Count < 28) {
                            client.Entity.Money += 10000;
                            client.Entity.ConquerPoints += 50;
                            client.Inventory.Add(SuperBowPack, 0, 1);
                            client.Inventory.Add(BeginnerPackLv110, 0, 1);
                            client.Inventory.Add(ExpPotion, 0, 10);
                            if (IsTrojan(client.Entity.Class))
                                client.Inventory.Add(BladeSoulLv100, 0, 1);
                            else if (IsWarrior(client.Entity.Class))
                                client.Inventory.Add(ShieldSoulLv100, 0, 1);
                            else if (IsArcher(client.Entity.Class))
                                client.Inventory.Add(BowSoulLv100, 0, 1);
                            else if (IsNinja(client.Entity.Class) || IsMonk(client.Entity.Class))
                                client.Inventory.Add(BladeSoulLv100, 0, 1);
                            else
                                client.Inventory.Add(BackswordSoulLv100, 0, 1);

                            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                        }
                        else {
                            client.Send(new Message("You need to make atleast 12 free spots in your inventory.",
                                Color.Red, Message.TopLeft));
                        }
                    }
                    else {
                        client.Send(new Message("You must be atleast level 100", Color.Red, Message.TopLeft));
                    }

                    break;

                case BeginnerPackLv110:
                    if (client.Entity.Level >= 110) {
                        if (client.Inventory.Count < 27) {
                            client.Inventory.Add(BeginnerPackLv120, 0, 1);
                            client.Inventory.Add(MoonBox, 0, 1);
                            client.Inventory.Add(ExpPotion, 0, 10);
                            if (IsTrojan(client.Entity.Class))
                                PacketHandler.CheckCommand("@item MythicBlade Super 0 0 0 13 0", client);
                            else if (IsWarrior(client.Entity.Class))
                                PacketHandler.CheckCommand("@item DragonWand Super 0 0 0 13 0", client);
                            else if (IsArcher(client.Entity.Class))
                                PacketHandler.CheckCommand("@item AncientBow Super 0 0 0 13 0", client);
                            else if (IsNinja(client.Entity.Class))
                                PacketHandler.CheckCommand("@item FlameKatana Super 0 0 0 13 0", client);
                            else if (IsMonk(client.Entity.Class))
                                PacketHandler.CheckCommand("@item BeadsOfConsciousness Super 0 0 0 13 0", client);
                            else
                                PacketHandler.CheckCommand("@item ThunBacksword Super 0 0 0 3 0", client);
                            client.Entity.ConquerPoints += 100;
                            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                        }
                        else {
                            client.Send(new Message("You need to make atleast 13 free spots in your inventory.",
                                Color.Red, Message.TopLeft));
                        }
                    }
                    else {
                        client.Send(new Message("You must be atleast level 110", Color.Red, Message.TopLeft));
                    }

                    break;

                case BeginnerPackLv120:
                    if (client.Entity.Level >= 120) {
                        client.Entity.Money += 10000;
                        client.Entity.ConquerPoints += 500;
                        client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                    }
                    else {
                        client.Send(new Message("You must be atleast level 120", Color.Red, Message.TopLeft));
                    }

                    break;
            }
        }
    }
}
