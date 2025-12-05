using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// VIP Seller NPC - Sells VIP levels for ConquerPoints
    /// </summary>
    [NpcHandler(121211)]
    public static class Npc_VipSeller
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            uint VIP1_PRICE = 300000;
            uint VIP2_PRICE = 600000;
            uint VIP3_PRICE = 1000000;
            uint VIP4_PRICE = 1500000;
            uint VIP5_PRICE = 2000000;
            uint VIP6_PRICE = 2500000;

            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello! I can upgrade your VIP level using CPs.");
                        dialog.Option("Exchange CPs for VIP Level", 12);
                        dialog.Option("Never mind.", 255);
                        dialog.Send();
                        break;
                    }
                case 12:
                    {
                        dialog.Text("Which VIP level would you like to purchase?");
                        dialog.Option("VIP 1 = " + VIP1_PRICE + " CPs", 1);
                        dialog.Option("VIP 2 = " + VIP2_PRICE + " CPs", 2);
                        dialog.Option("VIP 3 = " + VIP3_PRICE + " CPs", 3);
                        dialog.Option("VIP 4 = " + VIP4_PRICE + " CPs", 4);
                        dialog.Option("VIP 5 = " + VIP5_PRICE + " CPs", 5);
                        dialog.Option("VIP 6 = " + VIP6_PRICE + " CPs", 6);
                        dialog.Option("Never mind.", 255);

                        dialog.Send();
                        break;
                    }
                case 1:
                    {
                        if (client.Entity.ConquerPoints >= VIP1_PRICE)
                        {
                            client.Entity.ConquerPoints -= VIP1_PRICE;
                            client.Entity.VIPLevel = 1;
                        }
                        else
                        {
                            dialog.Text("You need " + VIP1_PRICE + " CPs to purchase VIP Level 1.");
                            dialog.Option("Understood.", 255);

                            dialog.Send();
                        }
                        break;
                    }
                case 2:
                    {
                        if (client.Entity.ConquerPoints >= VIP2_PRICE)
                        {
                            client.Entity.ConquerPoints -= VIP2_PRICE;
                            client.Entity.VIPLevel = 2;
                        }
                        else
                        {
                            dialog.Text("You need " + VIP2_PRICE + " CPs to purchase VIP Level 2.");
                            dialog.Option("Understood.", 255);

                            dialog.Send();
                        }
                        break;
                    }
                case 3:
                    {
                        if (client.Entity.ConquerPoints >= VIP3_PRICE)
                        {
                            client.Entity.ConquerPoints -= VIP3_PRICE;
                            client.Entity.VIPLevel = 3;
                        }
                        else
                        {
                            dialog.Text("You need " + VIP3_PRICE + " CPs to purchase VIP Level 3.");
                            dialog.Option("Understood.", 255);

                            dialog.Send();
                        }
                        break;
                    }
                case 4:
                    {
                        if (client.Entity.ConquerPoints >= VIP4_PRICE)
                        {
                            client.Entity.ConquerPoints -= VIP4_PRICE;
                            client.Entity.VIPLevel = 4;
                        }
                        else
                        {
                            dialog.Text("You need " + VIP4_PRICE + " CPs to purchase VIP Level 4.");
                            dialog.Option("Understood.", 255);

                            dialog.Send();
                        }
                        break;
                    }
                case 5:
                    {
                        if (client.Entity.ConquerPoints >= VIP5_PRICE)
                        {
                            client.Entity.ConquerPoints -= VIP5_PRICE;
                            client.Entity.VIPLevel = 5;
                        }
                        else
                        {
                            dialog.Text("You need " + VIP5_PRICE + " CPs to purchase VIP Level 5.");
                            dialog.Option("Understood.", 255);

                            dialog.Send();
                        }
                        break;
                    }
                case 6:
                    {
                        if (client.Entity.ConquerPoints >= VIP6_PRICE)
                        {
                            client.Entity.ConquerPoints -= VIP6_PRICE;
                            client.Entity.VIPLevel = 6;
                        }
                        else
                        {
                            dialog.Text("You need " + VIP6_PRICE + " CPs to purchase VIP Level 6.");
                            dialog.Option("Understood.", 255);

                            dialog.Send();
                        }
                        break;
                    }
            }
        }
    }
}

