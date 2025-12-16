using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Cup Seller - Sells premium cups for CPs
    /// </summary>
    [NpcHandler(505444)]
    public static class NpcCupSeller {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            const uint HOLY_GRAIL_PRICE = 20000000;
            const uint GOLD_PRIZE_PRICE = 15000000;
            const uint GOLD_TROPHY_PRICE = 10000000;
            const uint HOLY_PHOENIX_CUP_PRICE = 5000000;

            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text($"Hello, what would you like to buy today?");
                    dialog.Option($"Holy Grail - {HOLY_GRAIL_PRICE:N0} CPs", 1);
                    dialog.Option($"Gold Prize - {GOLD_PRIZE_PRICE:N0} CPs", 2);
                    dialog.Option($"Gold Trophy - {GOLD_TROPHY_PRICE:N0} CPs", 3);
                    dialog.Option($"Holy Phoenix Cup - {HOLY_PHOENIX_CUP_PRICE:N0} CPs", 4);
                    dialog.Option("Just passing by.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    dialog.Text($"The Holy Grail costs {HOLY_GRAIL_PRICE:N0} CPs. Would you like to purchase it?");
                    dialog.Option("Yes, purchase Holy Grail.", 10);
                    dialog.Option("No, thanks.", 255);
                    dialog.Send();
                    break;
                }
                case 2: {
                    dialog.Text($"The Gold Prize costs {GOLD_PRIZE_PRICE:N0} CPs. Would you like to purchase it?");
                    dialog.Option("Yes, purchase Gold Prize.", 11);
                    dialog.Option("No, thanks.", 255);
                    dialog.Send();
                    break;
                }
                case 3: {
                    dialog.Text($"The Gold Trophy costs {GOLD_TROPHY_PRICE:N0} CPs. Would you like to purchase it?");
                    dialog.Option("Yes, purchase Gold Trophy.", 12);
                    dialog.Option("No, thanks.", 255);
                    dialog.Send();
                    break;
                }
                case 4: {
                    dialog.Text(
                        $"The Holy Phoenix Cup costs {HOLY_PHOENIX_CUP_PRICE:N0} CPs. Would you like to purchase it?");
                    dialog.Option("Yes, purchase Holy Phoenix Cup.", 13);
                    dialog.Option("No, thanks.", 255);
                    dialog.Send();
                    break;
                }
                case 10: {
                    if (client.Entity.ConquerPoints >= HOLY_GRAIL_PRICE) {
                        client.Entity.ConquerPoints -= HOLY_GRAIL_PRICE;
                        client.Inventory.Add(2100095, 0, 1);
                        dialog.Text("Thank you for your purchase! The Holy Grail has been added to your inventory.");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I'm sorry, but you need 20,000,000 CPs to purchase the Holy Grail.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 11: {
                    if (client.Entity.ConquerPoints >= GOLD_PRIZE_PRICE) {
                        client.Entity.ConquerPoints -= GOLD_PRIZE_PRICE;
                        client.Inventory.Add(2100075, 0, 1);
                        dialog.Text("Thank you for your purchase! The Gold Prize has been added to your inventory.");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I'm sorry, but you need 15,000,000 CPs to purchase the Gold Prize.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 12: {
                    if (client.Entity.ConquerPoints >= GOLD_TROPHY_PRICE) {
                        client.Entity.ConquerPoints -= GOLD_TROPHY_PRICE;
                        client.Inventory.Add(2100085, 0, 1);
                        dialog.Text("Thank you for your purchase! The Gold Trophy has been added to your inventory.");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I'm sorry, but you need 10,000,000 CPs to purchase the Gold Trophy.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 13: {
                    if (client.Entity.ConquerPoints >= HOLY_PHOENIX_CUP_PRICE) {
                        client.Entity.ConquerPoints -= HOLY_PHOENIX_CUP_PRICE;
                        client.Inventory.Add(2100245, 0, 1);
                        dialog.Text(
                            "Thank you for your purchase! The Holy Phoenix Cup has been added to your inventory.");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I'm sorry, but you need 5,000,000 CPs to purchase the Holy Phoenix Cup.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}