namespace MTA.Database
{
    using MTA;
    using System;

    internal class rates
    {
        public static uint BotJail;
        public static uint CaptureFlag;
        public static uint ChangeName;
        public static uint classpk;
        public static string coder = "";
        public static string cpsmethod = "";
        public static uint CpsMethodNum;
        public static string cryptkey = "";
        public static uint DailyPk;
        public static uint DamageGarment;
        public static uint DamageTails;
        public static uint DemonCave;
        public static uint EliteGw;
        public static uint elitepk;
        public static uint Garment;
        public static uint GuildWar;
        public static uint housepromete;
        public static uint houseupgrade;
        public static uint itembox;
        public static uint king;
        public static uint LastMan;
        public static uint LevelUp;
        public static uint maxcps;
        public static uint minicps;
        public static uint MonthlyPk;
        public static uint Mount;
        public static uint Night;
        // public static string PopUpURL = "";
        public static uint prince;
        public static uint RemoveBound;
        public static uint Riencration;
        public static string servername = "";
        public static string serversite = "";
        public static uint SkillTeam;
        public static uint SnowBanshe;
        public static uint SoulP6;
        public static uint Steed;
        public static uint SteedRace;
        public static uint TeratoDragon;
        public static uint ThrillingSpook;
        public static uint TopSpouse;
        public static uint TreasureLow;
        public static uint TreasureMax;
        public static uint TreasureMin;
        public static uint VotePrize;
        public static string VoteUrl = "";
        public static uint Weather;
        public static uint weeklypk;
        public static uint KoCount,
          //  blackname,
         //   redname,
            lastitem,
            lastentity,
            PartyDrop,
            plus13,
            plus14,
            plus15;
        /// <summary>
        /// Rates 2
        /// </summary>
        public static string GMphone;
        public static string GM;
        public static string GMyahoo;
        public static uint GoldenOctopus;
        public static uint VipMonster;
        public static uint DragonSon1;
        public static uint DragonSon2;
        public static uint conquer;
        public static uint conquer7;
        public static uint dragonball7;
        public static uint changesex;

        public static void LoadRates()
        {

            MySqlReader reader = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT).Select("rates").Where("Coder", "abdoumatrix"));
            if (reader.Read())
            {
                lastentity = reader.ReadUInt32("LastEntity");
                //Program.EntityUID = new ServerBase.Counter(r.ReadUInt32("LastEntity"));
                lastitem = reader.ReadUInt32("LastItem");
                KoCount = reader.ReadUInt32("KoCount");
                plus13 = reader.ReadUInt32("Plus13");
                plus14 = reader.ReadUInt32("Plus14");
                plus15 = reader.ReadUInt32("Plus15");
                PartyDrop = reader.ReadUInt32("PartyDrop");
                CaptureFlag = reader.ReadUInt32("CaptureFlag");
                SkillTeam = reader.ReadUInt32("SkillTeam");
                DemonCave = reader.ReadUInt32("DemonCave");
                VoteUrl = reader.ReadString("VoteUrl");
                VotePrize = reader.ReadUInt32("VotePrize");
                Weather = reader.ReadUInt32("Weather");
                Night = reader.ReadUInt32("Night");
                SoulP6 = reader.ReadUInt32("SoulP6");
                Garment = reader.ReadUInt32("Garment");
                Steed = reader.ReadUInt32("Steed");
                Mount = reader.ReadUInt32("Mount");
                DamageTails = reader.ReadUInt32("DamageTails");
                DamageGarment = reader.ReadUInt32("DamageGarment");
                LevelUp = reader.ReadUInt32("LevelUp");
                TeratoDragon = reader.ReadUInt32("TeratoDragon");
                ThrillingSpook = reader.ReadUInt32("ThrillingSpook");
                SnowBanshe = reader.ReadUInt32("SnowBanshe");
                TreasureLow = reader.ReadUInt32("TreasureLow");
                TreasureMax = reader.ReadUInt32("TreasureMax");
                TreasureMin = reader.ReadUInt32("TreasureMin");
                GuildWar = reader.ReadUInt32("GuildWar");
                BotJail = reader.ReadUInt32("BotJail");
                RemoveBound = reader.ReadUInt32("RemoveBound");
                elitepk = reader.ReadUInt32("elitepk");
                SteedRace = reader.ReadUInt32("SteedRace");
                ChangeName = reader.ReadUInt32("ChangeName");
                MonthlyPk = reader.ReadUInt32("MonthlyPk");
                EliteGw = reader.ReadUInt32("EliteGw");
                TopSpouse = reader.ReadUInt32("TopSpouse");
                DailyPk = reader.ReadUInt32("DailyPk");
                LastMan = reader.ReadUInt32("LastMan");
                Riencration = reader.ReadUInt32("Riencration");
                king = reader.ReadUInt32("kings");
                prince = reader.ReadUInt32("prince");
                housepromete = reader.ReadUInt32("HousePromete");
                houseupgrade = reader.ReadUInt32("HouseUpgrade");
                itembox = reader.ReadUInt32("ItemBox");
                serversite = reader.ReadString("ServerWebsite");
                servername = reader.ReadString("ServerName");
                maxcps = reader.ReadUInt32("MaxCps");
                minicps = reader.ReadUInt32("MiniCps");
                cpsmethod = reader.ReadString("CpsMethod");
                CpsMethodNum = reader.ReadUInt32("CpsMethodNum");
                classpk = reader.ReadUInt32("ClassPk");
                weeklypk = reader.ReadUInt32("WeeklyPk");
                coder = reader.ReadString("Coder");                 
            }
            MTA.Console.WriteLine("Rates Loaded.");
            //  Reader.Close();
            ////  Reader.Dispose();
            LoadRates2();
        }
        public static void LoadRates2()
        {
            MySqlReader reader = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT).Select("rates2").Where("Coder", "abdoumatrix"));
            if (reader.Read())
            {

                coder = reader.ReadString("Coder");
                GM = reader.ReadString("GM");
                GMphone = reader.ReadString("GMphone");
                GMyahoo = reader.ReadString("GMyahoo");                
                GoldenOctopus = reader.ReadUInt32("GoldenOctopus");
                VipMonster = reader.ReadUInt32("VipMonster");
                DragonSon1 = reader.ReadUInt32("DragonSon1");
                DragonSon2 = reader.ReadUInt32("DragonSon2");
                conquer = reader.ReadUInt32("conquer");
                conquer7 = reader.ReadUInt32("conquer7");
                dragonball7 = reader.ReadUInt32("7dragonball");
                changesex = reader.ReadUInt32("changesex");
            }
            MTA.Console.WriteLine("Rates2 Loaded By abdoumatrix.");           
        }
    }    
}

