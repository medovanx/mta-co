using MTA.Network;

namespace MTA.Client.Commands
{
    public static class StuffCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            return (global::System.String)data[0] switch
            {
                "stuff" => HandleStuffCommand(client, data, mess),
                _ => false,
            };
        }

        private static bool HandleStuffCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                return false;
            }

            switch (data[1])
            {
                case "ninja":
                    HandleNinjaStuff(client);
                    break;

                case "monk":
                    HandleMonkStuff(client);
                    break;

                case "fire":
                    HandleFireStuff(client);
                    break;

                case "water":
                    HandleWaterStuff(client);
                    break;

                case "warrior":
                    HandleWarriorStuff(client);
                    break;

                case "trojan":
                    HandleTrojanStuff(client);
                    break;

                case "archer":
                    HandleArcherStuff(client);
                    break;

                case "pirate":
                    HandlePirateStuff(client);
                    break;

                case "leelong":
                    HandleLeelongStuff(client);
                    break;

                case "windwalker":
                    HandleWindwalkerStuff(client);
                    break;
            }

            return true;
        }

        private static void HandleNinjaStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item HanzoKatana Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item HanzoKatana Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item NightmareVest Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item NightmareHood Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item RambleVeil Super 12 7 250 13 13", client);
        }

        private static void HandleMonkStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item LazuritePrayerBeads Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item LazuritePrayerBeads Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item WhiteLotusFrock Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item XumiCap Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Volcano Super 12 7 250 13 13", client);
        }

        private static void HandleFireStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item SupremeSword Super 12 7 250 3 3", client);
            PacketHandler.CheckCommand("@item EternalRobe Super 12 7 250 3 3", client);
            PacketHandler.CheckCommand("@item DistinctCap Super 12 7 250 3 3", client);
            PacketHandler.CheckCommand("@item WyvernBracelet Super 12 7 250 3 3", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 250 3 3", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 250 3 3", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 250 3 3", client);
            PacketHandler.CheckCommand("@item SpearOfWrath Super 12 7 250 3 3", client);
            PacketHandler.CheckCommand("@item NiftyBag Super 12 7 250 3 3", client);
        }

        private static void HandleWaterStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item SupremeSword Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item EternalRobe Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item DistinctCap Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item WyvernBracelet Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item SpearOfWrath Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item NiftyBag Super 12 7 250 13 13", client);
        }

        private static void HandleWarriorStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item SpearOfWrath Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item SkyBlade Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item ImperiousArmor Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item SteelHelmet Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item CelestialShield Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item OccultWand Super 12 7 250 13 13", client);
        }

        private static void HandleTrojanStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item SkyBlade Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FangCrossSaber Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FangCrossSaber Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item ObsidianArmor Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item SquallSword Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item NirvanaClub Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item SkyBlade Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item SquallSword Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item NirvanaClub Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item PeerlessCoronet Super 12 7 250 13 13", client);
        }

        private static void HandleArcherStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item HeavenlyBow Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item WelkinCoat Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item WhiteTigerHat Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Volcano Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item HeavenPlume Super 12 7 250 13 13", client);
        }

        private static void HandlePirateStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item CaptainRapier Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item LordPistol Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item DarkDragonCoat Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item DominatorHat Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 250 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 250 13 13", client);
        }

        private static void HandleLeelongStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item HeavenFan Super 12 1 000 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 1 000 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 000 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 1 000 00 00", client);
            PacketHandler.CheckCommand("@item SkyNunchaku Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item SkyNunchaku Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item CombatSuit(Lv.140) Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item LegendHood Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 255 13 13", client);
        }

        private static void HandleWindwalkerStuff(GameState client)
        {
            PacketHandler.CheckCommand("@item TempestWing Super 12 7 255 103 123", client);
            PacketHandler.CheckCommand("@item JusticeFan Super 12 7 255 103 103", client);
            PacketHandler.CheckCommand("@item StarTower Super 12 7 255 123 123", client);
            PacketHandler.CheckCommand("@item Steed Fixed 12 00 000 00 00", client);
            PacketHandler.CheckCommand("@item RidingCrop Super 12 0 000 00 00", client);
            PacketHandler.CheckCommand("@item MysticWindrobe Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item WindHood Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item CrimsonRing Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item Blizzard Super 12 7 255 13 13", client);
            PacketHandler.CheckCommand("@item FloridNecklace Super 12 7 255 13 13", client);
        }
    }
}