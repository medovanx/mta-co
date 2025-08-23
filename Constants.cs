using System;
using MTA.Network.GamePackets;
using System.Collections.Generic;

namespace MTA
{
    public class Constants
    {
        public static readonly System.Collections.Generic.List<ushort> RideForbiddenMaps = new System.Collections.Generic.List<ushort>()
        {
            1004,
            1511
        };
        public static readonly Message FullInventory = new Message("There is not enough room in your inventory!", System.Drawing.Color.Red, Message.TopLeft),
            OneFlowerADay = new Message("You may only send 1 flower a day", System.Drawing.Color.Red, Message.TopLeft),
            TradeRequest = new Message("Trade request sent.", System.Drawing.Color.Red, Message.TopLeft),
            TradeInventoryFull = new Message("There is not enough room in your partner inventory.", System.Drawing.Color.Red, Message.TopLeft),
            TradeInProgress = new Message("An trade is already in progress. Try again later.", System.Drawing.Color.Red, Message.TopLeft),
            FloorItemNotAvailable = new Message("You need to wait until you will be able to pick this item up!", System.Drawing.Color.Red, Message.TopLeft),
            JailItemUnusable = new Message("You can't use this item in here!", System.Drawing.Color.Red, Message.TopLeft),
            PKForbidden = new Message("PK Forbidden in this map.", System.Drawing.Color.Red, Message.TopLeft),
            ExpBallsUsed = new Message("You can use only ten exp balls a day. Try tomorrow.", System.Drawing.Color.Red, Message.TopLeft),
            SpellLeveled = new Message("Congratulation, you have just leveled your spell.", System.Drawing.Color.Red, Message.TopLeft),
            OneKissADay = new Message("You may only send free Kiss a day", System.Drawing.Color.Red, Message.TopLeft),
            ProficiencyLeveled = new Message("Congratulation, you have just leveled your proficiency.", System.Drawing.Color.Red, Message.TopLeft),
            FrankosReloaded = new Message("Frankos Reloaded.", System.Drawing.Color.Red, Message.TopLeft),
            Warrent = new Message("The guards are looking for you!", System.Drawing.Color.Red, Message.TopLeft),
            VIPExpired = new Message("Your VIP has expired. Please reactivate your VIP if you wish to keep VIP services.", System.Drawing.Color.Red, Message.World),
            VIPLifetime = new Message("Your VIP service is unlimited.", System.Drawing.Color.Red, Message.World),
            WrongAccessory = new Message("You cannot wear this accessory and this item at the same time.", System.Drawing.Color.Red, Message.World),
            NoAccessory = new Message("You cannot wear an accessory without a support item.", System.Drawing.Color.Red, Message.World),
            vipteleport = new Message("You can't teleport in this map.", System.Drawing.Color.Red, Message.World),
        Noteleport = new Message("You can't teleport to this map.", System.Drawing.Color.Red, Message.World);

        public static List<ushort> QuestsMaps = new List<ushort> {
           6752
        };
        public static readonly System.Collections.Generic.List<ulong> ActiveNPC = new System.Collections.Generic.List<ulong>()
        {
            10081,
            10082,
            2031,
            140
        };
        public static readonly List<ushort> NoRevHere = new List<ushort>()
        {
            1090,1559,1518,4021,12345,12346,1707,14785,3333,3935,
            6412,1844,4025,3071,2527,2522,1655,2090,2091,1002,2014
        };
        public static Message VIPRemaining(string days, string hours)
        {
            return new Message("You have " + days + " day(s) and " + hours + " hour(s) of VIP service remaining.", System.Drawing.Color.Red, Message.World);
        }
        public static Message NoFrankos(string name)
        {
            return new Message("Can't reload Frankos, you are out of " + name + "s!", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message Stigma(float percent, int time)
        {
            return new Message("Stigma activated. Your attack will be increased with " + percent + " for " + time + ".", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message Accuracy(int time)
        {
            return new Message("Accuracy activated. Your agility will be increased a bit for " + time + ".", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message Invisibility(int time)
        {
            return new Message("Invisibility activated. You will be invisible for monsters as long as you don't attack for " + time + ".", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message Shield(float percent, int time)
        {
            return new Message("Shield activated. Your defence will be increased with " + percent + " for " + time + ".", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message Shackled(int time)
        {
            return new Message("You have been shackled and can not move for " + time + " Seconds.", System.Drawing.Color.Red, Message.TopLeft);
        }

        public static Message Dodge(float percent, int time)
        {
            return new Message("Dodge activated. Your dodge will be increased with " + percent + " for " + time + ".", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message NoDrugs(int time)
        {
            return new Message("Poison star activated. You will not be able to use drugs for " + time + " seconds.", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message ExtraExperience(uint experience)
        {
            return new Message("You have gained extra " + experience + " experience for killing the monster.", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message TeamExperience(uint experience)
        {
            return new Message("One of your teammates killed a monster so you gained " + experience + " experience.", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message NoobTeamExperience(uint experience)
        {
            return new Message("One of your teammates killed a monster and because you have a noob inside your team, you gained " + experience + " experience.", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message PickupGold(uint amount)
        {
            return new Message("You have picked up " + amount + " gold.", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message PickupConquerPoints(uint amount)
        {
            return new Message("You have picked up " + amount + " Conquer Points.", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message PickupItem(string name)
        {
            return new Message("You have picked up a/an " + name + " item.", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message DummyLevelTooHigh()
        {
            return new Message("You can't attack this dummy because your level is not high enough.", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message BoothItemSell(string buyername, string itemname, bool conquerpoints, uint cost)
        {
            return new Message("Congratulations. You just have just sold " + itemname + " to " + buyername + " for " + cost + (conquerpoints ? " ConquerPoints." : " Gold."), System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message Enchant(int origEnch, int newEnch)
        {
            if (newEnch <= origEnch)
                return new Message("You were unlucky. You didn't gain any more enchantment in your item. Your generated enchant is " + newEnch + ".", System.Drawing.Color.Red, Message.TopLeft);
            else
                return new Message("You were lucky. You gained more enchantment in your item. Your generated enchant is " + newEnch + ".", System.Drawing.Color.Red, Message.TopLeft);
        }
        public static Message VoteSpan(Client.GameState client)
        {
            if (DateTime.Now <= client.LastVote.AddHours(12))
            {
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
            return new Message("You haven't voted in the past 12 hours. Vote now to gain an extra point!", System.Drawing.Color.Red, Message.TopLeft);
        }

        // Base database path - change this to relocate all database files
        public const string DatabaseBasePath = "..\\..\\Database\\";

        // Base scripts path - change this to relocate all script files
        public const string ScriptsBasePath = DatabaseBasePath + "scripts\\";

        public const string DataHolderPath = DatabaseBasePath,
        NpcFilePath = DatabaseBasePath + "Npcs.txt",
        DMapsPath = DatabaseBasePath,
        ShopsPath = DatabaseBasePath + "shops\\Shop.dat",
        EShopsPath = DatabaseBasePath + "shops\\emoneyshop.ini",
        EShopsV2Path = DatabaseBasePath + "shops\\emoneyshopV2.ini",
        HonorShopPath = DatabaseBasePath + "shops\\HonorShop.ini",
        RaceShopPath = DatabaseBasePath + "shops\\RacePointShop.ini",
        ChampionShopPath = DatabaseBasePath + "shops\\GoldenLeagueShop.ini",
        PortalsPath = DatabaseBasePath + "Portals.ini",
        RevivePoints = DatabaseBasePath + "RevivePoints.ini",
        MonstersPath = DatabaseBasePath + "Monsters.txt",
        ItemBaseInfosPath = DatabaseBasePath + "Items.txt",
        ItemPlusInfosPath = DatabaseBasePath + "ItemAdd.ini",
        SoulGearInformation = DatabaseBasePath + "soulgear.txt",
        UnhandledExceptionsPath = DatabaseBasePath + "exceptions\\",
        ServerKey = "TQServer",
        WelcomeMessages = DatabaseBasePath + "WelcomeMessages.txt",
        QuizShow = DatabaseBasePath + "QuizShow.txt",
        GameCryptographyKey = "qL0UVCXB6BY9txb2";
        public static string ServerName;
        public const int MaxBroadcasts = 50;
        public static uint ExtraExperienceRate, ExtraSpellRate, ExtraProficiencyRate, ConquerPointsDropRate, ConquerPointsDropMultiple, ItemDropRate;
        public static ulong MoneyDropRate, MoneyDropMultiple;
        public static string[] ItemDropQualityRates;
        public static string WebAccExt, ServerWebsite, WebVoteExt, WebDonateExt, ServerGMPass;
        public const sbyte pScreenDistance = 19;
        public const sbyte nScreenDistance = 19;
        public const sbyte remScreenDistance = 19;
        public const ushort DisconnectTimeOutSeconds = 10,
            FloorItemSeconds = 20,
            FloorItemAvailableAfter = 15;

        public const ushort SocketOneProgress = 100,
            SocketTwoProgress = 300;
        public static readonly System.Collections.Generic.List<ushort> revnomap = new System.Collections.Generic.List<ushort> {
            1, 2, 3, 0x80c, 0x1b61, 0x80c, 0x79e, 0x3ed, 0x1b5d, 0x1b5e, 0x1b60, 0x1770, 0x1774, 0x1771, 0x1772, 0x1773,
            0x734, 0x1b59, 0x709, 0x5e4, 0x5ee, 0x1e61, 0x22ad, 0xd05, 0x442, 0x4c9, 1860, 700, 3073
         };
        public static readonly System.Collections.Generic.List<ushort> MemoryAgateNotAllowedMap = new System.Collections.Generic.List<ushort>
        {

        };

        public static readonly System.Collections.Generic.List<ulong> NoVipTele = new System.Collections.Generic.List<ulong> {
            1645, 1, 2, 3, 0x80c, 0x1b61, 0x40e, 0x3ed, 0x80c, 0x1770, 0x1774, 0x1771, 0x1772, 0x1773, 0x1b5d, 0x1b5e, 0x1b60,
            0x734, 8892, 1645, 0x1b59, 0x817, 0x709, 0x5e4, 0x5ee, 0x1e61, 0x22ad, 0xd05, 0x442, 0x4c9, 0x5e5, 0x79e, 1860, 700, 3070, 3071, 3691, 3692, 3693, 3694, 1730, 1731, 1732, 1733, 1734, 1735, 3073, 3072
         };

        public static readonly System.Collections.Generic.List<ulong> novip = new System.Collections.Generic.List<ulong>()
        {
            3090,
            8892,
            1645
        };
        public static readonly System.Collections.Generic.List<ulong> fbss = new System.Collections.Generic.List<ulong>()
        {
            1707,
            1238
        };
        public static readonly System.Collections.Generic.List<ulong> horsepk = new System.Collections.Generic.List<ulong>()
        {
            3707
        };
        public static readonly System.Collections.Generic.List<string> NoFog = new System.Collections.Generic.List<string>()
        {
            "Clannad",
            "Btooom",
            "Cyclops",
            "Hades",
            "Centar",
        };
        public static readonly System.Collections.Generic.List<ushort> JiangPKMaps = new System.Collections.Generic.List<ushort>()
        {
            1002,
            1000,
            1015,
            1020,
            1011
        };
        public static readonly System.Collections.Generic.List<ushort> JiangForbiddenMaps = new System.Collections.Generic.List<ushort>()
        {
            1036,
            700,
            3090,
            7010,
            11030,
            11034,
            11042,
            1039,
            8880,
            8881,
            8892,
            1950,
            8800,
            8801,
            8802,
            8803,
            1511,
            1004,
            1006,
            1008,
            1632,
            1633,
            1024,
            2351,
            3033,
            601
        };
        public static readonly System.Collections.Generic.List<ushort> PKForbiddenMaps = new System.Collections.Generic.List<ushort>()
        {
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
            11042,
            8880,
            8881,
            1950,
            8800,
            8801,
            8802,
            8803,
            1632,
            1633,
            1024,
            2351,
            3033,
            601
        };
        public static readonly System.Collections.Generic.List<ushort> NoHp = new System.Collections.Generic.List<ushort> {
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
         };
        public static readonly System.Collections.Generic.List<ulong> blackname = new System.Collections.Generic.List<ulong>()
        {
            3071
        };
        public static readonly System.Collections.Generic.List<ulong> FBandSSEvent = new System.Collections.Generic.List<ulong>()
        {
            1543,
            1544,
            1545,
            1546,
            1547,
            1548
        };
        public static readonly System.Collections.Generic.List<ulong> EtaleMaps = new System.Collections.Generic.List<ulong>()
        {
        1543,
        1544,
        1545,
        1546,
        1547,
        1548,
        };
        public static readonly System.Collections.Generic.List<ulong> SSFB = new System.Collections.Generic.List<ulong>()
        {

        1543,
        1544,
        1545,
        1546,
        1547,
        1548
        };
        public static readonly System.Collections.Generic.List<ushort> PKFreeMaps = new System.Collections.Generic.List<ushort>()
        {
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
            //3090,
            16414,
            7015,
            1458,
            1459,
            1460,
            3693,
            3071,
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
        };

        public static readonly System.Collections.Generic.List<int> SoulList = new System.Collections.Generic.List<int>()
        {
            80032000
        };
        public static readonly System.Collections.Generic.List<int> MaxItems = new System.Collections.Generic.List<int>()
        {
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
        };
        public static readonly System.Collections.Generic.List<string> monsters = new System.Collections.Generic.List<string>()
        {
             "CaptainCook",
             "PirateTurner",
             "DukeArena",
             "EarlArena",
             "BaronArena",
             "KnightArena",
             "KingArena",
             "EidArenaking",
             "EidArena"
        };
        public static readonly System.Collections.Generic.List<ushort> twinskill = new System.Collections.Generic.List<ushort>()
        {
                       8001 ,
           1165 ,
           7011 ,
           7012 ,
            7014 ,
            7015 ,
            7017 ,
            10309 ,
            11660 ,
            11610 ,
            11590 ,
            11600 ,
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
        };
        public static readonly System.Collections.Generic.List<int> AvaibleSpells = new System.Collections.Generic.List<int>()
        {
           1045,
           12020,
           12030,
           12040,
           12050,
           8001,
           1046

        };
        public static readonly System.Collections.Generic.List<ushort> Damage1Map = new System.Collections.Generic.List<ushort>()
        {
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
        };
    }
}
