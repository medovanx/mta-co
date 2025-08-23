using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conquer_Online_Server.Network.GamePackets;
using Conquer_Online_Server.Game.ConquerStructures.Society;

namespace Conquer_Online_Server.Game
{
    public class GuildWar2
    {
        public static SobNpcSpawn Pole, RightGate, LeftGate;

        public static SobNpcSpawn Poles;

        public static SafeDictionary<uint, Guild> Scores = new SafeDictionary<uint, Guild>(100);

        public static bool IsWar = false, Flame10th = false, FirstRound = false;

        public static Time32 ScoreSendStamp, LastWin;

        public static Guild PoleKeeper3, CurrentTopLeader;

        private static bool changed = false;

        private static string[] scoreMessages;

        public static DateTime StartTime;

        public static bool Claim
        {
            get { return Program.Vars["gwclaim"]; }
            set { Program.Vars["gwclaim"] = value; }
        }
        public static uint KeeperID
        {
            get { return Program.Vars["gwkeeperid"]; }
            set { Program.Vars["gwkeeperid"] = value; }
        }
        public static void Initiate()
        {
            var Map = Kernel.Maps[4542];
            Pole = (SobNpcSpawn)Map.Npcs[600];
            LeftGate = (SobNpcSpawn)Map.Npcs[516078];
            RightGate = (SobNpcSpawn)Map.Npcs[516079];
        }

        public static void EliteGwint()
        {
            var Map = Kernel.Maps[2071];
            Poles = (SobNpcSpawn)Map.Npcs[811];
        }

        public static void Start()
        {
            if (LeftGate == null) return;
            Scores = new SafeDictionary<uint, Guild>(100);
            StartTime = DateTime.Now;
            LeftGate.Mesh = (ushort)(240 + LeftGate.Mesh % 10);
            RightGate.Mesh = (ushort)(270 + LeftGate.Mesh % 10);
            Kernel.SendWorldMessage(new Message("حرب النقابة قد بدائت!", System.Drawing.Color.Red, Message.Center), Program.Values);
            FirstRound = true;
            foreach (Guild guild in Kernel.Guilds.Values)
            {
                guild.WarScore = 0;
            }
            Update upd = new Update(true);
            upd.UID = LeftGate.UID;
            upd.Append(Update.Mesh, LeftGate.Mesh);
            upd.Append(Update.Hitpoints, LeftGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)4542);
            upd.Clear();
            upd.UID = RightGate.UID;
            upd.Append(Update.Mesh, RightGate.Mesh);
            upd.Append(Update.Hitpoints, RightGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)4542);
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
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)4542);
            upd.Clear();
            upd.UID = RightGate.UID;
            upd.Append(Update.Mesh, RightGate.Mesh);
            upd.Append(Update.Hitpoints, RightGate.Hitpoints);
            Kernel.SendWorldMessage(upd, Program.Values, (ushort)4542);

            foreach (Guild guild in Kernel.Guilds.Values)
            {
                guild.WarScore = 0;
            }

            IsWar = true;
        }

        public static void FinishRound()
        {
            if (PoleKeeper3 != null && !FirstRound)
            {
                if (PoleKeeper3.Wins == 0)
                    PoleKeeper3.Losts++;
                else
                    PoleKeeper3.Wins--;
                Database.GuildTable.UpdateGuildWarStats(PoleKeeper3);
            }
            LastWin = Time32.Now;

            FirstRound = false;
            SortScores(out PoleKeeper3);
            if (PoleKeeper3 != null)
            {
                KeeperID = PoleKeeper3.ID;
                Kernel.SendWorldMessage(new Message("نقابة, " + PoleKeeper3.Name + ", بواسطة " + PoleKeeper3.LeaderName + " قد فاز بهذه الجولة!", System.Drawing.Color.Red, Message.Center), Program.Values);
                Kernel.SendWorldMessage(new Message("It is generald pardon time. You have 5 minutes to leave, run for your life!", System.Drawing.Color.White, Message.TopLeft), Program.Values, (ushort)6001);
                if (PoleKeeper3.Losts == 0)
                    PoleKeeper3.Wins++;
                else
                    PoleKeeper3.Losts--;
                Database.GuildTable.UpdateGuildWarStats(PoleKeeper3);
                Pole.Name = PoleKeeper3.Name;
            }
            Pole.Hitpoints = Pole.MaxHitpoints;
            Kernel.SendWorldMessage(Pole, Program.Values, (ushort)4542);
            Reset();
        }

        public static void End()
        {
            if (PoleKeeper3 != null)
            {
                Kernel.SendWorldMessage(new Message("نقابة, " + PoleKeeper3.Name + ", بواسطة " + PoleKeeper3.LeaderName + " قد فاز بحرب النقابة وقد انتهت حرب النقابة!", System.Drawing.Color.White, Message.Center), Program.Values);
                //Conquer_Online_Server.Database.EntityTable.Status2();
            }
            else
            {
                Kernel.SendWorldMessage(new Message("حرب النقابة قد انتهت ولايوجد فائز!", System.Drawing.Color.Red, Message.Center), Program.Values);
                //Conquer_Online_Server.Database.EntityTable.Status2();
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
                guild.WarScore += addScore;
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
            if (Scores == null)
                return;
            if (Scores.Count == 0)
                return;
            if (changed)
                SortScores(out CurrentTopLeader);

            for (int c = 0; c < scoreMessages.Length; c++)
            {
                Message msg = new Message(scoreMessages[c], System.Drawing.Color.Red, c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)4542);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)6001);
            }
        }

        private static void SortScores(out Guild winner)
        {
            winner = null;
            List<string> ret = new List<string>();

            int Place = 0;
            foreach (Guild guild in Scores.Values.OrderByDescending((p) => p.WarScore))
            {
                if (Place == 0)
                    winner = guild;
                string str = "No  " + (Place + 1).ToString() + ": " + guild.Name + "(" + guild.WarScore + ")";
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
            new Database.MySqlCommand(Conquer_Online_Server.Database.MySqlCommandType.UPDATE)
            .Update("sobnpcs").Set("name", pole.Name).Set("life", Pole.Hitpoints).Where("id", pole.UID).Execute();
        }
    }
}
