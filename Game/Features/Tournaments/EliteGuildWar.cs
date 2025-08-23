using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Game
{
    public class EliteGuildWar
    {
        public static SobNpcSpawn Poles;
        public static SafeDictionary<uint, Guild> Scores = new SafeDictionary<uint, Guild>(100);
        public static bool IsWar = false, Flame10th = false, FirstRound = false;
        public static Time32 ScoreSendStamp, LastWin;
        public static Guild PoleKeeper, CurrentTopLeader;
        private static bool changed = false;
        private static string[] scoreMessages;
        public static DateTime StartTime;
        public static bool Claim
        {
            get { return Program.Vars["egwclaim"]; }
            set { Program.Vars["egwclaim"] = value; }
        }
        public static uint KeeperID
        {
            get { return Program.Vars["egwkeeperid"]; }
            set { Program.Vars["egwkeeperid"] = value; }
        }
        public static void EliteGwint()
        {
            var Map = Kernel.Maps[2071];
            Poles = (SobNpcSpawn)Map.Npcs[811];
        }
        public static void Start()
        {
            if (Poles == null) return;
            Claim = false;
            Scores = new SafeDictionary<uint, Guild>(100);
            StartTime = DateTime.Now;
            Kernel.SendWorldMessage(new Message("Elite Guild war has began!", System.Drawing.Color.Red, Message.Center), Program.Values);
            FirstRound = true;
            foreach (Guild guild in Kernel.Guilds.Values)
            {
                guild.EWarScore = 0;
            }
            IsWar = true;
        }
        public static void Reset()
        {
            Scores = new SafeDictionary<uint, Guild>(100);
            Poles.Hitpoints = Poles.MaxHitpoints;
            foreach (Guild guild in Kernel.Guilds.Values)
            {
                guild.EWarScore = 0;
            }
            IsWar = true;
        }
        public static void FinishRound()
        {
            if (PoleKeeper != null && !FirstRound)
            {
                if (PoleKeeper.Wins == 0)
                    PoleKeeper.Losts++;
                else
                    PoleKeeper.Wins--;
                Database.GuildTable.UpdateGuildWarStats(PoleKeeper);
            }
            LastWin = Time32.Now;
            FirstRound = false;
            SortScores(out PoleKeeper);
            if (PoleKeeper != null)
            {
                KeeperID = PoleKeeper.ID;
                Kernel.SendWorldMessage(new Message("The guild, " + PoleKeeper.Name + ", owned by " + PoleKeeper.LeaderName + " has won this guild war round!", System.Drawing.Color.Red, Message.Center), Program.Values);
                if (PoleKeeper.Losts == 0)
                    PoleKeeper.Wins++;
                else
                    PoleKeeper.Losts--;
                Database.GuildTable.UpdateGuildWarStats(PoleKeeper);
                Poles.Name = PoleKeeper.Name;
            }
            Poles.Hitpoints = Poles.MaxHitpoints;
            Kernel.SendWorldMessage(Poles, Program.Values, (ushort)2071);
            Reset();
        }
        public static void End()
        {
            if (PoleKeeper != null)
            {
                Kernel.SendWorldMessage(new Message("The Elite guild, " + PoleKeeper.Name + ", owned by " + PoleKeeper.LeaderName + " has won this guild war!---Guild war has ended!", System.Drawing.Color.White, Message.Center), Program.Values);
            }
            else
            {
                Kernel.SendWorldMessage(new Message("Elite Guild war has ended and there was no winner!", System.Drawing.Color.Red, Message.Center), Program.Values);
            }
            IsWar = false;
            Claim = true;
            UpdatePole(Poles);
        }
        public static void AddScore(uint addScore, Guild guild)
        {
            if (guild != null)
            {
                guild.EWarScore += addScore;
                changed = true;
                if (!Scores.ContainsKey(guild.ID))
                    Scores.Add(guild.ID, guild);
                if ((int)Poles.Hitpoints <= 0)
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
                Message msg = new Message(scoreMessages[c], System.Drawing.Color.Red,
                    c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)2071);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)6001);
            }
        }
        private static void SortScores(out Guild winner)
        {
            winner = null;
            List<string> ret = new List<string>();
            int Place = 0;
            foreach (Guild guild in Scores.Values.OrderByDescending((p) => p.EWarScore))
            {
                if (Place == 0)
                    winner = guild;
                string str = "No  " + (Place + 1).ToString() + ": " + guild.Name + "(" + guild.EWarScore + ")";
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
                .Update("sobnpcs").Set("name", pole.Name).Set("life", Poles.Hitpoints).Where("id", pole.UID).Execute();
        }
    }
}