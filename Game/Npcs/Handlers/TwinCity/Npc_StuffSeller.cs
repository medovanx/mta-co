using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Stuff Seller NPC - Sells full equipment sets and special items
    /// </summary>
    [NpcHandler(225584)]
    public static class Npc_StuffSeller
    {
        private const int MAX_INVENTORY_SLOTS = 40;

        // Common soul item parameters
        private const byte SOUL_TYPE = 6;
        private const byte SOUL_LEVEL = 12;
        private const byte SOUL_EXP = 12;
        private const byte SOUL_QUALITY = 1;


        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello, would you like to purchase a full equipment set?");
                        dialog.Option("Dragon Warrior P7 Set", 1);
                        dialog.Option("Ninja P7 Set", 3);
                        dialog.Option("Monk P7 Set", 4);
                        dialog.Option("Taoist P7 Set", 5);
                        dialog.Option("Trojan P7 Set", 6);
                        dialog.Option("Pirate P7 Set", 7);
                        dialog.Option("Warrior P7 Set", 8);
                        dialog.Option("Archer P7 Set", 9);
                        dialog.Option("Wind Walker P7 Set", 11);
                        dialog.Option("Fan, Tower, Wing, Crop, Steed", 10);
                        dialog.Send();
                        break;
                    }
                #region Fan, Tower, Wing, Crop, and Steed Set
                case 10:
                    {
                        const int requiredSlots = 5;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        // Steed
                        var steed = new ConquerItem(true)
                        {
                            ID = 300000,
                            Color = Game.Enums.Color.White,
                            Plus = 12
                        };
                        steed.Durability = steed.MaximDurability = Database.ConquerItemInformation.BaseInformations[300000].Durability;
                        AddItem(client, steed);

                        // Tower
                        var tower = new ConquerItem(true)
                        {
                            ID = 202009,
                            Color = Game.Enums.Color.White,
                            Plus = 12,
                            Bless = 1,
                            SocketOne = Game.Enums.Gem.SuperGloryGem,
                            SocketTwo = Game.Enums.Gem.SuperGloryGem
                        };
                        tower.Durability = tower.MaximDurability = Database.ConquerItemInformation.BaseInformations[202009].Durability;
                        AddItem(client, tower);

                        // Fan
                        var fan = new ConquerItem(true)
                        {
                            ID = 201009,
                            Color = Game.Enums.Color.White,
                            Plus = 12,
                            Bless = 1,
                            SocketOne = Game.Enums.Gem.SuperThunderGem,
                            SocketTwo = Game.Enums.Gem.SuperThunderGem
                        };
                        fan.Durability = fan.MaximDurability = Database.ConquerItemInformation.BaseInformations[201009].Durability;
                        AddItem(client, fan);

                        // Wing
                        var wing = new ConquerItem(true)
                        {
                            ID = 204009,
                            Color = Game.Enums.Color.White,
                            Plus = 12,
                            Bless = 0,
                            SocketOne = Game.Enums.Gem.SuperThunderGem,
                            SocketTwo = Game.Enums.Gem.SuperGloryGem
                        };
                        wing.Durability = wing.MaximDurability = Database.ConquerItemInformation.BaseInformations[204009].Durability;
                        AddItem(client, wing);

                        // Crop
                        var crop = new ConquerItem(true)
                        {
                            ID = 203009,
                            Color = Game.Enums.Color.White,
                            Plus = 12,
                            Bless = 1
                        };
                        crop.Durability = crop.MaximDurability = Database.ConquerItemInformation.BaseInformations[203009].Durability;
                        AddItem(client, crop);
                        break;
                    }
                #endregion

                #region Wind Walker P7 Set
                case 11:
                    {
                        const int requiredSlots = 7;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 626439, 801308); // Dragon Fan (x2)
                        AddSoulItem(client, 626439, 801308);
                        AddSoulItem(client, 101309, 822071); // Armor
                        AddSoulItem(client, 170309, 820073); // Hat
                        AddSoulItem(client, 120269, 821033); // Necklace
                        AddSoulItem(client, 150269, 823058); // Ring
                        AddSoulItem(client, 160249, 824018); // Boot
                        break;
                    }
                #endregion

                #region Bruce Lee P7 Set
                case 1:
                    {
                        const int requiredSlots = 7;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 617439, 801004); // Sky Nunchaku (x2)
                        AddSoulItem(client, 617439, 801004);
                        AddSoulItem(client, 138309, 822071); // Bruce Lee Armor
                        AddSoulItem(client, 148309, 820073); // Bruce Lee Hood
                        AddSoulItem(client, 120269, 821033); // Necklace
                        AddSoulItem(client, 150269, 823058); // Ring
                        AddSoulItem(client, 160249, 824018); // Boot
                        break;
                    }
                #endregion

                #region Ninja P7 Set
                case 3:
                    {
                        const int requiredSlots = 14;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 511439, 800255); // Scythe (x2)
                        AddSoulItem(client, 511439, 800255);
                        AddSoulItem(client, 616439, 800111); // Katana (x2)
                        AddSoulItem(client, 616439, 800111);
                        AddSoulItem(client, 601439, 800111); // Katana (x2)
                        AddSoulItem(client, 601439, 800111);
                        AddSoulItem(client, 601439, 800142); // Katana (x2)
                        AddSoulItem(client, 601439, 800142);
                        AddSoulItem(client, 135309, 822071); // Ninja Armor
                        AddSoulItem(client, 123309, 820073); // Ninja Hood
                        AddSoulItem(client, 120269, 821033); // Necklace
                        AddSoulItem(client, 150269, 823058); // Ring
                        AddSoulItem(client, 160249, 824018); // Boot

                        // Epic Ninja Equipment
                        AddItem(client, CreateEpicItem(621439));
                        break;
                    }
                #endregion

                #region Monk P7 Set
                case 4:
                    {
                        const int requiredSlots = 11;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 622439, 800725); // Beads (x2)
                        AddSoulItem(client, 622439, 800725);
                        AddSoulItem(client, 610439, 800725); // Beads (x2)
                        AddSoulItem(client, 610439, 800725);
                        AddSoulItem(client, 136309, 822071); // Monk Armor
                        AddSoulItem(client, 143309, 820073); // Monk Cap
                        AddSoulItem(client, 120269, 821033); // Necklace
                        AddSoulItem(client, 150269, 823058); // Ring
                        AddSoulItem(client, 160249, 824018); // Boot

                        // Epic Monk Equipment (2x)
                        AddItem(client, CreateEpicItem(622439));
                        AddItem(client, CreateEpicItem(622439));
                        break;
                    }
                #endregion

                #region Taoist P7 Set
                case 5:
                    {
                        const int requiredSlots = 8;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 421439, 800522); // Back Sword
                        AddSoulItem(client, 620439, 800522); // Imperial Backsword
                        AddSoulItem(client, 619439, 801104); // Universe Hossu
                        AddSoulItem(client, 134309, 822071); // Fire Armor
                        AddSoulItem(client, 114309, 820076); // Fire Cap
                        AddSoulItem(client, 152279, 823060); // Bracelet
                        AddSoulItem(client, 121269, 821034); // Bag
                        AddSoulItem(client, 160249, 824018); // Boot
                        break;
                    }
                #endregion

                #region Trojan P7 Set
                case 6:
                    {
                        const int requiredSlots = 11;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 480439, 800111); // Club
                        AddSoulItem(client, 614429, 800111); // Shadow Cross Saber (x2)
                        AddSoulItem(client, 614429, 800111);
                        AddSoulItem(client, 410439, 800111); // Blade
                        AddSoulItem(client, 420439, 800111); // Sword
                        AddSoulItem(client, 130309, 822071); // Trojan Armor
                        AddSoulItem(client, 118309, 820073); // Trojan Cap
                        AddSoulItem(client, 120269, 821033); // Necklace
                        AddSoulItem(client, 150269, 823058); // Ring
                        AddSoulItem(client, 160249, 824018); // Boot

                        // Vigorous Heavy Ring
                        AddItem(client, CreateEpicItem(151269));
                        break;
                    }
                #endregion

                #region Pirate P7 Set
                case 7:
                    {
                        const int requiredSlots = 7;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 611439, 800811); // Rapier
                        AddSoulItem(client, 612439, 800810); // Pistol
                        AddSoulItem(client, 139309, 822071); // Pirate Armor
                        AddSoulItem(client, 144309, 820073); // Pirate Cap
                        AddSoulItem(client, 120269, 821033); // Necklace
                        AddSoulItem(client, 150269, 823058); // Ring
                        AddSoulItem(client, 160249, 824018); // Boot
                        break;
                    }
                #endregion

                #region Warrior P7 Set
                case 8:
                    {
                        const int requiredSlots = 7;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 560439, 800215); // Spear
                        AddSoulItem(client, 900309, 800422); // Shield
                        AddSoulItem(client, 131309, 822071); // Warrior Armor
                        AddSoulItem(client, 111309, 820073); // Warrior Cap
                        AddSoulItem(client, 120269, 821033); // Necklace
                        AddSoulItem(client, 150269, 823058); // Ring
                        AddSoulItem(client, 160249, 824018); // Boot
                        break;
                    }
                #endregion

                #region Archer P7 Set
                case 9:
                    {
                        const int requiredSlots = 8;
                        if (!CheckInventorySpace(client, dialog, requiredSlots)) break;

                        AddSoulItem(client, 613429, 800917); // Knife (x2)
                        AddSoulItem(client, 613429, 800917);
                        AddSoulItem(client, 500429, 800618); // Bow
                        AddSoulItem(client, 133309, 822071); // Archer Armor
                        AddSoulItem(client, 113309, 820073); // Archer Cap
                        AddSoulItem(client, 120269, 821033); // Necklace
                        AddSoulItem(client, 150269, 823058); // Ring
                        AddSoulItem(client, 160249, 824018); // Boot
                        break;
                    }
                    #endregion
            }
        }

        #region Utility Methods
        /// <summary>
        /// Checks inventory space and shows error if insufficient
        /// </summary>
        private static bool CheckInventorySpace(Client.GameState client, MTA.Npcs dialog, int requiredSlots)
        {
            if (client.Inventory.Count > MAX_INVENTORY_SLOTS - requiredSlots)
            {
                dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                dialog.Option("Understood.", 255);
                dialog.Send();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates an Epic item with standard configuration
        /// </summary>
        private static ConquerItem CreateEpicItem(uint itemId, Game.Enums.Gem socketGem = Game.Enums.Gem.SuperDragonGem)
        {
            var item = new ConquerItem(true)
            {
                ID = itemId,
                Color = Game.Enums.Color.White,
                Plus = 12,
                Bless = 7,
                Enchant = 240,
                SocketOne = socketGem,
                SocketTwo = socketGem
            };
            item.Durability = item.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId].Durability;
            return item;
        }

        /// <summary>
        /// Adds a soul item with standard parameters
        /// </summary>
        private static void AddSoulItem(Client.GameState client, uint itemId, uint soulId)
        {
            client.Inventory.AddSoul(itemId, soulId, SOUL_TYPE, SOUL_LEVEL, SOUL_EXP, SOUL_QUALITY, true, false);
        }

        /// <summary>
        /// Creates and adds a ConquerItem to inventory
        /// </summary>
        private static void AddItem(Client.GameState client, ConquerItem item)
        {
            client.Inventory.Add(item, Game.Enums.ItemUse.CreateAndAdd);
        }
        #endregion
    }
}
