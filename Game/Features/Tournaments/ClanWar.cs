using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using MTA.Network.GamePackets;

namespace MTA.Game
{
    public class ClanWarArena
    {
        public class ClientWar
        {
            public uint UID;
            public uint Reward;
            public uint NextReward;
            public uint OccupationDays;
            public string Name = "None";
            public string CurentMap = "";
            public string DominationMap = "";
            public ClientWar(Clan clan)
            {
                UID = clan.ID;
                Name = clan.Name;

            }
            public ClientWar()
            {

            }
            public override string ToString()
            {
                if (Name.Contains('#'))
                    Name = Name.Replace("#", "");
                if (Name.Contains('^'))
                    Name = Name.Replace("^", "");
                StringBuilder build = new StringBuilder();
                build.Append(UID + "#" + Reward + "#" + NextReward
                    + "#" + OccupationDays + "#" + Name + "#" + CurentMap + "#" + DominationMap + "#");
                return build.ToString();
            }
            public void Load(string data)
            {
                if (data == null) return;
                if (!data.Contains('#')) return;
                string[] line = data.Split('#');
                UID = uint.Parse(line[0]);
                Reward = uint.Parse(line[1]);
                NextReward = uint.Parse(line[2]);
                OccupationDays = uint.Parse(line[3]);
                Name = line[4];
                CurentMap = line[5];
                DominationMap = line[6];
            }

        }
        public enum ClanArena : ushort
        {
            TwinCityClan = 0,
            PhoenixCastleClan = 1,
            ApeCityClan = 2,
            DesertCityClan = 3,
            BirdCityClan = 4,
            WindPlainClan = 5,
            MapleForestClan = 6,
            LoveCanyonClan = 7,
            DesertClan = 8,
            BirdIslandClan = 9,
            Count = 10
        }
        public static uint GetItemReward(ClanArena typ)
        {
            switch (typ)
            {
                case ClanArena.TwinCityClan:
                    return 722454;
                case ClanArena.WindPlainClan:
                    return 722455;
                case ClanArena.DesertCityClan:
                    return 722459;
                case ClanArena.DesertClan:
                    return 722460;
                case ClanArena.BirdCityClan:
                    return 722464;
                case ClanArena.BirdIslandClan:
                    return 722465;
                case ClanArena.ApeCityClan:
                    return 722469;
                case ClanArena.LoveCanyonClan:
                    return 722470;
                case ClanArena.PhoenixCastleClan:
                    return 722474;
                case ClanArena.MapleForestClan:
                    return 722475;
            }
            return 0;
        }
        public static ushort GetMap(ClanArena typ)
        {
            switch (typ)
            {
                case ClanArena.TwinCityClan:
                    return 1505;
                case ClanArena.WindPlainClan:
                    return 1555;
                case ClanArena.PhoenixCastleClan:
                    return 1509;
                case ClanArena.MapleForestClan:
                    return 1590;
                case ClanArena.DesertCityClan:
                    return 1508;
                case ClanArena.DesertClan:
                    return 1580;
                case ClanArena.BirdCityClan:
                    return 1507;
                case ClanArena.BirdIslandClan:
                    return 1570;
                case ClanArena.ApeCityClan:
                    return 1506;
                case ClanArena.LoveCanyonClan:
                    return 1560;
            }
            return 0;
        }
        private static ClanArena GetTournament(uint NpcUID)
        {
            if (NpcUID == 101895)
                return ClanArena.TwinCityClan;
            if (NpcUID == 101897)
                return ClanArena.WindPlainClan;
            if (NpcUID == 101901)
                return ClanArena.PhoenixCastleClan;
            if (NpcUID == 101902)
                return ClanArena.MapleForestClan;
            if (NpcUID == 101905)
                return ClanArena.ApeCityClan;
            if (NpcUID == 101906)
                return ClanArena.LoveCanyonClan;
            if (NpcUID == 101913)
                return ClanArena.DesertCityClan;
            if (NpcUID == 101914)
                return ClanArena.DesertClan;
            if (NpcUID == 101909)
                return ClanArena.BirdCityClan;
            if (NpcUID == 101910)
                return ClanArena.BirdIslandClan;
            return ClanArena.Count;
        }
        public static ClanTournament GetNpcTournament(uint NpcUID)
        {
            try
            {
                return Tournaments[(byte)GetTournament(NpcUID)];
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
                return null;
            }
        }
        public static ClanTournament[] Tournaments;
        public static void Create()
        {
            Tournaments = new ClanTournament[(byte)ClanArena.Count];
            for (byte x = 0; x < (byte)ClanArena.Count; x++)
                Tournaments[x] = new ClanTournament((ClanArena)x);
            Load();
            Subscriber = World.Subscribe(work, 1000);
        }
        private static IDisposable Subscriber;
        private static void work(int time)
        {
            DateTime Now64 = DateTime.Now;
            #region Clan War
            if (Matrix_Times.Start.ClanWarArena2 && Now64.Second == 0)
            {
                var name = new object[] { "ClanLeader Go to every map to ClanWar npc to Apply 5 Minute And closed Apply !?" };
                Kernel.SendWorldMessage(new Message(string.Concat(name), "ALLUSERS", "[ClanWar]", System.Drawing.Color.Red, Message.BroadcastMessage), Program.Values);

            }
            if (Matrix_Times.Start.ClanWarArena && Now64.Second == 0)
            {

                ClanWarArena.Start();
                var name = new object[] { "ClanWar has begun! Go to every map to ClanWar npc to join !?" };
                Kernel.SendWorldMessage(new Message(string.Concat(name), "ALLUSERS", "[ClanWar]", System.Drawing.Color.Red, Message.BroadcastMessage), Program.Values);
            }
            #endregion
        }
        public static bool GetMyWar(uint CLAN_ID, out ClientWar war)
        {
            foreach (var client_War in Tournaments)
                if (client_War.Client.UID == CLAN_ID)
                {
                    war = client_War.Client;
                    return true;
                }
            war = null;
            return false;
        }
        public static void Start()
        {

            for (byte x = 0; x < (byte)ClanArena.Count; x++)
                Tournaments[x].Start();
            Program.World.SendServerMessaj("ClanWar has begun! Go to every map to ClanWar npc to join !");
        }
        public static void Save()
        {
            using (Database.Write _wr = new Database.Write("database\\ClanWar.txt"))
            {
                string[] items = new string[(byte)ClanArena.Count];
                for (byte x = 0; x < Tournaments.Length; x++)
                    items[x] = "" + (byte)Tournaments[x].Map + "^" + Tournaments[x].Client.ToString();
                _wr.Add(items, items.Length).Execute(Database.Mode.Open);
            }
        }
        public static void Load()
        {
            using (Database.Read r = new Database.Read("database\\ClanWar.txt"))
            {
                if (r.Reader())
                {
                    int count = r.Count;
                    for (uint x = 0; x < count; x++)
                    {
                        string data = r.ReadString("");
                        if (data != null)
                        {
                            ClanArena tour = (ClanArena)byte.Parse(data.Split('^')[0]);
                            Tournaments[(byte)tour].Client.Load(data.Split('^')[1]);
                        }
                    }
                }
            }
        }
        public class ClanTournament
        {
            public const int MinuteTimes = 20;
            public ClientWar Client;
            private DateTime StartTimer;
            private IDisposable Subscribe;
            public ClanArena Map;
            public ClanTournament(ClanArena map)
            {
                if (!Constants.PKFreeMaps.Contains(GetMap(map)))
                    Constants.PKFreeMaps.Add(GetMap(map));
                Map = map;
                Client = new ClientWar();
                Client.DominationMap = Client.CurentMap = Map.ToString();
                Client.Reward = Client.NextReward = GetItemReward(Map);
            }
            public void Teleport(Client.GameState client)
            {
                if (Open)
                {
                    var telemap = Kernel.Maps[GetMap(Map)];
                    var mapcoord = telemap.RandomCoordinates();
                    client.Entity.Teleport(GetMap(Map), mapcoord.Item1, mapcoord.Item2);
                }
            }
            public bool Open = false;
            public void Start()
            {
                if (!Open)
                {
                    StartTimer = DateTime.Now;
                    Open = true;
                    Subscribe = World.Subscribe(Work, 5000);

                }
            }
            private void Finish()
            {
                if (Open)
                {
                    if (DateTime.Now > StartTimer.AddMinutes(MinuteTimes))
                    {
                        Open = false;
                        Subscribe.Dispose();
                    }
                }
            }
            private int ClansOnMap(Client.GameState[] clients)
            {
                List<uint> ClansIDs = new List<uint>();
                foreach (var obj in clients)
                {
                    if (obj.Entity.ClanId != 0)
                    {
                        if (!ClansIDs.Contains(obj.Entity.ClanId))
                            ClansIDs.Add(obj.Entity.ClanId);
                    }
                }

                return ClansIDs.Count;
            }
            private void Work(int time)
            {
                var clients_on_MAP = Kernel.GamePool.Values.Where(p => p.Entity.MapID == GetMap(Map) && !p.Entity.Dead).ToArray();
                int count_clans = ClansOnMap(clients_on_MAP);
                byte[] Messaje = new Network.GamePackets.Message("Alive Clans In " + Map.ToString() + " : " + count_clans + "", System.Drawing.Color.Yellow, Network.GamePackets.Message.FirstRightCorner).ToArray();
                foreach (var obj in clients_on_MAP)
                    obj.Send(Messaje);
                if (DateTime.Now > StartTimer.AddMinutes(MinuteTimes))
                {
                    if (count_clans < 2)
                    {
                        if (clients_on_MAP != null && clients_on_MAP.Length > 0)
                        {
                            var clan = clients_on_MAP[0].Entity.GetClan;
                            foreach (var member in clients_on_MAP)
                                GetClientReward(member);
                            UpdateWarInfo(clan);
                        }
                        Finish();
                    }

                }
            }
            private void UpdateWarInfo(Clan clan)
            {
                if (Client.UID == clan.ID)
                {
                    Client.OccupationDays++;
                }
                else
                {
                    Client = new ClientWar(clan);
                    Client.OccupationDays++;
                    Client.DominationMap = Client.CurentMap = Map.ToString();
                    Client.Reward = Client.NextReward = GetItemReward(Map);
                }
            }
            private void GetClientReward(Client.GameState obj)
            {

                uint Reward = 3000;
                if (Map != ClanArena.TwinCityClan)
                    Reward /= 2;
                if (obj.Entity.ClanRank == Clan.Ranks.ClanLeader)
                {
                    obj.Entity.ConquerPoints += Reward;
                    obj.Send(new Network.GamePackets.Message("You win " + Reward + " ConquerPoints for domination " + Map.ToString() + "", System.Drawing.Color.Red, Network.GamePackets.Message.System).ToArray());
                }
                else
                {
                    Reward /= 3;
                    obj.Entity.ConquerPoints += Reward;
                    obj.Send(new Network.GamePackets.Message("You win " + Reward + " ConquerPoints for domination " + Map.ToString() + "", System.Drawing.Color.Red, Network.GamePackets.Message.System).ToArray());
                }
                obj.Entity.GetClan.CalnWar = false;
                obj.Entity.Teleport(1002, 303, 278);
            }
        }
    }
}
