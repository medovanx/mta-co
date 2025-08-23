using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Database;
using MTA.Network.GamePackets;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Game
{
    public class PolePhoenix
    {
        public static SobNpcSpawn Pole, RightGate, LeftGate;

        public static SobNpcSpawn Poles;

        public static SafeDictionary<uint, Guild> Scores = new SafeDictionary<uint, Guild>(100);

        public static bool IsWar = false, Flame10th = false, FirstRound = false;

        public static Time32 ScoreSendStamp, LastWin;

        public static Guild PoleKeeperPh, CurrentTopLeader;

        private static bool changed = false;

        private static string[] scoreMessages;

        public static DateTime StartTime;

        public static bool Claim
        {
            get { return Program.Vars["phclaim"]; }
            set { Program.Vars["phclaim"] = value; }
        }

        public static uint KeeperID
        {
            get { return Program.Vars["phkeeperid"]; }
            set { Program.Vars["phkeeperid"] = value; }
        }

        public static void Initiate()
        {
            var Map = Kernel.Maps[2073];
            Pole = (SobNpcSpawn)Map.Npcs[814];
            LeftGate = (SobNpcSpawn)Map.Npcs[516078];
            RightGate = (SobNpcSpawn)Map.Npcs[516079];
        }

        //public static void EliteGwint()
        //{
        //    var Map = Kernel.Maps[2071];
        //    Poles = (SobNpcSpawn)Map.Npcs[811];
        //}

        public static void Start()
        {
            if (LeftGate == null) return;
            Scores = new SafeDictionary<uint, Guild>(100);
            StartTime = DateTime.Now;
            LeftGate.Mesh = (ushort)(240 + LeftGate.Mesh % 10);
            RightGate.Mesh = (ushort)(270 + LeftGate.Mesh % 10);
            LeftGate.Hitpoints = LeftGate.MaxHitpoints;
            RightGate.Hitpoints = RightGate.MaxHitpoints;
            Pole.Hitpoints = Pole.MaxHitpoints;
            Kernel.SendWorldMessage(new Message("PolePhoenix war has began!", System.Drawing.Color.Red, Message.Center),
                Program.Values);
            FirstRound = true;
            foreach (Guild guild in Kernel.Guilds.Values)
            {
                guild.PhScore = 0;
            }
            Update upd = new Update(true);
            upd.UID = LeftGate.UID;
            upd.Append(Update.Mesh, LeftGate.Mesh);
            upd.Append(Update.Hitpoints, LeftGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)2073);
            upd.Clear();
            upd.UID = RightGate.UID;
            upd.Append(Update.Mesh, RightGate.Mesh);
            upd.Append(Update.Hitpoints, RightGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)2073);
            Claim = false;
            IsWar = true;
        }

        public static void Reset()
        {
            Scores = new SafeDictionary<uint, Guild>(100);

            LeftGate.Mesh = (ushort)(240 + LeftGate.Mesh % 10);
            RightGate.Mesh = (ushort)(270 + LeftGate.Mesh % 10);

            LeftGate.Hitpoints = LeftGate.MaxHitpoints;
            RightGate.Hitpoints = RightGate.MaxHitpoints;
            Pole.Hitpoints = Pole.MaxHitpoints;

            Update upd = new Update(true);
            upd.UID = LeftGate.UID;
            upd.Append(Update.Mesh, LeftGate.Mesh);
            upd.Append(Update.Hitpoints, LeftGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)2073);
            upd.Clear();
            upd.UID = RightGate.UID;
            upd.Append(Update.Mesh, RightGate.Mesh);
            upd.Append(Update.Hitpoints, RightGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)2073);

            foreach (Guild guild in Kernel.Guilds.Values)
            {
                guild.PhScore = 0;
            }

            IsWar = true;
        }

        public static void FinishRound()
        {
            if (PoleKeeperPh != null && !FirstRound)
            {
                if (PoleKeeperPh.Wins == 0)
                    PoleKeeperPh.Losts++;
                else
                    PoleKeeperPh.Wins--;
                Database.GuildTable.UpdateGuildWarStats(PoleKeeperPh);
            }
            LastWin = Time32.Now;

            FirstRound = false;
            SortScores(out PoleKeeperPh);
            if (PoleKeeperPh != null)
            {
                KeeperID = PoleKeeperPh.ID;
                Kernel.SendWorldMessage(
                    new Message(
                        "The guild, " + PoleKeeperPh.Name + ", owned by " + PoleKeeperPh.LeaderName +
                        " has won this guild war round!", System.Drawing.Color.Red, Message.Center), Program.Values);
                Kernel.SendWorldMessage(
                    new Message("It is generald pardon time. You have 5 minutes to leave, run for your life!",
                        System.Drawing.Color.White, Message.TopLeft), Program.Values, (ushort)6001);

                if (PoleKeeperPh.Losts == 0)
                    PoleKeeperPh.Wins++;
                else
                    PoleKeeperPh.Losts--;
                Database.GuildTable.UpdateGuildWarStats(PoleKeeperPh);
                Pole.Name = PoleKeeperPh.Name;
            }
            Pole.Hitpoints = Pole.MaxHitpoints;
            Kernel.SendWorldMessage(Pole, Program.Values, (ushort)2073);
            Reset();
        }

        public static void End()
        {
            if (PoleKeeperPh != null)
            {
                Kernel.SendWorldMessage(
                    new Message(
                        "The Guild War, " + PoleKeeperPh.Name + ", owned by " + PoleKeeperPh.LeaderName +
                        " has won this guild war!---Guild war has ended!", System.Drawing.Color.White, Message.Center),
                    Program.Values);
                Database.GuildTable.UpdatePoleKeeperPh(PoleKeeperPh);
            }
            else
            {
                Kernel.SendWorldMessage(
                    new Message("Guild war has ended and there was no winner!", System.Drawing.Color.Red, Message.Center),
                    Program.Values);
            }
            IsWar = false;
            Claim = true;
            UpdatePole(Pole);
        }

        public static void AddScore(uint addScore, Guild guild)
        {
            if (guild != null)
            {
                guild.PhScore += addScore;
                changed = true;
                if (!Scores.ContainsKey(guild.ID))
                    Scores.Add(guild.ID, guild);
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
                Message msg = new Message(scoreMessages[c], System.Drawing.Color.Red,
                    c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)2073);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)6001);
            }
        }

        private static void SortScores(out Guild winner)
        {
            winner = null;
            List<string> ret = new List<string>();

            int Place = 0;
            foreach (Guild guild in Scores.Values.OrderByDescending((p) => p.PhScore))
            {
                if (Place == 0)
                    winner = guild;
                string str = "No  " + (Place + 1).ToString() + ": " + guild.Name + "(" + guild.PhScore + ")";
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