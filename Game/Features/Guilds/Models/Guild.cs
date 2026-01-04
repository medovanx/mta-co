using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Database;
using MTA.Game.ConquerStructures;
using MTA.Game.Events.GuildWar;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Packets.Handlers;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Game.Features.Guilds.Services;
using MTA.Interfaces;
using MTA.Network;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Guilds.Models;

public class Guild : Writer {
    public static Counter GuildCounter = new(0);

    private readonly byte[] _buffer;

    public readonly GuildRecruitment AdvertiseRecruit;
    public readonly SafeDictionary<uint, Guild> Ally;

    public readonly Arsenal[] Arsenals;
    public readonly List<uint> BlackList = [];
    public readonly SafeDictionary<uint, Guild> Enemy;

    public readonly ushort[] RanksCounts = new ushort[(ushort)MemberRank.GuildLeader + 1];

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

    public GuildMember? Leader;
    public uint LiliesDonation = 0;
    public uint Loses;

    public SafeDictionary<uint, GuildMember> Members;
    public ulong MoneyDonation = 0;

    public string Name;
    public uint OrchidDonation = 0;
    public uint PaScore;
    public uint PkpDonation = 0;
    public uint PpScore;
    public GuildMember[] RankArsenalDonations = [];
    public GuildMember[] RankCpDonations = [];
    public GuildMember[] RankGuideDonations = [];
    public GuildMember[] RankLiliesDonations = [];
    public GuildMember[] RankOrchidsDonations = [];
    public GuildMember[] RankPkDonations = [];
    public GuildMember[] RankRoseDonations = [];


    public GuildMember[] RankSilversDonations = [];
    public GuildMember[] RankTotalDonations = [];
    public GuildMember[] RankTulipsDonations = [];
    public uint RoseDonation = 0;
    public uint SWarScore;
    public uint TulipDonation = 0;
    public uint WarScore;
    public uint Wins;

    public Guild(string leaderName) {
        _buffer = new byte[92 + 8];
        Members = new SafeDictionary<uint, GuildMember>();
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
        Members = new SafeDictionary<uint, GuildMember>(1000);
        Ally = new SafeDictionary<uint, Guild>(1000);
        Enemy = new SafeDictionary<uint, Guild>(1000);

        Arsenals = new Arsenal[8];
        for (byte i = 0; i < 8; i++) {
            Arsenals[i] = new Arsenal(this) {
                Position = (byte)(i + 1)
            };
        }

        AdvertiseRecruit = new GuildRecruitment();
    }

    private int UnlockedArsenals {
        get {
            var unlocked = 0;
            for (var i = 0; i < 8; i++) {
                if (Arsenals[i].Unlocked)
                    unlocked++;
            }

            return unlocked;
        }
    }

    public int ArsenalTotalBattlePower {
        get => _arsenalBp;
        set {
            _arsenalBp = value;
            foreach (var member in Members.Values) {
                if (Kernel.TryGetPlayer(member.Id, out var client))
                    client.Entity.GuildBattlePower = GetSharedBattlePower(member.Rank);
            }
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
        get => (ClassRequirement & GuildClassRequirements.Trojan) != GuildClassRequirements.Trojan;
        set {
            if (value)
                ClassRequirement &= ~GuildClassRequirements.Trojan;
            else
                ClassRequirement |= GuildClassRequirements.Trojan;
        }
    }

    public bool AllowWarriors {
        get => (ClassRequirement & GuildClassRequirements.Warrior) != GuildClassRequirements.Warrior;
        set {
            if (value)
                ClassRequirement &= ~GuildClassRequirements.Warrior;
            else
                ClassRequirement |= GuildClassRequirements.Warrior;
        }
    }

    public bool AllowTaoists {
        get => (ClassRequirement & GuildClassRequirements.Taoist) != GuildClassRequirements.Taoist;
        set {
            if (value)
                ClassRequirement &= ~GuildClassRequirements.Taoist;
            else
                ClassRequirement |= GuildClassRequirements.Taoist;
        }
    }

    public bool AllowArchers {
        get => (ClassRequirement & GuildClassRequirements.Archer) != GuildClassRequirements.Archer;
        set {
            if (value)
                ClassRequirement &= ~GuildClassRequirements.Archer;
            else
                ClassRequirement |= GuildClassRequirements.Archer;
        }
    }

    public bool AllowNinjas {
        get => (ClassRequirement & GuildClassRequirements.Ninja) != GuildClassRequirements.Ninja;
        set {
            if (value)
                ClassRequirement &= ~GuildClassRequirements.Ninja;
            else
                ClassRequirement |= GuildClassRequirements.Ninja;
        }
    }

    public bool AllowMonks {
        get => (ClassRequirement & GuildClassRequirements.Monk) != GuildClassRequirements.Monk;
        set {
            if (value)
                ClassRequirement &= ~GuildClassRequirements.Monk;
            else
                ClassRequirement |= GuildClassRequirements.Monk;
        }
    }

    public bool AllowPirates {
        get => (ClassRequirement & GuildClassRequirements.Pirate) != GuildClassRequirements.Pirate;
        set {
            if (value)
                ClassRequirement &= ~GuildClassRequirements.Pirate;
            else
                ClassRequirement |= GuildClassRequirements.Pirate;
        }
    }

    public byte Level {
        get => _buffer[60];
        private set => _buffer[60] = value;
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

    /// <summary>
    ///     Calculates cost to unlock next arsenal slot based on how many are already unlocked.
    /// </summary>
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

    /// <summary>
    ///     Calculates maximum shared battle power from arsenals, taking top 5 arsenals and updating guild level based on arsenal quality.
    /// </summary>
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
        foreach (var arsenal in arsenals) {
            if (arsenal.TotalSharedBattlePower >= 2)
                level++;
        }

        Level = level;

        return _arsenalBp;
    }

    /// <summary>
    ///     Calculates total member potency based on arsenal battle power, used for promotion rank limits.
    /// </summary>
    public uint GetMemberPotency() {
        return (uint)_arsenalBp;
    }

    /// <summary>
    ///     Gets potency for specific rank, currently returns total arsenal battle power (maybe enhanced with rank-specific calculations).
    /// </summary>
    public uint GetMemberPotency(MemberRank rank) {
        // Calculate potency based on rank and arsenal BP
        // This is a placeholder - the actual calculation may need to be implemented based on game logic
        return (uint)_arsenalBp;
    }

    /// <summary>
    ///     Gets shared battle power for numeric rank, used to calculate member's bonus battle power from guild arsenals.
    /// </summary>
    public uint GetSharedBattlePower(int rank) {
        return
            GetMemberPotency(
                (MemberRank)rank); //(uint)(arsenal_bp * SharedBattlePowerPercentage[rank / 100]);
    }

    /// <summary>
    ///     Gets shared battle power for rank enum, used to calculate member's bonus battle power from guild arsenals.
    /// </summary>
    public uint GetSharedBattlePower(MemberRank rank) {
        return GetSharedBattlePower((int)rank);
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

    /// <summary>
    ///     Saves arsenal data to database, persisting all inscribed items and arsenal states.
    /// </summary>
    public void SaveArsenal() {
        GuildArsenalTable.Save(this);
    }

    /// <summary>
    ///     Static method to send guild profile packet, displaying member donation information to the client.
    /// </summary>
    public static void GuildProfile(byte[] packet, GameState client) {
        var p = new GuildProfilePacket(packet);
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
    /// <summary>
    ///     Calculates Capture the Flag rankings and rewards, ranking members by exploits and distributing CTF rewards.
    /// </summary>
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
            GuildMemberTable.Save(mem);
        }
    }

    private uint[] CalculateRewardCtf(uint rank) {
        var rew = new uint[2];
        rew[0] = CTFDonationSilverOld / (rank + 1);
        rew[1] = CTFDonationCPSold / (rank + 1);
        return rew;
    }

    // CreateMembersRank has been moved to GuildRankAssignment.AssignRanks()
    // This method is kept for backward compatibility but delegates to the new location
    /// <summary>
    ///     Initializes member ranking arrays, delegating to GuildRankAssignment for automatic rank assignment based on donations.
    /// </summary>
    public void CreateMembersRank() {
        GuildRankAssignment.AssignRanks(this);
    }

    /// <summary>
    ///     Retrieves member by name, returning true if found and setting the member output parameter.
    /// </summary>
    public bool GetMember(string name, out GuildMember? member) {
        foreach (var mem in Members.Values.Where(mem => mem.Name == name)) {
            member = mem;
            return true;
        }

        member = null;
        return false;
    }

    /// <summary>
    ///     Validates if guild name is available, checking against all existing guild names.
    /// </summary>
    public static bool CheckNameExist(string name) {
        return Kernel.Guilds.Values.Any(guilds => guilds.Name == name);
    }

    private void Create(string name) {
        if (name.Length >= 16) return;
        if (Leader == null) return;
        Name = name;
        SilverFund = 500000;
        LeaderId = Leader.Id;
        Members.Add(Leader.Id, Leader);
        GuildTable.Create(this);
        Kernel.Guilds.Add(Id, this);
        var message = new Message(
            "Congratulations, " + _leaderName + " has created guild " + name + " Successfully!",
            Color.White, Message.World);
        foreach (var client in Program.Values) {
            client.Send(message);
        }

        CreateTime();
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
        client.AsMember = new GuildMember(guild.Id) {
            SilverDonation = 500000,
            Id = client.Entity.UID,
            Level = client.Entity.Level,
            Name = client.Entity.Name,
            Rank = MemberRank.GuildLeader
        };

        if (client.NobilityInformation != null) {
            client.AsMember.Gender = client.NobilityInformation.Gender;
            client.AsMember.NobilityRank = client.NobilityInformation.Rank;
        }

        // Set up entity
        client.Entity.GuildID = (ushort)guild.Id;
        client.Entity.GuildRank = (ushort)MemberRank.GuildLeader;
        guild.Leader = client.AsMember;
        client.Guild = guild;

        // Create guild in database
        guild.Create(guildName);

        // Insert leader into guild_members table
        // Note: Create() method already adds leader to Members dictionary, but we need to insert into database
        GuildMemberTable.Insert(client.AsMember);

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

    /// <summary>
    ///     Sets bulletin creation timestamp, used to track when guild bulletin was last updated.
    /// </summary>
    public void CreateBulletinTime(uint time = 0) {
        if (time == 0) {
            var timers = DateTime.Now;
            time = GetTime((uint)timers.Year, (uint)timers.Month, (uint)timers.Day);
            GuildTable.SaveEnrolls(this);
        }

        BulletinEnroll = time;
    }

    /// <summary>
    ///     Sets guild creation timestamp, tracking when the guild was originally created.
    /// </summary>
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

    /// <summary>
    ///     Adds new member to guild, initializing member record, updating entity references, and sending notifications.
    /// </summary>
    public void AddMember(GameState client) {
        client.AsMember = new GuildMember(Id) {
            Id = client.Entity.UID,
            Level = client.Entity.Level,
            Name = client.Entity.Name,
            Rank = MemberRank.Member,
            Mesh = client.Entity.Mesh,
            LastLogin = (ulong)DateTime.Now.Ticks
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
        for (var i = 0; i < client.ArsenalDonations.Length; i++) {
            client.ArsenalDonations[i] = 0;
        }

        // Insert member into guild_members table
        GuildMemberTable.Insert(client.AsMember);
        Members.Add(client.Entity.UID, client.AsMember);
        SendGuild(client);
        client.Screen.FullWipe();
        client.Screen.Reload();
        SendGuildMessage(new Message(client.AsMember.Name + " has joined our guild.",
            Color.Black, Message.Guild));

        var minGuildDonation = new GuildMinDonations(31);
        minGuildDonation.AppendGuild(this);
        client.Send(minGuildDonation.ToArray());
    }


    /// <summary>
    ///     Sends member list packet to client with pagination, showing online members first, then offline members.
    /// </summary>
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
        var online = new List<GuildMember>(250);
        var offline = new List<GuildMember>(250);
        foreach (var member in Members.Values) {
            if (Kernel.TryGetPlayer(member.Id, out _))
                online.Add(member);
            else
                offline.Add(member);
        }

        online = online.OrderByDescending(mem => mem.Rank).ToList();
        var unite = online.Union(offline);
        foreach (var member in unite) {
            if (count >= minMembers && count < maxMembers) {
                wtr.Write((uint)0);
                var name = Encoding.Default.GetBytes(member.Name);

                for (var j = 0; j < 16; j++) {
                    if (name.Length > j) wtr.Write(name[j]);
                    else wtr.Write((byte)0);
                }

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

    /// <summary>
    ///     Broadcasts message to all online members of the guild.
    /// </summary>
    public void SendGuildMessage(IPacket message) {
        foreach (var member in Members.Values) {
            if (Kernel.TryGetPlayer(member.Id, out var client))
                client.Send(message);
        }
    }

    /// <summary>
    ///     Gets member by name, returning null if not found.
    /// </summary>
    public GuildMember? GetMemberByName(string memberName) {
        return Members.Values.FirstOrDefault(member => member.Name == memberName);
    }

    /// <summary>
    ///     Removes member from guild, uninscribing all their items, updating database, and sending notifications.
    /// </summary>
    public void ExpelMember(string memberName, bool quit) {
        var member = GetMemberByName(memberName);
        if (member == null) return;
        if (Kernel.TryGetPlayer(member.Id, out var client))
            GuildArsenalHandler.UniscribeAllItems(client);
        else
            foreach (var arsenal in Arsenals) {
                arsenal.RemoveInscribedItemsBy(member.Id);
            }

        if (quit)
            SendGuildMessage(new Message(member.Name + " has quit our guild.", Color.Black,
                Message.Guild));
        else
            SendGuildMessage(new Message(member.Name + " have been expelled from our guild.",
                Color.Black, Message.Guild));
        var uid = member.Id;
        if (member.Rank == MemberRank.DeputyLeader)
            RanksCounts[(ushort)MemberRank.DeputyLeader]--;
        if (Kernel.TryGetPlayer(member.Id, out var onlineClient)) {
            var command = new GuildCommand(true) {
                Type = GuildCommand.Disband,
                DwParam = Id
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
            // Delete from guild_members table
            GuildMemberTable.Delete(uid);
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
                    DwParam = Id
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
                foreach (var arsenal in Arsenals) {
                    arsenal.RemoveInscribedItemsBy(member.Id);
                }

                member.GuildId = 0;
                // Delete from guild_members table
                GuildMemberTable.Delete(uid);
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

    /// <summary>
    ///     Adds alliance relationship, removing any existing enemy relationship and notifying all members.
    /// </summary>
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

    /// <summary>
    ///     Removes alliance relationship, notifying all members and updating database.
    /// </summary>
    public void RemoveAlly(string name) {
        foreach (var guild in Ally.Values) {
            if (guild.Name != name) continue;
            var cmd = new GuildCommand(true) {
                Type = GuildCommand.Neutral1,
                DwParam = guild.Id
            };
            SendGuildMessage(cmd);
            GuildTable.RemoveAlly(this, guild.Id);
            Ally.Remove(guild.Id);
            return;
        }
    }

    /// <summary>
    ///     Adds enemy relationship, removing any existing alliance and notifying all members.
    /// </summary>
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

    /// <summary>
    ///     Removes enemy relationship, notifying all members and updating database.
    /// </summary>
    public void RemoveEnemy(string name) {
        foreach (var guild in Enemy.Values) {
            if (guild.Name != name) continue;
            var cmd = new GuildCommand(true) {
                Type = GuildCommand.Neutral2,
                DwParam = guild.Id
            };
            SendGuildMessage(cmd);
            SendGuildMessage(cmd);
            GuildTable.RemoveEnemy(this, guild.Id);
            Enemy.Remove(guild.Id);

            return;
        }
    }


    /// <summary>
    ///     Sends guild name to client, displaying guild information in the UI.
    /// </summary>
    public void SendName(GameState client) {
        var stringPacket = new _String(true) {
            UID = Id,
            Type = _String.GuildName
        };
        stringPacket.Texts.Add(Name + " " + LeaderName + " 0 " + MemberCount);
        client.Send(stringPacket);
    }

    /// <summary>
    ///     Sends complete guild data packet to client, including bulletin, member donations, and rank.
    /// </summary>
    public void SendGuild(GameState client) {
        if (!Members.ContainsKey(client.Entity.UID)) return;
        if (client.AsMember == null) return;
        Bulletin ??= "This is a new guild!";

        client.Send(new GuildCommand((uint)Bulletin.Length)
            { Type = GuildCommand.Bulletin, DwParam = BulletinEnroll, Str = Bulletin });
        //client.Send(new Message(Bulletin, System.Drawing.Color.White, Message.GuildAnnouncement));
        WriteUInt32((uint)client.AsMember.SilverDonation, 8, _buffer);
        WriteUInt32((ushort)client.AsMember.Rank, 28, _buffer);
        client.Send(_buffer);
    }

    /// <summary>
    ///     Sends alliance/enemy list to client, displaying all diplomatic relationships.
    /// </summary>
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

    /// <summary>
    ///     Validates guild name format, checking length and disallowed characters.
    /// </summary>
    public static bool ValidName(string name) {
        if (name.Length is < 4 or > 15) return false;
        if (name.IndexOfAny([
                ' ', '#', '%', '^', '&', '*', '(', ')', ';', ':', '\'', '\"', '/', '\\', ',', '.', '{', '}',
                '[', ']'
            ]) > 0) return false;
        return true;
    }
}