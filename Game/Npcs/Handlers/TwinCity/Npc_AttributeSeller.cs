using System;
using System.Collections.Generic;
using System.Linq;
using static MTA.Game.Enums;
using MTA.Network;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Attribute Seller NPC - Provides Chi attribute modification, Perfection level enhancement, and Damage enhancement services
    /// </summary>
    [NpcHandler(888540)]
    public static class Npc_AttributeSeller
    {
        // Service option IDs
        private const byte CHI_SERVICE = 1;
        private const byte PERFECTION_SERVICE = 2;
        private const byte DAMAGE_SERVICE = 3;

        // Chi service option ranges
        private const byte CHI_STAGE_BASE = 10;       // 10-13: Select Chi stage (1-4)
        private const byte CHI_ATTRIBUTE_BASE = 20;   // 20-23: Select attribute slot (1-4)
        private const byte CHI_TYPE_SELECT = 30;       // 30+: Select attribute type (multiplied by 10)

        // Perfection service option IDs (100+)
        private const byte PERFECTION_MENU = 100;
        private const byte PERFECTION_ITEM_BASE = 101;

        // Damage service option IDs (200+)
        private const byte DAMAGE_MENU = 200;
        private const byte DAMAGE_ITEM_BASE = 201;

        // Prices
        private const uint CHI_CHANGE_PRICE = 1000;
        private const uint PERFECTION_PRICE = 1000000;
        private const uint PERFECTION_WING_PRICE = 200;
        private const uint DAMAGE_PRICE = 250000;

        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello! I provide various enhancement services for your equipment and attributes. What would you like to do?");
                        dialog.Option("Chi Attribute Modification", CHI_SERVICE);
                        dialog.Option("Perfection Level Enhancement", PERFECTION_SERVICE);
                        dialog.Option("Damage Enhancement (Bless)", DAMAGE_SERVICE);
                        dialog.Send();
                        break;
                    }

                #region Chi Attribute Modification
                case CHI_SERVICE:
                    {
                        if (client.ChiPowers.Count == 0)
                        {
                            dialog.Text("Sorry, but you don't have Chi yet, you need to unlock it first.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                            break;
                        }

                        dialog.Text("Hello! I'm the Chi Seller. Which Chi stage would you like to modify?");
                        for (int i = 0; i < client.ChiPowers.Count; i++)
                        {
                            dialog.Option(((ChiPowerType)(i + 1)).ToString(), (byte)(CHI_STAGE_BASE + i));
                        }
                        dialog.Option("Back", 0);
                        dialog.Send();
                        break;
                    }

                // Chi stage selection (10-13)
                case 10:
                case 11:
                case 12:
                case 13:
                    {
                        byte stage = (byte)(npcRequest.OptionID - CHI_STAGE_BASE + 1);
                        client.Entity.SelectedStage = stage;
                        dialog.Text("You have chosen the " + ((ChiPowerType)stage).ToString() + " stage. Please select which attribute you want to change.");
                        for (int i = 0; i < 4; i++)
                        {
                            dialog.Option("Attribute [" + (i + 1) + "]", (byte)(CHI_ATTRIBUTE_BASE + i));
                        }
                        dialog.Option("Back", CHI_SERVICE);
                        dialog.Send();
                        break;
                    }

                // Chi attribute slot selection (20-23)
                case 20:
                case 21:
                case 22:
                case 23:
                    {
                        byte attributeSlot = (byte)(npcRequest.OptionID - CHI_ATTRIBUTE_BASE + 1);
                        client.Entity.SelectedAttribute = attributeSlot;
                        var powers = client.ChiPowers[client.Entity.SelectedStage - 1];
                        var existingAttributes = new List<ChiAttribute>();

                        for (int i = 0; i < powers.Attributes.Length; i++)
                        {
                            var attr = powers.Attributes[i];
                            existingAttributes.Add(attr.Type);
                        }

                        dialog.Text("You have chosen Attribute [" + attributeSlot + "]. Please select the attribute type you want to change it to.");
                        for (int i = 0; i < (int)ChiAttribute.MagicDamageDecrease; i++)
                        {
                            var type = (ChiAttribute)(i + 1);
                            if (!existingAttributes.Contains(type))
                            {
                                dialog.Option(type.ToString(), (byte)(CHI_TYPE_SELECT + (i + 1) * 10));
                            }
                        }
                        dialog.Option("Back", (byte)(CHI_STAGE_BASE + client.Entity.SelectedStage - 1));
                        dialog.Send();
                        break;
                    }

                #region Perfection Level Enhancement
                case PERFECTION_SERVICE:
                case PERFECTION_MENU:
                    {
                        HandlePerfectionService(client, dialog, npcRequest.OptionID);
                        break;
                    }
                #endregion

                #region Damage Enhancement
                case DAMAGE_SERVICE:
                case DAMAGE_MENU:
                    {
                        HandleDamageService(client, dialog, npcRequest.OptionID);
                        break;
                    }
                #endregion

                // Chi attribute type selection and item selections (30+)
                default:
                    {
                        int optionId = npcRequest.OptionID;

                        // Chi attribute type selection (30-99)
                        if (optionId >= CHI_TYPE_SELECT && optionId < PERFECTION_MENU)
                        {
                            HandleChiAttributeChange(client, dialog, optionId);
                        }
                        // Perfection item selections (101-199)
                        else if (optionId >= PERFECTION_ITEM_BASE && optionId < DAMAGE_MENU)
                        {
                            HandlePerfectionService(client, dialog, optionId);
                        }
                        // Damage item selections (201+)
                        else if (optionId >= DAMAGE_ITEM_BASE)
                        {
                            HandleDamageService(client, dialog, optionId);
                        }
                        break;
                    }
                    #endregion
            }
        }

        #region Chi Service Methods
        private static void HandleChiAttributeChange(Client.GameState client, MTA.Npcs dialog, int optionId)
        {
            if (client.Entity.ConquerPoints < CHI_CHANGE_PRICE)
            {
                dialog.Text("Sorry, but you don't have enough Conquer Points. You need at least " + CHI_CHANGE_PRICE + " CPs.");
                dialog.Option("I understand.", 255);
                dialog.Send();
                return;
            }

            int attributeType = (optionId - CHI_TYPE_SELECT) / 10;
            int stage = client.Entity.SelectedStage;
            int attributePosition = client.Entity.SelectedAttribute - 1;
            var powers = client.ChiPowers[stage - 1];
            var attributes = powers.Attributes;

            // Check if attribute type already exists
            foreach (var attr in attributes)
            {
                if (attr.Type == (ChiAttribute)attributeType)
                {
                    client.MessageBox("Sorry, you can't have duplicate attribute types.", null, null);
                    return;
                }
            }

            // Apply the change
            attributes[attributePosition].Type = (ChiAttribute)attributeType;
            attributes[attributePosition].Value = (ushort)ChiMaxValues(attributes[attributePosition].Type);
            powers.CalculatePoints();
            Database.ChiTable.Sort((ChiPowerType)stage);
            powers.Power = (ChiPowerType)stage;
            client.Entity.ConquerPoints -= CHI_CHANGE_PRICE;
            client.Send(new ChiPowers(true).Query(client));

            // Update rankings
            UpdateChiRankings(client, (ChiPowerType)stage);

            Database.ChiTable.Save(client);
            dialog.Text("Your Chi attribute has been successfully changed!");
            dialog.Option("Thank you!", 255);
            dialog.Send();
        }

        private static void UpdateChiRankings(Client.GameState client, ChiPowerType powerType)
        {
            Database.ChiTable.ChiData[] array = null;
            switch (powerType)
            {
                case ChiPowerType.Dragon:
                    array = Database.ChiTable.Dragon;
                    break;
                case ChiPowerType.Phoenix:
                    array = Database.ChiTable.Phoenix;
                    break;
                case ChiPowerType.Tiger:
                    array = Database.ChiTable.Tiger;
                    break;
                case ChiPowerType.Turtle:
                    array = Database.ChiTable.Turtle;
                    break;
            }

            if (array == null) return;

            foreach (var chiData in array)
            {
                if (Kernel.GamePool.ContainsKey(chiData.UID))
                {
                    var pClient = Kernel.GamePool[chiData.UID];
                    if (pClient == null || pClient.ChiData == null) continue;

                    PacketHandler.SendRankingQuery(
                        new GenericRanking(true) { Mode = GenericRanking.QueryCount },
                        pClient,
                        GenericRanking.Chi + (uint)powerType,
                        pClient.ChiData.SelectRank(powerType),
                        pClient.ChiData.SelectPoints(powerType));

                    if (pClient.Entity.UID == client.Entity.UID || pClient.ChiData.SelectRank(powerType) < 50)
                    {
                        pClient.LoadItemStats();
                    }
                }
            }
        }
        #endregion

        #region Perfection Service Methods
        private static void HandlePerfectionService(Client.GameState client, MTA.Npcs dialog, int optionId)
        {
            if (optionId == PERFECTION_SERVICE || optionId == PERFECTION_MENU)
            {
                dialog.Text("Hello! I can enhance your equipment to Perfection Level 54. Most items cost 1,000,000 CPs, but Wings only cost 200 CPs. Which item would you like to enhance?");
                dialog.Option("Necklace", PERFECTION_ITEM_BASE + (byte)ConquerItem.Necklace);
                dialog.Option("Armor", PERFECTION_ITEM_BASE + (byte)ConquerItem.Armor);
                dialog.Option("Tower", PERFECTION_ITEM_BASE + (byte)ConquerItem.Tower);
                dialog.Option("Fan", PERFECTION_ITEM_BASE + (byte)ConquerItem.Fan);
                dialog.Option("Crop", PERFECTION_ITEM_BASE + (byte)ConquerItem.SteedCrop);
                dialog.Option("Wing", PERFECTION_ITEM_BASE + (byte)ConquerItem.Wing);
                dialog.Option("Ring", PERFECTION_ITEM_BASE + (byte)ConquerItem.Ring);
                dialog.Option("Head", PERFECTION_ITEM_BASE + (byte)ConquerItem.Head);
                dialog.Option("Boots", PERFECTION_ITEM_BASE + (byte)ConquerItem.Boots);
                dialog.Option("Steed", PERFECTION_ITEM_BASE + (byte)ConquerItem.Steed);
                dialog.Option("Right Weapon", PERFECTION_ITEM_BASE + (byte)ConquerItem.RightWeapon);
                dialog.Option("Left Weapon", PERFECTION_ITEM_BASE + (byte)ConquerItem.LeftWeapon);
                dialog.Option("Back", 0);
                dialog.Send();
                return;
            }

            byte itemSlot = (byte)(optionId - PERFECTION_ITEM_BASE);
            ConquerItem item = client.Equipment.TryGetItem(itemSlot);

            if (item == null)
            {
                dialog.Text("You don't have an item equipped in that slot.");
                dialog.Option("I understand.", 255);
                dialog.Send();
                return;
            }

            uint price = (itemSlot == (byte)ConquerItem.Wing) ? PERFECTION_WING_PRICE : PERFECTION_PRICE;

            if (item.Perfectionlevel >= 0 && client.Entity.ConquerPoints >= price)
            {
                client.Entity.ConquerPoints -= price;
                item.Perfectionlevel = 54;
                item.Mode = ItemMode.Update;
                item.Send(client);
                Database.ConquerItemTable.UpdatePerfection(item);

                dialog.Text("Your equipment has been enhanced to Perfection Level 54!");
                dialog.Option("Thank you!", 255);
                dialog.Send();
            }
            else
            {
                dialog.Text("Please come back with " + price + " Conquer Points.");
                dialog.Option("I understand.", 255);
                dialog.Send();
            }
        }
        #endregion

        #region Damage Service Methods
        private static void HandleDamageService(Client.GameState client, MTA.Npcs dialog, int optionId)
        {
            if (optionId == DAMAGE_SERVICE || optionId == DAMAGE_MENU)
            {
                dialog.Text("Hello! I can enhance your equipment with Damage (Bless +1) for 250,000 CPs. Which item would you like to enhance?");
                dialog.Option("Mount Armor", DAMAGE_ITEM_BASE + (byte)ConquerItem.SteedArmor);
                dialog.Option("Garment", DAMAGE_ITEM_BASE + (byte)ConquerItem.Garment);
                dialog.Option("Tower", DAMAGE_ITEM_BASE + (byte)ConquerItem.Tower);
                dialog.Option("Fan", DAMAGE_ITEM_BASE + (byte)ConquerItem.Fan);
                dialog.Option("Crop", DAMAGE_ITEM_BASE + (byte)ConquerItem.SteedCrop);
                dialog.Option("Bottle", DAMAGE_ITEM_BASE + (byte)ConquerItem.Bottle);
                dialog.Option("Wing", DAMAGE_ITEM_BASE + (byte)ConquerItem.Wing);
                dialog.Option("Right Accessory", DAMAGE_ITEM_BASE + (byte)ConquerItem.RightWeaponAccessory);
                dialog.Option("Left Accessory", DAMAGE_ITEM_BASE + (byte)ConquerItem.LeftWeaponAccessory);
                dialog.Option("Back", 0);
                dialog.Send();
                return;
            }

            byte itemSlot = (byte)(optionId - DAMAGE_ITEM_BASE);
            ConquerItem item = client.Equipment.TryGetItem(itemSlot);

            if (item == null)
            {
                dialog.Text("You don't have an item equipped in that slot.");
                dialog.Option("I understand.", 255);
                dialog.Send();
                return;
            }

            if (item.Bless == 0 && client.Entity.ConquerPoints >= DAMAGE_PRICE)
            {
                client.Entity.ConquerPoints -= DAMAGE_PRICE;
                item.Bless = 1;
                item.Mode = ItemMode.Update;
                item.Send(client);
                Database.ConquerItemTable.UpdateBless(item);

                dialog.Text("Your equipment has been enhanced with Damage (Bless +1)!");
                dialog.Option("Thank you!", 255);
                dialog.Send();
            }
            else if (item.Bless > 0)
            {
                dialog.Text("This item already has Damage enhancement.");
                dialog.Option("I understand.", 255);
                dialog.Send();
            }
            else
            {
                dialog.Text("Please come back with " + DAMAGE_PRICE + " Conquer Points.");
                dialog.Option("I understand.", 255);
                dialog.Send();
            }
        }
        #endregion
    }
}