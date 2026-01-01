using System.Collections.Generic;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.ConquerStructures;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Database.Mappers;
using MTA.Game.Features.Guilds.Database.Schema;

namespace MTA.Game.Features.Guilds.Database;

using Member = Guild.Member;

public static class GuildTable {
    public static void Load() {
        var dict = new Dictionary<uint, SafeDictionary<uint, Member>>();
        using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("guildid", 0, true))
        using (var reader = new MySqlReader(cmd)) {
            while (reader.Read()) {
                var member = new Member(reader.ReadUInt16("guildid")) {
                    Id = reader.ReadUInt32("uid"),
                    Name = reader.ReadString("name"),
                    Level = reader.ReadByte("level"),
                    Spouse = reader.ReadString("Spouse")
                };

                if (Nobility.Board.TryGetValue(member.Id, out var value)) {
                    member.NobilityRank = value.Rank;
                    member.Gender = value.Gender;
                }

                member.Rank = (MemberRank)reader.ReadUInt16("guildrank");
                member.SilverDonation = reader.ReadUInt64("GuildSilverDonation");
                member.ConquerPointDonation = reader.ReadUInt64("GuildConquerPointDonation");
                member.ArsenalDonation = reader.ReadUInt32("GuildArsenalDonation");
                member.Class = reader.ReadByte("Class");
                member.VirtuePoints = reader.ReadUInt32("VirtuePoints");

                member.Lilies = reader.ReadUInt32("GuildLilies");
                member.Roses = reader.ReadUInt32("GuildRouses");
                member.Orchids = reader.ReadUInt32("GuildOrchids");
                member.Tulips = reader.ReadUInt32("GuildTulips");
                member.PkDonation = reader.ReadUInt32("GuildPkDonation");
                member.LastLogin = reader.ReadUInt64("GuildLastlod");

                member.Exploits = reader.ReadUInt32("Exploits");
                member.CtfCpsReward = reader.ReadUInt32("CTFCpsReward");
                member.CtfSilverReward = reader.ReadUInt32("CTFSilverReward");

                member.Mesh =
                    uint.Parse(reader.ReadUInt16("Face").ToString() + reader.ReadUInt16("Body").ToString());
                if (!dict.ContainsKey(member.GuildId)) dict.Add(member.GuildId, new SafeDictionary<uint, Member>());
                dict[member.GuildId].Add(member.Id, member);
            }
        }

        using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(GuildSchema.Tables.GuildsTable))
        using (var reader = new MySqlReader(cmd)) {
            while (reader.Read()) {
                var record = GuildMappers.MapGuild(reader);
                var leaderId = record.LeaderID;
                // Get leader name from entities table using LeaderID (LeaderName column is deprecated)
                var leaderName = string.Empty;
                if (leaderId > 0) {
                    try {
                        using var nameCmd = new MySqlCommand(MySqlCommandType.SELECT)
                            .Select("entities")
                            .Where("UID", leaderId);
                        using var nameReader = new MySqlReader(nameCmd);
                        if (nameReader.Read()) {
                            leaderName = nameReader.ReadString("Name");
                        }
                    }
                    catch {
                        // If query fails, use empty string (computed property will handle it)
                    }
                }

                var guild = new Guild(leaderName) {
                    Id = record.Id,
                    Name = record.Name,
                    Wins = record.Wins,
                    Loses = record.Loses,
                    Bulletin = record.Bulletin,
                    SilverFund = record.SilverFund,
                    CtfPoints = record.CTFPoints,
                    CtfReward = record.CTFReward,
                    ConquerPointFund = record.ConquerPointFund,
                    LevelRequirement = record.LevelRequirement,
                    RebornRequirement = record.RebornRequirement,
                    ClassRequirement = record.ClassRequirement,
                    LeaderId = leaderId
                };
                guild.AdvertiseRecruit.Load(record.Advertise);
                guild.GuildEnroll = uint.TryParse(record.GuildEnroll, out var guildEnroll) ? guildEnroll : 0;
                guild.CreateTime(guild.GuildEnroll);
                guild.BulletinEnroll = uint.TryParse(record.BulletinEnroll, out var bulletinEnroll) ? bulletinEnroll : 0;
                guild.CTFDonationCPs = uint.TryParse(record.CTFDonationCps, out var ctfCps) ? ctfCps : 0;
                guild.CTFDonationSilver = uint.TryParse(record.CTFDonationSilver, out var ctfSilver) ? ctfSilver : 0;
                guild.CTFDonationSilverOld = uint.TryParse(record.CTFdonationSilverold, out var ctfSilverOld) ? ctfSilverOld : 0;
                guild.CTFDonationCPSold = uint.TryParse(record.CTFdonationCpsold, out var ctfCpsOld) ? ctfCpsOld : 0;

                guild.CreateTime(guild.BulletinEnroll);
                if (dict.TryGetValue(guild.Id, out var value)) {
                    guild.Members = value;
                    guild.MemberCount = (uint)guild.Members.Count;
                }
                else
                    guild.Members = new SafeDictionary<uint, Member>();

                Kernel.Guilds.Add(guild.Id, guild);
                foreach (var member in guild.Members.Values) {
                    if (member.Rank == MemberRank.GuildLeader) {
                        guild.Leader = member;
                        // Ensure LeaderID matches the leader's ID
                        if (guild.LeaderId == 0 || guild.LeaderId != member.Id) {
                            guild.LeaderId = member.Id;
                        }
                    }

                    guild.RanksCounts[(ushort)member.Rank]++;
                }

                GuildArsenalTable.Load(guild);
                //foreach (var member in guild.Members.Values)
                //{
                //    uint getdonation = 0;
                //    foreach (var ars in guild.Arsenals)
                //    {
                //        if (ars.Unlocked)
                //        {
                //            foreach (var arsobj in ars.ItemDictionary.Values)
                //                getdonation += arsobj.DonationWorth;
                //        }
                //    }
                //    member.ArsenalDonation = getdonation;
                //}
            }
        }

        LoadAllyEnemy();

        //create members ranks
        foreach (var guild in Kernel.Guilds.Values) {
            guild.CreateMembersRank();
            if (guild.AdvertiseRecruit.WasLoad)
                Guild.Advertise.Add(guild);
            guild.CalculateCTFRank();
        }

        Guild.Advertise.FixedRank();
        //create leader spouse
        foreach (var guild in Kernel.Guilds.Values) {
            foreach (var member in guild.Members.Values
                         .Where(member => member.Spouse != "None").Where(member =>
                             member.Rank != MemberRank.GuildLeader)) {
                foreach (var findSpouse in
                         guild.Members.Values.Where(findSpouse => member.Spouse == findSpouse.Name)) {
                    if (findSpouse.Rank == MemberRank.GuildLeader) {
                        member.Rank = MemberRank.LeaderSpouse;
                        break;
                    }

                    if (findSpouse.Rank != MemberRank.DeputyLeader) continue;
                    if (member.Rank == MemberRank.DeputyLeader)
                        break;
                    if (member.Rank > MemberRank.DLeaderSpouse)
                        break;
                    member.Rank = MemberRank.DLeaderSpouse;
                    break;
                }
            }
        }

        Console.WriteLine("Guild information loaded.");
    }

    private static void LoadAllyEnemy() {
        // Load allies from guild_relations (bidirectional - relation_type=1)
        using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(GuildSchema.Tables.GuildRelationsTable)
                   .Where(GuildSchema.GuildRelations.RelationType, 1))
        using (var reader = new MySqlReader(cmd)) {
            while (reader.Read()) {
                var record = GuildMappers.MapGuildRelation(reader);
                var guildId = record.GuildId;
                var allyId = record.RelatedGuildId;
                if (!Kernel.Guilds.TryGetValue(guildId, out var guild)) continue;
                if (Kernel.Guilds.TryGetValue(allyId, out var ally))
                    guild.Ally.Add(allyId, ally);
            }
        }

        // Load enemies from guild_relations (one-way storage, but display on both sides)
        using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(GuildSchema.Tables.GuildRelationsTable)
                   .Where(GuildSchema.GuildRelations.RelationType, 0))
        using (var reader = new MySqlReader(cmd)) {
            while (reader.Read()) {
                var record = GuildMappers.MapGuildRelation(reader);
                var guildId = record.GuildId;
                var enemyId = record.RelatedGuildId;

                // Add to initiator's enemy list (for removal permissions)
                if (Kernel.Guilds.TryGetValue(guildId, out var initiatorGuild))
                    if (Kernel.Guilds.TryGetValue(enemyId, out var enemyGuild))
                        initiatorGuild.Enemy.Add(enemyId, enemyGuild);

                // Also add to related guild's enemy list (for display only)
                if (!Kernel.Guilds.TryGetValue(enemyId, out var relatedGuild)) continue;
                if (Kernel.Guilds.TryGetValue(guildId, out var initiatorGuild2))
                    relatedGuild.Enemy.Add(guildId, initiatorGuild2);
            }
        }
    }

    public static void UpdateBulletin(Guild guild, string bulletin) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.Bulletin, bulletin)
            .Set(GuildSchema.Guilds.BulletinEnroll, guild.BulletinEnroll)
            .Where(GuildSchema.Guilds.Id, guild.Id);
        cmd.Execute();
    }

    public static void SaveFunds(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.ConquerPointFund, guild.ConquerPointFund)
            .Set(GuildSchema.Guilds.SilverFund, guild.SilverFund)
            .Set(GuildSchema.Guilds.CTFdonationCpsold, guild.CTFDonationCPSold.ToString())
            .Set(GuildSchema.Guilds.CTFdonationSilverold, guild.CTFDonationSilverOld.ToString())
            .Set(GuildSchema.Guilds.CTFDonationCps, guild.CTFDonationCPs.ToString())
            .Set(GuildSchema.Guilds.CTFDonationSilver, guild.CTFDonationSilver.ToString())
            .Where(GuildSchema.Guilds.Id, guild.Id);
        cmd.Execute();
    }

    public static void SaveEnrolls(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.GuildEnroll, guild.GuildEnroll.ToString())
            .Set(GuildSchema.Guilds.BulletinEnroll, guild.BulletinEnroll.ToString())
            .Where(GuildSchema.Guilds.Id, guild.Id);
        cmd.Execute();
    }

    public static void SaveAdvertise(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.Advertise, guild.AdvertiseRecruit.ToString())
            .Set(GuildSchema.Guilds.SilverFund, guild.SilverFund)
            .Where(GuildSchema.Guilds.Id, guild.Id);
        cmd.Execute();
    }

    public static void SaveCtfPoins(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.CTFPoints, guild.CtfPoints)
            .Where(GuildSchema.Guilds.Id, guild.Id);
        cmd.Execute();
    }

    public static void SaveCtfReward(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.CTFReward, guild.CtfReward)
            .Set(GuildSchema.Guilds.CTFPoints, guild.CtfPoints)
            .Where(GuildSchema.Guilds.Id, guild.Id);
        cmd.Execute();
    }

    public static void Disband(Guild guild) {
        using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("entities")
                   .Set("guildid", 0)
                   .Where("guildid", guild.Id))
            cmd.Execute();
        using (var cmd = new MySqlCommand(MySqlCommandType.DELETE).Delete(GuildSchema.Tables.GuildsTable,
                   GuildSchema.Guilds.Id, guild.Id))
            cmd.Execute();
    }

    public static void Create(Guild guild) {
        while (true) {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(GuildSchema.Tables.GuildsTable)
                .Where(GuildSchema.Guilds.Id, guild.Id);
            using var reader = cmd.CreateReader();
            if (reader.Read())
                guild.Id = Guild.GuildCounter.Next;
            else
                break;
        }

        using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert(GuildSchema.Tables.GuildsTable)
                   .Insert(GuildSchema.Guilds.Id, guild.Id)
                   .Insert(GuildSchema.Guilds.Name, guild.Name)
                   .Insert(GuildSchema.Guilds.Bulletin, "")
                   .Insert(GuildSchema.Guilds.SilverFund, 500000)
                   .Insert(GuildSchema.Guilds.LeaderID, guild.LeaderId))
            cmd.Execute();
    }

    public static void ChangeName(GameState client, string name) {
        using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
                   .Set(GuildSchema.Guilds.Name, name).Where(GuildSchema.Guilds.Id, client.Guild!.Id))
            cmd.Execute();
        using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(GuildSchema.Tables.GuildsTable))
        using (var reader = new MySqlReader(cmd)) {
            while (reader.Read()) {
                var record = GuildMappers.MapGuild(reader);
                if (record.Id == client.Guild.Id) {
                    client.Guild.Name = record.Name;
                    break;
                }
            }
        }
    }

    public static void AddEnemy(Guild guild, uint enemy) {
        // Insert one-way enemy relationship (independent per guild)
        using var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert(GuildSchema.Tables.GuildRelationsTable)
            .Insert(GuildSchema.GuildRelations.GuildId, guild.Id)
            .Insert(GuildSchema.GuildRelations.RelatedGuildId, enemy)
            .Insert(GuildSchema.GuildRelations.RelationType, 0);
        cmd.Execute();
    }

    public static void AddAlly(Guild guild, uint ally) {
        // Insert bidirectional ally relationship (both directions)
        using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert(GuildSchema.Tables.GuildRelationsTable)
                   .Insert(GuildSchema.GuildRelations.GuildId, guild.Id)
                   .Insert(GuildSchema.GuildRelations.RelatedGuildId, ally)
                   .Insert(GuildSchema.GuildRelations.RelationType, 1))
            cmd.Execute();
        using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert(GuildSchema.Tables.GuildRelationsTable)
                   .Insert(GuildSchema.GuildRelations.GuildId, ally)
                   .Insert(GuildSchema.GuildRelations.RelatedGuildId, guild.Id)
                   .Insert(GuildSchema.GuildRelations.RelationType, 1))
            cmd.Execute();
    }

    public static bool IsEnemyInitiator(Guild guild, uint enemyId) {
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
            .Select(GuildSchema.Tables.GuildRelationsTable)
            .Where(GuildSchema.GuildRelations.GuildId, guild.Id)
            .And(GuildSchema.GuildRelations.RelatedGuildId, enemyId)
            .And(GuildSchema.GuildRelations.RelationType, 0);
        using var reader = new MySqlReader(cmd);
        return reader.Read();
    }

    public static void RemoveEnemy(Guild guild, uint enemy) {
        // Remove one-way enemy relationship (only from initiator)
        using var command = new MySqlCommand(MySqlCommandType.DELETE);
        command.Delete(GuildSchema.Tables.GuildRelationsTable, GuildSchema.GuildRelations.GuildId, guild.Id)
            .And(GuildSchema.GuildRelations.RelatedGuildId, enemy)
            .And(GuildSchema.GuildRelations.RelationType, 0)
            .Execute();
    }

    public static void RemoveAlly(Guild guild, uint ally) {
        // Remove bidirectional ally relationship (both directions)
        using (var command = new MySqlCommand(MySqlCommandType.DELETE))
            command.Delete(GuildSchema.Tables.GuildRelationsTable, GuildSchema.GuildRelations.GuildId, guild.Id)
                .And(GuildSchema.GuildRelations.RelatedGuildId, ally)
                .And(GuildSchema.GuildRelations.RelationType, 1)
                .Execute();
        using (var command = new MySqlCommand(MySqlCommandType.DELETE))
            command.Delete(GuildSchema.Tables.GuildRelationsTable, GuildSchema.GuildRelations.GuildId, ally)
                .And(GuildSchema.GuildRelations.RelatedGuildId, guild.Id)
                .And(GuildSchema.GuildRelations.RelationType, 1)
                .Execute();
    }

    public static void UpdateGuildWarStats(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.Wins, guild.Wins)
            .Set(GuildSchema.Guilds.Losts, guild.Loses)
            .Where(GuildSchema.Guilds.Id, guild.Id);
        cmd.Execute();
    }

    // NOTE: PoleKeeper columns (PoleKeeperTc, PoleKeeperPh, PoleKeeperAp) do not exist in the database.
    // These methods are kept for API compatibility but will need the columns added to the database to function.
    public static void UpdatePoleKeeperTc(Guild guild) {
        // Columns don't exist in database - method disabled
        // TODO: Add PoleKeeperTc column to guilds table if this functionality is needed
        // using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
        //            .Set("PoleKeeperTc", 0))
        //     cmd.Execute();
        // using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
        //            .Set("PoleKeeperTc", 1).Where(GuildSchema.Guilds.Id, guild.Id))
        //     cmd.Execute();
    }

    public static void UpdatePoleKeeperPh(Guild guild) {
        // Columns don't exist in database - method disabled
        // TODO: Add PoleKeeperPh column to guilds table if this functionality is needed
        // using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
        //            .Set("PoleKeeperPh", 0))
        //     cmd.Execute();
        // using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
        //            .Set("PoleKeeperPh", 1).Where(GuildSchema.Guilds.Id, guild.Id))
        //     cmd.Execute();
    }

    public static void UpdatePoleKeeperAp(Guild guild) {
        // Columns don't exist in database - method disabled
        // TODO: Add PoleKeeperAp column to guilds table if this functionality is needed
        // using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
        //            .Set("PoleKeeperAp", 0))
        //     cmd.Execute();
        // using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
        //            .Set("PoleKeeperAp", 1).Where(GuildSchema.Guilds.Id, guild.Id))
        //     cmd.Execute();
    }

    public static void SaveLeader(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.LeaderID, guild.LeaderId)
            .Where(GuildSchema.Guilds.Id, guild.Id);
        cmd.Execute();
    }

    internal static void SaveRequirements(Guild guild) {
        using var command = new MySqlCommand(MySqlCommandType.UPDATE);
        command.Update(GuildSchema.Tables.GuildsTable)
            .Set(GuildSchema.Guilds.LevelRequirement, guild.LevelRequirement)
            .Set(GuildSchema.Guilds.RebornRequirement, guild.RebornRequirement)
            .Set(GuildSchema.Guilds.ClassRequirement, guild.ClassRequirement)
            .Where(GuildSchema.Guilds.Id, guild.Id).Execute();
    }
}