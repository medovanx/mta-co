using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Game
{
    public class PoleRakion
    {
        public static SobNpcSpawn Pole, RightGate, LeftGate;

        public static SobNpcSpawn Poles;
        public const ushort MapID = 3990;
        public static SafeDictionary<uint, Guild> Scores = new SafeDictionary<uint, Guild>(100);

        public static bool IsWar = false, Flame10th = false, FirstRound = false;

        public static Time32 ScoreSendStamp, LastWin;

        public static Guild PoleKeeper, CurrentTopLeader;

        private static bool changed = false;

        private static string[] scoreMessages;

        public static DateTime StartTime;

        public static bool Claim
        {
            get { return Program.Vars["pwclaim"]; }
            set { Program.Vars["pwclaim"] = value; }
        }
        public static uint KeeperID
        {
            get { return Program.Vars["pwkeeperid"]; }
            set { Program.Vars["pwkeeperid"] = value; }
        }

        public static void PoleRakionIni()
        {
            var Map = Kernel.Maps[3990];
            Pole = (SobNpcSpawn)Map.Npcs[819];
            LeftGate = (SobNpcSpawn)Map.Npcs[516025];
            RightGate = (SobNpcSpawn)Map.Npcs[516026];
        }

        public static void Start()
        {
            object[] name;
            if (LeftGate == null) return;
            Scores = new SafeDictionary<uint, Guild>(100);
            StartTime = DateTime.Now;
            LeftGate.Mesh = (ushort)(240 + LeftGate.Mesh % 10);
            RightGate.Mesh = (ushort)(270 + LeftGate.Mesh % 10);
            name = new object[] { "Quest PoleRakion Has Started Go To Guild Controller At TwinCity (249,215)" };
            Kernel.SendWorldMessage(new Message(string.Concat(name), "ALLUSERS", "PoleRakion", System.Drawing.Color.Red, 2500), Program.Values);
            Kernel.SendWorldMessage(new Message("PoleRakion has began!", System.Drawing.Color.Red, Message.Center), Program.Values);
            FirstRound = true;
            foreach (Guild guild in Kernel.Guilds.Values)
            {
                guild.PIScore = 0;
            }
            Update upd = new Update(true);
            upd.UID = LeftGate.UID;
            upd.Append(Update.Mesh, LeftGate.Mesh);
            upd.Append(Update.Hitpoints, LeftGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)3990);
            upd.Clear();
            upd.UID = RightGate.UID;
            upd.Append(Update.Mesh, RightGate.Mesh);
            upd.Append(Update.Hitpoints, RightGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)3990);
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
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)3990);
            upd.Clear();
            upd.UID = RightGate.UID;
            upd.Append(Update.Mesh, RightGate.Mesh);
            upd.Append(Update.Hitpoints, RightGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)3990);

            foreach (Guild guild in Kernel.Guilds.Values)
            {
                guild.PIScore = 0;
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
                Kernel.SendWorldMessage(new Message("The guild, " + PoleKeeper.Name + ", owned by " + PoleKeeper.LeaderName + " has won this PoleRakion round!", System.Drawing.Color.Red, Message.Center), Program.Values);
                Kernel.SendWorldMessage(new Message("It is generald pardon time. You have 5 minutes to leave, run for your life!", System.Drawing.Color.White, Message.TopLeft), Program.Values, (ushort)6001);
                if (PoleKeeper.Losts == 0)
                    PoleKeeper.Wins++;
                else
                    PoleKeeper.Losts--;
                Database.GuildTable.UpdateGuildWarStats(PoleKeeper);
                Pole.Name = PoleKeeper.Name;
            }
            Pole.Hitpoints = Pole.MaxHitpoints;
            Kernel.SendWorldMessage(Pole, Program.Values, (ushort)3990);
            Reset();
        }

        public static void End()
        {
            if (PoleKeeper != null)
            {
                Kernel.SendWorldMessage(new Message("The guild, " + PoleKeeper.Name + ", owned by " + PoleKeeper.LeaderName + " has won this PoleRakion!---PoleRakion has ended!", System.Drawing.Color.White, Message.Center), Program.Values);
                //MTA.Database.EntityTable.Status2(); 
            }
            else
            {
                Kernel.SendWorldMessage(new Message("PoleRakion has ended and there was no winner!", System.Drawing.Color.Red, Message.Center), Program.Values);
                //MTA.Database.EntityTable.Status2(); 
            }
            IsWar = false;
            Claim = true;
            UpdatePole(Pole);
            foreach (Client.GameState client in Program.Values)
            {
                //client.Entity.Status2 = 0; 
                //client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.TopDeputyLeader); 
                //client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.TopGuildLeader); 
            }
        }

        public static void AddScore(uint addScore, Guild guild)
        {
            if (guild != null)
            {
                guild.PIScore += addScore;
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
                Message msg = new Message(scoreMessages[c], System.Drawing.Color.Red, c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)3990);
                //Kernel.SendWorldMessage(msg, Program.Values, (ushort)6001); 
            }
        }

        private static void SortScores(out Guild winner)
        {
            winner = null;
            List<string> ret = new List<string>();

            int Place = 0;
            foreach (Guild guild in Scores.Values.OrderByDescending((p) => p.RaScore))
            {
                if (Place == 0)
                    winner = guild;
                string str = "No  " + (Place + 1).ToString() + ": " + guild.Name + "(" + guild.RaScore + ")";
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