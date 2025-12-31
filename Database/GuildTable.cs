using System.Collections.Generic;

namespace MTA.Database {
    using Member = Game.ConquerStructures.Society.Guild.Member;
    using Game.ConquerStructures.Society;

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

                    if (Game.ConquerStructures.Nobility.Board.TryGetValue(member.Id, out var value)) {
                        member.NobilityRank = value.Rank;
                        member.Gender = value.Gender;
                    }

                    member.Rank = (Game.Enums.GuildMemberRank)reader.ReadUInt16("guildrank");
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

            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guilds"))
            using (var reader = new MySqlReader(cmd)) {
                while (reader.Read()) {
                    var guild = new Guild(reader.ReadString("LeaderName")) {
                        Id = reader.ReadUInt32("Id"),
                        Name = reader.ReadString("Name"),
                        Wins = reader.ReadUInt32("Wins"),
                        Loses = reader.ReadUInt32("Losts"),
                        Bulletin = reader.ReadString("Bulletin"),
                        SilverFund = reader.ReadUInt64("SilverFund"),
                        CtfPoints = reader.ReadUInt32("CTFPoints"),
                        CtfReward = reader.ReadUInt32("CTFReward"),
                        ConquerPointFund = reader.ReadUInt32("ConquerPointFund"),
                        LevelRequirement = reader.ReadUInt32("LevelRequirement"),
                        RebornRequirement = reader.ReadUInt32("RebornRequirement"),
                        ClassRequirement = reader.ReadUInt32("ClassRequirement")
                    };
                    guild.AdvertiseRecruit.Load(reader.ReadString("Advertise"));
                    guild.GuildEnroll = reader.ReadUInt32("GuildEnroll");
                    guild.CreateTime(guild.GuildEnroll);
                    guild.BulletinEnroll = reader.ReadUInt32("BulletinEnroll");
                    guild.CTFDonationCPs = reader.ReadUInt32("CTFdonationCPs");
                    guild.CTFDonationSilver = reader.ReadUInt32("CTFdonationSilver");
                    guild.CTFDonationSilverOld = reader.ReadUInt32("CTFdonationSilverold");
                    guild.CTFDonationCPSold = reader.ReadUInt32("CTFdonationCPsold");

                    guild.CreateTime(guild.BulletinEnroll);
                    if (dict.TryGetValue(guild.Id, out var value)) {
                        guild.Members = value;
                        guild.MemberCount = (uint)guild.Members.Count;
                    }
                    else
                        guild.Members = new SafeDictionary<uint, Member>();

                    Kernel.Guilds.Add(guild.Id, guild);
                    foreach (var member in guild.Members.Values) {
                        if (member.Rank == Game.Enums.GuildMemberRank.GuildLeader)
                            guild.Leader = member;

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
                foreach (var member in guild.Members.Values) {
                    if (member.Spouse == "None" || member.Spouse == "No")
                        continue;
                    if (member.Rank == Game.Enums.GuildMemberRank.GuildLeader)
                        continue;
                    foreach (var findSpouse in guild.Members.Values) {
                        if (member.Spouse == findSpouse.Name) {
                            if (findSpouse.Rank == Game.Enums.GuildMemberRank.GuildLeader) {
                                member.Rank = Game.Enums.GuildMemberRank.LeaderSpouse;
                                break;
                            }

                            if (findSpouse.Rank == Game.Enums.GuildMemberRank.DeputyLeader) {
                                if (member.Rank == Game.Enums.GuildMemberRank.DeputyLeader)
                                    break;
                                if (member.Rank > Game.Enums.GuildMemberRank.DLeaderSpouse)
                                    break;
                                member.Rank = Game.Enums.GuildMemberRank.DLeaderSpouse;
                                break;
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Guild information loaded.");
        }

        private static void LoadAllyEnemy() {
            // Load allies from guild_relations (bidirectional - relation_type=1)
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guild_relations")
                       .Where("relation_type", 1))
            using (var reader = new MySqlReader(cmd)) {
                while (reader.Read()) {
                    var guildId = reader.ReadUInt32("guild_id");
                    var allyId = reader.ReadUInt32("related_guild_id");
                    if (!Kernel.Guilds.TryGetValue(guildId, out var guild)) continue;
                    if (Kernel.Guilds.TryGetValue(allyId, out var ally))
                        guild.Ally.Add(allyId, ally);
                }
            }

            // Load enemies from guild_relations (one-way storage, but display on both sides)
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guild_relations")
                       .Where("relation_type", 0))
            using (var reader = new MySqlReader(cmd)) {
                while (reader.Read()) {
                    var guildId = reader.ReadUInt32("guild_id");
                    var enemyId = reader.ReadUInt32("related_guild_id");

                    // Add to initiator's enemy list (for removal permissions)
                    if (Kernel.Guilds.TryGetValue(guildId, out var initiatorGuild))
                        if (Kernel.Guilds.TryGetValue(enemyId, out var enemyGuild))
                            initiatorGuild.Enemy.Add(enemyId, enemyGuild);

                    // Also add to related guild's enemy list (for display only)
                    if (Kernel.Guilds.TryGetValue(enemyId, out var relatedGuild))
                        if (Kernel.Guilds.TryGetValue(guildId, out var initiatorGuild2))
                            relatedGuild.Enemy.Add(guildId, initiatorGuild2);
                }
            }
        }

        public static void UpdateBulletin(Guild guild, string bulletin) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds").Set("Bulletin", bulletin)
                .Set("BulletinEnroll", guild.BulletinEnroll).Where("ID", guild.Id);
            cmd.Execute();
        }

        public static void SaveFunds(Guild guild) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("ConquerPointFund", guild.ConquerPointFund)
                .Set("SilverFund", guild.SilverFund)
                .Set("CTFdonationCPsold", guild.CTFDonationCPSold)
                .Set("CTFdonationSilverold", guild.CTFDonationSilverOld)
                .Set("CTFdonationCPs", guild.CTFDonationCPs)
                .Set("CTFdonationSilver", guild.CTFDonationSilver)
                .Where("ID", guild.Id);
            cmd.Execute();
        }

        public static void SaveEnrolls(Guild guild) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("GuildEnroll", guild.GuildEnroll)
                .Set("BulletinEnroll", guild.BulletinEnroll)
                .Where("ID", guild.Id);
            cmd.Execute();
        }

        public static void SaveAdvertise(Guild guild) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("Advertise", guild.AdvertiseRecruit.ToString())
                .Set("SilverFund", guild.SilverFund)
                .Where("ID", guild.Id);
            cmd.Execute();
        }

        public static void SaveCtfPoins(Guild guild) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("CTFPoints", guild.CtfPoints)
                .Where("ID", guild.Id);
            cmd.Execute();
        }

        public static void SaveCtfReward(Guild guild) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("CTFReward", guild.CtfReward).Set("CTFPoints", guild.CtfPoints)
                .Where("ID", guild.Id);
            cmd.Execute();
        }

        public static void Disband(Guild guild) {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("entities")
                       .Set("guildid", 0)
                       .Where("guildid", guild.Id))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE).Delete("guilds", "id", guild.Id))
                cmd.Execute();
        }

        public static void Create(Guild guild) {
            while (true) {
                using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guilds").Where("id", guild.Id);
                using var reader = cmd.CreateReader();
                if (reader.Read())
                    guild.Id = Guild.GuildCounter.Next;
                else
                    break;
            }

            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("guilds")
                       .Insert("ID", guild.Id).Insert("name", guild.Name).Insert("Bulletin", "")
                       .Insert("SilverFund", 500000).Insert("LeaderName", guild.LeaderName))
                cmd.Execute();
        }

        public static void ChangeName(Client.GameState client, string name) {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                       .Set("name", name).Where("ID", client.Guild!.Id))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guilds"))
            using (var reader = new MySqlReader(cmd)) {
                while (reader.Read()) {
                    client.Guild.Name = reader.ReadString("Name");
                }
            }
        }

        public static void AddEnemy(Guild guild, uint enemy) {
            // Insert one-way enemy relationship (only initiator can remove)
            using var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("guild_relations")
                .Insert("guild_id", guild.Id).Insert("related_guild_id", enemy).Insert("relation_type", 0);
            cmd.Execute();
        }

        public static void AddAlly(Guild guild, uint ally) {
            // Insert bidirectional ally relationship (both directions)
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("guild_relations")
                       .Insert("guild_id", guild.Id).Insert("related_guild_id", ally).Insert("relation_type", 1))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("guild_relations")
                       .Insert("guild_id", ally).Insert("related_guild_id", guild.Id).Insert("relation_type", 1))
                cmd.Execute();
        }

        public static bool IsEnemyInitiator(Guild guild, uint enemyId) {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                .Select("guild_relations")
                .Where("guild_id", guild.Id)
                .And("related_guild_id", enemyId)
                .And("relation_type", 0);
            using var reader = new MySqlReader(cmd);
            return reader.Read();
        }

        public static void RemoveEnemy(Guild guild, uint enemy) {
            // Remove one-way enemy relationship (only from initiator)
            using var command = new MySqlCommand(MySqlCommandType.DELETE);
            command.Delete("guild_relations", "guild_id", guild.Id).And("related_guild_id", enemy)
                .And("relation_type", 0)
                .Execute();
        }

        public static void RemoveAlly(Guild guild, uint ally) {
            // Remove bidirectional ally relationship (both directions)
            using (var command = new MySqlCommand(MySqlCommandType.DELETE))
                command.Delete("guild_relations", "guild_id", guild.Id).And("related_guild_id", ally)
                    .And("relation_type", 1)
                    .Execute();
            using (var command = new MySqlCommand(MySqlCommandType.DELETE))
                command.Delete("guild_relations", "guild_id", ally).And("related_guild_id", guild.Id)
                    .And("relation_type", 1)
                    .Execute();
        }

        public static void UpdateGuildWarStats(Guild guild) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("wins", guild.Wins).Set("losts", guild.Loses)
                .Where("id", guild.Id);
            cmd.Execute();
        }

        public static void UpdatePoleKeeperTc(Guild guild) {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                       .Set("PoleKeeperTc", 0))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                       .Set("PoleKeeperTc", 1).Where("id", guild.Id))
                cmd.Execute();
        }

        public static void UpdatePoleKeeperPh(Guild guild) {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                       .Set("PoleKeeperPh", 0))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                       .Set("PoleKeeperPh", 1).Where("id", guild.Id))
                cmd.Execute();
        }

        public static void UpdatePoleKeeperAp(Guild guild) {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                       .Set("PoleKeeperAp", 0))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                       .Set("PoleKeeperAp", 1).Where("id", guild.Id))
                cmd.Execute();
        }

        public static void SaveLeader(Guild guild) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("LeaderName", guild.LeaderName)
                .Where("id", guild.Id);
            cmd.Execute();
        }

        internal static void SaveRequirements(Guild guild) {
            using var command = new MySqlCommand(MySqlCommandType.UPDATE);
            command.Update("guilds").Set("LevelRequirement", guild.LevelRequirement)
                .Set("RebornRequirement", guild.RebornRequirement).Set("ClassRequirement", guild.ClassRequirement)
                .Where("ID", guild.Id).Execute();
        }
    }
}