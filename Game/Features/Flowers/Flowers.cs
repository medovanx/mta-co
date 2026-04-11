using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Game.Features.Flowers.Database;
using MTA.Game.Features.Flowers.Database.Models;
using MTA.Game.Features.Flowers.Database.Schema;
using MTA.Game.Features.Kisses.Database;
using MTA.Game.Features.Kisses.Database.Models;
using MTA.Game.Features.Kisses.Database.Schema;

namespace MTA.Game.Features.Flowers;

public enum Effect : byte {
    None = 0,
    Rose = 1,
    Lilies = 2,
    Orchids = 3,
    Tulips = 4,
    Kiss = 1,
    Love = 2,
    Wine = 3,
    Jade = 4
}

public enum FlowersT : byte {
    Roses = 0,
    Lilies = 1,
    Orchids = 2,
    Tulips = 3,
    Kiss = 4,
    Love = 5,
    Wine = 6,
    Jade = 7,

    /// <summary>Not ranked / no qualifying category — must not collide with <see cref="Roses" /> (0).</summary>
    None = 255
}

public class Flowers {
    // Static dictionaries removed - now using database storage

    public static Flowers[] KissTop100 = [];
    public static Flowers[] LoveTop100 = [];
    public static Flowers[] WineTop100 = [];
    public static Flowers[] JadeTop100 = [];

    public static Flowers[] RedRousesTop100 = [];
    public static Flowers[] LiliesTop100 = [];
    public static Flowers[] OrchidsTop100 = [];
    public static Flowers[] TulipsTop100 = [];

    public static object RoseLock = new();
    public static object LiliesLock = new();
    public static object OrchidsLock = new();
    public static object TulipsLock = new();

    public static object KissLock = new();
    public static object LoveLock = new();
    public static object WineLock = new();
    public static object JadeLock = new();

    public static List<Flowers> RankKiss = [];
    public static List<Flowers> RankLove = [];
    public static List<Flowers> RankWine = [];
    public static List<Flowers> RankJade = [];

    public static List<Flowers> RankRose = [];
    public static List<Flowers> Ranklili = [];
    public static List<Flowers> RankOrchid = [];
    public static List<Flowers> RankTulips = [];

    public uint AFlower = 1;

    public uint Lilies; //love
    public uint Lilies2day;

    public string Name = "";
    public uint Orchids; //wine
    public uint OrchidsToday;
    public int RankLilies; //max 10 start with -1.
    public int RankOrchids; //max 10 start with -1.

    public int RankRoses;
    public int RankTulops; //max 10 start with -1.

    public uint RedRoses; //kiss
    public uint RedRosesToday;
    public uint SendDay;
    public uint Tulips; //jade
    public uint TulipsToday;
    public uint Uid;

    public Flowers() { }

    public Flowers(uint uid, string name) {
        Uid = uid;
        Name = name;
    }

    public DateTime LastFlowerSent { get; set; }

    public int SendScreenValue(FlowersT typ, int rak) {
        if (rak < 0 || rak == 0 || rak > 100 || typ == FlowersT.None)
            return 0;
        return (int)(30000002 + (uint)(100 * (byte)typ) + GetRank(rak));
    }

    public ushort GetRank(int rank) {
        switch (rank) {
            case 1:
                return 0;
            case 2:
                return 10000;
            case 3:
                return 20000;
            case > 3:
                return 30000;
            default:
                return 0;
        }
    }

    public int BoySendScreenValue(FlowersT typ, int rak) {
        switch (rak) {
            case -1:
            case > 100:
                return 0;
            default:
                var ret = (int)(30000402 + (uint)(100 * (byte)typ));

                return ret;
        }
    }


    public void Reset() {
        if (SendDay == DateTime.Now.Day) return;
        RedRosesToday = Lilies2day = OrchidsToday = TulipsToday = 0;
        AFlower = 1;
        SendDay = (uint)DateTime.Now.Day;
    }

    public static void CalculateRankJade(Flowers afflow) {
        lock (JadeLock) {
            try {
                RankJade.RemoveAll(x => x.Uid == afflow.Uid);
                if (!RankJade.Contains(afflow))
                    RankJade.Add(afflow);
                var data = RankJade.ToArray();

                Array.Sort(data, (c1, c2) => c2.Tulips.CompareTo(c1.Tulips));

                var room = data.ToArray();

                List<Flowers> backUpd = [];

                var x = 1;
                foreach (var flow in room) {
                    if (flow.Tulips == 0) continue;
                    if (x < 100) {
                        flow.RankTulops = x;
                        backUpd.Add(flow);
                    }
                    else {
                        flow.RankTulops = 0;
                    }

                    x++;
                }

                lock (JadeTop100) {
                    RankJade = new List<Flowers>(backUpd);
                    JadeTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void CalculateRankWine(Flowers afflow) {
        lock (WineLock) {
            try {
                RankWine.RemoveAll(x => x.Uid == afflow.Uid);
                if (!RankWine.Contains(afflow))
                    RankWine.Add(afflow);
                var data = RankWine.ToArray();

                Array.Sort(data, (c1, c2) => c2.Orchids.CompareTo(c1.Orchids));

                var room = data.ToArray();

                List<Flowers> backUpd = [];

                var x = 1;
                foreach (var flow in room) {
                    if (flow.Orchids == 0) continue;
                    if (x < 100) {
                        flow.RankOrchids = x;
                        backUpd.Add(flow);
                    }
                    else {
                        flow.RankOrchids = 0;
                    }

                    x++;
                }

                lock (WineTop100) {
                    RankWine = new List<Flowers>(backUpd);
                    WineTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void CalculateRankLove(Flowers afflow) {
        lock (LoveLock) {
            try {
                RankLove.RemoveAll(x => x.Uid == afflow.Uid);
                if (!RankLove.Contains(afflow))
                    RankLove.Add(afflow);
                var data = RankLove.ToArray();

                Array.Sort(data, (c1, c2) => c2.Lilies.CompareTo(c1.Lilies));

                var room = data.ToArray();

                List<Flowers> backUpd = [];

                var x = 1;
                foreach (var flow in room) {
                    if (flow.Lilies == 0) continue;
                    if (x < 100) {
                        flow.RankLilies = x;
                        backUpd.Add(flow);
                    }
                    else {
                        flow.RankLilies = 0;
                    }

                    x++;
                }

                lock (LoveTop100) {
                    RankLove = new List<Flowers>(backUpd);
                    LoveTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void CalculateRankKiss(Flowers afflow) {
        lock (KissLock) {
            try {
                RankKiss.RemoveAll(x => x.Uid == afflow.Uid);
                if (!RankKiss.Contains(afflow))
                    RankKiss.Add(afflow);
                var data = RankKiss.ToArray();

                Array.Sort(data, (c1, c2) => c2.RedRoses.CompareTo(c1.RedRoses));

                var room = data.ToArray();

                List<Flowers> backUpd = [];

                var x = 1;
                foreach (var flow in room) {
                    if (flow.RedRoses == 0) continue;
                    if (x < 100) {
                        flow.RankRoses = x;
                        backUpd.Add(flow);
                    }
                    else {
                        flow.RankRoses = 0;
                    }

                    x++;
                }

                lock (KissTop100) {
                    RankKiss = new List<Flowers>(backUpd);
                    KissTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void CalculateRoseRank(Flowers afflow) {
        lock (RoseLock) {
            try {
                RankRose.RemoveAll(x => x.Uid == afflow.Uid);
                if (!RankRose.Contains(afflow))
                    RankRose.Add(afflow);
                var data = RankRose.ToArray();

                Array.Sort(data, (c1, c2) => c2.RedRoses.CompareTo(c1.RedRoses));

                var room = data.ToArray();

                List<Flowers> backUpd = [];

                var x = 1;
                foreach (var flow in room) {
                    if (flow.RedRoses == 0) continue;
                    if (x < 100) {
                        flow.RankRoses = x;
                        backUpd.Add(flow);
                    }
                    else {
                        flow.RankRoses = 0;
                    }

                    x++;
                }

                lock (RedRousesTop100) {
                    RankRose = new List<Flowers>(backUpd);
                    RedRousesTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void CalculateRankLilies(Flowers afflow) {
        lock (LiliesLock) {
            Ranklili.RemoveAll(x => x.Uid == afflow.Uid);
            if (!Ranklili.Contains(afflow))
                Ranklili.Add(afflow);
            var data = Ranklili.ToArray();

            Array.Sort(data, (c1, c2) => c2.Lilies.CompareTo(c1.Lilies));

            var room = data.ToArray();
            List<Flowers> backUpd = [];

            var x = 1;
            foreach (var flow in room) {
                if (flow.Lilies == 0) continue;
                if (x < 100) {
                    flow.RankLilies = x;
                    backUpd.Add(flow);
                }
                else {
                    flow.RankLilies = 0;
                }

                x++;
            }

            lock (LiliesTop100) {
                Ranklili = new List<Flowers>(backUpd);
                LiliesTop100 = backUpd.ToArray();
            }
        }
    }

    public static void CalculateRankOrchids(Flowers afflow) {
        lock (OrchidsLock) {
            RankOrchid.RemoveAll(x => x.Uid == afflow.Uid);
            if (!RankOrchid.Contains(afflow))
                RankOrchid.Add(afflow);
            var data = RankOrchid.ToArray();

            Array.Sort(data, (c1, c2) => c2.Orchids.CompareTo(c1.Orchids));

            var room = data.ToArray();

            List<Flowers> backUpd = [];

            var x = 1;
            foreach (var flow in room) {
                if (flow.Orchids == 0) continue;
                if (x < 100) {
                    flow.RankOrchids = x;
                    backUpd.Add(flow);
                }
                else {
                    flow.RankOrchids = 0;
                }

                x++;
            }

            lock (OrchidsTop100) {
                RankOrchid = new List<Flowers>(backUpd);
                OrchidsTop100 = backUpd.ToArray();
            }
        }
    }

    public static void CalculateRankTulips(Flowers afflow) {
        lock (TulipsLock) {
            RankTulips.RemoveAll(x => x.Uid == afflow.Uid);
            if (!RankTulips.Contains(afflow))
                RankTulips.Add(afflow);
            var data = RankTulips.ToArray();

            Array.Sort(data, (c1, c2) => c2.Tulips.CompareTo(c1.Tulips));

            var room = data.ToArray();

            List<Flowers> backUpd = [];

            var x = 1;
            foreach (var flow in room) {
                if (flow.Tulips == 0) continue;
                if (x < 100) {
                    flow.RankTulops = x;
                    backUpd.Add(flow);
                }
                else {
                    flow.RankTulops = 0;
                }

                x++;
            }

            lock (TulipsTop100) {
                RankTulips = new List<Flowers>(backUpd);
                TulipsTop100 = backUpd.ToArray();
            }
        }
    }

    /// <summary>
    ///     Seeds girl flower Top100 from MySQL so offline players appear after a server restart.
    /// </summary>
    public static void RebuildGirlTop100FromDatabase() {
        HydrateFlowerBoardFromDb($"f.{FlowerSchema.Flowers.RedRoses} > 0", $"f.{FlowerSchema.Flowers.RedRoses}",
            RoseLock, ref RankRose,
            ref RedRousesTop100, (flow, r) => flow.RankRoses = r);
        HydrateFlowerBoardFromDb($"f.{FlowerSchema.Flowers.Lilies} > 0", $"f.{FlowerSchema.Flowers.Lilies}",
            LiliesLock, ref Ranklili,
            ref LiliesTop100, (flow, r) => flow.RankLilies = r);
        HydrateFlowerBoardFromDb($"f.{FlowerSchema.Flowers.Orchids} > 0", $"f.{FlowerSchema.Flowers.Orchids}",
            OrchidsLock, ref RankOrchid,
            ref OrchidsTop100, (flow, r) => flow.RankOrchids = r);
        HydrateFlowerBoardFromDb($"f.{FlowerSchema.Flowers.Tulips} > 0", $"f.{FlowerSchema.Flowers.Tulips}",
            TulipsLock, ref RankTulips,
            ref TulipsTop100, (flow, r) => flow.RankTulops = r);
        Console.WriteLine(
            $"Flower leaderboards (girl) loaded from DB: roses={RedRousesTop100.Length} lilies={LiliesTop100.Length} orchids={OrchidsTop100.Length} tulips={TulipsTop100.Length}");
        RebuildBoyFlowerLeaderboardsFromKissDatabase();
    }

    /// <summary>
    ///     Boy-side leaderboards stored on <see cref="Flowers" /> (kiss/love/wine/jade) for send-flow and PacketHandler.
    ///     Filled from <c>kisses</c> table — same source as <see cref="Kisses.KissesTop100" /> but different shape.
    /// </summary>
    private static void RebuildBoyFlowerLeaderboardsFromKissDatabase() {
        HydrateBoyFlowerBoardFromKisses($"k.{KissSchema.Kisses.KissesCount} > 0", $"k.{KissSchema.Kisses.KissesCount}",
            KissLock, ref RankKiss,
            ref KissTop100, (flow, record) => flow.RedRoses = record.Kisses, (flow, r) => flow.RankRoses = r);
        HydrateBoyFlowerBoardFromKisses($"k.{KissSchema.Kisses.Letters} > 0", $"k.{KissSchema.Kisses.Letters}",
            LoveLock, ref RankLove,
            ref LoveTop100, (flow, record) => flow.Lilies = record.Letters, (flow, r) => flow.RankLilies = r);
        HydrateBoyFlowerBoardFromKisses($"k.{KissSchema.Kisses.Wine} > 0", $"k.{KissSchema.Kisses.Wine}", WineLock,
            ref RankWine,
            ref WineTop100, (flow, record) => flow.Orchids = record.Wine, (flow, r) => flow.RankOrchids = r);
        HydrateBoyFlowerBoardFromKisses($"k.{KissSchema.Kisses.Jades} > 0", $"k.{KissSchema.Kisses.Jades}", JadeLock,
            ref RankJade,
            ref JadeTop100, (flow, record) => flow.Tulips = record.Jades, (flow, r) => flow.RankTulops = r);
    }

    private static void HydrateBoyFlowerBoardFromKisses(string whereNonZero, string orderByColumn, object gate,
        ref List<Flowers> rankList, ref Flowers[] top100, Action<Flowers, KissRecord> fillLeaderColumn,
        Action<Flowers, int> applyRank) {
        var rows = KissTable.LoadTop100WithNames(whereNonZero, orderByColumn);
        var list = new List<Flowers>();
        var rank = 1;
        foreach (var (record, name) in rows) {
            var flow = new Flowers(record.EntityId, name);
            fillLeaderColumn(flow, record);
            applyRank(flow, rank);
            list.Add(flow);
            rank++;
        }

        lock (gate) {
            rankList = list;
            top100 = list.ToArray();
        }
    }

    private static void HydrateFlowerBoardFromDb(string whereNonZero, string orderByColumn, object gate,
        ref List<Flowers> rankList, ref Flowers[] top100, Action<Flowers, int> applyRank) {
        var rows = FlowerTable.LoadTop100WithNames(whereNonZero, orderByColumn);
        var list = new List<Flowers>();
        var rank = 1;
        foreach (var (record, name) in rows) {
            var flow = FlowerFromRecord(record, name);
            applyRank(flow, rank);
            list.Add(flow);
            rank++;
        }

        lock (gate) {
            rankList = list;
            top100 = list.ToArray();
        }
    }

    private static Flowers FlowerFromRecord(FlowerRecord r, string name) {
        var flow = new Flowers(r.EntityId, name) {
            RedRoses = r.RedRoses,
            RedRosesToday = r.RedRosesToday,
            Lilies = r.Lilies,
            Lilies2day = r.LiliesToday,
            Orchids = r.Orchids,
            OrchidsToday = r.OrchidsToday,
            Tulips = r.Tulips,
            TulipsToday = r.TulipsToday,
            SendDay = r.SendDay,
            AFlower = r.AFlower
        };
        if (r.LastFlowerSent != 0)
            flow.LastFlowerSent = DateTime.FromBinary(r.LastFlowerSent);
        return flow;
    }
}