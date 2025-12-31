using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MTA.Game.Events.GuildWar;
using MTA.Network.GamePackets;
using MTA.Network;
using System.IO;
using System.Text;
using System.Linq;
using MTA.Client;
using static MTA.Game.Constants.EntityClass;

namespace MTA.Game.ConquerStructures.Society {
    public enum ArsenalType {
        Headgear,
        Armor,
        Weapon,
        Ring,
        Boots,
        Necklace,
        Fan,
        Tower
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum GuildRank {
        Agent = 590,
        Aide = 0x25a,
        ArsenalAgent = 0x254,
        ArsFollower = 0x1f0,
        ASupervisor = 0x358,
        CPAgent = 0x255,
        CPFollower = 0x1f1,
        CPSupervisor = 0x359,
        DeputyLeader = 990,
        DeputySteward = 650,
        DLeaderAide = 0x263,
        DLeaderSpouse = 620,
        Follower = 490,
        GSupervisor = 0x356,
        GuideAgent = 0x252,
        GuideFollower = 0x1ee,
        GuildLeader = 0x3e8,
        HDeputyLeader = 980,
        HonoraryManager = 880,
        HonorarySteward = 680,
        HonorarySuperv = 840,
        LeaderSpouse = 920,
        LilyAgent = 0x24f,
        LilyFollower = 0x1eb,
        LilySupervisor = 0x353,
        LSpouseAide = 610,
        Manager = 890,
        ManagerAide = 510,
        ManagerSpouse = 520,
        Member = 200,
        None = 0,
        OrchidAgent = 0x256,
        OrchidFollower = 0x1f2,
        OSupervisor = 0x35a,
        PKAgent = 0x251,
        PKFollower = 0x1ed,
        PKSupervisor = 0x355,
        RoseAgent = 0x250,
        RoseFollower = 0x1ec,
        RoseSupervisor = 0x354,
        SeniorMember = 210,
        SilverAgent = 0x253,
        SilverFollower = 0x1ef,
        SSupervisor = 0x357,
        Steward = 690,
        StewardSpouse = 420,
        Supervisor = 850,
        SupervisorAide = 0x1ff,
        SupervSpouse = 0x209,
        TSupervisor = 0x35b,
        TulipAgent = 0x257,
        TulipFollower = 0x1f3
    }

    public class Guild : Writer {
        public abstract class Advertise {
            private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, Guild> AGuilds = new();

            public static Guild[] AdvertiseRanks = [];

            public static void Add(Guild obj) {
                if (!AGuilds.ContainsKey(obj.ID))
                    AGuilds.TryAdd(obj.ID, obj);
                CalculateRanks();
            }

            private static void CalculateRanks() {
                lock (AdvertiseRanks) {
                    var array = AGuilds.Values.ToArray();
                    array =
                        (from guil in array orderby guil.AdvertiseRecruit.Donations descending select guil).ToArray();
                    List<Guild> listarray = [];
                    for (ushort x = 0; x < array.Length; x++) {
                        listarray.Add(array[x]);
                        if (x == 40) break;
                    }

                    AdvertiseRanks = listarray.ToArray();
                }
            }

            public static void FixedRank() {
                AGuilds.Clear();
                foreach (var guil in AdvertiseRanks) {
                    AGuilds.TryAdd(guil.ID, guil);
                }
            }
        }

        public class Recruitment {
            public enum Mode {
                Requirements,
                Recruit
            }

            private abstract class Flags {
                public const int
                    NoneBlock = 0,
                    Trojan = 1,
                    Warrior = 2,
                    Taoist = 4,
                    Archer = 8,
                    Ninja = 16,
                    Monk = 32,
                    Pirate = 64;
            }

            public bool AutoJoin = true;
            public string Buletin = "Nothing";
            public int NotAllowFlag;
            public byte Level;
            public byte Reborn;
            public byte Grade;
            public ulong Donations;

            public bool ContainFlag(int val) {
                return (NotAllowFlag & val) == val;
            }

            public void AddFlag(int val) {
                if (!ContainFlag(val))
                    NotAllowFlag |= val;
            }

            public void Remove(int val) {
                if (ContainFlag(val))
                    NotAllowFlag &= ~val;
            }

            public void SetFlag(int mFlag, Mode mod) {
                switch (mod) {
                    case Mode.Requirements: {
                        switch (mFlag) {
                            case 0:
                                NotAllowFlag = Flags.NoneBlock;
                                break;
                            case >= 127:
                                AddFlag(Flags.Trojan | Flags.Warrior | Flags.Taoist | Flags.Archer | Flags.Ninja |
                                        Flags.Monk | Flags.Pirate);
                                break;
                        }

                        var nFlag = 127 - mFlag;
                        AddFlag(nFlag);
                        break;
                    }
                    case Mode.Recruit: {
                        if (mFlag == 0) NotAllowFlag = Flags.NoneBlock;
                        AddFlag(mFlag);
                        break;
                    }
                }
            }

            public bool Compare(Entity player, Mode mod) {
                if (player.Level < Level)
                    return false;
                if (player.Reborn < Reborn && Reborn != 0)
                    return false;
                if (IsArcher(player.Class) && ContainFlag(Flags.Archer))
                    return false;
                if (IsTaoist(player.Class) && ContainFlag(Flags.Taoist))
                    return false;
                if (IsWarrior(player.Class) && ContainFlag(Flags.Warrior))
                    return false;
                if (IsTrojan(player.Class) && ContainFlag(Flags.Trojan))
                    return false;
                if (IsPirate(player.Class) && ContainFlag(Flags.Pirate))
                    return false;
                if (IsMonk(player.Class) && ContainFlag(Flags.Monk))
                    return false;
                if (IsNinja(player.Class) && ContainFlag(Flags.Ninja))
                    return false;
                if (mod != Mode.Recruit) return true;
                return Grade == 0 || true;
            }

            public override string ToString() {
                var build = new StringBuilder();
                build.Append(NotAllowFlag + "^" + Level + "^" + Reborn + "^" + Grade + "^" + Donations + "^"
                             + (byte)(AutoJoin ? 1 : 0) + "^" + Buletin + "^0" + "^0");
                return build.ToString();
            }

            public bool WasLoad;

            public void Load(string line) {
                if (line == "") return;
                if (!line.Contains('^')) return;
                var data = line.Split('^');
                NotAllowFlag = int.Parse(data[0]);
                Level = byte.Parse(data[1]);
                Reborn = byte.Parse(data[2]);
                Grade = byte.Parse(data[3]);
                Donations = ulong.Parse(data[4]);
                AutoJoin = byte.Parse(data[5]) == 1;
                Buletin = data[6];
                WasLoad = true;
            }

            public void Save() { }
        }


        public Member[] RankSilversDonations = [];
        public Member[] RankArsenalDonations = [];
        public Member[] RankCPDonations = [];
        public Member[] RankPkDonations = [];
        public Member[] RankLiliesDonations = [];
        public Member[] RankOrchidsDonations = [];
        public Member[] RankRosseDonations = [];
        public Member[] RankTulipsDonations = [];
        public Member[] RankGuideDonations = [];
        public Member[] RankTotalDonations = [];

        public ushort[] RanksCounts = new ushort[(ushort)Enums.GuildMemberRank.GuildLeader + 1];

        private abstract class ClassRequirements {
            public const uint
                Trojan = 1,
                Warrior = 2,
                Taoist = 4,
                Archer = 8,
                Ninja = 16,
                Monk = 32,
                Pirate = 64;
        }

        public Arsenal[] Arsenals;
        public List<uint> BlackList = [];
        public bool ArsenalBpChanged = true;

        public int UnlockedArsenals {
            get {
                var unlocked = 0;
                for (var i = 0; i < 8; i++)
                    if (Arsenals[i].Unlocked)
                        unlocked++;
                return unlocked;
            }
        }

        public uint GetCurrentArsenalCost() {
            var val = UnlockedArsenals;
            if (val is >= 0 and <= 1)
                return 5000000;
            else if (val is >= 2 and <= 4)
                return 10000000;
            else if (val is >= 5 and <= 6)
                return 15000000;
            else
                return 20000000;
        }

        private int _arsenalBp;

        public override int GetHashCode() {
            return (int)ID;
        }

        public int ArsenalTotalBattlepower {
            get => _arsenalBp;
            set {
                _arsenalBp = value;
                foreach (var member in Members.Values.Where(member => member.IsOnline)) {
                    member.Client.Entity.GuildBattlePower = GetSharedBattlepower(member.Rank);
                }
            }
        }

        public int GetMaxSharedBattlepower(bool force = false) {
            if (!ArsenalBpChanged && !force) return _arsenalBp;
            var aBp = 0;
            var arsenals = Arsenals.OrderByDescending(p => p.TotalSharedBattlePower);
            var a = 0;
            foreach (var arsenal in arsenals) {
                if (a == 5) break;
                aBp += (int)(arsenal.TotalSharedBattlePower);
                a++;
            }

            ArsenalTotalBattlepower = aBp;
            ArsenalBpChanged = false;

            byte lev = 1;
            foreach (var getlev in arsenals)
                if (getlev.TotalSharedBattlePower >= 2)
                    lev++;

            Level = lev;

            return _arsenalBp;
        }

        public uint GetMemberPotency(Enums.GuildMemberRank rankMember) {
            var getArsenalPotency = (uint)_arsenalBp;

            return getArsenalPotency;

            //if (RankMember == Enums.GuildMemberRank.GuildLeader || RankMember == Enums.GuildMemberRank.LeaderSpouse || RankMember == Enums.GuildMemberRank.DeputyLeader  )
            //    return GetArsenalPotency;

            //if (RankMember == Enums.GuildMemberRank.HDeputyLeader)
            //    return (uint)Math.Ceiling((double)(GetArsenalPotency * 90 / 100));

            //if (RankMember == Enums.GuildMemberRank.Manager || RankMember == Enums.GuildMemberRank.HonoraryManager
            //    || RankMember == Enums.GuildMemberRank.Supervisor)
            //    return (uint)Math.Ceiling((double)(GetArsenalPotency * 80 / 100));

            //if ((ushort)RankMember <= 859 && (ushort)RankMember >= 850 || RankMember == Enums.GuildMemberRank.ASupervisor || RankMember == Enums.GuildMemberRank.HonorarySuperv)
            //    return (uint)Math.Ceiling((double)(GetArsenalPotency * 70 / 100));

            //if (RankMember == Enums.GuildMemberRank.Steward || RankMember == Enums.GuildMemberRank.DLeaderSpouse
            //    || RankMember == Enums.GuildMemberRank.DLeaderAide)
            //    return (uint)Math.Ceiling((double)(GetArsenalPotency * 50 / 100));

            //if (RankMember == Enums.GuildMemberRank.DeputySteward)
            //    return (uint)Math.Ceiling((double)(GetArsenalPotency * 40 / 100));

            //if (RankMember == Enums.GuildMemberRank.Agent || (ushort)RankMember <= 599 && (ushort)RankMember >= 590
            //    || RankMember == Enums.GuildMemberRank.SSupervisor || RankMember == Enums.GuildMemberRank.ManagerSpouse
            //    || RankMember == Enums.GuildMemberRank.SupervisorAide || RankMember == Enums.GuildMemberRank.ManagerAide)
            //    return (uint)Math.Ceiling((double)(GetArsenalPotency * 30 / 100));

            //if (RankMember == Enums.GuildMemberRank.StewardSpouse || RankMember == Enums.GuildMemberRank.SeniorMember)
            //    return (uint)Math.Ceiling((double)(GetArsenalPotency * 15 / 100));

            //if (RankMember == Enums.GuildMemberRank.Member)
            //    return (uint)Math.Ceiling((double)(GetArsenalPotency * 10 / 100));

            //return (uint)Math.Ceiling((double)(GetArsenalPotency * 20 / 100));//Fallower
        }

        public uint GetSharedBattlepower(int rank) {
            return
                GetMemberPotency(
                    (Enums.GuildMemberRank)rank); //(uint)(arsenal_bp * SharedBattlepowerPercentage[rank / 100]);
        }

        public uint GetSharedBattlepower(Enums.GuildMemberRank rank) {
            return GetSharedBattlepower((int)rank);
        }

        /// <summary>
        /// Gets the maximum number of deputy leaders allowed based on guild level
        /// </summary>
        public byte GetMaxDeputyLeaders() {
            return Level switch {
                >= 1 and <= 3 => 2,
                >= 4 and <= 6 => 3,
                >= 7 and <= 9 => 4,
                _ => 2 // Default fallback
            };
        }

        public void SaveArsenal() {
            Database.GuildArsenalTable.Save(this);
        }

        public static Counter GuildCounter;

        public static void GuildProfile(byte[] packet, GameState client) {
            var p = new GuildProfilePacket();
            p.Deserialize(packet);
            p.Silver = 0;
            p.Pk = client.Entity.PKPoints;
            p.Cps = 0;
            p.Guide = 0;
            p.Arsenal = 0;
            p.Rose = 0;
            p.Lily = 0;
            p.Orchid = 0;
            p.Tulip = 0;
            p.HistorySilvers = 0;
            p.HistoryCps = 0;
            p.HistoryGuide = 0;
            p.HistoryPk = 0;
            client.Send(packet);
        }

        public class Member //: Interfaces.IKnownPerson
        {
            public Member(uint GuildID) {
                this.GuildID = GuildID;
            }

            public uint ExploitsRank;
            public uint Exploits = 0;
            public uint ID { get; set; }
            public string Name { get; set; }
            public string Spouse;

            public bool IsOnline => Kernel.GamePool.ContainsKey(ID);

            public GameState? Client => !IsOnline ? null : Kernel.GamePool[ID];

            public ulong SilverDonation { get; set; }
            public ulong ConquerPointDonation { get; set; }
            public ulong LastLogin = 0;
            public uint GuildID { get; set; }

            public Guild Guild => Kernel.Guilds[GuildID];

            public Enums.GuildMemberRank Rank { get; set; }
            public byte Level { get; set; }
            public NobilityRank NobilityRank { get; set; }
            public byte Gender { get; set; }
            public uint Mesh;

            public byte Class;
            public uint VirtuePoints;

            public uint Lilies;
            public uint Rouses;
            public uint Orchids;
            public uint Tulips;
            public uint ArsenalDonation;
            public uint PkDonation;

            public uint TotalDonation =>
                (uint)(Lilies + Orchids + Tulips + Rouses + ConquerPointDonation + VirtuePoints +
                       (uint)SilverDonation + ArsenalDonation + PkDonation);

            public uint CTFSilverReward;
            public uint CTFCpsReward;
            public uint WarScore;
        }

        private byte[] Buffer;
        public uint GuildScoreWar;
        public uint WarScore;
        public uint sWarScore;

        public bool SuperPoleKeeper => SuperGuildWar.Pole.Name == Name;

        public bool PoleKeeper {
            get {
                // Check database history first (works even after server restart)
                var latest = Database.GuildWarHistoryTable.GetLatest();
                if (latest != null && latest.GuildId == ID) {
                    return true;
                }

                // Fallback to active event (for during active war)
                var gwEvent = GuildWarEvent.GetActiveEvent();
                return gwEvent?.Pole?.Name == Name;
            }
        }

        public bool PoleKeeper2 => EliteGuildWar.Poles.Name == Name;

        public Guild(string leadername) {
            Buffer = new byte[92 + 8];
            Members = new SafeDictionary<uint, Member>();
            Enemy = new SafeDictionary<uint, Guild>();
            LeaderName = leadername;
            WriteUInt16(92, 0, Buffer);
            WriteUInt16(1106, 2, Buffer);
            Buffer[48] = 0x2;
            //  Buffer[49] = 0x1;

            //            Buffer[75] = 0x1;
            //            Buffer[87] = 0x20;
            LevelRequirement = 1;
            Members = new SafeDictionary<uint, Member>(1000);
            Ally = new SafeDictionary<uint, Guild>(1000);
            Enemy = new SafeDictionary<uint, Guild>(1000);

            Arsenals = new Arsenal[8];
            for (byte i = 0; i < 8; i++) {
                Arsenals[i] = new Arsenal(this) {
                    Position = (byte)(i + 1)
                };
            }

            AdvertiseRecruit = new Recruitment();
        }

        public uint CTFdonationCPs = 0;
        public uint CTFdonationSilver = 0;


        public uint CTFdonationCPsold = 0;
        public uint CTFdonationSilverold = 0;

        public void CalculateCtfrank(bool createPlayersReward = false) {
            var rankCtf = Members.Values.Where(p => p.Exploits != 0).OrderByDescending(p => p.Exploits).ToArray();
            for (ushort x = 0; x < rankCtf.Length; x++) {
                var aMem = rankCtf[x];
                var mem = Members[aMem.ID];
                mem.ExploitsRank = (uint)(x + 1);

                if (!createPlayersReward) continue;
                var rewardCtf = CalculateRewardCTF(mem.ExploitsRank);
                mem.CTFSilverReward = rewardCtf[0];
                mem.CTFCpsReward = rewardCtf[1];
            }
        }

        private uint[] CalculateRewardCTF(uint Rank) {
            var rew = new uint[2];
            rew[0] = (CTFdonationSilverold / (Rank + 1));
            rew[1] = (CTFdonationCPsold / (Rank + 1));
            return rew;
        }

        public Recruitment AdvertiseRecruit;

        public void CreateMembersRank() {
            lock (this) {
                //remove all ranks
                foreach (var memb in Members.Values.Where(memb => (ushort)memb.Rank < 920)) {
                    if (RanksCounts[(ushort)memb.Rank] > 0)
                        RanksCounts[(ushort)memb.Rank]--;
                    memb.Rank = Enums.GuildMemberRank.Member;
                    RanksCounts[(ushort)memb.Rank]++;
                }

                //calculate manager`s
                const byte maxMannager = 5; //0,1,2,3,4
                const byte maxHonorManager = 2; //5,6,
                const byte maxSupervisor = 2; //7,8,
                const byte maxSteward = 4; //9,10,11,12
                const byte maxArsFollower = 2; //13,14
                byte amount = 0; //8
                Member[] poll = (from memb in Members.Values orderby memb.ArsenalDonation descending select memb)
                    .ToArray();
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.Manager)
                        continue;
                    if (amount < maxMannager) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.Manager;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxMannager + maxHonorManager) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.HonoraryManager;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxHonorManager + maxMannager + maxSupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.Supervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxHonorManager + maxMannager + maxSupervisor + maxSteward) {
                        if (membru.Rank > Enums.GuildMemberRank.Steward)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.Steward;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxHonorManager + maxMannager + maxSupervisor + maxSteward + maxArsFollower) {
                        if (membru.Rank > Enums.GuildMemberRank.ArsFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.ArsFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankArsenalDonations = poll.ToArray();

                //calculate rank cps
                const byte maxCpSupervisor = 3; //0,1,2
                const byte maxCpAgent = 2; //3,4
                const byte maxCpFollower = 2; //5,6
                amount = 0; //3
                poll = (from memb in Members.Values orderby memb.ConquerPointDonation descending select memb).ToArray();
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.CPSupervisor)
                        continue;
                    if (amount < maxCpSupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.CPSupervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxCpSupervisor + maxCpAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.CPAgent)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.CPAgent;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxCpSupervisor + maxCpAgent + maxCpFollower) {
                        if (membru.Rank > Enums.GuildMemberRank.CPFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.CPFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankCPDonations = poll.ToArray();

                //calculate pk ranks
                const byte maxPkSupervisor = 3; //0,1,2
                const byte maxPkAgent = 2; //3,4,
                const byte maxPkFollower = 2; //5,6
                amount = 0; //3
                poll = (from memb in Members.Values orderby memb.PkDonation descending select memb).ToArray();
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.PKSupervisor)
                        continue;
                    if (amount < maxPkSupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.PKSupervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxPkSupervisor + maxPkAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.PKAgent)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.PKAgent;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxPkSupervisor + maxPkAgent + maxPkFollower) {
                        if (membru.Rank > Enums.GuildMemberRank.PKFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.PKFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankPkDonations = poll.ToArray();

                //calculate RoseSupervisor
                const byte maxRoseSupervisor = 3; //0,1,2
                const byte maxRoseAgent = 2; //3,4
                const byte maxRoseFollower = 2; //5,6
                amount = 0; //3
                poll = (from memb in Members.Values orderby memb.Rouses descending select memb).ToArray();
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.RoseSupervisor)
                        continue;
                    if (amount < maxRoseSupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.RoseSupervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxRoseSupervisor + maxRoseAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.RoseAgent)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.RoseAgent;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxRoseSupervisor + maxRoseAgent + maxRoseFollower) {
                        if (membru.Rank > Enums.GuildMemberRank.RoseFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.RoseFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankRosseDonations = poll.ToArray();

                //calculate LilySupervisor
                const byte maxLilySupervisor = 3;
                const byte maxLilyAgent = 2;
                const byte maxLilyFollower = 2;
                amount = 0; //3
                poll = (from memb in Members.Values orderby memb.Lilies descending select memb).ToArray();
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.LilySupervisor)
                        continue;
                    if (amount < maxLilySupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.LilySupervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxLilySupervisor + maxLilyAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.LilyAgent)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.LilyAgent;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxLilySupervisor + maxLilyAgent + maxLilyFollower) {
                        if (membru.Rank > Enums.GuildMemberRank.LilyFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.LilyFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankLiliesDonations = poll.ToArray();

                //calculate TulipAgent
                const byte maxTSupervisor = 3;
                const byte maxTulipAgent = 2;
                const byte maxTulupFollower = 2;
                amount = 0; //3
                poll = (from memb in Members.Values orderby memb.Tulips descending select memb).ToArray();
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.TSupervisor)
                        continue;
                    if (amount < maxTSupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.TSupervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxTSupervisor + maxTulipAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.TulipAgent)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.TulipAgent;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxTSupervisor + maxTulipAgent + maxTulupFollower) {
                        if (membru.Rank > Enums.GuildMemberRank.TulipFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.TulipFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankTulipsDonations = poll.ToArray();

                // calculate OrchidAgent
                const byte maxOSupervisor = 3;
                const byte maxOrchidAgent = 2;
                const byte maxOrchidFollower = 2;
                amount = 0; //3
                poll = (from memb in Members.Values
                    orderby memb.Tulips descending
                    select memb).ToArray();
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.OSupervisor)
                        continue;
                    if (amount < maxOSupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.OSupervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxOSupervisor + maxOrchidAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.OrchidAgent)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.OrchidAgent;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < maxOSupervisor + maxOrchidFollower + maxOrchidAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.OrchidFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.OrchidFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankOrchidsDonations = poll.ToArray();


                poll = (from memb in Members.Values
                    orderby memb.TotalDonation descending
                    select memb).ToArray();

                const byte hDeputyLeader = 2; //0,1
                const byte maxHonorarySteward = 2; //2,3
                amount = 0; //20
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.HDeputyLeader)
                        continue;
                    if (amount < hDeputyLeader) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.HDeputyLeader;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < hDeputyLeader + maxHonorarySteward) {
                        if (membru.Rank > Enums.GuildMemberRank.HonorarySteward)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.HonorarySteward;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankTotalDonations = poll.ToArray();


                const byte sSupervisor = 5; //0,1,2,3
                const byte maxSilverAgent = 2; //4,5
                const byte maxSilverFollowr = 2; //6,7
                amount = 0; //20
                poll = (from memb in Members.Values
                    orderby memb.SilverDonation descending
                    select memb).ToArray();
                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.SSupervisor)
                        continue;
                    if (amount < sSupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.SSupervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < sSupervisor + maxSilverAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.SilverAgent)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.SilverAgent;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < sSupervisor + maxSilverAgent + maxSilverFollowr) {
                        if (membru.Rank > Enums.GuildMemberRank.SilverFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.SilverFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankSilversDonations = poll.ToArray();

                const byte gSupervisor = 3; //0,1,2
                const byte maxGAgent = 2; //3,4
                const byte maxGFollower = 2; //5,6
                amount = 0; //20
                poll = (from memb in Members.Values
                    orderby memb.VirtuePoints descending
                    select memb).ToArray();

                for (byte x = 0; x < poll.Length; x++) {
                    var membru = poll[x];
                    if (membru.Rank > Enums.GuildMemberRank.GSupervisor)
                        continue;
                    if (amount < gSupervisor) {
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.GSupervisor;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < gSupervisor + maxGAgent) {
                        if (membru.Rank > Enums.GuildMemberRank.GuideAgent)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.GuideAgent;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else if (amount < gSupervisor + maxGAgent + maxGFollower) {
                        if (membru.Rank > Enums.GuildMemberRank.GuideFollower)
                            continue;
                        if (RanksCounts[(ushort)membru.Rank] > 0)
                            RanksCounts[(ushort)membru.Rank]--;
                        membru.Rank = Enums.GuildMemberRank.GuideFollower;
                        RanksCounts[(ushort)membru.Rank]++;
                        amount++;
                    }
                    else
                        break;
                }

                RankGuideDonations = poll.ToArray();
            }
        }

        public uint ID {
            get => BitConverter.ToUInt32(Buffer, 4);
            set => WriteUInt32(value, 4, Buffer);
        }

        public ulong SilverFund {
            get => BitConverter.ToUInt64(Buffer, 12);
            set => WriteUInt64(value, 12, Buffer);
        }

        public uint ConquerPointFund {
            get => BitConverter.ToUInt32(Buffer, 20);
            set => WriteUInt32(value, 20, Buffer);
        }

        public uint MemberCount {
            get => BitConverter.ToUInt32(Buffer, 24);
            set => WriteUInt32(value, 24, Buffer);
        }

        public uint LevelRequirement {
            get => BitConverter.ToUInt32(Buffer, 48);
            set => WriteUInt32(value, 48, Buffer);
        }

        public bool GetMember(string name, out Member getmem) {
            foreach (var mem in Members.Values.Where(mem => mem.Name == name)) {
                getmem = mem;
                return true;
            }

            getmem = null;
            return false;
        }

        public uint RebornRequirement {
            get => BitConverter.ToUInt32(Buffer, 52);
            set => WriteUInt32(value, 52, Buffer);
        }

        public uint ClassRequirement {
            get => BitConverter.ToUInt32(Buffer, 56);
            set => WriteUInt32(value, 56, Buffer);
        }

        public bool AllowTrojans {
            get => ((ClassRequirement & ClassRequirements.Trojan) != ClassRequirements.Trojan);
            set => ClassRequirement ^= ClassRequirements.Trojan;
        }

        public bool AllowWarriors {
            get => ((ClassRequirement & ClassRequirements.Warrior) != ClassRequirements.Warrior);
            set => ClassRequirement ^= ClassRequirements.Warrior;
        }

        public bool AllowTaoists {
            get => ((ClassRequirement & ClassRequirements.Taoist) != ClassRequirements.Taoist);
            set => ClassRequirement ^= ClassRequirements.Taoist;
        }

        public bool AllowArchers {
            get => ((ClassRequirement & ClassRequirements.Archer) != ClassRequirements.Archer);
            set => ClassRequirement ^= ClassRequirements.Archer;
        }

        public bool AllowNinjas {
            get => ((ClassRequirement & ClassRequirements.Ninja) != ClassRequirements.Ninja);
            set => ClassRequirement ^= ClassRequirements.Ninja;
        }

        public bool AllowMonks {
            get => ((ClassRequirement & ClassRequirements.Monk) != ClassRequirements.Monk);
            set => ClassRequirement ^= ClassRequirements.Monk;
        }

        public bool AllowPirates {
            get => ((ClassRequirement & ClassRequirements.Pirate) != ClassRequirements.Pirate);
            set => ClassRequirement ^= ClassRequirements.Pirate;
        }

        public byte Level {
            get => Buffer[60];
            set => Buffer[60] = value;
        }

        public string Name;

        public SafeDictionary<uint, Member> Members;
        public SafeDictionary<uint, Guild> Ally;
        public SafeDictionary<uint, Guild> Enemy;
        public uint Wins;
        public uint Losts;
        public uint cp_donation = 0;
        public ulong money_donation = 0;
        public uint honor_donation = 0;
        public uint pkp_donation = 0;
        public uint rose_donation = 0;
        public uint tuil_donation = 0;
        public uint orchid_donation = 0;
        public uint lilies_donation = 0;

        public string Bulletin;

        public Member Leader;
        private string leaderName;
        public uint PtScore;
        public uint PhScore;
        public uint PaScore;
        public uint BiScore;

        public uint EWarScore;
        public uint PTScore;
        public uint DCScore;
        public uint DPScore;
        public uint PIScore;
        public uint PPScore;
        public uint APScore;
        public uint RaScore;
        public uint MaScore;
        public uint CTFPoints;
        public uint CTFReward = 0;
        public uint CTFFlagScore;

        public string LeaderName {
            get => leaderName;
            set {
                leaderName = value;
                WriteString(value, 32, Buffer);
            }
        }

        public static Boolean CheckNameExist(String name) {
            return Kernel.Guilds.Values.Any(guilds => guilds.Name == name);
        }

        public bool Create(string name) {
            if (name.Length >= 16) return false;
            Name = name;
            SilverFund = 500000;
            Members.Add(Leader.ID, Leader);
            try {
                Database.GuildTable.Create(this);
            }
            catch {
                return false;
            }

            Kernel.Guilds.Add(ID, this);
            var message = new Message(
                "Congratulations, " + leaderName + " has created guild " + name + " Successfully!",
                System.Drawing.Color.White, Message.World);
            foreach (var client in Program.Values) {
                client.Send(message);
            }

            CreateTime();
            return true;
        }

        /// <summary>
        /// Creates a new guild for a player
        /// </summary>
        /// <param name="client">The player creating the guild</param>
        /// <param name="guildName">The name for the new guild</param>
        /// <param name="initialFund">The initial silver fund for the guild</param>
        /// <returns>True if the guild was created successfully, false otherwise</returns>
        public static bool CreateGuild(GameState client, string guildName, uint initialFund) {
            if (string.IsNullOrEmpty(guildName) || guildName.Length is < 1 or > 16) return false;
            if (CheckNameExist(guildName)) return false;

            // Create guild
            var guild = new Guild(client.Entity.Name) {
                ID = GuildCounter.Next,
                SilverFund = initialFund
            };

            // Create leader member
            client.AsMember = new Member(guild.ID) {
                SilverDonation = 500000,
                ID = client.Entity.UID,
                Level = client.Entity.Level,
                Name = client.Entity.Name,
                Rank = Enums.GuildMemberRank.GuildLeader,
            };

            if (client.NobilityInformation != null) {
                client.AsMember.Gender = client.NobilityInformation.Gender;
                client.AsMember.NobilityRank = client.NobilityInformation.Rank;
            }

            // Set up entity
            client.Entity.GuildID = (ushort)guild.ID;
            client.Entity.GuildRank = (ushort)Enums.GuildMemberRank.GuildLeader;
            guild.Leader = client.AsMember;
            client.Guild = guild;

            // Create guild in database
            if (!guild.Create(guildName)) {
                // Rollback on failure
                client.AsMember = null;
                client.Guild = null;
                client.Entity.GuildID = 0;
                client.Entity.GuildRank = 0;
                return false;
            }

            // Update entity in database
            Database.EntityTable.UpdateGuildID(client);
            Database.EntityTable.UpdateGuildRank(client);
            guild.Name = guildName;
            guild.MemberCount++;
            guild.SendGuild(client);
            guild.SendName(client);
            Database.GuildArsenalTable.Insert(guild.ID);
            client.Screen.FullWipe();
            client.Screen.Reload();

            // Send world message
            Kernel.SendWorldMessage(
                new Message(
                    $"A new guild [{guildName}] has been created by {client.Entity.Name}!",
                    System.Drawing.Color.Red, Message.Center),
                Program.Values);

            return true;
        }

        /// <summary>
        /// Changes the guild name
        /// </summary>
        /// <param name="client">The guild leader requesting the name change</param>
        /// <param name="newName">The new guild name</param>
        /// <returns>True if the name was changed successfully, false otherwise</returns>
        public bool ChangeName(GameState client, string newName) {
            if (string.IsNullOrEmpty(newName) || newName.Length is < 1 or > 16) return false;
            if (CheckNameExist(newName)) return false;

            var oldName = Name;
            Database.GuildTable.ChangeName(client, newName);
            Name = newName;
            SendGuild(client);
            SendName(client);
            client.Screen.FullWipe();
            client.Screen.Reload();

            // Send world message
            Kernel.SendWorldMessage(
                new Message(
                    $"The guild [{oldName}] has been renamed to [{newName}] by {client.Entity.Name}.",
                    System.Drawing.Color.Red, Message.Center),
                Program.Values);

            return true;
        }

        public uint GuildEnrole;
        public uint BuletinEnrole;

        public void CreateBuletinTime(uint time = 0) {
            if (time == 0) {
                var timers = DateTime.Now;
                time = GetTime((uint)timers.Year, (uint)timers.Month, (uint)timers.Day);
                Database.GuildTable.SaveEnroles(this);
            }

            BuletinEnrole = time;
        }

        public void CreateTime(uint time = 0) {
            if (time == 0) {
                var timers = DateTime.Now;
                time = GetTime((uint)timers.Year, (uint)timers.Month, (uint)timers.Day);
                Database.GuildTable.SaveEnroles(this);
            }

            GuildEnrole = time;
            WriteUInt32(time, 67, Buffer);
        }

        public static uint GetTime(uint year, uint month, uint day) {
            var timer = year * 10000 + month * 100 + day;
            return timer;
        }

        public void AddMember(GameState client) {
            client.AsMember = new Member(ID) {
                ID = client.Entity.UID,
                Level = client.Entity.Level,
                Name = client.Entity.Name,
                Rank = Enums.GuildMemberRank.Member,
                Mesh = client.Entity.Mesh
            };
            if (Nobility.Board.TryGetValue(client.Entity.UID, out var value)) {
                client.AsMember.Gender = value.Gender;
                client.AsMember.NobilityRank = value.Rank;
            }

            MemberCount++;
            client.Guild = this;
            client.Entity.GuildID = (ushort)client.Guild.ID;
            client.Entity.GuildRank = (ushort)client.AsMember.Rank;
            if (client.Entity.BattlePower < 405)
                client.Entity.GuildBattlePower = GetSharedBattlepower(client.AsMember.Rank);
            for (var i = 0; i < client.ArsenalDonations.Length; i++)
                client.ArsenalDonations[i] = 0;
            Database.EntityTable.UpdateGuildID(client);
            Database.EntityTable.UpdateGuildRank(client);
            Members.Add(client.Entity.UID, client.AsMember);
            SendGuild(client);
            client.Screen.FullWipe();
            client.Screen.Reload();
            SendGuildMessage(new Message(client.AsMember.Name + " has joined our guild.",
                System.Drawing.Color.Black, Message.Guild));

            var mindonation = new GuildMinDonations(31);
            mindonation.AprendGuild(this);
            client.Send(mindonation.ToArray());
        }


        public void SendMembers(GameState client, ushort page) {
            var timernow = (ulong)DateTime.Now.Ticks;
            var strm = new MemoryStream();
            var wtr = new BinaryWriter(strm);
            wtr.Write((ushort)0);
            wtr.Write((ushort)2102);
            wtr.Write((uint)0);
            wtr.Write((uint)page);
            var left = (int)MemberCount - page;
            if (left > 12) left = 12;
            if (left < 0) left = 0;
            wtr.Write((uint)left);
            var count = 0;
            var maxmem = page + 12;
            int minmem = page;
            var online = new List<Member>(250);
            var offline = new List<Member>(250);
            foreach (var member in Members.Values) {
                if (member.IsOnline)
                    online.Add(member);
                else
                    offline.Add(member);
            }

            online.OrderByDescending((mem) => mem.Rank);
            var unite = online.Union(offline);
            foreach (var member in unite) {
                if (count >= minmem && count < maxmem) {
                    wtr.Write((uint)0);
                    var name = Encoding.Default.GetBytes(member.Name);

                    for (var j = 0; j < 16; j++) {
                        if (name.Length > j) wtr.Write(name[j]);
                        else wtr.Write((byte)0);
                    }

                    wtr.Write((uint)(member.IsOnline ? 1 : 0));
                    wtr.Write((uint)member.NobilityRank);
                    if (member.Gender == 3)
                        wtr.Write((uint)(member.Gender - 2));
                    else
                        wtr.Write((uint)(member.Gender + 1));
                    wtr.Write((uint)member.Level);
                    wtr.Write((uint)member.Rank);
                    wtr.Write((uint)0); // EXP
                    wtr.Write(member.ArsenalDonation);
                    wtr.Write((uint)0);
                    wtr.Write((uint)0);
                    wtr.Write((uint)member.Class);
                    wtr.Write((uint)(((timernow - member.LastLogin) / 10000000)));
                    wtr.Write(client.Entity.Mesh);
                }

                count++;
            }

            var packetlength = (int)strm.Length;
            strm.Position = 0;
            wtr.Write((ushort)packetlength);
            strm.Position = strm.Length;
            wtr.Write(Encoding.Default.GetBytes("TQServer"));
            strm.Position = 0;
            var buf = new byte[strm.Length];
            strm.ReadExactly(buf, 0, buf.Length);
            wtr.Close();
            strm.Close();
            client.Send(buf);
        }

        public void SendGuildMessage(Interfaces.IPacket message) {
            foreach (var member in Members.Values.Where(member => member.IsOnline)) {
                member.Client.Send(message);
            }
        }

        public Member? GetMemberByName(string membername) {
            return Members.Values.FirstOrDefault(member => member.Name == membername);
        }

        public void ExpelMember(string membername, bool ownquit) {
            var member = GetMemberByName(membername);
            if (member is { IsOnline: true })
                PacketHandler.UninscribeAllItems(member.Client);
            else
                foreach (var arsenal in Arsenals)
                    arsenal.RemoveInscribedItemsBy(member.ID);

            if (ownquit)
                SendGuildMessage(new Message(member.Name + " has quit our guild.", System.Drawing.Color.Black,
                    Message.Guild));
            else
                SendGuildMessage(new Message(member.Name + " have been expelled from our guild.",
                    System.Drawing.Color.Black, Message.Guild));
            var uid = member.ID;
            if (member.Rank == Enums.GuildMemberRank.DeputyLeader)
                RanksCounts[(ushort)Enums.GuildMemberRank.DeputyLeader]--;
            if (member.IsOnline) {
                var command = new GuildCommand(true) {
                    Type = GuildCommand.Disband,
                    dwParam = ID
                };
                member.Client.Send(command);
                member.Client.AsMember = null;
                member.Client.Guild = null;
                member.Client.Entity.GuildID = 0;
                member.Client.Entity.GuildRank = 0;
                member.Client.Screen.FullWipe();
                member.Client.Screen.Reload();
                member.Client.Entity.GuildBattlePower = 0;
            }
            else {
                member.GuildID = 0;
                Database.EntityTable.UpdateData(member.ID, "GuildID", 0);
            }

            MemberCount--;
            Members.Remove(uid);
        }

        /// <summary>
        /// Disbands the guild
        /// </summary>
        /// <param name="disbandedBy">Optional name of the player who disbanded the guild (for world message)</param>
        public void Disband(string? disbandedBy = null) {
            var guildName = Name;
            var members = Members.Values.ToArray();
            foreach (var member in members) {
                var uid = member.ID;
                if (member.IsOnline) {
                    PacketHandler.UninscribeAllItems(member.Client);
                    member.Client.Entity.GuildBattlePower = 0;
                    var command = new GuildCommand(true) {
                        Type = GuildCommand.Disband,
                        dwParam = ID
                    };
                    member.Client.Entity.GuildID = 0;
                    member.Client.Entity.GuildRank = 0;
                    member.Client.Send(command);
                    member.Client.Screen.FullWipe();
                    member.Client.Screen.Reload();
                    member.Client.AsMember = null;
                    member.Client.Guild = null;
                }
                else {
                    foreach (var arsenal in Arsenals)
                        arsenal.RemoveInscribedItemsBy(member.ID);
                    member.GuildID = 0;
                    Database.EntityTable.UpdateData(member.ID, "GuildID", 0);
                }

                MemberCount--;
                Members.Remove(uid);
            }

            var allies = Ally.Values.ToArray();
            foreach (var ally in allies) {
                RemoveAlly(ally.Name);
                ally.RemoveAlly(Name);
            }

            Database.GuildTable.Disband(this);
            Kernel.GamePool.Remove(ID);

            // Send world message if disbanded by a player
            if (!string.IsNullOrEmpty(disbandedBy)) {
                Kernel.SendWorldMessage(
                    new Message(
                        $"The guild [{guildName}] has been disbanded by {disbandedBy}.",
                        System.Drawing.Color.Red, Message.Center),
                    Program.Values);
            }
        }

        public void AddAlly(string name) {
            foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name)) {
                /*if (Enemy.ContainsKey(guild.ID))
                        RemoveEnemy(guild.Name);
                    if (!Ally.ContainsKey(guild.ID))
                    {
                        Database.GuildTable.AddAlly(this, guild.ID);
                        Ally.Add(guild.ID, guild);
                        _String stringPacket = new _String(true);
                        stringPacket.UID = guild.ID;
                        stringPacket.Type = _String.GuildAllies;
                        stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " " + guild.Level + " " + guild.MemberCount);
                        SendGuildMessage(stringPacket);
                    }
                    return;*/
                if (Enemy.ContainsKey(guild.ID)) {
                    RemoveEnemy(guild.Name);
                }

                Ally.Add(guild.ID, guild);
                var message = new _String(true) {
                    UID = guild.ID,
                    Type = 0x15
                };
                message.Texts.Add(string.Concat(new object[]
                    { guild.Name, " ", guild.LeaderName, " 0 ", guild.MemberCount }));
                SendGuildMessage(message);
                SendGuildMessage(message);
                Database.GuildTable.AddAlly(this, guild.ID);
                return;
            }
        }

        public void RemoveAlly(string name) {
            foreach (var guild in Ally.Values) {
                if (guild.Name != name) continue;
                var cmd = new GuildCommand(true) {
                    Type = GuildCommand.Neutral1,
                    dwParam = guild.ID
                };
                SendGuildMessage(cmd);
                Database.GuildTable.RemoveAlly(this, guild.ID);
                Ally.Remove(guild.ID);
                return;
            }
        }

        public void AddEnemy(string name) {
            foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name)) {
                if (Ally.ContainsKey(guild.ID)) {
                    RemoveAlly(guild.Name);
                    guild.RemoveAlly(Name);
                }

                Enemy.Add(guild.ID, guild);
                var stringPacket = new _String(true) {
                    UID = guild.ID,
                    Type = _String.GuildEnemies
                };
                stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " " + guild.Level + " " +
                                       guild.MemberCount);
                SendGuildMessage(stringPacket);
                SendGuildMessage(stringPacket);
                Database.GuildTable.AddEnemy(this, guild.ID);
                return;
            }
        }

        public void RemoveEnemy(string name) {
            foreach (var guild in Enemy.Values) {
                if (guild.Name != name) continue;
                var cmd = new GuildCommand(true) {
                    Type = GuildCommand.Neutral2,
                    dwParam = guild.ID
                };
                SendGuildMessage(cmd);
                SendGuildMessage(cmd);
                Database.GuildTable.RemoveEnemy(this, guild.ID);
                Enemy.Remove(guild.ID);
                return;
            }
        }


        public void SendName(GameState client) {
            var stringPacket = new _String(true) {
                UID = ID,
                Type = _String.GuildName
            };
            stringPacket.Texts.Add(Name + " " + LeaderName + " 0 " + MemberCount);
            client.Send(stringPacket);
        }

        public void SendGuild(GameState client) {
            if (!Members.ContainsKey(client.Entity.UID)) return;
            Bulletin ??= "This is a new guild!";

            client.Send(new GuildCommand((uint)Bulletin.Length)
                { Type = GuildCommand.Bulletin, dwParam = BuletinEnrole, Str_ = Bulletin });
            //client.Send(new Message(Bulletin, System.Drawing.Color.White, Message.GuildAnnouncement));
            WriteUInt32((uint)client.AsMember.SilverDonation, 8, Buffer);
            WriteUInt32((ushort)client.AsMember.Rank, 28, Buffer);
            client.Send(Buffer);
        }

        public void SendAllyAndEnemy(GameState client) {
            foreach (var guild in Enemy.Values) {
                var stringPacket = new _String(true) {
                    UID = guild.ID,
                    Type = _String.GuildEnemies
                };
                stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " 0 " + guild.MemberCount);
                client.Send(stringPacket);
                client.Send(stringPacket);
            }

            foreach (var guild in Ally.Values) {
                var stringPacket = new _String(true) {
                    UID = guild.ID,
                    Type = _String.GuildAllies
                };
                stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " 0 " + guild.MemberCount);
                client.Send(stringPacket);
                client.Send(stringPacket);
            }
        }

        public static bool ValidName(string name) {
            if (name.Length is < 4 or > 15) return false;
            else if (name.IndexOfAny([
                         ' ', '#', '%', '^', '&', '*', '(', ')', ';', ':', '\'', '\"', '/', '\\', ',', '.', '{', '}',
                         '[', ']'
                     ]) > 0) return false;
            else return true;
        }
    }
}