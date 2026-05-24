using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Game.Features.Kisses.Database;
using MTA.Game.Features.Kisses.Database.Models;
using MTA.Game.Features.Kisses.Database.Schema;

namespace MTA.Game.Features.Kisses;

public class Kisses {
    // Static ranking lists
    public static Kisses[] KissesTop100 = [];
    public static Kisses[] LettersTop100 = [];
    public static Kisses[] WineTop100 = [];
    public static Kisses[] JadesTop100 = [];

    // Static locks for thread safety
    public static object KissesLock = new();
    public static object LettersLock = new();
    public static object WineLock = new();
    public static object JadesLock = new();

    // Static ranking lists
    public static List<Kisses> RankKiss = [];
    public static List<Kisses> RankLetter = [];
    public static List<Kisses> RankWineList = [];
    public static List<Kisses> RankJade = [];

    // Legacy lists (kept for compatibility)
    public static List<ListKissRank> Kiss2 = [];
    public static List<ListKissRank> Wine2 = [];
    public static List<ListKissRank> Letters2 = [];
    public static List<ListKissRank> Jades2 = [];
    public static List<ListKissRank> KissesToday = [];
    public static List<ListKissRank> JadeToday = [];

    public uint id;
    public bool Jadestoday = false;
    public bool Jadestoday2;
    public bool Jadestoday3;
    public bool Jadestoday4;
    public SafeDictionary<uint, Kisses> Kiss = new(1000);
    public bool kissestoday = false;
    public bool kissestoday2;
    public bool kissestoday3;
    public bool kissestoday4;
    public bool letterstoday = false;
    public bool letterstoday2;
    public bool letterstoday3;
    public bool letterstoday4;
    public bool winetoday = false;
    public bool winetoday2;
    public bool winetoday3;
    public bool winetoday4;

    public Kisses() { }

    public Kisses(uint uid, string name) {
        Uid = uid;
        this.name = name;
    }

    public DateTime LastKissesSent { get; set; }

    public string name { get; set; }

    public uint Letters { get; set; }

    public uint LetterToday { get; set; }

    public uint Wine { get; set; }

    public uint WineToday { get; set; }

    public uint Count { get; set; }

    public uint TodayCount { get; set; }

    public uint Jades { get; set; }

    public uint JadesToday { get; set; }

    // Ranking properties
    public int RankKisses { get; set; }
    public int RankLetters { get; set; }
    public int RankWine { get; set; }
    public int RankJades { get; set; }

    public uint Uid { get; set; }

    public int SendScreenValue(KissType typ, int rak) {
        if (rak == 0 || rak > 100)
            return 0;
        return (int)(30000402u + (uint)(100 * (byte)typ) + GetRank(rak));
    }

    public ushort GetRank(int rank) {
        switch (rank) {
            case 1: return 1;
            case 2: return 2;
            case 3: return 3;
            case >= 4 and <= 10: return 4;
            case >= 11 and <= 20: return 5;
            case >= 21 and <= 30: return 6;
            case >= 31 and <= 40: return 7;
            case >= 41 and <= 50: return 8;
            case >= 51 and <= 60: return 9;
            case >= 61 and <= 70: return 10;
            case >= 71 and <= 80: return 11;
            case >= 81 and <= 90: return 12;
            case >= 91 and <= 100: return 13;
            default: return 0;
        }
    }

    public static void CalculateRankKisses(Kisses akiss) {
        lock (KissesLock) {
            try {
                RankKiss.RemoveAll(x => x.Uid == akiss.Uid);
                if (!RankKiss.Contains(akiss))
                    RankKiss.Add(akiss);
                var data = RankKiss.ToArray();

                Array.Sort(data, (c1, c2) => c2.Count.CompareTo(c1.Count));

                var room = data.ToArray();

                List<Kisses> backUpd = [];

                var x = 1;
                foreach (var kiss in room) {
                    if (kiss.Count == 0) continue;
                    if (x < 100) {
                        kiss.RankKisses = x;
                        backUpd.Add(kiss);
                    }
                    else {
                        kiss.RankKisses = 0;
                    }

                    x++;
                }

                lock (KissesTop100) {
                    RankKiss = new List<Kisses>(backUpd);
                    KissesTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void CalculateRankLetters(Kisses akiss) {
        lock (LettersLock) {
            try {
                RankLetter.RemoveAll(x => x.Uid == akiss.Uid);
                if (!RankLetter.Contains(akiss))
                    RankLetter.Add(akiss);
                var data = RankLetter.ToArray();

                Array.Sort(data, (c1, c2) => c2.Letters.CompareTo(c1.Letters));

                var room = data.ToArray();

                List<Kisses> backUpd = [];

                var x = 1;
                foreach (var kiss in room) {
                    if (kiss.Letters == 0) continue;
                    if (x < 100) {
                        kiss.RankLetters = x;
                        backUpd.Add(kiss);
                    }
                    else {
                        kiss.RankLetters = 0;
                    }

                    x++;
                }

                lock (LettersTop100) {
                    RankLetter = new List<Kisses>(backUpd);
                    LettersTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void CalculateRankWine(Kisses akiss) {
        lock (WineLock) {
            try {
                RankWineList.RemoveAll(x => x.Uid == akiss.Uid);
                if (!RankWineList.Contains(akiss))
                    RankWineList.Add(akiss);
                var data = RankWineList.ToArray();

                Array.Sort(data, (c1, c2) => c2.Wine.CompareTo(c1.Wine));

                var room = data.ToArray();

                List<Kisses> backUpd = [];

                var x = 1;
                foreach (var kiss in room) {
                    if (kiss.Wine == 0) continue;
                    if (x < 100) {
                        kiss.RankWine = x;
                        backUpd.Add(kiss);
                    }
                    else {
                        kiss.RankWine = 0;
                    }

                    x++;
                }

                lock (WineTop100) {
                    RankWineList = new List<Kisses>(backUpd);
                    WineTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public static void CalculateRankJades(Kisses akiss) {
        lock (JadesLock) {
            try {
                RankJade.RemoveAll(x => x.Uid == akiss.Uid);
                if (!RankJade.Contains(akiss))
                    RankJade.Add(akiss);
                var data = RankJade.ToArray();

                Array.Sort(data, (c1, c2) => c2.Jades.CompareTo(c1.Jades));

                var room = data.ToArray();

                List<Kisses> backUpd = [];

                var x = 1;
                foreach (var kiss in room) {
                    if (kiss.Jades == 0) continue;
                    if (x < 100) {
                        kiss.RankJades = x;
                        backUpd.Add(kiss);
                    }
                    else {
                        kiss.RankJades = 0;
                    }

                    x++;
                }

                lock (JadesTop100) {
                    RankJade = new List<Kisses>(backUpd);
                    JadesTop100 = backUpd.ToArray();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }

    /// <summary>
    ///     Seeds kiss Top100 from MySQL so offline players appear after a server restart.
    /// </summary>
    public static void RebuildTop100FromDatabase() {
        HydrateKissBoardFromDb($"k.{KissSchema.Kisses.KissesCount} > 0", $"k.{KissSchema.Kisses.KissesCount}",
            KissesLock, ref RankKiss,
            ref KissesTop100, (k, r) => k.RankKisses = r);
        HydrateKissBoardFromDb($"k.{KissSchema.Kisses.Letters} > 0", $"k.{KissSchema.Kisses.Letters}", LettersLock,
            ref RankLetter,
            ref LettersTop100, (k, r) => k.RankLetters = r);
        HydrateKissBoardFromDb($"k.{KissSchema.Kisses.Wine} > 0", $"k.{KissSchema.Kisses.Wine}", WineLock,
            ref RankWineList,
            ref WineTop100, (k, r) => k.RankWine = r);
        HydrateKissBoardFromDb($"k.{KissSchema.Kisses.Jades} > 0", $"k.{KissSchema.Kisses.Jades}", JadesLock,
            ref RankJade,
            ref JadesTop100, (k, r) => k.RankJades = r);
        Console.WriteLine(
            $"Kiss leaderboards loaded from DB: kiss={KissesTop100.Length} letters={LettersTop100.Length} wine={WineTop100.Length} jades={JadesTop100.Length}");
    }

    private static void HydrateKissBoardFromDb(string whereNonZero, string orderByColumn, object gate,
        ref List<Kisses> rankList, ref Kisses[] top100, Action<Kisses, int> applyRank) {
        var rows = KissTable.LoadTop100WithNames(whereNonZero, orderByColumn);
        var list = new List<Kisses>();
        var rank = 1;
        foreach (var (record, name) in rows) {
            var k = KissFromRecord(record, name);
            applyRank(k, rank);
            list.Add(k);
            rank++;
        }

        lock (gate) {
            rankList = list;
            top100 = list.ToArray();
        }
    }

    private static Kisses KissFromRecord(KissRecord r, string name) {
        var k = new Kisses(r.EntityId, name) {
            Count = r.Kisses,
            TodayCount = r.KissesToday,
            Letters = r.Letters,
            LetterToday = r.LettersToday,
            Wine = r.Wine,
            WineToday = r.WineToday,
            Jades = r.Jades,
            JadesToday = r.JadesToday
        };
        if (r.LastKissesSent != 0)
            k.LastKissesSent = DateTime.FromBinary(r.LastKissesSent);
        return k;
    }

    public struct ListKissRank {
        public string name;
        public uint Kisses;
        public uint Wine;
        public uint Letter;
        public uint Jade;
        public int rank;
        public short body;
        public uint uid;
    }
}