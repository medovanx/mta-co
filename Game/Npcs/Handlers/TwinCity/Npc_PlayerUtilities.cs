using static MTA.Game.Enums;
using MTA.Network.GamePackets;
using MTA.Database;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Player Utilities NPC - Provides various utility services for players
    /// </summary>
    [NpcHandler(34227)]
    public static class Npc_PlayerUtilities
    {
        // Wing definitions: (type, id)
        private static readonly (short Type, short Id)[] Wings =
        [
            (WingsConstants.Type_KingWing, WingsConstants.Id_KingWing),
            (WingsConstants.Type_FlagWing, WingsConstants.Id_FlagWing),
            (WingsConstants.Type_StarlightWing, WingsConstants.Id_StarlightWing),
            (WingsConstants.Type_MoonlightWing, WingsConstants.Id_MoonlightWing),
            (WingsConstants.Type_WinnerWing, WingsConstants.Id_WinnerWing),
            (WingsConstants.Type_FairyWing, WingsConstants.Id_FairyWing),
            (WingsConstants.Type_PlanetWing, WingsConstants.Id_PlanetWing),
            (WingsConstants.Type_VioletLightningWing, WingsConstants.Id_VioletLightningWing),
            (WingsConstants.Type_VioletCloudWing, WingsConstants.Id_VioletCloudWing),
            (WingsConstants.Type_SupremeWing, WingsConstants.Id_SupremeWing),
            (WingsConstants.Type_RomanceWing, WingsConstants.Id_RomanceWing),
            (WingsConstants.Type_EmeraldGlowWing, WingsConstants.Id_EmeraldGlowWing),
            (WingsConstants.Type_OrangeGlowWing, WingsConstants.Id_OrangeGlowWing),
            (WingsConstants.Type_FlameGlowWing, WingsConstants.Id_FlameGlowWing),
            (WingsConstants.Type_BrightGlowWing, WingsConstants.Id_BrightGlowWing),
            (WingsConstants.Type_SirenSongWing, WingsConstants.Id_SirenSongWing)
        ];

        // Title definitions: (type, id)
        private static readonly (short Type, short Id)[] Titles =
        [
            (TitlesConstants.Type_Overlord, TitlesConstants.Id_Overlord),
            (TitlesConstants.Type_ChiBloom, TitlesConstants.Id_ChiBloom),
            (TitlesConstants.Type_Experienced, TitlesConstants.Id_Experienced),
            (TitlesConstants.Type_Valiant, TitlesConstants.Id_Valiant),
            (TitlesConstants.Type_Supremacy, TitlesConstants.Id_Supremacy),
            (TitlesConstants.Type_Original, TitlesConstants.Id_Original),
            (TitlesConstants.Type_Luxury, TitlesConstants.Id_Luxury),
            (TitlesConstants.Type_Brilliant, TitlesConstants.Id_Brilliant),
            (TitlesConstants.Type_MrConquerChampion, TitlesConstants.Id_MrConquerChampion),
            (TitlesConstants.Type_Beauty, TitlesConstants.Id_Beauty),
            (TitlesConstants.Type_MsConquerPlace, TitlesConstants.Id_MsConquerPlace),
            (TitlesConstants.Type_MsConquerChampion, TitlesConstants.Id_MsConquerChampion),
            (TitlesConstants.Type_Wise, TitlesConstants.Id_Wise),
            (TitlesConstants.Type_EarthKnight, TitlesConstants.Id_EarthKnight),
            (TitlesConstants.Type_GloryKnight, TitlesConstants.Id_GloryKnight),
            (TitlesConstants.Type_SkyKnight, TitlesConstants.Id_SkyKnight),
            (TitlesConstants.Type_Paladin, TitlesConstants.Id_Paladin),
            (TitlesConstants.Type_WorldRenowned, TitlesConstants.Id_WorldRenowned),
            (TitlesConstants.Type_WorldDominator, TitlesConstants.Id_WorldDominator),
            (TitlesConstants.Type_BigFan, TitlesConstants.Id_BigFan),
            (TitlesConstants.Type_EuroCollector, TitlesConstants.Id_EuroCollector),
            (TitlesConstants.Type_Invincible, TitlesConstants.Id_Invincible),
            (TitlesConstants.Type_Pioneer, TitlesConstants.Id_Pioneer),
            (TitlesConstants.Type_Mighty, TitlesConstants.Id_Mighty),
            (TitlesConstants.Type_Supernatural, TitlesConstants.Id_Supernatural),
            (TitlesConstants.Type_SaintRider, TitlesConstants.Id_SaintRider),
            (TitlesConstants.Type_Legendary, TitlesConstants.Id_Legendary),
            (TitlesConstants.Type_Peerless, TitlesConstants.Id_Peerless),
            (TitlesConstants.Type_Outstanding, TitlesConstants.Id_Outstanding),
            (TitlesConstants.Type_Expert, TitlesConstants.Id_Expert),
            (TitlesConstants.Type_PeerlessBeauty, TitlesConstants.Id_PeerlessBeauty),
            (TitlesConstants.Type_SuperIdol, TitlesConstants.Id_SuperIdol),
            (TitlesConstants.Type_MostRadiant, TitlesConstants.Id_MostRadiant),
            (TitlesConstants.Type_MostAttractive, TitlesConstants.Id_MostAttractive),
            (TitlesConstants.Type_CelestialFox, TitlesConstants.Id_CelestialFox),
            (TitlesConstants.Type_CelestialFoxFantasy, TitlesConstants.Id_CelestialFoxFantasy),
            (TitlesConstants.Type_FashionIcon, TitlesConstants.Id_FashionIcon),
            (TitlesConstants.Type_FashionIdol, TitlesConstants.Id_FashionIdol),
            (TitlesConstants.Type_FashionMaster, TitlesConstants.Id_FashionMaster),
            (TitlesConstants.Type_HonoredHero, TitlesConstants.Id_HonoredHero),
            (TitlesConstants.Type_BasketballLeader, TitlesConstants.Id_BasketballLeader),
            (TitlesConstants.Type_PokerKing, TitlesConstants.Id_PokerKing),
            (TitlesConstants.Type_PokerLord, TitlesConstants.Id_PokerLord),
            (TitlesConstants.Type_PokerMaster, TitlesConstants.Id_PokerMaster),
            (TitlesConstants.Type_PokerStar, TitlesConstants.Id_PokerStar),
            (TitlesConstants.Type_KingTeam, TitlesConstants.Id_KingTeam),
            (TitlesConstants.Type_DominatorTeam, TitlesConstants.Id_DominatorTeam),
            (TitlesConstants.Type_PowerTeam, TitlesConstants.Id_PowerTeam),
            (TitlesConstants.Type_EliteTeam, TitlesConstants.Id_EliteTeam)
        ];

        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello, how can I help you today?");
                        dialog.Option("Clear my inventory", 1);
                        dialog.Option("Clear all PK points", 3);
                        dialog.Option("Get wardrobe items", 5);
                        dialog.Option("Add Study Point +62000", 9);
                        dialog.Option("Get Level 140", 10);
                        dialog.Option("Adjust Attribute Points", 11);
                        dialog.Option("Change Sex", 21);
                        dialog.Send();
                        break;
                    }
                case 1:
                    {
                        dialog.Text("Are you sure you want to clear your inventory?");
                        dialog.Option("Yes, clear", 2);
                        dialog.Option("No, cancel", 255);
                        dialog.Send();
                        break;
                    }
                case 2:
                    {
                        var inventory = new ConquerItem[client.Inventory.Objects.Length];
                        client.Inventory.Objects.CopyTo(inventory, 0);
                        foreach (var item in inventory)
                        {
                            client.Inventory.Remove(item, ItemUse.Remove);
                        }
                        break;
                    }
                case 3:
                    {
                        if (client.Entity.PKPoints > 0)
                        {
                            dialog.Text("Are you sure you want to clear all PK points?");
                            dialog.Option("Yes, clear all PK", 4);
                            dialog.Option("No, cancel", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You don't have any PK points to clear.");
                            dialog.Option("I see.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 4:
                    {
                        if (client.Entity.PKPoints > 0)
                        {
                            client.Entity.PKPoints = 0;
                        }
                        break;
                    }
                case 5:
                    {
                        dialog.Text("Which wardrobe items would you like to receive?");
                        dialog.Option("Wings", 6);
                        dialog.Option("Titles", 7);
                        dialog.Option("Add Title Points +5000", 8);
                        dialog.Option("Back", 0);
                        dialog.Send();
                        break;
                    }
                case 6:
                    {
                        foreach (var (type, id) in Wings)
                        {
                            new TitleStorage().AddTitle(client, type, id, true);
                        }
                        break;
                    }
                case 7:
                    {
                        foreach (var (type, id) in Titles)
                        {
                            new TitleStorage().AddTitle(client, type, id, true);
                        }
                        break;
                    }
                case 8:
                    {
                        var titlePoints = 5000;
                        if (client.Entity.WTitles != null)
                        {
                            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                            {
                                cmd.Update("titles").Set("Points", titlePoints + client.Entity.WTitles.Points).Where("Id", client.Entity.UID).Execute();
                            }
                            StorageManager.Load();
                            client.Entity.TitlePoints += titlePoints;
                        }
                        break;
                    }
                case 9:
                    {
                        ushort studyPoint = 62000;
                        if (client.Entity.SubClasses.StudyPoints < studyPoint)
                        {
                            client.Entity.SubClasses.StudyPoints = studyPoint;
                        }
                        break;
                    }
                case 10:
                    {
                        if (client.Entity.Level < 140)
                        {
                            client.Entity.Level = 140;
                        }
                        break;
                    }
                case 11:
                    {
                        dialog.Text("I can adjust your attribute points. Choose your preferred class distribution:");
                        dialog.Option("Monk", 12);
                        dialog.Option("Ninja", 13);
                        dialog.Option("Taoist", 14);
                        dialog.Option("Archer", 15);
                        dialog.Option("Warrior", 16);
                        dialog.Option("Trojan", 17);
                        dialog.Option("Pirate", 18);
                        dialog.Option("Full HP (Vitality)", 19);
                        dialog.Option("Back", 0);
                        dialog.Send();
                        break;
                    }
                case 12: // Monk-Master
                    {
                        if (client.Entity.Level >= 140)
                        {
                            client.Entity.Strength = 119;
                            client.Entity.Vitality = 73;
                            client.Entity.Agility = 36;
                            client.Entity.Spirit = 0;
                            client.Entity.Atributes = 310;
                        }
                        break;
                    }
                case 13: // Ninja-Master
                    {
                        if (client.Entity.Level >= 140)
                        {
                            client.Entity.Strength = 34;
                            client.Entity.Vitality = 100;
                            client.Entity.Agility = 275;
                            client.Entity.Spirit = 20;
                            client.Entity.Atributes = 109;
                        }
                        break;
                    }
                case 14: // Taoist-Master
                    {
                        if (client.Entity.Level >= 140)
                        {
                            client.Entity.Strength = 0;
                            client.Entity.Vitality = 60;
                            client.Entity.Agility = 100;
                            client.Entity.Spirit = 250;
                            client.Entity.Atributes = 128;
                        }
                        break;
                    }
                case 15: // Archer-Master
                    {
                        if (client.Entity.Level >= 140)
                        {
                            client.Entity.Strength = 100;
                            client.Entity.Vitality = 70;
                            client.Entity.Agility = 250;
                            client.Entity.Spirit = 170;
                            client.Entity.Atributes = 118;
                        }
                        break;
                    }
                case 16: // Warrior-Master
                    {
                        if (client.Entity.Level >= 140)
                        {
                            client.Entity.Strength = 176;
                            client.Entity.Vitality = 100;
                            client.Entity.Agility = 50;
                            client.Entity.Spirit = 80;
                            client.Entity.Atributes = 132;
                        }
                        break;
                    }
                case 17: // Trojan-Master
                    {
                        if (client.Entity.Level >= 140)
                        {
                            client.Entity.Strength = 176;
                            client.Entity.Vitality = 300;
                            client.Entity.Agility = 40;
                            client.Entity.Spirit = 20;
                            client.Entity.Atributes = 2;
                        }
                        break;
                    }
                case 18: // Pirate-Master
                    {
                        if (client.Entity.Level >= 140)
                        {
                            client.Entity.Strength = 176;
                            client.Entity.Vitality = 150;
                            client.Entity.Agility = 50;
                            client.Entity.Spirit = 20;
                            client.Entity.Atributes = 122;
                        }
                        break;
                    }
                case 19: // Full HP
                    {
                        if (client.Entity.Level >= 140)
                        {
                            client.Entity.Strength = 0;
                            client.Entity.Vitality = 538;
                            client.Entity.Agility = 0;
                            client.Entity.Spirit = 0;
                            client.Entity.Atributes = 0;
                        }
                        break;
                    }
                case 21: // Change Gender
                    {
                        // Check if player is a boy (1003 = Small Boy, 1004 = Big Boy)
                        if (client.Entity.Body == 1003 || client.Entity.Body == 1004)
                        {
                            dialog.Text("Please choose the size for your new gender:");
                            dialog.Option("Big", 24);
                            dialog.Option("Small", 25);
                            dialog.Send();
                        }
                        // Check if player is a girl (2001 = Small Girl, 2002 = Big Girl)
                        else if (client.Entity.Body == 2001 || client.Entity.Body == 2002)
                        {
                            dialog.Text("Please choose the size for your new gender:");
                            dialog.Option("Big", 26);
                            dialog.Option("Small", 27);
                            dialog.Send();
                        }
                        break;
                    }
                case 24: // Big Girl (2002)
                    {
                        if (client.Entity.Body == 1003 || client.Entity.Body == 1004)
                        {
                            client.Equipment.Remove(9);
                            if (client.Equipment.Objects[9] != null)
                            {
                                client.Equipment.Objects[9] = null;
                            }
                            ClientEquip equips = new();
                            equips.DoEquips(client);
                            client.Send(equips);
                            client.NobilityInformation.Gender = 0;
                            client.Entity.Spouse = "None";
                            client.Entity.Body = 2002;
                            client.NobilityInformation.Mesh = client.Entity.Mesh;
                            client.Equipment.UpdateEntityPacket();
                            Database.EntityTable.SaveEntity(client);
                        }
                        break;
                    }
                case 25: // Small Girl (2001)
                    {
                        if (client.Entity.Body == 1003 || client.Entity.Body == 1004)
                        {
                            client.Equipment.Remove(9);
                            if (client.Equipment.Objects[9] != null)
                                client.Equipment.Objects[9] = null;
                        }
                        ClientEquip equips = new();
                        equips.DoEquips(client);
                        client.Send(equips);
                        client.NobilityInformation.Gender = 0;
                        client.Entity.Spouse = "None";
                        client.Entity.Body = 2001;
                        client.NobilityInformation.Mesh = client.Entity.Mesh;
                        client.Equipment.UpdateEntityPacket();
                        Database.EntityTable.SaveEntity(client);
                    }
                    break;
                case 26: // Big Boy (1004)
                    {
                        if (client.Entity.Body == 2001 || client.Entity.Body == 2002)
                        {
                            client.Equipment.Remove(9);
                            if (client.Equipment.Objects[9] != null)
                                client.Equipment.Objects[9] = null;
                        }
                        ClientEquip equips = new();
                        equips.DoEquips(client);
                        client.Send(equips);
                        client.NobilityInformation.Gender = 1;
                        client.Entity.Spouse = "None";
                        client.Entity.Body = 1004;
                        client.NobilityInformation.Mesh = client.Entity.Mesh;
                        client.Equipment.UpdateEntityPacket();
                        Database.EntityTable.SaveEntity(client);
                    }
                    break;

                case 27: // Small Boy (1003)
                    {
                        if (client.Entity.Body == 2001 || client.Entity.Body == 2002)
                        {
                            client.Equipment.Remove(9);
                            if (client.Equipment.Objects[9] != null)
                                client.Equipment.Objects[9] = null;
                        }
                        ClientEquip equips = new();
                        equips.DoEquips(client);
                        client.Send(equips);
                        client.NobilityInformation.Gender = 1;
                        client.Entity.Spouse = "None";
                        client.Entity.Body = 1003;
                        client.NobilityInformation.Mesh = client.Entity.Mesh;
                        client.Equipment.UpdateEntityPacket();
                        Database.EntityTable.SaveEntity(client);
                    }
                    break;
            }
        }
    }
}