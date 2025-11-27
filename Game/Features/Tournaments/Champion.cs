/*
 * The following code belongs to Ultimation and impulse.
 * Ultimation is credited for most of this piece of code.
 * Thanks to him, we have arena implented.
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MTA.Client;
using MTA.Network.GamePackets;
using System.Threading;
using System.Globalization;
using System.Collections.Concurrent;
using MTA.Game;
using System.Drawing;
using MTA.Network;

namespace MTA.Game
{
    public class Champion
    {
        public static ConcurrentDictionary<uint, GameState> PlayerList = new ConcurrentDictionary<uint, GameState>();
        public static ConcurrentDictionary<uint, GameState> WaitingPlayerList = new ConcurrentDictionary<uint, GameState>();

        public static ConcurrentDictionary<uint, ChampionStatistic> ChampionStats = new ConcurrentDictionary<uint, ChampionStatistic>();

        public static SafeDictionary<uint, ChampionStatistic> YesterdayChampionStatss = new SafeDictionary<uint, ChampionStatistic>();

        public static List<ChampionStatistic>[] ChampionStatsList;
        public static List<ChampionStatistic> YesterdayChampionList;

        public static int InArenaCount = 0, HistoryArenaCount = 0;

        public static void Sort()
        {
            ChampionStatsList = new List<ChampionStatistic>[8];
            for (int j = 1; j < 8; j++)
            {
                try
                {
                    var where = ChampionStats.Values.Where((a) => a.Points != 0 && a.Grade == j);
                    List<ChampionStatistic> stats = where.OrderByDescending((p) => p.Points).ToList();
                    InArenaCount = stats.Count;
                    for (uint i = 0; i < stats.Count; i++)
                        stats[(int)i].Rank = (byte)(i + 1);
                    ChampionStatsList[j] = stats;
                }
                catch { ChampionStatsList[j] = new List<ChampionStatistic>(); }
            }
        }
        public static void YesterdaySort()
        {
            var where = ChampionStats.Values.Where((a) => a.YesterdayPoints != 0);
            List<ChampionStatistic> stats = where.OrderByDescending((p) => p.YesterdayPoints).ToList();
            HistoryArenaCount = stats.Count;
            for (uint i = 0; i < stats.Count; i++)
                stats[(int)i].YesterdayRank = i + 1;
            YesterdayChampionList = stats;
        }

        private static void SaveArenaStats()
        {
            var array = ChampionStats.Values.ToArray();
            foreach (ChampionStatistic stats in array)
                if (stats != null)
                    Database.ChampionTable.SaveStats(stats);
        }

        public static class QualifierList
        {
            public static ConcurrentDictionary<uint, QualifierGroup> Groups = new ConcurrentDictionary<uint, QualifierGroup>();
            public static Counter GroupCounter = new MTA.Counter();

            public class QualifierGroup
            {
                public Time32 CreateTime;
                public Time32 DoneStamp;

                public uint Player1Damage, Player2Damage;

                public bool Done;

                private Game.Enums.PkMode P1Mode, P2Mode;

                public uint ID;

                public Client.GameState Winner, Loser;
                public Client.GameState Player1, Player2;

                private Map dynamicMap;
                public Time32 ImportTime;
                public QualifierGroup(Client.GameState player1, Client.GameState player2)
                {
                    Player1 = player1;
                    Player2 = player2;
                    CreateTime = Time32.Now;
                    Player1Damage = 0;
                    Player2Damage = 0;
                    Done = false;
                    ID = GroupCounter.Next;
                    Done = false;
                    Groups.Add(ID, this);
                }

                public GameState OppositeClient(GameState client)
                {
                    if (client == Player1)
                        return Player2;
                    else
                        return Player1;
                }

                public void Import()
                {
                    Player1.ChampionGroup = this;
                    Player2.ChampionGroup = this;

                    if (!Kernel.Maps.ContainsKey(700))
                        new Map(700, Database.DMaps.MapPaths[700]);
                    Map origMap = Kernel.Maps[700];
                    dynamicMap = origMap.MakeDynamicMap();
                    Player1.Entity.Teleport(origMap.ID, dynamicMap.ID, (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                    Player2.Entity.Teleport(origMap.ID, dynamicMap.ID, (ushort)Kernel.Random.Next(35, 70), (ushort)Kernel.Random.Next(35, 70));
                    ImportTime = Time32.Now;
                    if (Player1.Map.ID == Player2.Map.ID)
                    {
                        Player1.Entity.timerInportChampion = DateTime.Now;
                        Player2.Entity.timerInportChampion = DateTime.Now;
                        Player1.Send(ChampionKernel.OpponentInfo(Player2.ChampionStats).BuildPacket());
                        Player2.Send(ChampionKernel.OpponentInfo(Player1.ChampionStats).BuildPacket());//player 1
                        Player1.Send(ChampionKernel.Stats(0, 0).BuildPacket());
                        Player2.Send(ChampionKernel.Stats(0, 0).BuildPacket());
                        Player1.Send(Player2.ChampionStats);
                        Player2.Send(Player1.ChampionStats);
                        Player1.Entity.BringToLife();
                        Player2.Entity.BringToLife();
                        Player1.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                        Player2.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                        P1Mode = Player1.Entity.PKMode;
                        Player1.Entity.PKMode = MTA.Game.Enums.PkMode.PK;
                        Player1.Send(new Data(true) { UID = Player1.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)Player1.Entity.PKMode });
                        P2Mode = Player2.Entity.PKMode;
                        Player2.Entity.PKMode = MTA.Game.Enums.PkMode.PK;
                        Player2.Send(new Data(true) { UID = Player2.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)Player2.Entity.PKMode });
                        Player1.LoadItemStats();
                        Player2.LoadItemStats();
                    }
                    else
                        End();
                }

                public void Export()
                {
                    Groups.Remove(ID);

                    if (dynamicMap != null)
                        dynamicMap.Dispose();

                    Player1.Entity.PreviousTeleport();
                    if (Player1.Map.BaseID == 700 && Player1.Entity.MapID != 700)
                        Player1.Entity.Teleport(1002, 301, 279);
                    Player2.Entity.PreviousTeleport();
                    if (Player2.Map.BaseID == 700 && Player2.Entity.MapID != 700)
                        Player2.Entity.Teleport(1002, 301, 279);
                    Loser.Entity.Ressurect();
                    Winner.Entity.Ressurect();

                    Player1.Entity.PKMode = P1Mode;
                    Player1.Send(new Data(true) { UID = Player1.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)Player1.Entity.PKMode });
                    Player2.Entity.PKMode = P2Mode;
                    Player2.Send(new Data(true) { UID = Player2.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)Player2.Entity.PKMode });
                    Player1.QualifierGroup = null;
                    Player2.QualifierGroup = null;

                }

                public void End()
                {
                    if (Done) return;
                    if (Player1Damage > Player2Damage)
                    {
                        Winner = Player1;
                        Loser = Player2;
                    }
                    else
                    {
                        Winner = Player2;
                        Loser = Player1;
                    }

                    Done = true;
                    DoneStamp = Time32.Now;
                }

                public void End(GameState loser)
                {
                    if (Done) return;
                    if (Player1.Entity.UID == loser.Entity.UID)
                    {
                        Winner = Player2;
                        Loser = Player1;
                    }
                    else
                    {
                        Winner = Player1;
                        Loser = Player2;
                    }

                    Done = true;
                    DoneStamp = Time32.Now;
                }

                public void UpdateDamage(GameState client, uint damage)
                {
                    if (client != null && Player1 != null)
                    {
                        if (client.Entity.UID == Player1.Entity.UID)
                            Player1Damage += damage;
                        else
                            Player2Damage += damage;
                        var packet = ChampionKernel.Stats(Player1Damage, Player2Damage).BuildPacket();
                        Player1.Send(packet);
                        Player2.Send(packet);
                    }
                }
            }
        }

        public const int
                None = 0,
                FindMatch = 1,
                WaitForBox = 2,
                WaitForOther = 3,
                Fight = 4;

        public class QualifyEngine
        {
            public static void DoQuit(Client.GameState client)
            {
                PlayerList.Remove(client.Entity.UID);

                if (client.QualifierGroup != null)
                {
                    client.ChampionStats.GivenUp = true;
                    client.QualifierGroup.End(client);
                    client.QualifierGroup.CreateTime = Time32.Now.AddSeconds(-100);
                }
                else
                {
                    if (client.ChampionStats.Opponent != null)
                        Win(client.ChampionStats.Opponent, client);
                    Clear(client);
                }
            }

            public static void DoSignup(Client.GameState client)
            {
                DateTime now = DateTime.Now;
                // if (now.DayOfWeek != DayOfWeek.Sunday && now.DayOfWeek != DayOfWeek.Saturday)
                if (!(now.Hour == 20 || now.Hour == 21 || now.Hour == 19))// now.Minute >= 55))
                    return;
                if (client.ChampionStats.SignedUp
                    || client.TeamArenaStatistic.Status != TeamArenaStatistic.NotSignedUp
                    || client.ArenaStatistic.Status != ArenaStatistic.NotSignedUp)
                {
                    client.Send(new Message("You already joined a qualifier arena! Quit the other one and sign up again.", Color.Beige, Message.Agate));
                    return;
                }
                if (client.InQualifier())
                {
                    return;
                }
                if (client.Entity.MapID == 601) return;
                if (client.Map.BaseID >= 6000 && client.Map.BaseID <= 6003) return;

                if (WaitingPlayerList.ContainsKey(client.Entity.UID))
                {
                    if (client.QualifierGroup == null)
                        WaitingPlayerList.Remove(client.Entity.UID);
                    else
                        return;
                }
                PlayerList.Add(client.Entity.UID, client);
                client.ChampionStats.SignUpTime = Time32.Now;
                client.ChampionStats.SignedUp = true;
                client.ArenaState = FindMatch;
                client.Send(client.ChampionStats);
            }
            public static void DoGiveUp(Client.GameState client)
            {
                if (client.ArenaState == WaitForBox)
                {
                    client.ChampionStats.AcceptBox = false;
                    client.ArenaState = WaitForOther;
                }
                else
                {
                    if (client.ChampionStats.Opponent != null)
                    {
                        client.ChampionStats.GivenUp = true;
                        if (WaitingPlayerList.ContainsKey(client.Entity.UID))
                        {
                            WaitingPlayerList.Remove(client.Entity.UID);
                            WaitingPlayerList.Remove(client.ChampionStats.Opponent.Entity.UID);
                        }
                        if (client.QualifierGroup != null)
                        {
                            if (!client.QualifierGroup.Done)
                            {
                                client.QualifierGroup.End(client);
                            }
                            else
                            {
                                Win(client.ChampionStats.Opponent, client);
                            }
                        }
                        else
                        {
                            Win(client.ChampionStats.Opponent, client);
                        }
                    }
                }
            }

            public static void DoAccept(Client.GameState client)
            {
                if (client.ArenaState == WaitForBox)
                {
                    client.ChampionStats.AcceptBox = true;
                    client.ArenaState = WaitForOther;
                }
            }
        }

        public class Statistics
        {
            public static void ShowTop(Client.GameState client, int type = 2602)
            {
                MemoryStream strm = new MemoryStream();
                BinaryWriter wtr = new BinaryWriter(strm);
                wtr.Write((ushort)0);
                wtr.Write((ushort)type);
                List<ChampionStatistic> list = null;
                IEnumerable<ChampionStatistic> ienum = null;
                if (type == 2603)
                {
                    list = new List<ChampionStatistic>();
                    for (int i = 7; i > 0; i--)
                        if (ChampionStatsList[i].Count != 0)
                            list.Add(ChampionStatsList[i][0]);
                        else
                            list.Add(new ChampionStatistic(true) { Grade = (byte)i, Name = "" });
                    ienum = list;
                }
                else
                    ienum = YesterdayChampionList;//.OrderByDescending(p => p.YesterdayPoints);

                int len = Math.Min(10, ienum.Count());
                wtr.Write((byte)len);
                var _enum = ienum.GetEnumerator();

                for (int i = 0; i < len; i++)
                {
                    if (!_enum.MoveNext()) break;
                    var entry = _enum.Current;

                    wtr.Write((byte)entry.Level);
                    if (type == 2603)
                        wtr.Write((byte)entry.Grade);
                    else
                        wtr.Write((byte)i);
                    wtr.Write((byte)entry.Class);
                    wtr.Write((uint)entry.Model);
                    if (type == 2603)
                        wtr.Write((uint)entry.Points);
                    else
                        wtr.Write((uint)entry.YesterdayPoints);
                    byte[] array = Encoding.Default.GetBytes(entry.Name);
                    for (int j = 0; j < 16; j++)
                    {
                        if (j < entry.Name.Length)
                            wtr.Write(array[j]);
                        else
                            wtr.Write((byte)0);
                    }
                }
                int packetlength = (int)strm.Length;
                strm.Position = 0;
                wtr.Write((ushort)packetlength);
                strm.Position = strm.Length;
                wtr.Write(Encoding.Default.GetBytes("TQServer"));
                strm.Position = 0;
                byte[] buf = new byte[strm.Length];
                strm.Read(buf, 0, buf.Length);
                wtr.Close();
                strm.Close();
                client.Send(buf);
            }

            public static void ShowRankingPage(ushort group, int pageIndex, Client.GameState client)
            {
                MemoryStream strm = new MemoryStream();
                BinaryWriter wtr = new BinaryWriter(strm);
                wtr.Write((ushort)0);
                wtr.Write((ushort)2600);
                int first = (pageIndex - 1) * 10;
                //var list = ChampionStatsList[group];
                int len = 0;
                //if (list.Count >= first) len = list.Count - first;
                len = Math.Min(10, len);
                wtr.Write((byte)group);
                wtr.Write((byte)pageIndex);
                wtr.Write((byte)(first + len));
                //wtr.Write((byte)list.Count);

                for (int i = 0; i < len; i++)
                {
                    //var entry = list[first + i];
                    //wtr.Write((byte)entry.Level);
                    //wtr.Write((byte)entry.Rank);
                    //wtr.Write((byte)entry.Class);
                    //wtr.Write((uint)entry.Model);
                    //wtr.Write((uint)entry.Points);
                    //byte[] array = Program.Encoding.GetBytes(entry.Name);
                    for (int j = 0; j < 16; j++)
                    {
                        //if (j < entry.Name.Length)
                        //wtr.Write(array[j]);
                        //else
                        wtr.Write((byte)0);
                    }
                }
                int packetlength = (int)strm.Length;
                strm.Position = 0;
                wtr.Write((ushort)packetlength);
                strm.Position = strm.Length;
                wtr.Write(Program.Encoding.GetBytes("TQServer"));
                strm.Position = 0;
                byte[] buf = new byte[strm.Length];
                strm.Read(buf, 0, buf.Length);
                wtr.Close();
                strm.Close();
                client.Send(buf);
            }
        }

        public class ChampionKernel : Writer
        {
            public enum KernelType : int
            {
                SignUp = 0,
                DialogCountdown = 1,
                OpponentInfo = 4,
                WinDialog = 6,
                LoseDialog = 7,
                MatchStats = 8
            }
            object[] strs;
            KernelType type;

            public static ChampionKernel SignUp()
            {
                return new ChampionKernel(KernelType.SignUp);
            }
            public static ChampionKernel DialogCountdown()
            {
                return new ChampionKernel(KernelType.DialogCountdown);
            }
            public static ChampionKernel Stats(uint damage1, uint damage2)
            {
                return new ChampionKernel(KernelType.MatchStats, damage1.ToString(), damage2.ToString());
            }
            public static ChampionKernel OpponentInfo(ChampionStatistic stats)
            {
                return new ChampionKernel(KernelType.OpponentInfo,
                    stats.UID, stats.Name, stats.Class, "1", stats.Rank, stats.Level,
                    stats.Class, stats.BattlePower);
            }
            public static ChampionKernel Win(bool forfeit, uint pts, int streak, uint extrapts)
            {
                return new ChampionKernel(KernelType.WinDialog,
                    forfeit ? 1 : 0, pts, streak, extrapts);
            }
            public static ChampionKernel Lose(uint pts)
            {
                return new ChampionKernel(KernelType.LoseDialog, pts);
            }

            public ChampionKernel(KernelType type, params object[] strs)
            {
                this.type = type;
                this.strs = strs;
            }

            public byte[] BuildPacket()
            {
                var array = new string[strs.Length];
                for (int i = 0; i < array.Length; i++)
                    array[i] = strs[i].ToString();

                int len = strs.Length;
                for (int i = 0; i < strs.Length; i++)
                    len += array[i].Length;
                byte[] data = new byte[16 + len];
                WriteUInt16((ushort)(data.Length - 8), 0, data);
                WriteUInt16(2604, 2, data);
                data[4] = (byte)type;
                WriteStringList(array, 5, data);
                return data;
            }
        }

        public static uint[][] ChampionPoints = new uint[][]
        {
            new uint[] {0,0,0,0,0,0},
            new uint[] {6,10,13,16,19,22},
            new uint[] {8,14,20,23,26,29},
            new uint[] {10,18,27,30,33,36},
            new uint[] {20,30,42,45,48,51},
            new uint[] {35,50,65,68,71,74},
            new uint[] {50,80,98,101,104,107 },
            new uint[] {60,100,121,124,127,130 }
        };

        public static void Win(Client.GameState winner, Client.GameState loser)
        {
            if (winner.ChampionStats.Opponent != null && loser.ChampionStats.Opponent != null)
            {
                winner.ChampionStats.Opponent = null;
                loser.ChampionStats.Opponent = null;

                WaitingPlayerList.Remove(loser.Entity.UID);
                WaitingPlayerList.Remove(winner.Entity.UID);

                winner.ChampionStats.TotalMatches++;
                winner.ChampionStats.WinStreak++;
                loser.ChampionStats.TotalMatches++;
                loser.ChampionStats.WinStreak = 0;

                int wt = Math.Min(5, (int)winner.ChampionStats.WinStreak);
                uint wp = ChampionPoints[winner.ChampionStats.IsGrade][wt];
                uint lp = ChampionPoints[loser.ChampionStats.IsGrade][0];

                winner.ChampionStats.TodayPoints += wp;
                winner.ChampionStats.Points += wp;
                winner.ChampionStats.TotalPoints += wp;

                loser.ChampionStats.TodayPoints += lp;
                loser.ChampionStats.Points += lp;
                loser.ChampionStats.TotalPoints += lp;

                if (winner.ChampionStats.Points > 304400)
                    winner.ChampionStats.Points = 304400;
                if (loser.ChampionStats.Points > 304400)
                    loser.ChampionStats.Points = 304400;

                winner.ChampionStats.Grade = (byte)winner.ChampionStats.IsGrade;
                loser.ChampionStats.Grade = (byte)loser.ChampionStats.IsGrade;

                Sort();

                winner.Send(winner.ChampionStats);
                loser.Send(loser.ChampionStats);

                winner.ArenaState = FindMatch;
                winner.ChampionGroup = null;

                loser.ArenaState = FindMatch;
                loser.ChampionGroup = null;

                StringBuilder builder = new StringBuilder();
                if (winner.Entity.GuildID != 0)
                {
                    builder.Append("(");
                    builder.Append(winner.Guild.Name);
                    builder.Append(") ");
                }
                builder.Append(winner.Entity.Name);
                builder.Append(" has defeated ");

                if (loser.Entity.GuildID != 0)
                {
                    builder.Append("(");
                    builder.Append(loser.Guild.Name);
                    builder.Append(") ");
                }
                builder.Append(loser.Entity.Name);
                if (winner.ChampionStats.Rank > 0)
                {
                    builder.Append(" in the Qualifier, and is currently ranked No. "); builder.Append(winner.ChampionStats.Rank);
                }
                else { builder.Append(" in the Qualifier"); }

                builder.Append(".");

                Kernel.SendWorldMessage(new Message(builder.ToString(), System.Drawing.Color.Red, Message.ArenaQualifier), Program.Values);

                winner.QualifierGroup = null;
                loser.QualifierGroup = null;

                loser.Send(ChampionKernel.Lose(lp).BuildPacket());
                winner.Send(ChampionKernel.Win(loser.ChampionStats.GivenUp,
                    wp, winner.ChampionStats.WinStreak,
                    wp - ChampionPoints[winner.ChampionStats.IsGrade][1]).BuildPacket());

                Clear(loser);
                loser.Send(loser.ChampionStats);

                Clear(winner);
                winner.Send(winner.ChampionStats);

                PlayerList.Add(loser.Entity.UID, loser);
                PlayerList.Add(winner.Entity.UID, winner);
                loser.ArenaState = FindMatch;
                winner.ArenaState = FindMatch;
                loser.ChampionStats.SignUpTime = Time32.Now;
                winner.ChampionStats.SignUpTime = Time32.Now;

                loser.LoadItemStats();
                winner.LoadItemStats();
            }
        }

        private static DateTime YesterdaySorted = DateTime.Now;

        public static void Clear(Client.GameState client)
        {
            var champ_stat = client.ChampionStats;
            champ_stat.AcceptBox = false;
            champ_stat.AcceptBoxShow = Time32.Now;
            champ_stat.GivenUp = false;
            champ_stat.Opponent = null;
            client.Send(champ_stat);
            client.ArenaState = Arena.None;
            client.QualifierGroup = null;
            client.TeamQualifierGroup = null;
            WaitingPlayerList.Remove(champ_stat.UID);
            PlayerList.Remove(champ_stat.UID);
        }

        public static void EngagePlayers()
        {
            DateTime now = DateTime.Now;
            if (!(now.Hour == 20 || now.Hour == 21 || now.Hour == 19))
                return;
            if (PlayerList.Count < 2)
                return;
            var Players = new GameState[PlayerList.Count];
            PlayerList.Values.CopyTo(Players, 0);
            int i, j;
            int iPlus, jPlus;
            int iEnd, jEnd;
            iPlus = Kernel.Random.Next(2);
            jPlus = Kernel.Random.Next(2);
            if (iPlus == 0) { i = 0; iPlus = 1; iEnd = Players.Length; } else { i = Players.Length - 1; iPlus = -1; iEnd = -1; }
            if (jPlus == 0) { j = 0; jPlus = 1; jEnd = Players.Length; } else { j = Players.Length - 1; jPlus = -1; jEnd = -1; }
            Time32 Now = Time32.Now;
            for (; i != iEnd; i += iPlus)
            {
                var Challanger = Players[i];

                if (CanJoin(Challanger, Now) && Challanger.ArenaState == FindMatch)
                {
                    if (Time32.Now > Challanger.ChampionStats.SignUpTime.AddSeconds(10))
                    {
                        for (; j != jEnd; j += jPlus)
                        {
                            var Challanged = Players[j];
                            if (CanJoin(Challanged, Now) && Challanged.ArenaState == FindMatch)
                            {
                                if (Time32.Now > Challanged.ChampionStats.SignUpTime.AddSeconds(10))
                                {
                                    if (Challanger.Entity.UID != Challanged.Entity.UID && Challanger.ChampionStats.Grade == Challanged.ChampionStats.Grade)
                                    {
                                        Challanger.ArenaState = Challanged.ArenaState = WaitForBox;
                                        Challanged.ChampionStats.AcceptBoxShow = Challanger.ChampionStats.AcceptBoxShow = Time32.Now;
                                        Challanged.Send(Challanged.ChampionStats);
                                        Challanger.Send(Challanger.ChampionStats);
                                        Challanged.ChampionStats.Opponent = Challanger;
                                        Challanger.ChampionStats.Opponent = Challanged;
                                        Challanger.Send(ChampionKernel.DialogCountdown().BuildPacket());
                                        Challanged.Send(ChampionKernel.DialogCountdown().BuildPacket());
                                        PlayerList.Remove(Challanged.Entity.UID);
                                        PlayerList.Remove(Challanger.Entity.UID);
                                        WaitingPlayerList.Add(Challanged.Entity.UID, Challanged);
                                        WaitingPlayerList.Add(Challanger.Entity.UID, Challanger);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void VerifyAwaitingPeople()
        {
            foreach (var Challanger in WaitingPlayerList.Values)
            {
                if (Challanger != null)
                {
                    if (Challanger.ChampionStats == null)
                    {
                        WaitingPlayerList.Remove(Challanger.Entity.UID);
                        continue;
                    }
                    if (Challanger.ChampionStats.Opponent != null)
                    {
                        Client.GameState Challanged = Challanger.ChampionStats.Opponent;
                        if (Challanger.ArenaState == WaitForBox || Challanged.ArenaState == WaitForBox)
                        {
                            if (Time32.Now > Challanger.ChampionStats.AcceptBoxShow.AddSeconds(60))
                            {
                                if (Challanger.ArenaState == WaitForBox)
                                {
                                    Challanger.ChampionStats.GivenUp = true;
                                    Win(Challanged, Challanger);
                                }
                                else
                                {
                                    Challanged.ChampionStats.GivenUp = true;
                                    Win(Challanger, Challanged);
                                }
                                return;
                            }
                        }
                        if (Challanger.ArenaState == WaitForOther && !Challanger.ChampionStats.AcceptBox)
                        {
                            Challanger.ChampionStats.GivenUp = true;
                            Win(Challanged, Challanger);
                        }
                        else if (Challanged.ArenaState == WaitForOther && !Challanged.ChampionStats.AcceptBox)
                        {
                            Challanged.ChampionStats.GivenUp = true;
                            Win(Challanger, Challanged);
                        }
                        else if (Challanger.ArenaState == WaitForOther && Challanged.ArenaState == WaitForOther)
                        {
                            if (!Challanger.ChampionStats.AcceptBox || !Challanged.ChampionStats.AcceptBox)
                            {
                                if (!Challanger.ChampionStats.AcceptBox)
                                {
                                    Challanger.ChampionStats.GivenUp = true;
                                    Win(Challanged, Challanger);
                                }
                                else
                                {
                                    Challanged.ChampionStats.GivenUp = true;
                                    Win(Challanger, Challanged);
                                }
                            }
                            else
                            {
                                if (Challanged.Entity.MapID == 1081 || Challanged.Entity.ContainsFlag2(Update.Flags2.SoulShackle))
                                {
                                    Arena.QualifyEngine.DoQuit(Challanged);
                                    continue;
                                }
                                if (Challanger.Entity.MapID == 1081 || Challanger.Entity.ContainsFlag2(Update.Flags2.SoulShackle))
                                {
                                    Arena.QualifyEngine.DoQuit(Challanger);
                                    continue;
                                }
                                Challanger.ArenaState = Challanged.ArenaState = Fight;
                                QualifierList.QualifierGroup group = new QualifierList.QualifierGroup(Challanger, Challanged);
                                group.Import();
                            }
                        }
                    }
                }
            }
        }
        public static void CheckGroups()
        {
            if (QualifierList.Groups.Count > 0)
            {
                foreach (var group in QualifierList.Groups.Values)
                {
                    if (Time32.Now > group.CreateTime.AddSeconds(5))
                    {
                        if (!group.Done)
                        {
                            if (Time32.Now > group.CreateTime.AddMinutes(3))
                            {
                                group.End();
                            }
                        }
                        else
                        {
                            if (Time32.Now > group.DoneStamp.AddSeconds(4))
                            {
                                group.DoneStamp = Time32.Now.AddDays(1);
                                group.Export();
                                Win(group.Winner, group.Loser);
                            }
                        }
                    }
                }
            }
        }

        private static bool DayPassed = false;
        private static DateTime StartDateTime = DateTime.Now;
        public static void Reset()
        {
            DateTime now = DateTime.Now;
            if (!DayPassed)
            {

                if (now.DayOfWeek != DayOfWeek.Saturday && now.DayOfWeek != DayOfWeek.Sunday)
                {

                    if (now.Hour == 21 && now.Minute >= 25)
                    {

                        DayPassed = true;
                        var array = ChampionStats.Values.ToArray();
                        foreach (ChampionStatistic stat in array)
                            Database.ChampionTable.Reset(stat);
                        try { SaveArenaStats(); }
                        catch { }
                        YesterdaySort();
                    }


                }
            }
            else
            {
                if (now.Hour == 22)
                    DayPassed = false;
            }
        }

        private static bool CanJoin(Client.GameState client, Time32 now)
        {
            if (client != null)
            {
                if (client.Entity.MapID == 1081) return false;
                if (client.ChampionStats.Opponent == null)
                {
                    if (client.ChampionGroup == null)
                    {
                        if (client.Map.BaseID == 1038) return false;
                        if (client.Map.BaseID == 700) return false;
                        if (client.Entity.MapID >= 1090 && client.Entity.MapID <= 1094) return false;
                        if (client.Entity.MapID >= 1505 && client.Entity.MapID <= 1509) return false;
                        if (client.Entity.ContainsFlag2(Update.Flags2.SoulShackle)) return false;
                        if (!Constants.PKFreeMaps.Contains(client.Map.ID) || client.Map.ID == 1005)
                            if (client.ArenaState == Champion.FindMatch)
                                return true;
                    }
                }
            }
            return false;
        }

        public static void Handle(GameState client, byte[] packet)
        {
            uint DialogID = BitConverter.ToUInt16(packet, 4);
            switch (DialogID)
            {
                case 0:
                    Game.Champion.QualifyEngine.DoSignup(client);
                    client.Send(packet);
                    break;
                case 2:
                    Game.Champion.QualifyEngine.DoAccept(client);
                    break;
                case 3:
                    Game.Champion.QualifyEngine.DoGiveUp(client);
                    break;
            }
        }
    }
}