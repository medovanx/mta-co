using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Database;

namespace MTA.Game
{
    public class ClanWar
    {
        public static SobNpcSpawn Pole;
        public static SafeDictionary<uint, Clan> Scores = new SafeDictionary<uint, Clan>(100);
        public static bool IsWar = false, FirstRound = false;
        public static Time32 ScoreSendStamp, LastWin;
        public static Clan PoleKeeper, CurrentTopLeader;
        private static bool changed = false;
        private static string[] scoreMessages;
        public static DateTime StartTime;
        public static bool Claim
        {
            get { return Program.Vars["cwclaim"]; }
            set { Program.Vars["cwclaim"] = value; }
        }
        public static uint KeeperID
        {
            get { return Program.Vars["cwkeeperid"]; }
            set { Program.Vars["cwkeeperid"] = value; }
        }
        public static void Initiate()
        {
            var Map = Kernel.Maps[1509];
            Pole = (SobNpcSpawn)Map.Npcs[812];
        }
        public static void Start()
        {
            Scores = new SafeDictionary<uint, Clan>(100);
            StartTime = DateTime.Now;
            Kernel.SendWorldMessage(new Message("Clan war has began!", System.Drawing.Color.Red, Message.Center), Program.Values);
            FirstRound = true;
            foreach (Clan clan in Kernel.Clans.Values)
            {
                clan.WarScore = 0;
            }
            Claim = false;
            IsWar = true;
        }

        public static void Reset()
        {
            Scores = new SafeDictionary<uint, Clan>(100);
            Pole.Hitpoints = Pole.MaxHitpoints;

            foreach (Clan clan in Kernel.Clans.Values)
            {
                clan.WarScore = 0;
            }

            IsWar = true;
        }

        public static void FinishRound()
        {
            LastWin = Time32.Now;

            FirstRound = false;
            SortScores(out PoleKeeper);
            if (PoleKeeper != null)
            {
                KeeperID = PoleKeeper.ID;
                Kernel.SendWorldMessage(new Message("The Clan, " + PoleKeeper.Name + ", owned by " + PoleKeeper.LeaderName + " has won this Clan war round!", System.Drawing.Color.Red, Message.Center), Program.Values);

                Pole.Name = PoleKeeper.Name;
            }
            Pole.Hitpoints = Pole.MaxHitpoints;
            Kernel.SendWorldMessage(Pole, Program.Values, (ushort)1509);
            Reset();
        }

        public static void End()
        {
            if (PoleKeeper != null)
            {
                Kernel.SendWorldMessage(new Message("The clan, " + PoleKeeper.Name + ", owned by " + PoleKeeper.LeaderName + " has won " + rates.Clanwarday + " cps!---Clan war has ended!", System.Drawing.Color.White, Message.Center), Program.Values);

                PoleKeeper.PoleKeeper = true;
            }
            else
            {
                Kernel.SendWorldMessage(new Message("Clan war has ended and there was no winner!", System.Drawing.Color.Red, Message.Center), Program.Values);

            }
            Claim = true;
            IsWar = false;
            UpdatePole(Pole);
        }

        public static void AddScore(uint addScore, Clan clan)
        {
            if (clan != null)
            {
                clan.WarScore += addScore;
                changed = true;
                if (!Scores.ContainsKey(clan.ID))
                    Scores.Add(clan.ID, clan);
                if ((int)Pole.Hitpoints <= 0)
                {
                    FinishRound();

                    return;
                }
            }
        }

        public static void SendScores()
        {
            if (scoreMessages == null)
                scoreMessages = new string[0];
            if (Scores.Count == 0)
                return;
            if (changed)
                SortScores(out CurrentTopLeader);

            for (int c = 0; c < scoreMessages.Length; c++)
            {
                Message msg = new Message(scoreMessages[c], System.Drawing.Color.Red, c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)1509);
            }
        }

        private static void SortScores(out Clan winner)
        {
            winner = null;
            List<string> ret = new List<string>();
            int Place = 0;
            foreach (Clan clan in Scores.Values.OrderByDescending((p) => p.WarScore))
            {
                if (Place == 0)
                    winner = clan;
                string str = "No  " + (Place + 1).ToString() + ": " + clan.Name + "(" + clan.WarScore + ")";
                ret.Add(str);
                Place++;
                if (Place == 4)
                    break;
            }

            changed = false;
            scoreMessages = ret.ToArray();
        }

        private static void UpdatePole(SobNpcSpawn pole)
        {
            new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE)
            .Update("sobnpcs").Set("name", pole.Name).Set("life", Pole.Hitpoints).Where("id", pole.UID).Execute();
        }
    }
}
