using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Steed Seller NPC - Sells special +12 steeds for CPs
    /// </summary>
    [NpcHandler(34216)]
    public static class Npc_SteedSeller
    {
        private const uint STEED_PRICE = 10000;

        // Steed color definitions: (optionId, colorR, colorG, colorB, useSingleValue)
        // useSingleValue: true if only R value should be used (for option 28)
        private static readonly (int OptionId, byte R, byte G, byte B, bool UseSingleValue)[] SteedColors =
        [
            (1, 150, 150, 50, false),      // MTA Steed (commented out in original)
            (2, 62, 63, 184, false),       // FrostBite Steed
            (3, 147, 134, 122, false),     // Spotted Steed
            (4, 148, 156, 137, false),     // BlazeHoof Steed
            (5, 142, 39, 46, false),       // Zebra Steed
            (6, 154, 136, 236, false),     // Pasion Steed
            (7, 187, 135, 201, false),     // SpitFire Steed
            (8, 11, 42, 12, false),        // Star Steed
            (9, 125, 125, 200, false),     // Paisley Steed
            (10, 196, 1, 196, false),      // Pink Steed
            (11, 130, 79, 186, false),     // Red Steed
            (12, 90, 142, 190, false),     // Pine Steed
            (13, 0, 206, 122, false),      // Jade Steed
            (14, 130, 40, 205, false),     // Shadow Steed
            (15, 50, 160, 90, false),      // Emerald Steed
            (16, 125, 1, 125, false),      // Marble Steed
            (17, 220, 220, 1, false),      // Tawny Steed
            (18, 50, 50, 150, false),      // Mauve Steed
            (19, 50, 200, 50, false),      // Pearl Steed
            (20, 100, 100, 190, false),    // Ivory Steed
            (21, 150, 50, 150, false),     // Agate Steed
            (22, 230, 236, 50, false),     // Moon Steed
            (23, 230, 236, 1, false),      // Kahki Steed
            (24, 148, 1, 196, false),      // Indigo Steed
            (25, 153, 159, 32, false),     // Misty Steed
            (26, 50, 50, 50, false),       // Beige Steed
            (27, 251, 0, 124, false),      // Coral Steed
            (28, 10, 0, 0, true),          // Lavender Steed (only R value provided)
            (29, 212, 214, 121, false),    // Amber Steed
            (30, 135, 211, 211, false),    // Sunset Steed
            (31, 135, 237, 178, false),    // Diamond Steed
            (32, 212, 122, 166, false),    // Purple Steed
            (33, 212, 156, 137, false),    // Plaid Steed
            (34, 147, 156, 137, false),    // Golden Steed
            (35, 172, 125, 23, false)      // Brown Steed
        ];

        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello, I offer a fine selection of special +12 horses.");
                        dialog.Text("Each steed costs " + STEED_PRICE + " CPs. Which one would you like to purchase?");
                        dialog.Option("Frost Bite Steed", 2);
                        dialog.Option("Spotted Steed", 3);
                        dialog.Option("Blaze Hoof Steed", 4);
                        dialog.Option("Zebra Steed", 5);
                        dialog.Option("Pasion Steed", 6);
                        dialog.Option("Spit Fire Steed", 7);
                        dialog.Option("Star Steed", 8);
                        dialog.Option("Paisley Steed", 9);
                        dialog.Option("Pink Steed", 10);
                        dialog.Option("Red Steed", 11);
                        dialog.Option("Pine Steed", 12);
                        dialog.Option("Jade Steed", 13);
                        dialog.Option("Shadow Steed", 14);
                        dialog.Option("Emerald Steed", 15);
                        dialog.Option("Marble Steed", 16);
                        dialog.Option("Tawny Steed", 17);
                        dialog.Option("Mauve Steed", 18);
                        dialog.Option("Pearl Steed", 19);
                        dialog.Option("Ivory Steed", 20);
                        dialog.Option("Agate Steed", 21);
                        dialog.Option("Moon Steed", 22);
                        dialog.Option("Kahki Steed", 23);
                        dialog.Option("Indigo Steed", 24);
                        dialog.Option("Misty Steed", 25);
                        dialog.Option("Beige Steed", 26);
                        dialog.Option("Coral Steed", 27);
                        dialog.Option("Lavender Steed", 28);
                        dialog.Option("Amber Steed", 29);
                        dialog.Option("Sunset Steed", 30);
                        dialog.Option("Diamond Steed", 31);
                        dialog.Option("Purple Steed", 32);
                        dialog.Option("Plaid Steed", 33);
                        dialog.Option("Golden Steed", 34);
                        dialog.Option("Brown Steed", 35);
                        client.Entity.Update(_String.Effect, "ErLongTengFei", true);
                        dialog.Send();
                        break;
                    }
                default:
                    {
                        // Find the steed color for this option ID
                        var steedColor = Array.Find(SteedColors, s => s.OptionId == npcRequest.OptionID);

                        if (client.Entity.ConquerPoints >= STEED_PRICE)
                        {
                            client.Entity.ConquerPoints -= STEED_PRICE;

                            // Build the command string with color values
                            string colorParams;
                            if (steedColor.UseSingleValue)
                            {
                                // Special case: only use R value (for option 28)
                                colorParams = steedColor.R.ToString();
                            }
                            else
                            {
                                colorParams = steedColor.R + " " + steedColor.G + " " + steedColor.B;
                            }

                            MTA.Network.PacketHandler.CheckCommand2("@tegotegatege Steed Fixed 12 0 0 0 0 " + colorParams, client);

                            dialog.Text("Congratulations! You have successfully purchased your new special steed.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I'm sorry, but you need " + STEED_PRICE + " CPs to purchase this steed.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                        }
                        break;
                    }
            }
        }
    }
}
