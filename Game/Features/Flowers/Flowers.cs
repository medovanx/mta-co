using MTA.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace MTA.Game.Features.Flowers
{
    public enum Effect : byte
    {
        None = 0,
        Rouse = 1,
        Lilies = 2,
        Orchids = 3,
        Tulips = 4,
        Kiss = 1,
        love = 2,
        Tins = 3,
        Jade = 4,
    }

    public enum FlowersT : byte
    {
        Rouse = 0,
        Lilies = 1,
        Orchids = 2,
        Tulips = 3,
        Kiss = 4,
        love = 5,
        Tins = 6,
        Jade = 7,
    }

    public class Flowers
    {

        public Flowers() { }
        public Flowers(uint _UID, string name)
        {
            UID = _UID;
            Name = name;
        }
        public uint UID;

        public uint aFlower = 1;
        public uint SendDay = 0;

        public string Name = "";

        public int SendScreenValue(FlowersT typ, int rak)
        {
            if (rak == 0 || rak > 100)
                return 0;
            return (int)(30000002 + (uint)(100 * (byte)typ) + GetRank(rak));
        }
        public ushort GetRank(int rank)
        {
            if (rank == 1)
                return 0;
            if (rank == 2)
                return 10000;
            if (rank == 3)
                return 20000;
            if (rank > 3)
                return 30000;

            return 0;
        }
        public int BoySendScreenValue(FlowersT typ, int rak)
        {
            int ret = 0;
            if (rak == -1) return 0;
            if (rak > 100) return 0;

            ret = (int)(30000402 + (uint)(100 * (byte)typ));

            return ret;
        }

        public int RankRoses = 0;
        public int RankLilies = 0;//max 10 start with -1.
        public int RankOrchids = 0;//max 10 start with -1.
        public int RankTuilps = 0;//max 10 start with -1.

        public uint RedRoses;//kiss
        public uint RedRoses2day;
        public uint Lilies;//love
        public uint Lilies2day;
        public uint Orchads;//wine
        public uint Orchads2day;
        public uint Tulips;//jade
        public uint Tulips2day;

        private DateTime _LastFlowerSent;
        public DateTime LastFlowerSent
        {
            get
            {
                return this._LastFlowerSent;
            }
            set
            {
                this._LastFlowerSent = value;
            }
        }

        public override string ToString()
        {
            return UID + "[]"
                + Name + "[]"
                + RedRoses + "[]"
                + RedRoses2day + "[]"
                + Lilies + "[]"
                + Lilies2day + "[]"
                + Orchads + "[]"
                + Orchads2day + "[]"
                + Tulips + "[]"
                + Tulips2day + "[]"
                + SendDay + "[]" + aFlower + "[]" + 0 + "[]" + 0 + "[]" + 0 + "[]" + 0 + "[]"
                + 0 + "[]";
        }

        public void Read(string aLine)
        {
            if (aLine == "" || aLine == null)
                return;
            string[] line = aLine.Split(new string[] { "[]" }, StringSplitOptions.RemoveEmptyEntries);
            UID = uint.Parse(line[0]);
            Name = line[1];
            RedRoses = uint.Parse(line[2]);
            RedRoses2day = uint.Parse(line[3]);
            Lilies = uint.Parse(line[4]);
            Lilies2day = uint.Parse(line[5]);
            Orchads = uint.Parse(line[6]);
            Orchads2day = uint.Parse(line[7]);
            Tulips = uint.Parse(line[8]);
            Tulips2day = uint.Parse(line[9]);
            SendDay = uint.Parse(line[10]);
            aFlower = uint.Parse(line[11]);

            Reset();
        }

        public void Reset()
        {
            if (SendDay != DateTime.Now.Day)
            {
                RedRoses2day = Lilies2day = Orchads2day = Tulips2day = 0;
                aFlower = 1;
                SendDay = (uint)DateTime.Now.Day;
            }
        }

        public static ConcurrentDictionary<uint, Flowers> Flowers_Poll = new ConcurrentDictionary<uint, Flowers>();

        public static ConcurrentDictionary<uint, Flowers> BoyFlowers = new ConcurrentDictionary<uint, Flowers>();

        public static Flowers[] KissTop100 = new Flowers[0];
        public static Flowers[] LoveTop100 = new Flowers[0];
        public static Flowers[] TineTop100 = new Flowers[0];
        public static Flowers[] JadeTop100 = new Flowers[0];

        public static Flowers[] RedRousesTop100 = new Flowers[0];
        public static Flowers[] LiliesTop100 = new Flowers[0];
        public static Flowers[] OrchidsTop100 = new Flowers[0];
        public static Flowers[] TulipsTop100 = new Flowers[0];

        public static object RouseLock = new object();
        public static object LilisLock = new object();
        public static object OrchidsLock = new object();
        public static object TulipsLock = new object();

        public static object KissLock = new object();
        public static object LoveLock = new object();
        public static object TineLock = new object();
        public static object JadeLock = new object();

        public static List<Flowers> RankKiss = new List<Flowers>();
        public static List<Flowers> RankLove = new List<Flowers>();
        public static List<Flowers> RankTine = new List<Flowers>();
        public static List<Flowers> RankJade = new List<Flowers>();

        public static List<Flowers> Rankrose = new List<Flowers>();
        public static List<Flowers> Ranklili = new List<Flowers>();
        public static List<Flowers> Rankorchid = new List<Flowers>();
        public static List<Flowers> RankTulips = new List<Flowers>();

        public static void CulculateRankJade(Flowers afflow)
        {
            lock (JadeLock)
            {
                try
                {
                    if (!RankJade.Contains(afflow))
                        RankJade.Add(afflow);
                    var data = RankJade.ToArray();

                    Array.Sort(data, (c1, c2) => { return c2.Tulips.CompareTo(c1.Tulips); });

                    var room = data.ToArray();

                    List<Flowers> backUpd = new List<Flowers>();

                    int x = 1;
                    foreach (Flowers flow in room)
                    {
                        if (flow.Tulips == 0) continue;
                        if (x < 100)
                        {
                            flow.RankTuilps = x;
                            backUpd.Add(flow);
                        }
                        else
                            flow.RankTuilps = 0;
                        x++;
                    }
                    lock (JadeTop100)
                    {
                        RankJade = new List<Flowers>(backUpd);
                        JadeTop100 = backUpd.ToArray();
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
        public static void CulculateRankTine(Flowers afflow)
        {
            lock (TineLock)
            {
                try
                {
                    if (!RankTine.Contains(afflow))
                        RankTine.Add(afflow);
                    var data = RankTine.ToArray();

                    Array.Sort(data, (c1, c2) => { return c2.Orchads.CompareTo(c1.Orchads); });

                    var room = data.ToArray();

                    List<Flowers> backUpd = new List<Flowers>();

                    int x = 1;
                    foreach (Flowers flow in room)
                    {
                        if (flow.Orchads == 0) continue;
                        if (x < 100)
                        {
                            flow.RankOrchids = x;
                            backUpd.Add(flow);
                        }
                        else
                            flow.RankOrchids = 0;
                        x++;
                    }
                    lock (TineTop100)
                    {
                        RankTine = new List<Flowers>(backUpd);
                        TineTop100 = backUpd.ToArray();
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
        public static void CulculateRankLove(Flowers afflow)
        {
            lock (LoveLock)
            {
                try
                {
                    if (!RankLove.Contains(afflow))
                        RankLove.Add(afflow);
                    var data = RankLove.ToArray();

                    Array.Sort(data, (c1, c2) => { return c2.Lilies.CompareTo(c1.Lilies); });

                    var room = data.ToArray();

                    List<Flowers> backUpd = new List<Flowers>();

                    int x = 1;
                    foreach (Flowers flow in room)
                    {
                        if (flow.Lilies == 0) continue;
                        if (x < 100)
                        {
                            flow.RankLilies = x;
                            backUpd.Add(flow);
                        }
                        else
                            flow.RankLilies = 0;
                        x++;
                    }
                    lock (LoveTop100)
                    {
                        RankLove = new List<Flowers>(backUpd);
                        LoveTop100 = backUpd.ToArray();
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }

        public static void CulculateRankKiss(Flowers afflow)
        {
            lock (KissLock)
            {
                try
                {
                    if (!RankKiss.Contains(afflow))
                        RankKiss.Add(afflow);
                    var data = RankKiss.ToArray();

                    Array.Sort(data, (c1, c2) => { return c2.RedRoses.CompareTo(c1.RedRoses); });

                    var room = data.ToArray();

                    List<Flowers> backUpd = new List<Flowers>();

                    int x = 1;
                    foreach (Flowers flow in room)
                    {
                        if (flow.RedRoses == 0) continue;
                        if (x < 100)
                        {
                            flow.RankRoses = x;
                            backUpd.Add(flow);
                        }
                        else
                            flow.RankRoses = 0;
                        x++;
                    }
                    lock (KissTop100)
                    {
                        RankKiss = new List<Flowers>(backUpd);
                        KissTop100 = backUpd.ToArray();
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }

        public static void CulculateRankRouse(Flowers afflow)
        {
            lock (RouseLock)
            {
                try
                {
                    if (!Rankrose.Contains(afflow))
                        Rankrose.Add(afflow);
                    var data = Rankrose.ToArray();

                    Array.Sort(data, (c1, c2) => { return c2.RedRoses.CompareTo(c1.RedRoses); });

                    var room = data.ToArray();

                    List<Flowers> backUpd = new List<Flowers>();

                    int x = 1;
                    foreach (Flowers flow in room)
                    {
                        if (flow.RedRoses == 0) continue;
                        if (x < 100)
                        {
                            flow.RankRoses = x;
                            backUpd.Add(flow);
                        }
                        else
                        {
                            flow.RankRoses = 0;
                        }

                        x++;
                    }
                    lock (RedRousesTop100)
                    {
                        Rankrose = new List<Flowers>(backUpd);
                        RedRousesTop100 = backUpd.ToArray();
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
        public static void CulculateRankLilies(Flowers afflow)
        {
            lock (LilisLock)
            {
                if (!Ranklili.Contains(afflow))
                    Ranklili.Add(afflow);
                var data = Ranklili.ToArray();

                Array.Sort(data, (c1, c2) => { return c2.Lilies.CompareTo(c1.Lilies); });

                var room = data.ToArray();
                List<Flowers> backUpd = new List<Flowers>();

                int x = 1;
                foreach (Flowers flow in room)
                {
                    if (flow.Lilies == 0) continue;
                    if (x < 100)
                    {
                        flow.RankLilies = x;
                        backUpd.Add(flow);
                    }
                    else
                    {
                        flow.RankLilies = 0;
                    }

                    x++;
                }
                lock (LiliesTop100)
                {
                    Ranklili = new List<Flowers>(backUpd);
                    LiliesTop100 = backUpd.ToArray();
                }
            }
        }

        public static void CulculateRankOrchids(Flowers afflow)
        {
            lock (OrchidsLock)
            {
                if (!Rankorchid.Contains(afflow))
                    Rankorchid.Add(afflow);
                var data = Rankorchid.ToArray();

                Array.Sort(data, (c1, c2) => { return c2.Orchads.CompareTo(c1.Orchads); });

                var room = data.ToArray();

                List<Flowers> backUpd = new List<Flowers>();

                int x = 1;
                foreach (Flowers flow in room)
                {
                    if (flow.Orchads == 0) continue;
                    if (x < 100)
                    {
                        flow.RankOrchids = x;
                        backUpd.Add(flow);
                    }
                    else
                    {
                        flow.RankOrchids = 0;
                    }

                    x++;
                }
                lock (OrchidsTop100)
                {
                    Rankorchid = new List<Flowers>(backUpd);
                    OrchidsTop100 = backUpd.ToArray();
                }
            }
        }

        public static void CulculateRankTulips(Flowers afflow)
        {
            lock (TulipsLock)
            {
                if (!RankTulips.Contains(afflow))
                    RankTulips.Add(afflow);
                var data = RankTulips.ToArray();

                Array.Sort(data, (c1, c2) => { return c2.Tulips.CompareTo(c1.Tulips); });

                var room = data.ToArray();

                List<Flowers> backUpd = new List<Flowers>();

                int x = 1;
                foreach (Flowers flow in room)
                {
                    if (flow.Tulips == 0) continue;
                    if (x < 100)
                    {
                        flow.RankTuilps = x;
                        backUpd.Add(flow);
                    }
                    else
                    {
                        flow.RankTuilps = 0;
                    }

                    x++;
                }
                lock (TulipsTop100)
                {
                    RankTulips = new List<Flowers>(backUpd);
                    TulipsTop100 = backUpd.ToArray();
                }
            }
        }

        // Legacy members for compatibility
        public SafeDictionary<uint, Flowers> flower = new SafeDictionary<uint, Flowers>(1000);
        public static List<ListFlowerRank> Redrosse = new List<ListFlowerRank>();
        public static List<ListFlowerRank> Orchides = new List<ListFlowerRank>();
        public static List<ListFlowerRank> Lilise = new List<ListFlowerRank>();
        public static List<ListFlowerRank> Tuplise = new List<ListFlowerRank>();
        public static List<ListFlowerRank> RedrosseToday = new List<ListFlowerRank>();
        public static List<ListFlowerRank> OrchidesToday = new List<ListFlowerRank>();
        public static List<ListFlowerRank> LiliseToday = new List<ListFlowerRank>();
        public static List<ListFlowerRank> TupliseToday = new List<ListFlowerRank>();
        public bool liliestoday = false;
        public bool orchadstoday = false;
        public bool tulpistoday = false;
        public bool redroesstoday = false;
        public bool liliestoday2;
        public bool orchadstoday2;
        public bool tulpistoday2;
        public bool redroesstoday2;
        public bool liliestoday3;
        public bool orchadstoday3;
        public bool tulpistoday3;
        public bool redroesstoday3;
        public bool liliestoday4;
        public bool orchadstoday4;
        public bool tulpistoday4;
        public bool redroesstoday4;
        public struct ListFlowerRank
        {
            public string name;
            public uint redrosse;
            public uint orchides;
            public uint lilise;
            public uint tuplise;
            public int rank;
            public short body;
            public uint uid;
        }
        public uint id;
    }
}
