using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Database;
using MTA.Game.ConquerStructures;
using MTA.Game.ConquerStructures.Society;
using MTA.Game.Events.GuildWar;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Handlers;
using MTA.Game.Features.Guilds.Packets;
using MTA.Interfaces;
using MTA.Network;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.EntityClass;

namespace MTA.Game.Features.Guilds;

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
    HManager = 880,
    HSteward = 680,
    HSupervisor = 840,
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
    SupervisorSpouse = 0x209,
    TSupervisor = 0x35b,
    TulipAgent = 0x257,
    TulipFollower = 0x1f3
}

public class Guild : Writer {
    public static Counter GuildCounter = new(0);

    private readonly byte[] _buffer;

    public readonly Recruitment AdvertiseRecruit;
    public readonly SafeDictionary<uint, Guild> Ally;

    public readonly Arsenal[] Arsenals;
    public readonly List<uint> BlackList = [];
    public readonly SafeDictionary<uint, Guild> Enemy;

    public readonly ushort[] RanksCounts = new ushort[(ushort)Enums.GuildMemberRank.GuildLeader + 1];

    private int _arsenalBp;
    private string _leaderName;
    public bool ArsenalBpChanged = true;

    public string? Bulletin;
    public uint BulletinEnroll;
    public uint CpDonation = 0;

    // ReSharper disable once InconsistentNaming
    public uint CTFDonationCPs = 0;

    // ReSharper disable once InconsistentNaming
    public uint CTFDonationCPSold = 0;

    // ReSharper disable once InconsistentNaming
    public uint CTFDonationSilver = 0;

    // ReSharper disable once InconsistentNaming
    public uint CTFDonationSilverOld = 0;
    public uint CtfFlagScore;
    public uint CtfPoints;
    public uint CtfReward = 0;

    public uint EWarScore;

    public uint GuildEnroll;
    public uint GuildScoreWar;
    public uint HonorDonation = 0;

    public Member? Leader;
    public uint LiliesDonation = 0;
    public uint Loses;

    public SafeDictionary<uint, Member> Members;
    public ulong MoneyDonation = 0;

    public string Name;
    public uint OrchidDonation = 0;
    public uint PaScore;
    public uint PkpDonation = 0;
    public uint PpScore;
    public Member[] RankArsenalDonations = [];
    public Member[] RankCpDonations = [];
    public Member[] RankGuideDonations = [];
    public Member[] RankLiliesDonations = [];
    public Member[] RankOrchidsDonations = [];
    public Member[] RankPkDonations = [];
    public Member[] RankRoseDonations = [];


    public Member[] RankSilversDonations = [];
    public Member[] RankTotalDonations = [];
    public Member[] RankTulipsDonations = [];
    public uint RoseDonation = 0;
    public uint SWarScore;
    public uint TulipDonation = 0;
    public uint WarScore;
    public uint Wins;

    public Guild(string leaderName) {
        _buffer = new byte[92 + 8];
        Members = new SafeDictionary<uint, Member>();
        Enemy = new SafeDictionary<uint, Guild>();
        _leaderName = leaderName;
        Name = string.Empty;
        Bulletin = null; // Will be set later or defaulted in SendGuild
        Leader = null; // Will be set later in Create or CreateGuild
        LeaderName = leaderName;
        WriteUInt16(92, 0, _buffer);
        WriteUInt16(1106, 2, _buffer);
        _buffer[48] = 0x2;
        //  Buffer[49] = 0x1;

        //            Buffer[75] = 0x1;
        //            Buffer[87] = 0x20;
        LevelRequirement = 1;
        Members = new SafeDictionary<uint, Member>(1000);
        Ally = new SafeDictionary<uint, Guild>(1000);
        Enemy = new SafeDictionary<uint, Guild>(1000);

        Arsenals = new Arsenal[8];
        for (byte i = 0; i < 8; i++)
            Arsenals[i] = new Arsenal(this) {
                Position = (byte)(i + 1)
            };

        AdvertiseRecruit = new Recruitment();
    }

    private int UnlockedArsenals {
        get {
            var unlocked = 0;
            for (var i = 0; i < 8; i++)
                if (Arsenals[i].Unlocked)
                    unlocked++;
            return unlocked;
        }
    }

    public int ArsenalTotalBattlePower {
        get => _arsenalBp;
        set {
            _arsenalBp = value;
            foreach (var member in Members.Values)
                if (Kernel.TryGetPlayer(member.Id, out var client))
                    client.Entity.GuildBattlePower = GetSharedBattlePower(member.Rank);
        }
    }

    public bool SuperPoleKeeper => SuperGuildWar.Pole.Name == Name;

    public bool PoleKeeper {
        get {
            // Check database history first (works even after server restart)
            var latest = GuildWarHistoryTable.GetLatest();
            if (latest is { GuildId: var guildId } && guildId == Id) return true;

            // Fallback to active event (for during active war)
            var gwEvent = GuildWarEvent.GetActiveEvent();
            return gwEvent?.Pole?.Name == Name;
        }
    }

    public bool PoleKeeper2 => EliteGuildWar.Poles.Name == Name;

    public uint Id {
        get => BitConverter.ToUInt32(_buffer, 4);
        set => WriteUInt32(value, 4, _buffer);
    }

    public ulong SilverFund {
        get => BitConverter.ToUInt64(_buffer, 12);
        set => WriteUInt64(value, 12, _buffer);
    }

    public uint ConquerPointFund {
        get => BitConverter.ToUInt32(_buffer, 20);
        set => WriteUInt32(value, 20, _buffer);
    }

    public uint MemberCount {
        get => BitConverter.ToUInt32(_buffer, 24);
        set => WriteUInt32(value, 24, _buffer);
    }

    public uint LevelRequirement {
        get => BitConverter.ToUInt32(_buffer, 48);
        set => WriteUInt32(value, 48, _buffer);
    }

    public uint RebornRequirement {
        get => BitConverter.ToUInt32(_buffer, 52);
        set => WriteUInt32(value, 52, _buffer);
    }

    public uint ClassRequirement {
        get => BitConverter.ToUInt32(_buffer, 56);
        set => WriteUInt32(value, 56, _buffer);
    }

    public bool AllowTrojans {
        get => (ClassRequirement & ClassRequirements.Trojan) != ClassRequirements.Trojan;
        set {
            if (value)
                ClassRequirement &= ~ClassRequirements.Trojan;
            else
                ClassRequirement |= ClassRequirements.Trojan;
        }
    }

    public bool AllowWarriors {
        get => (ClassRequirement & ClassRequirements.Warrior) != ClassRequirements.Warrior;
        set {
            if (value)
                ClassRequirement &= ~ClassRequirements.Warrior;
            else
                ClassRequirement |= ClassRequirements.Warrior;
        }
    }

    public bool AllowTaoists {
        get => (ClassRequirement & ClassRequirements.Taoist) != ClassRequirements.Taoist;
        set {
            if (value)
                ClassRequirement &= ~ClassRequirements.Taoist;
            else
                ClassRequirement |= ClassRequirements.Taoist;
        }
    }

    public bool AllowArchers {
        get => (ClassRequirement & ClassRequirements.Archer) != ClassRequirements.Archer;
        set {
            if (value)
                ClassRequirement &= ~ClassRequirements.Archer;
            else
                ClassRequirement |= ClassRequirements.Archer;
        }
    }

    public bool AllowNinjas {
        get => (ClassRequirement & ClassRequirements.Ninja) != ClassRequirements.Ninja;
        set {
            if (value)
                ClassRequirement &= ~ClassRequirements.Ninja;
            else
                ClassRequirement |= ClassRequirements.Ninja;
        }
    }

    public bool AllowMonks {
        get => (ClassRequirement & ClassRequirements.Monk) != ClassRequirements.Monk;
        set {
            if (value)
                ClassRequirement &= ~ClassRequirements.Monk;
            else
                ClassRequirement |= ClassRequirements.Monk;
        }
    }

    public bool AllowPirates {
        get => (ClassRequirement & ClassRequirements.Pirate) != ClassRequirements.Pirate;
        set {
            if (value)
                ClassRequirement &= ~ClassRequirements.Pirate;
            else
                ClassRequirement |= ClassRequirements.Pirate;
        }
    }

    public byte Level {
        get => _buffer[60];
        set => _buffer[60] = value;
    }

    public ulong LeaderId { get; set; }

    public string LeaderName {
        get {
            // First try to get name from Leader object if available
            if (Leader != null) return Leader.Name;

            // If Leader is null, but we have a cached name, return it
            if (!string.IsNullOrEmpty(_leaderName)) return _leaderName;

            // If we have LeaderID, query the database for the name
            if (LeaderId <= 0) return string.Empty;
            try {
                using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                    .Select("entities")
                    .Where("UID", LeaderId);
                using var reader = new MySqlReader(cmd);
                if (reader.Read()) {
                    _leaderName = reader.ReadString("Name");
                    return _leaderName;
                }
            }
            catch {
                // If query fails, return empty string
            }

            return string.Empty;
        }
        set {
            _leaderName = value;
            WriteString(value, 32, _buffer);
        }
    }

    public uint GetCurrentArsenalCost() {
        var val = UnlockedArsenals;
        return val switch {
            >= 0 and <= 1 => 5000000,
            >= 2 and <= 4 => 10000000,
            >= 5 and <= 6 => 15000000,
            _ => 20000000
        };
    }

    public override int GetHashCode() {
        return (int)Id;
    }

    public int GetMaxSharedBattlePower(bool force = false) {
        if (!ArsenalBpChanged && !force) return _arsenalBp;
        var aBp = 0;
        var arsenals = Arsenals.OrderByDescending(p => p.TotalSharedBattlePower).ToArray();
        var a = 0;
        foreach (var arsenal in arsenals) {
            if (a == 5) break;
            aBp += (int)arsenal.TotalSharedBattlePower;
            a++;
        }

        ArsenalTotalBattlePower = aBp;
        ArsenalBpChanged = false;

        byte level = 1;
        foreach (var arsenal in arsenals)
            if (arsenal.TotalSharedBattlePower >= 2)
                level++;

        Level = level;

        return _arsenalBp;
    }

    public uint GetMemberPotency() {
        return (uint)_arsenalBp;
    }

    public uint GetMemberPotency(Enums.GuildMemberRank rank) {
        // Calculate potency based on rank and arsenal BP
        // This is a placeholder - the actual calculation may need to be implemented based on game logic
        return (uint)_arsenalBp;
    }

    public uint GetSharedBattlePower(int rank) {
        return
            GetMemberPotency(
                (Enums.GuildMemberRank)rank); //(uint)(arsenal_bp * SharedBattlePowerPercentage[rank / 100]);
    }

    public uint GetSharedBattlePower(Enums.GuildMemberRank rank) {
        return GetSharedBattlePower((int)rank);
    }

    /// <summary>
    ///     Gets the maximum number of deputy leaders allowed based on guild level
    /// </summary>
    public byte GetMaxDeputyLeaders() {
        return Level switch {
            >= 1 and <= 3 => 2,
            >= 4 and <= 6 => 3,
            >= 7 and <= 9 => 4,
            _ => 2 // Default fallback
        };
    }

    /// <summary>
    ///     Gets the maximum number of allies allowed based on guild level
    /// </summary>
    public byte GetMaxAllies() {
        return Level switch {
            1 => 5,
            2 => 7,
            3 => 9,
            4 => 12,
            >= 5 => 15,
            _ => 5 // Default fallback
        };
    }

    /// <summary>
    ///     Gets the maximum number of enemies allowed based on guild level
    /// </summary>
    public byte GetMaxEnemies() {
        return Level switch {
            1 => 5,
            2 => 7,
            3 => 9,
            4 => 12,
            >= 5 => 15,
            _ => 5 // Default fallback
        };
    }

    public void SaveArsenal() {
        GuildArsenalTable.Save(this);
    }

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

    // ReSharper disable once InconsistentNaming
    public void CalculateCTFRank(bool createPlayersReward = false) {
        var rankCtf = Members.Values.Where(p => p.Exploits != 0).OrderByDescending(p => p.Exploits).ToArray();
        for (ushort x = 0; x < rankCtf.Length; x++) {
            var aMem = rankCtf[x];
            var mem = Members[aMem.Id];
            mem.ExploitsRank = (uint)(x + 1);

            if (!createPlayersReward) continue;
            var rewardCtf = CalculateRewardCtf(mem.ExploitsRank);
            mem.CtfSilverReward = rewardCtf[0];
            mem.CtfCpsReward = rewardCtf[1];
        }
    }

    private uint[] CalculateRewardCtf(uint rank) {
        var rew = new uint[2];
        rew[0] = CTFDonationSilverOld / (rank + 1);
        rew[1] = CTFDonationCPSold / (rank + 1);
        return rew;
    }

    public void CreateMembersRank() {
        lock (this) {
            //remove all ranks
            foreach (var member in Members.Values.Where(member => (ushort)member.Rank < 920)) {
                if (RanksCounts[(ushort)member.Rank] > 0)
                    RanksCounts[(ushort)member.Rank]--;
                member.Rank = Enums.GuildMemberRank.Member;
                RanksCounts[(ushort)member.Rank]++;
            }

            //calculate manager`s
            const byte maxManager = 5; //0,1,2,3,4
            const byte maxHonorManager = 2; //5,6,
            const byte maxSupervisor = 2; //7,8,
            const byte maxSteward = 4; //9,10,11,12
            const byte maxArsFollower = 2; //13,14
            byte amount = 0; //8
            Member[] poll = (from member in Members.Values orderby member.ArsenalDonation descending select member)
                .ToArray();
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.Manager)
                    continue;
                if (amount < maxManager) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.Manager;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxManager + maxHonorManager) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.HonoraryManager;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxHonorManager + maxManager + maxSupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.Supervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxHonorManager + maxManager + maxSupervisor + maxSteward) {
                    if (member.Rank > Enums.GuildMemberRank.Steward)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.Steward;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxHonorManager + maxManager + maxSupervisor + maxSteward + maxArsFollower) {
                    if (member.Rank > Enums.GuildMemberRank.ArsFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.ArsFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankArsenalDonations = poll.ToArray();

            //calculate rank cps
            const byte maxCpSupervisor = 3; //0,1,2
            const byte maxCpAgent = 2; //3,4
            const byte maxCpFollower = 2; //5,6
            amount = 0; //3
            poll = (from member in Members.Values orderby member.ConquerPointDonation descending select member)
                .ToArray();
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.CPSupervisor)
                    continue;
                if (amount < maxCpSupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.CPSupervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxCpSupervisor + maxCpAgent) {
                    if (member.Rank > Enums.GuildMemberRank.CPAgent)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.CPAgent;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxCpSupervisor + maxCpAgent + maxCpFollower) {
                    if (member.Rank > Enums.GuildMemberRank.CPFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.CPFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankCpDonations = poll.ToArray();

            //calculate pk ranks
            const byte maxPkSupervisor = 3; //0,1,2
            const byte maxPkAgent = 2; //3,4,
            const byte maxPkFollower = 2; //5,6
            amount = 0; //3
            poll = (from member in Members.Values orderby member.PkDonation descending select member).ToArray();
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.PKSupervisor)
                    continue;
                if (amount < maxPkSupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.PKSupervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxPkSupervisor + maxPkAgent) {
                    if (member.Rank > Enums.GuildMemberRank.PKAgent)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.PKAgent;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxPkSupervisor + maxPkAgent + maxPkFollower) {
                    if (member.Rank > Enums.GuildMemberRank.PKFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.PKFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankPkDonations = poll.ToArray();

            //calculate RoseSupervisor
            const byte maxRoseSupervisor = 3; //0,1,2
            const byte maxRoseAgent = 2; //3,4
            const byte maxRoseFollower = 2; //5,6
            amount = 0; //3
            poll = (from member in Members.Values orderby member.Roses descending select member).ToArray();
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.RoseSupervisor)
                    continue;
                if (amount < maxRoseSupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.RoseSupervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxRoseSupervisor + maxRoseAgent) {
                    if (member.Rank > Enums.GuildMemberRank.RoseAgent)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.RoseAgent;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxRoseSupervisor + maxRoseAgent + maxRoseFollower) {
                    if (member.Rank > Enums.GuildMemberRank.RoseFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.RoseFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankRoseDonations = poll.ToArray();

            //calculate LilySupervisor
            const byte maxLilySupervisor = 3;
            const byte maxLilyAgent = 2;
            const byte maxLilyFollower = 2;
            amount = 0; //3
            poll = (from member in Members.Values orderby member.Lilies descending select member).ToArray();
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.LilySupervisor)
                    continue;
                if (amount < maxLilySupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.LilySupervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxLilySupervisor + maxLilyAgent) {
                    if (member.Rank > Enums.GuildMemberRank.LilyAgent)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.LilyAgent;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxLilySupervisor + maxLilyAgent + maxLilyFollower) {
                    if (member.Rank > Enums.GuildMemberRank.LilyFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.LilyFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankLiliesDonations = poll.ToArray();

            //calculate TulipAgent
            const byte maxTSupervisor = 3;
            const byte maxTulipAgent = 2;
            const byte maxTulipFollower = 2;
            amount = 0; //3
            poll = (from member in Members.Values orderby member.Tulips descending select member).ToArray();
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.TSupervisor)
                    continue;
                if (amount < maxTSupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.TSupervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxTSupervisor + maxTulipAgent) {
                    if (member.Rank > Enums.GuildMemberRank.TulipAgent)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.TulipAgent;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxTSupervisor + maxTulipAgent + maxTulipFollower) {
                    if (member.Rank > Enums.GuildMemberRank.TulipFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.TulipFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankTulipsDonations = poll.ToArray();

            // calculate OrchidAgent
            const byte maxOSupervisor = 3;
            const byte maxOrchidAgent = 2;
            const byte maxOrchidFollower = 2;
            amount = 0; //3
            poll = (from member in Members.Values
                orderby member.Tulips descending
                select member).ToArray();
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.OSupervisor)
                    continue;
                if (amount < maxOSupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.OSupervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxOSupervisor + maxOrchidAgent) {
                    if (member.Rank > Enums.GuildMemberRank.OrchidAgent)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.OrchidAgent;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < maxOSupervisor + maxOrchidFollower + maxOrchidAgent) {
                    if (member.Rank > Enums.GuildMemberRank.OrchidFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.OrchidFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankOrchidsDonations = poll.ToArray();


            poll = (from member in Members.Values
                orderby member.TotalDonation descending
                select member).ToArray();

            const byte hDeputyLeader = 2; //0,1
            const byte maxHonorarySteward = 2; //2,3
            amount = 0; //20
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.HDeputyLeader)
                    continue;
                if (amount < hDeputyLeader) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.HDeputyLeader;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < hDeputyLeader + maxHonorarySteward) {
                    if (member.Rank > Enums.GuildMemberRank.HonorarySteward)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.HonorarySteward;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankTotalDonations = poll.ToArray();


            const byte sSupervisor = 5; //0,1,2,3
            const byte maxSilverAgent = 2; //4,5
            const byte maxSilverFollower = 2; //6,7
            amount = 0; //20
            poll = (from member in Members.Values
                orderby member.SilverDonation descending
                select member).ToArray();
            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.SSupervisor)
                    continue;
                if (amount < sSupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.SSupervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < sSupervisor + maxSilverAgent) {
                    if (member.Rank > Enums.GuildMemberRank.SilverAgent)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.SilverAgent;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < sSupervisor + maxSilverAgent + maxSilverFollower) {
                    if (member.Rank > Enums.GuildMemberRank.SilverFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.SilverFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankSilversDonations = poll.ToArray();

            const byte gSupervisor = 3; //0,1,2
            const byte maxGAgent = 2; //3,4
            const byte maxGFollower = 2; //5,6
            amount = 0; //20
            poll = (from member in Members.Values
                orderby member.VirtuePoints descending
                select member).ToArray();

            for (byte x = 0; x < poll.Length; x++) {
                var member = poll[x];
                if (member.Rank > Enums.GuildMemberRank.GSupervisor)
                    continue;
                if (amount < gSupervisor) {
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.GSupervisor;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < gSupervisor + maxGAgent) {
                    if (member.Rank > Enums.GuildMemberRank.GuideAgent)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.GuideAgent;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else if (amount < gSupervisor + maxGAgent + maxGFollower) {
                    if (member.Rank > Enums.GuildMemberRank.GuideFollower)
                        continue;
                    if (RanksCounts[(ushort)member.Rank] > 0)
                        RanksCounts[(ushort)member.Rank]--;
                    member.Rank = Enums.GuildMemberRank.GuideFollower;
                    RanksCounts[(ushort)member.Rank]++;
                    amount++;
                }
                else {
                    break;
                }
            }

            RankGuideDonations = poll.ToArray();
        }
    }

    public bool GetMember(string name, out Member? member) {
        foreach (var mem in Members.Values.Where(mem => mem.Name == name)) {
            member = mem;
            return true;
        }

        member = null;
        return false;
    }

    public static bool CheckNameExist(string name) {
        return Kernel.Guilds.Values.Any(guilds => guilds.Name == name);
    }

    private bool Create(string name) {
        if (name.Length >= 16) return false;
        if (Leader == null) return false;
        Name = name;
        SilverFund = 500000;
        LeaderId = Leader.Id;
        Members.Add(Leader.Id, Leader);
        try {
            GuildTable.Create(this);
        }
        catch {
            return false;
        }

        Kernel.Guilds.Add(Id, this);
        var message = new Message(
            "Congratulations, " + _leaderName + " has created guild " + name + " Successfully!",
            Color.White, Message.World);
        foreach (var client in Program.Values) client.Send(message);

        CreateTime();
        return true;
    }

    /// <summary>
    ///     Creates a new guild for a player
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
            Id = GuildCounter.Next,
            SilverFund = initialFund,
            LeaderId = client.Entity.UID
        };

        // Create leader member
        client.AsMember = new Member(guild.Id) {
            SilverDonation = 500000,
            Id = client.Entity.UID,
            Level = client.Entity.Level,
            Name = client.Entity.Name,
            Rank = Enums.GuildMemberRank.GuildLeader
        };

        if (client.NobilityInformation != null) {
            client.AsMember.Gender = client.NobilityInformation.Gender;
            client.AsMember.NobilityRank = client.NobilityInformation.Rank;
        }

        // Set up entity
        client.Entity.GuildID = (ushort)guild.Id;
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
        EntityTable.UpdateGuildID(client);
        EntityTable.UpdateGuildRank(client);
        guild.Name = guildName;
        guild.MemberCount++;
        guild.SendGuild(client);
        guild.SendName(client);
        GuildArsenalTable.Insert(guild.Id);
        client.Screen.FullWipe();
        client.Screen.Reload();

        // Send world message
        Kernel.SendWorldMessage(
            new Message(
                $"A new guild [{guildName}] has been created by {client.Entity.Name}!",
                Color.Red, Message.Center),
            Program.Values);

        return true;
    }

    /// <summary>
    ///     Changes the guild name
    /// </summary>
    /// <param name="client">The guild leader requesting the name change</param>
    /// <param name="newName">The new guild name</param>
    /// <returns>True if the name was changed successfully, false otherwise</returns>
    public bool ChangeName(GameState client, string newName) {
        if (string.IsNullOrEmpty(newName) || newName.Length is < 1 or > 16) return false;
        if (CheckNameExist(newName)) return false;

        var oldName = Name;
        GuildTable.ChangeName(client, newName);
        Name = newName;
        SendGuild(client);
        SendName(client);
        client.Screen.FullWipe();
        client.Screen.Reload();

        // Send world message
        Kernel.SendWorldMessage(
            new Message(
                $"The guild [{oldName}] has been renamed to [{newName}] by {client.Entity.Name}.",
                Color.Red, Message.Center),
            Program.Values);

        return true;
    }

    public void CreateBulletinTime(uint time = 0) {
        if (time == 0) {
            var timers = DateTime.Now;
            time = GetTime((uint)timers.Year, (uint)timers.Month, (uint)timers.Day);
            GuildTable.SaveEnrolls(this);
        }

        BulletinEnroll = time;
    }

    public void CreateTime(uint time = 0) {
        if (time == 0) {
            var timers = DateTime.Now;
            time = GetTime((uint)timers.Year, (uint)timers.Month, (uint)timers.Day);
            GuildTable.SaveEnrolls(this);
        }

        GuildEnroll = time;
        WriteUInt32(time, 67, _buffer);
    }

    private static uint GetTime(uint year, uint month, uint day) {
        var timer = year * 10000 + month * 100 + day;
        return timer;
    }

    public void AddMember(GameState client) {
        client.AsMember = new Member(Id) {
            Id = client.Entity.UID,
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
        client.Entity.GuildID = (ushort)client.Guild.Id;
        client.Entity.GuildRank = (ushort)client.AsMember.Rank;
        if (client.Entity.BattlePower < 405)
            client.Entity.GuildBattlePower = GetSharedBattlePower(client.AsMember.Rank);
        for (var i = 0; i < client.ArsenalDonations.Length; i++)
            client.ArsenalDonations[i] = 0;
        EntityTable.UpdateGuildID(client);
        EntityTable.UpdateGuildRank(client);
        Members.Add(client.Entity.UID, client.AsMember);
        SendGuild(client);
        client.Screen.FullWipe();
        client.Screen.Reload();
        SendGuildMessage(new Message(client.AsMember.Name + " has joined our guild.",
            Color.Black, Message.Guild));

        var minGuildDonation = new GuildMinDonations(31);
        minGuildDonation.AprendGuild(this);
        client.Send(minGuildDonation.ToArray());
    }


    public void SendMembers(GameState client, ushort page) {
        var currentTime = (ulong)DateTime.Now.Ticks;
        var memoryStream = new MemoryStream();
        var wtr = new BinaryWriter(memoryStream);
        wtr.Write((ushort)0);
        wtr.Write((ushort)2102);
        wtr.Write((uint)0);
        wtr.Write((uint)page);
        var left = (int)MemberCount - page;
        if (left > 12) left = 12;
        if (left < 0) left = 0;
        wtr.Write((uint)left);
        var count = 0;
        var maxMembers = page + 12;
        int minMembers = page;
        var online = new List<Member>(250);
        var offline = new List<Member>(250);
        foreach (var member in Members.Values)
            if (Kernel.TryGetPlayer(member.Id, out _))
                online.Add(member);
            else
                offline.Add(member);

        online = online.OrderByDescending(mem => mem.Rank).ToList();
        var unite = online.Union(offline);
        foreach (var member in unite) {
            if (count >= minMembers && count < maxMembers) {
                wtr.Write((uint)0);
                var name = Encoding.Default.GetBytes(member.Name);

                for (var j = 0; j < 16; j++)
                    if (name.Length > j) wtr.Write(name[j]);
                    else wtr.Write((byte)0);

                wtr.Write((uint)(Kernel.TryGetPlayer(member.Id, out _) ? 1 : 0));
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
                wtr.Write((uint)((currentTime - member.LastLogin) / 10000000));
                wtr.Write(client.Entity.Mesh);
            }

            count++;
        }

        var packetLength = (int)memoryStream.Length;
        memoryStream.Position = 0;
        wtr.Write((ushort)packetLength);
        memoryStream.Position = memoryStream.Length;
        wtr.Write(Encoding.Default.GetBytes("TQServer"));
        memoryStream.Position = 0;
        var buf = new byte[memoryStream.Length];
        memoryStream.ReadExactly(buf, 0, buf.Length);
        wtr.Close();
        memoryStream.Close();
        client.Send(buf);
    }

    public void SendGuildMessage(IPacket message) {
        foreach (var member in Members.Values)
            if (Kernel.TryGetPlayer(member.Id, out var client))
                client.Send(message);
    }

    public Member? GetMemberByName(string memberName) {
        return Members.Values.FirstOrDefault(member => member.Name == memberName);
    }

    public void ExpelMember(string memberName, bool quit) {
        var member = GetMemberByName(memberName);
        if (member == null) return;
        if (Kernel.TryGetPlayer(member.Id, out var client))
            GuildArsenalHandler.UniscribeAllItems(client);
        else
            foreach (var arsenal in Arsenals)
                arsenal.RemoveInscribedItemsBy(member.Id);

        if (quit)
            SendGuildMessage(new Message(member.Name + " has quit our guild.", Color.Black,
                Message.Guild));
        else
            SendGuildMessage(new Message(member.Name + " have been expelled from our guild.",
                Color.Black, Message.Guild));
        var uid = member.Id;
        if (member.Rank == Enums.GuildMemberRank.DeputyLeader)
            RanksCounts[(ushort)Enums.GuildMemberRank.DeputyLeader]--;
        if (Kernel.TryGetPlayer(member.Id, out var onlineClient)) {
            var command = new GuildCommand(true) {
                Type = GuildCommand.Disband,
                dwParam = Id
            };
            onlineClient.Send(command);
            onlineClient.AsMember = null;
            onlineClient.Guild = null;
            onlineClient.Entity.GuildID = 0;
            onlineClient.Entity.GuildRank = 0;
            onlineClient.Screen.FullWipe();
            onlineClient.Screen.Reload();
            onlineClient.Entity.GuildBattlePower = 0;
        }
        else {
            member.GuildId = 0;
            EntityTable.UpdateData(member.Id, "GuildID", 0);
        }

        MemberCount--;
        Members.Remove(uid);
    }

    /// <summary>
    ///     Disbands the guild
    /// </summary>
    /// <param name="disbandedBy">Optional name of the player who disbanded the guild (for world message)</param>
    public void Disband(string? disbandedBy = null) {
        var guildName = Name;
        var members = Members.Values.ToArray();
        foreach (var member in members) {
            var uid = member.Id;
            if (Kernel.TryGetPlayer(member.Id, out var client)) {
                GuildArsenalHandler.UniscribeAllItems(client);
                client.Entity.GuildBattlePower = 0;
                var command = new GuildCommand(true) {
                    Type = GuildCommand.Disband,
                    dwParam = Id
                };
                client.Entity.GuildID = 0;
                client.Entity.GuildRank = 0;
                client.Send(command);
                client.Screen.FullWipe();
                client.Screen.Reload();
                client.AsMember = null;
                client.Guild = null;
            }
            else {
                foreach (var arsenal in Arsenals)
                    arsenal.RemoveInscribedItemsBy(member.Id);
                member.GuildId = 0;
                EntityTable.UpdateData(member.Id, "GuildID", 0);
            }

            MemberCount--;
            Members.Remove(uid);
        }

        var allies = Ally.Values.ToArray();
        foreach (var ally in allies) {
            RemoveAlly(ally.Name);
            ally.RemoveAlly(Name);
        }

        GuildTable.Disband(this);
        Kernel.Guilds.Remove(Id);

        // Send world message if disbanded by a player
        if (!string.IsNullOrEmpty(disbandedBy))
            Kernel.SendWorldMessage(
                new Message(
                    $"The guild [{guildName}] has been disbanded by {disbandedBy}.",
                    Color.Red, Message.Center),
                Program.Values);
    }

    public void AddAlly(string name) {
        foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name)) {
            // Remove enemy relationship from initiator's side (if exists)
            if (Enemy.ContainsKey(guild.Id)) RemoveEnemy(guild.Name);

            // Remove enemy relationship from target guild's side (if they had marked us as enemy)
            if (guild.Enemy.ContainsKey(Id)) guild.RemoveEnemy(Name);

            Ally.Add(guild.Id, guild);
            var message = new _String(true) {
                UID = guild.Id,
                Type = 0x15
            };
            message.Texts.Add(string.Concat(new object[]
                { guild.Name, " ", guild.LeaderName, " 0 ", guild.MemberCount }));
            SendGuildMessage(message);
            SendGuildMessage(message);
            GuildTable.AddAlly(this, guild.Id);
            return;
        }
    }

    public void RemoveAlly(string name) {
        foreach (var guild in Ally.Values) {
            if (guild.Name != name) continue;
            var cmd = new GuildCommand(true) {
                Type = GuildCommand.Neutral1,
                dwParam = guild.Id
            };
            SendGuildMessage(cmd);
            GuildTable.RemoveAlly(this, guild.Id);
            Ally.Remove(guild.Id);
            return;
        }
    }

    public void AddEnemy(string name) {
        foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name)) {
            if (Ally.ContainsKey(guild.Id)) {
                RemoveAlly(guild.Name);
                guild.RemoveAlly(Name);
            }

            Enemy.Add(guild.Id, guild);
            var stringPacket = new _String(true) {
                UID = guild.Id,
                Type = _String.GuildEnemies
            };
            stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " " + guild.Level + " " +
                                   guild.MemberCount);
            SendGuildMessage(stringPacket);
            SendGuildMessage(stringPacket);
            GuildTable.AddEnemy(this, guild.Id);

            return;
        }
    }

    public void RemoveEnemy(string name) {
        foreach (var guild in Enemy.Values) {
            if (guild.Name != name) continue;
            var cmd = new GuildCommand(true) {
                Type = GuildCommand.Neutral2,
                dwParam = guild.Id
            };
            SendGuildMessage(cmd);
            SendGuildMessage(cmd);
            GuildTable.RemoveEnemy(this, guild.Id);
            Enemy.Remove(guild.Id);

            return;
        }
    }


    public void SendName(GameState client) {
        var stringPacket = new _String(true) {
            UID = Id,
            Type = _String.GuildName
        };
        stringPacket.Texts.Add(Name + " " + LeaderName + " 0 " + MemberCount);
        client.Send(stringPacket);
    }

    public void SendGuild(GameState client) {
        if (!Members.ContainsKey(client.Entity.UID)) return;
        if (client.AsMember == null) return;
        Bulletin ??= "This is a new guild!";

        client.Send(new GuildCommand((uint)Bulletin.Length)
            { Type = GuildCommand.Bulletin, dwParam = BulletinEnroll, Str_ = Bulletin });
        //client.Send(new Message(Bulletin, System.Drawing.Color.White, Message.GuildAnnouncement));
        WriteUInt32((uint)client.AsMember.SilverDonation, 8, _buffer);
        WriteUInt32((ushort)client.AsMember.Rank, 28, _buffer);
        client.Send(_buffer);
    }

    public void SendAllyAndEnemy(GameState client) {
        foreach (var guild in Enemy.Values) {
            var stringPacket = new _String(true) {
                UID = guild.Id,
                Type = _String.GuildEnemies
            };
            stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " 0 " + guild.MemberCount);
            client.Send(stringPacket);
            client.Send(stringPacket);
        }

        foreach (var guild in Ally.Values) {
            var stringPacket = new _String(true) {
                UID = guild.Id,
                Type = _String.GuildAllies
            };
            stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " 0 " + guild.MemberCount);
            client.Send(stringPacket);
            client.Send(stringPacket);
        }
    }

    public static bool ValidName(string name) {
        if (name.Length is < 4 or > 15) return false;
        if (name.IndexOfAny([
                ' ', '#', '%', '^', '&', '*', '(', ')', ';', ':', '\'', '\"', '/', '\\', ',', '.', '{', '}',
                '[', ']'
            ]) > 0) return false;
        return true;
    }

    public abstract class Advertise {
        private static readonly ConcurrentDictionary<uint, Guild> AGuilds = new();

        private static Guild[] _advertiseRanks = [];

        public static Guild[] AdvertiseRanks {
            get {
                lock (_advertiseRanks) {
                    return _advertiseRanks;
                }
            }
        }

        public static void Add(Guild obj) {
            if (!AGuilds.ContainsKey(obj.Id))
                AGuilds.TryAdd(obj.Id, obj);
            CalculateRanks();
        }

        private static void CalculateRanks() {
            lock (_advertiseRanks) {
                var array = AGuilds.Values.ToArray();
                array =
                    (from guild in array orderby guild.AdvertiseRecruit.Donations descending select guild)
                    .ToArray();
                List<Guild> guilds = [];
                for (ushort x = 0; x < array.Length; x++) {
                    guilds.Add(array[x]);
                    if (x == 40) break;
                }

                _advertiseRanks = guilds.ToArray();
            }
        }

        public static void FixedRank() {
            AGuilds.Clear();
            foreach (var guild in _advertiseRanks) AGuilds.TryAdd(guild.Id, guild);
        }
    }

    public class Recruitment {
        public enum Mode {
            Requirements,
            Recruit
        }

        public bool AutoJoin = true;
        public string Bulletin = "Nothing";
        public ulong Donations;
        public byte Grade;
        public byte Level;
        public int NotAllowFlag;
        public byte Reborn;

        public bool WasLoad;

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
                         + (byte)(AutoJoin ? 1 : 0) + "^" + Bulletin + "^0" + "^0");
            return build.ToString();
        }

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
            Bulletin = data[6];
            WasLoad = true;
        }

        public static void Save() { }

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
    }

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

    public class Member(uint guildId) //: Interfaces.IKnownPerson
    {
        public uint ArsenalDonation;

        public byte Class;
        public uint CtfCpsReward;

        public uint CtfSilverReward;
        public uint Exploits = 0;
        public uint ExploitsRank;
        public ulong LastLogin = 0;

        public uint Lilies;
        public uint Mesh;
        public uint Orchids;
        public uint PkDonation;
        public uint Roses;
        public string Spouse = string.Empty;
        public uint Tulips;
        public uint VirtuePoints;
        public uint WarScore;
        public uint Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public GameState? Client => Kernel.TryGetPlayer(Id, out var client) ? client : null;

        public ulong SilverDonation { get; set; }
        public ulong ConquerPointDonation { get; set; }
        public uint GuildId { get; set; } = guildId;

        public Guild Guild => Kernel.Guilds[GuildId];

        public Enums.GuildMemberRank Rank { get; set; }
        public byte Level { get; set; }
        public NobilityRank NobilityRank { get; set; }
        public byte Gender { get; set; }

        public uint TotalDonation =>
            (uint)(Lilies + Orchids + Tulips + Roses + ConquerPointDonation + VirtuePoints +
                   (uint)SilverDonation + ArsenalDonation + PkDonation);
    }
}