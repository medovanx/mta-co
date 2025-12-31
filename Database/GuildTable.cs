using System.Collections.Generic;

namespace MTA.Database
{
    using Member = Game.ConquerStructures.Society.Guild.Member;
    using Game.ConquerStructures.Society;
    using Network.GamePackets;

    public class GuildTable
    {
        public static void Load()
        {
            Dictionary<uint, SafeDictionary<uint, Member>> dict = new Dictionary<uint, SafeDictionary<uint, Member>>();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("guildid", 0, true))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    Member member = new Member(reader.ReadUInt16("guildid"));
                    member.Id = reader.ReadUInt32("uid");
                    member.Name = reader.ReadString("name");
                    member.Level = reader.ReadByte("level");
                    member.Spouse = reader.ReadString("Spouse");

                    if (Game.ConquerStructures.Nobility.Board.ContainsKey(member.Id))
                    {
                        member.NobilityRank = Game.ConquerStructures.Nobility.Board[member.Id].Rank;
                        member.Gender = Game.ConquerStructures.Nobility.Board[member.Id].Gender;
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

                    member.Mesh = uint.Parse(reader.ReadUInt16("Face").ToString() + reader.ReadUInt16("Body").ToString());
                    if (!dict.ContainsKey(member.GuildId)) dict.Add(member.GuildId, new SafeDictionary<uint, Member>());
                    dict[member.GuildId].Add(member.Id, member);
                }
            }
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guilds"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    Guild guild = new Guild(reader.ReadString("LeaderName"));
                    guild.Id = reader.ReadUInt32("Id");
                    guild.Name = reader.ReadString("Name");
                    guild.Wins = reader.ReadUInt32("Wins");
                    guild.Loses = reader.ReadUInt32("Losts");
                    guild.Bulletin = reader.ReadString("Bulletin");
                    guild.SilverFund = reader.ReadUInt64("SilverFund");
                    guild.CtfPoints = reader.ReadUInt32("CTFPoints");
                    guild.CtfReward = reader.ReadUInt32("CTFReward");
                    guild.ConquerPointFund = reader.ReadUInt32("ConquerPointFund");
                    guild.LevelRequirement = reader.ReadUInt32("LevelRequirement");
                    guild.RebornRequirement = reader.ReadUInt32("RebornRequirement");
                    guild.ClassRequirement = reader.ReadUInt32("ClassRequirement");
                    guild.AdvertiseRecruit.Load(reader.ReadString("Advertise"));
                    guild.GuildEnroll = reader.ReadUInt32("GuildEnroll");
                    guild.CreateTime(guild.GuildEnroll);
                    guild.BulletinEnroll = reader.ReadUInt32("BulletinEnroll");
                    guild.CTFDonationCPs = reader.ReadUInt32("CTFdonationCPs");
                    guild.CTFDonationSilver = reader.ReadUInt32("CTFdonationSilver");
                    guild.CTFDonationSilverOld = reader.ReadUInt32("CTFdonationSilverold");
                    guild.CTFDonationCPSold = reader.ReadUInt32("CTFdonationCPsold");

                    guild.CreateTime(guild.BulletinEnroll);
                    if (dict.ContainsKey(guild.Id))
                    {
                        guild.Members = dict[guild.Id];
                        guild.MemberCount = (uint)guild.Members.Count;
                    }
                    else
                        guild.Members = new SafeDictionary<uint, Member>();
                    Kernel.Guilds.Add(guild.Id, guild);
                    foreach (var member in guild.Members.Values)
                    {
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
            foreach (var guild in Kernel.Guilds.Values)
            {
                guild.CreateMembersRank();
                if (guild.AdvertiseRecruit.WasLoad)
                    Guild.Advertise.Add(guild);
                guild.CalculateCTFRank();
            }
            Guild.Advertise.FixedRank();
            //create leader spouse
            foreach (var guild in Kernel.Guilds.Values)
            {
                foreach (var Member in guild.Members.Values)
                {
                    if (Member.Spouse == "None" || Member.Spouse == "No")
                        continue;
                    if (Member.Rank == Game.Enums.GuildMemberRank.GuildLeader)
                        continue;
                    foreach (var findSpouse in guild.Members.Values)
                    {
                        if (Member.Spouse == findSpouse.Name)
                        {
                            if (findSpouse.Rank == Game.Enums.GuildMemberRank.GuildLeader)
                            {
                                Member.Rank = Game.Enums.GuildMemberRank.LeaderSpouse;
                                break;
                            }
                            if (findSpouse.Rank == Game.Enums.GuildMemberRank.DeputyLeader)
                            {
                                if (Member.Rank == Game.Enums.GuildMemberRank.DeputyLeader)
                                    break;
                                if (Member.Rank > Game.Enums.GuildMemberRank.DLeaderSpouse)
                                    break;
                                Member.Rank = Game.Enums.GuildMemberRank.DLeaderSpouse;
                                break;
                            }
                        }
                    }

                }
            }
            Console.WriteLine("Guild information loaded.");
        }
        public static void LoadAllyEnemy()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guildenemy"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    ushort guildID = reader.ReadUInt16("guildid");
                    ushort enemyID = reader.ReadUInt16("enemyid");
                    if (Kernel.Guilds.ContainsKey(guildID))
                        if (Kernel.Guilds.ContainsKey(enemyID))
                            Kernel.Guilds[guildID].Enemy.Add(enemyID, Kernel.Guilds[enemyID]);
                }
            }
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guildally"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    ushort guildID = reader.ReadUInt16("guildid");
                    ushort allyID = reader.ReadUInt16("allyid");
                    if (Kernel.Guilds.ContainsKey(guildID))
                        if (Kernel.Guilds.ContainsKey(allyID))
                            Kernel.Guilds[guildID].Ally.Add(allyID, Kernel.Guilds[allyID]);
                }
            }
        }

        public static void UpdateBulletin(Guild guild, string bulletin)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds").Set("Bulletin", bulletin).Set("BulletinEnroll", guild.BulletinEnroll).Where("ID", guild.Id))
                cmd.Execute();
        }
        public static void SaveFunds(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("ConquerPointFund", guild.ConquerPointFund)
                .Set("SilverFund", guild.SilverFund)
                .Set("CTFdonationCPsold", guild.CTFDonationCPSold)
                .Set("CTFdonationSilverold", guild.CTFDonationSilverOld)
                  .Set("CTFdonationCPs", guild.CTFDonationCPs)
                    .Set("CTFdonationSilver", guild.CTFDonationSilver)
                .Where("ID", guild.Id))
                cmd.Execute();
        }
        public static void SaveEnroles(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("GuildEnroll", guild.GuildEnroll)
                .Set("BulletinEnroll", guild.BulletinEnroll)
                .Where("ID", guild.Id))
                cmd.Execute();
        }

        public static void SaveAdvertise(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
.Set("Advertise", guild.AdvertiseRecruit.ToString())
.Set("SilverFund", guild.SilverFund)
.Where("ID", guild.Id))
                cmd.Execute();
        }
        public static void SaveCTFPoins(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("CTFPoints", guild.CtfPoints)
                .Where("ID", guild.Id))
                cmd.Execute();
        }
        public static void SaveCTFReward(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("CTFReward", guild.CtfReward).Set("CTFPoints", guild.CtfPoints)
                .Where("ID", guild.Id))
                cmd.Execute();
        }
        public static void Disband(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("entities")
                .Set("guildid", 0)
                .Where("guildid", guild.Id))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE).Delete("guilds", "id", guild.Id))
                cmd.Execute();
        }
        public static void Create(Guild guild)
        {
            while (true)
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guilds").Where("id", guild.Id))
                using (var reader = cmd.CreateReader())
                {
                    if (reader.Read())
                        guild.Id = Guild.GuildCounter.Next;
                    else
                        break;
                }
            }
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("guilds")
                .Insert("ID", guild.Id).Insert("name", guild.Name).Insert("Bulletin", "")
                .Insert("SilverFund", 500000).Insert("LeaderName", guild.LeaderName))
                cmd.Execute();
        }
        public static void ChangeName(Client.GameState client, string name)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("name", name).Where("ID", client.Guild.Id))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("guilds"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    client.Guild.Name = reader.ReadString("Name");
                }
            }
            Message message = null;
            message = new Message("Congratulations, " + client.Entity.Name + " has change guild name to " + name + " Succesfully!", System.Drawing.Color.White, Message.World);
            foreach (Client.GameState clients in Program.Values)
            {
                clients.Send(message);
            }
        }
        public static void AddEnemy(Guild guild, uint enemy)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("guildenemy")
                .Insert("guildID", guild.Id).Insert("enemyID", enemy))
                cmd.Execute();
        }
        public static void AddAlly(Guild guild, uint ally)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("guildally")
                .Insert("guildID", guild.Id).Insert("allyID", ally))
                cmd.Execute();
        }
        public static void RemoveEnemy(Guild guild, uint enemy)
        {
            using (var command = new MySqlCommand(MySqlCommandType.DELETE))
                command.Delete("guildenemy", "GuildID", guild.Id).And("EnemyID", enemy)
                    .Execute();
        }
        public static void RemoveAlly(Guild guild, uint ally)
        {
            using (var command = new MySqlCommand(MySqlCommandType.DELETE))
                command.Delete("guildally", "GuildID", guild.Id).And("AllyID", ally)
                    .Execute();
            using (var command = new MySqlCommand(MySqlCommandType.DELETE))
                command.Delete("guildally", "GuildID", ally).And("AllyID", guild.Id)
                    .Execute();
        }
        public static void UpdateGuildWarStats(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("wins", guild.Wins).Set("losts", guild.Loses)
                .Where("id", guild.Id))
                cmd.Execute();
        }
        public static void UpdatePoleKeeperTc(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("PoleKeeperTc", 0))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("PoleKeeperTc", 1).Where("id", guild.Id))
                cmd.Execute();
        }
        public static void UpdatePoleKeeperPh(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("PoleKeeperPh", 0))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("PoleKeeperPh", 1).Where("id", guild.Id))
                cmd.Execute();
        }
        public static void UpdatePoleKeeperAp(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("PoleKeeperAp", 0))
                cmd.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("PoleKeeperAp", 1).Where("id", guild.Id))
                cmd.Execute();
        }
        public static void SaveLeader(Guild guild)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("guilds")
                .Set("LeaderName", guild.LeaderName)
                .Where("id", guild.Id))
                cmd.Execute();
        }
        internal static void SaveRequirements(Guild guild)
        {
            using (var command = new MySqlCommand(MySqlCommandType.UPDATE))
                command.Update("guilds").Set("LevelRequirement", guild.LevelRequirement)
                    .Set("RebornRequirement", guild.RebornRequirement).Set("ClassRequirement", guild.ClassRequirement)
                    .Where("ID", guild.Id).Execute();
        }
    }
}
