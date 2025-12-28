using System;
using System.IO;
using MTA.Network.GamePackets;
using System.Collections.Generic;

namespace MTA.Game.Constants {
    public class GameConstants {
        /// <summary>
        /// Returns the full path to a file or folder in the Database folder. Throws an exception if the file/folder is not found.
        /// </summary>
        private static string Database(string filename) {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "Database")))
                directory = directory.Parent;

            if (directory == null)
                throw new DirectoryNotFoundException("Database directory not found from base directory upwards.");

            string dbPath = Path.Combine(directory.FullName, "Database");

            // If filename is empty, return the Database folder path
            if (string.IsNullOrEmpty(filename))
                return dbPath + Path.DirectorySeparatorChar;

            string fullPath = Path.Combine(dbPath, filename);

            // Check if it's a folder path (ends with separator) or a file path
            bool isFolder = filename.EndsWith(Path.DirectorySeparatorChar.ToString()) || filename.EndsWith("/") ||
                            filename.EndsWith("\\");

            if (isFolder) {
                // For folders, check if directory exists
                if (!Directory.Exists(fullPath))
                    throw new DirectoryNotFoundException(
                        $"Directory '{filename}' not found in Database folder at '{directory.FullName}'.");
            }
            else {
                // For files, check if file exists
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException(
                        $"File '{filename}' not found in Database folder at '{directory.FullName}'.");
            }

            return fullPath;
        }

        public static readonly List<ushort> RideForbiddenMaps = [
            1004,
            1511
        ];

        public static readonly Message FullInventory =
                new Message("There is not enough room in your inventory!", System.Drawing.Color.Red, Message.TopLeft),
            OneFlowerADay = new Message("You may only send 1 flower a day", System.Drawing.Color.Red, Message.TopLeft),
            TradeRequest = new Message("Trade request sent.", System.Drawing.Color.Red, Message.TopLeft),
            TradeInventoryFull = new Message("There is not enough room in your partner inventory.",
                System.Drawing.Color.Red, Message.TopLeft),
            TradeInProgress = new Message("An trade is already in progress. Try again later.", System.Drawing.Color.Red,
                Message.TopLeft),
            FloorItemNotAvailable = new Message("You need to wait until you will be able to pick this item up!",
                System.Drawing.Color.Red, Message.TopLeft),
            JailItemUnusable =
                new Message("You can't use this item in here!", System.Drawing.Color.Red, Message.TopLeft),
            PKForbidden = new Message("PK Forbidden in this map.", System.Drawing.Color.Red, Message.TopLeft),
            ExpBallsUsed = new Message("You can use only ten exp balls a day. Try tomorrow.", System.Drawing.Color.Red,
                Message.TopLeft),
            SpellLeveled = new Message("Congratulation, you have just leveled your spell.", System.Drawing.Color.Red,
                Message.TopLeft),
            OneKissADay = new Message("You may only send free Kiss a day", System.Drawing.Color.Red, Message.TopLeft),
            ProficiencyLeveled = new Message("Congratulation, you have just leveled your proficiency.",
                System.Drawing.Color.Red, Message.TopLeft),
            FrankosReloaded = new Message("Frankos Reloaded.", System.Drawing.Color.Red, Message.TopLeft),
            Warrent = new Message("The guards are looking for you!", System.Drawing.Color.Red, Message.TopLeft),
            VIPExpired =
                new Message("Your VIP has expired. Please reactivate your VIP if you wish to keep VIP services.",
                    System.Drawing.Color.Red, Message.World),
            VIPLifetime = new Message("Your VIP service is unlimited.", System.Drawing.Color.Red, Message.World),
            WrongAccessory = new Message("You cannot wear this accessory and this item at the same time.",
                System.Drawing.Color.Red, Message.World),
            NoAccessory = new Message("You cannot wear an accessory without a support item.", System.Drawing.Color.Red,
                Message.World),
            vipteleport = new Message("You can't teleport in this map.", System.Drawing.Color.Red, Message.World),
            Noteleport = new Message("You can't teleport to this map.", System.Drawing.Color.Red, Message.World);

        public static List<ushort> QuestsMaps = [6752];

        public static readonly List<ulong> ActiveNPC = [
            10081,
            10082,
            2031,
            140
        ];

        public static readonly List<ushort> NoRevHere = [
            1090, 1559, 1518, 4021, 12345, 12346, 1707, 14785, 3333, 3935,
            6412, 1844, 4025, 3071, 2527, 2522, 1655, 2090, 2091, 1002, 2014
        ];

        public static Message VIPRemaining(string days, string hours) {
            return new Message("You have " + days + " day(s) and " + hours + " hour(s) of VIP service remaining.",
                System.Drawing.Color.Red, Message.World);
        }

        public static Message NoFrankos(string name) {
            return new Message("Can't reload Frankos, you are out of " + name + "s!", System.Drawing.Color.Red,
                Message.TopLeft);
        }

        public static Message Stigma(float percent, int time) {
            return new Message("Stigma activated. Your attack will be increased with " + percent + " for " + time + ".",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message Accuracy(int time) {
            return new Message("Accuracy activated. Your agility will be increased a bit for " + time + ".",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message Invisibility(int time) {
            return new Message(
                "Invisibility activated. You will be invisible for monsters as long as you don't attack for " + time +
                ".", System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message Shield(float percent, int time) {
            return new Message(
                "Shield activated. Your defence will be increased with " + percent + " for " + time + ".",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message Shackled(int time) {
            return new Message("You have been shackled and can not move for " + time + " Seconds.",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message Dodge(float percent, int time) {
            return new Message("Dodge activated. Your dodge will be increased with " + percent + " for " + time + ".",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message NoDrugs(int time) {
            return new Message("Poison star activated. You will not be able to use drugs for " + time + " seconds.",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message ExtraExperience(uint experience) {
            return new Message("You have gained extra " + experience + " experience for killing the monster.",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message TeamExperience(uint experience) {
            return new Message("One of your teammates killed a monster so you gained " + experience + " experience.",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message NoobTeamExperience(uint experience) {
            return new Message(
                "One of your teammates killed a monster and because you have a noob inside your team, you gained " +
                experience + " experience.", System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message PickupGold(uint amount) {
            return new Message("You have picked up " + amount + " gold.", System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message PickupConquerPoints(uint amount) {
            return new Message("You have picked up " + amount + " Conquer Points.", System.Drawing.Color.Red,
                Message.TopLeft);
        }

        public static Message PickupItem(string name) {
            return new Message("You have picked up a/an " + name + " item.", System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message DummyLevelTooHigh() {
            return new Message("You can't attack this dummy because your level is not high enough.",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message BoothItemSell(string buyername, string itemname, bool conquerpoints, uint cost) {
            return new Message(
                "Congratulations. You just have just sold " + itemname + " to " + buyername + " for " + cost +
                (conquerpoints ? " ConquerPoints." : " Gold."), System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message Enchant(int origEnch, int newEnch) {
            if (newEnch <= origEnch)
                return new Message(
                    "You were unlucky. You didn't gain any more enchantment in your item. Your generated enchant is " +
                    newEnch + ".", System.Drawing.Color.Red, Message.TopLeft);
            else
                return new Message(
                    "You were lucky. You gained more enchantment in your item. Your generated enchant is " + newEnch +
                    ".", System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message VoteSpan(Client.GameState client) {
            if (DateTime.Now <= client.LastVote.AddHours(12)) {
                TimeSpan agospan = client.LastVote.Subtract(DateTime.Now);
                TimeSpan tillspan = DateTime.Now.Subtract(client.LastVote);
                string message = "You last voted ";
                if (agospan.Hours >= 0)
                    message += agospan.Hours.ToString() + " hours, ";
                if (agospan.Minutes >= 0)
                    message += agospan.Minutes.ToString() + " minutes, and ";
                message += agospan.Seconds.ToString() + " ago. Please wait ";
                if (tillspan.Hours >= 0)
                    message += tillspan.Hours.ToString() + " hours, ";
                if (tillspan.Minutes >= 0)
                    message += agospan.Minutes.ToString() + " minutes, and ";
                message += tillspan.Seconds.ToString() + " ago. To vote again!";
                return new Message(message, System.Drawing.Color.Red, Message.TopLeft);
            }

            return new Message("You haven't voted in the past 12 hours. Vote now to gain an extra point!",
                System.Drawing.Color.Red, Message.TopLeft);
        }

        public const string ScriptsBasePath = "scripts\\";
        public static readonly string DataHolderPath = Database("");
        public static readonly string DMapsPath = Database("");
        public static readonly string BroadcastsPath = Database("broadcasts.txt");
        public static readonly string NpcPath = Database("npc.ini");
        public static readonly string RaceRecordsPath = Database("racerecords.ini");
        public static readonly string ItemRefineCostPath = Database("item_refine_cost.txt");
        public static readonly string ItemRefineUpgradePath = Database("item_refine_upgrade.txt");
        public static readonly string AbilityScorePath = Database("ability_score.txt");
        public static readonly string CoatStorageTypePath = Database("coat_storage_type.txt");
        public static readonly string SoulProtectionPath = Database("souls_protection.txt");
        public static readonly string RoulettesPath = Database("Roulettes.txt");

        public static readonly string FurniturePath = Database("Furniture.txt");

        // public static readonly string DMapOwnerPath = Database("DMapOwner.dat");
        public static readonly string StatsPath = Database("Stats.ini");
        public static readonly string GameMapPath = Database("GameMap.dat");
        public static readonly string ShopsPath = Database("shops\\Shop.dat");
        public static readonly string EShopsPath = Database("shops\\emoneyshop.ini");
        public static readonly string EShopsV2Path = Database("shops\\emoneyshopV2.ini");
        public static readonly string HonorShopPath = Database("shops\\HonorShop.ini");
        public static readonly string RaceShopPath = Database("shops\\RacePointShop.ini");
        public static readonly string ChampionShopPath = Database("shops\\GoldenLeagueShop.ini");
        public static readonly string PortalsPath = Database("Portals.ini");
        public static readonly string RevivePoints = Database("RevivePoints.ini");
        public static readonly string ItemPlusInfosPath = Database("ItemAdd.ini");
        public static readonly string SoulGearInformation = Database("soulgear.txt");
        public static readonly string UnhandledExceptionsPath = Database("exceptions\\");
        public static readonly string QuizShow = Database("QuizShow.txt");
        public static readonly string ItemBaseInfosPath = Database("items.txt");
        public static readonly string QuestInfoPath = Database("Questinfo.ini");
        public static readonly string StoragePath = Database("Storage.ini");
        public static readonly string PokerTablesPath = Database("PokerTables.txt");
        public static readonly string PoleDominationPath = Database("poledomination.txt");
        public static readonly string ClanWarPath = Database("ClanWar.txt");
        public static readonly string BoothsPath = Database("Booths.txt");
        public static readonly string FlowersPath = Database("flowers.txt");
        public static readonly string BoyFlowersPath = Database("boyflowers.txt");

        public const string ServerKey = "TQServer";
        public const string GameCryptographyKey = "C238xs65pjy7HU9Q";
        public static string? ServerName;
        public const int MaxBroadcasts = 50;

        public static uint ExtraExperienceRate,
            ExtraSpellRate,
            ExtraProficiencyRate,
            ConquerPointsDropRate,
            ConquerPointsDropMultiple,
            ItemDropRate;

        public static ulong MoneyDropRate, MoneyDropMultiple;
        public static string[]? ItemDropQualityRates;
        public static string? WebAccExt, ServerWebsite, WebVoteExt, WebDonateExt, ServerGMPass;
        public const sbyte pScreenDistance = 19;
        public const sbyte nScreenDistance = 19;
        public const sbyte remScreenDistance = 19;

        public const ushort DisconnectTimeOutSeconds = 10,
            FloorItemSeconds = 20,
            FloorItemAvailableAfter = 15;

        public const ushort SocketOneProgress = 100,
            SocketTwoProgress = 300;

        public static readonly List<ushort> revnomap = [
            1, 2, 3, 0x80c, 0x1b61, 0x80c, 0x79e, 0x3ed, 0x1b5d, 0x1b5e, 0x1b60, 0x1770, 0x1774, 0x1771, 0x1772, 0x1773,
            0x734, 0x1b59, 0x709, 0x5e4, 0x5ee, 0x1e61, 0x22ad, 0xd05, 0x442, 0x4c9, 1860, 700, 3073
        ];

        public static readonly List<ushort> MemoryAgateNotAllowedMap = [];

        public static readonly List<ulong> NoVipTele = [
            1645, 1, 2, 3, 0x80c, 0x1b61, 0x40e, 0x3ed, 0x80c, 0x1770, 0x1774, 0x1771, 0x1772, 0x1773, 0x1b5d, 0x1b5e,
            0x1b60,
            0x734, 8892, 1645, 0x1b59, 0x817, 0x709, 0x5e4, 0x5ee, 0x1e61, 0x22ad, 0xd05, 0x442, 0x4c9, 0x5e5, 0x79e,
            1860, 700, 3070, 3071, 3691, 3692, 3693, 3694, 1730, 1731, 1732, 1733, 1734, 1735, 3073, 3072
        ];

        public static readonly List<ulong> novip = [
            3090,
            8892,
            1645
        ];

        public static readonly List<ulong> fbss = [
            1707,
            1238
        ];

        public static readonly List<ulong> horsepk = [3707];

        public static readonly List<string> NoFog = [
            "Clannad",
            "Btooom",
            "Cyclops",
            "Hades",
            "Centar"
        ];

        public static readonly List<ushort> PKForbiddenMaps = [
            1036,
            1002,
            700,
            3090,
            7010,
            1039,
            1004,
            1006,
            11030,
            11034,
            8880,
            Maps.POKER_GOLD,
            Maps.POKER_CPs,
            1950,
            8800,
            8801,
            8802,
            8803,
            1632,
            1633,
            1024,
            2351,
            601,
            Maps.CAPTAIN_CASTLE_BEGINNER,
            Maps.HOUSE_LV1,
            Maps.HOUSE_LV2,
            Maps.HOUSE_LV3,
            Maps.HOUSE_LV4,
            Maps.HOUSE_LV5
        ];

        /// <summary>
        /// Checks if a map is PK forbidden. For dynamic maps (like houses), checks both MapID and BaseID.
        /// </summary>
        /// <param name="mapID">The MapID to check</param>
        /// <param name="map">Optional Map object to check BaseID if MapID doesn't match</param>
        /// <returns>True if PK is forbidden on this map</returns>
        public static bool IsPKForbidden(ushort mapID, Game.Map map = null) {
            // First check if the MapID itself is in the forbidden list
            if (PKForbiddenMaps.Contains(mapID))
                return true;

            // For dynamic maps (houses), check BaseID if map is provided
            if (map != null && map.BaseID != map.ID) {
                if (PKForbiddenMaps.Contains(map.BaseID)) {
                    return true;
                }
            }

            return false;
        }

        public static readonly List<ushort> NoHp = [
            1707,
            3070,
            1238,
            3071,
            1543,
            1544,
            1545,
            1546,
            1547,
            1548
        ];

        public static readonly List<ulong> blackname = [3071];

        public static readonly List<ulong> FBandSSEvent = [
            1543,
            1544,
            1545,
            1546,
            1547,
            1548
        ];

        public static readonly List<ulong> EtaleMaps = [
            1543,
            1544,
            1545,
            1546,
            1547,
            1548
        ];

        public static readonly List<ulong> SSFB = [
            1543,
            1544,
            1545,
            1546,
            1547,
            1548
        ];

        // <summary>
        // Maps that are free from PK, killing players here will not give you PK points.
        // </summary>
        public static readonly List<ushort> PKFreeMaps = [
            3073,
            3691,
            3692,
            2078,
            2057,
            2072,
            2076,
            11225,
            11224,
            2073,
            2075,
            3694,
            1702,
            8892,
            16414,
            7015,
            1458,
            1459,
            1460,
            3693,
            Maps.ProudSea,
            3070,
            1707,
            2065,
            1038,
            10380,
            1005,
            6000,
            6004,
            6001,
            6002,
            6003,
            1844,
            7001,
            2071,
            1801,
            1508,
            1518,
            7777,
            8877,
            2014,
            3333,
            1090,
            700,
            3072,
            8510,
            8511,
            8512,
            8513,
            8514,
            8515,
            8516,
            8517,
            8518,
            8519,
            8520,
            8521,
            8522,
            8523,
            8524,
            8525,
            8526,
            3990,
            3995,
            1509
        ];

        public static readonly List<int> SoulList = [80032000];

        public static readonly List<int> MaxItems = [
            410439,
            420439,
            480439,
            610439,
            601439,
            421439,
            823052,
            824001,
            823043,
            822052,
            800014,
            800017,
            800513,
            822053,
            820056,
            800110,
            800320
        ];

        public static readonly List<string> monsters = [
            "CaptainCook",
            "PirateTurner",
            "DukeArena",
            "EarlArena",
            "BaronArena",
            "KnightArena",
            "KingArena",
            "EidArenaking",
            "EidArena"
        ];

        public static readonly List<ushort> twinskill = [
            8001,
            1165,
            7011,
            7012,
            7014,
            7015,
            7017,
            10309,
            11660,
            11610,
            11590,
            11600,
            8030,
            1120,
            1000,
            1001,
            1002,
            11060,
            11050,
            11040,
            11070,
            11650
        ];

        public static readonly List<int> AvaibleSpells = [
            1045,
            12020,
            12030,
            12040,
            12050,
            8001,
            1046
        ];

        public static readonly List<ushort> Damage1Map = [
            12470,
            1844,
            1801,
            7001,
            4000,
            4010,
            4020,
            4050,
            4060,
            4070,
            12020,
            12030,
            12040,
            12050,
            9876
        ];
    }
}