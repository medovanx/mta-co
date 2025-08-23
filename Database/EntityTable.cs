using System;
using System.IO;
using System.Text;

using System.Collections.Generic;
using MTA.Network.GamePackets;
using System.Collections.Concurrent;
using MTA.Game;
using MTA.Client;
namespace MTA.Database
{
    public static class EntityTable
    {
        public static void SetFlowers(Client.GameState client)
        {
            if (Network.PacketHandler.IsGirl((uint)client.Entity.Body))
            {
                if (!Game.Features.Flowers.Flowers_Poll.ContainsKey(client.Entity.UID))
                {
                    client.Entity.MyFlowers = new Game.Features.Flowers(client.Entity.UID, client.Entity.Name);
                    Game.Features.Flowers.Flowers_Poll.TryAdd(client.Entity.UID, client.Entity.MyFlowers);
                    return;
                }
                client.Entity.MyFlowers = Game.Features.Flowers.Flowers_Poll[client.Entity.UID];                return;
            }
            else
            {
                if (!Game.Features.Flowers.BoyFlowers.ContainsKey(client.Entity.UID))
                {
                    client.Entity.MyFlowers = new Game.Features.Flowers(client.Entity.UID, client.Entity.Name);
                    Game.Features.Flowers.BoyFlowers.TryAdd(client.Entity.UID, client.Entity.MyFlowers);
                    return;
                }
                client.Entity.MyFlowers = Game.Features.Flowers.BoyFlowers[client.Entity.UID];
                return;
            }
        }

        public static bool LoadEntity(Client.GameState client, uint uid = 0)
        {
            if (uid == 0)
                uid = client.Account.EntityID;
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("UID", uid))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    client.WarehousePW = reader.ReadUInt32("WarehousePW");
                    client.Entity = new Game.Entity(Game.EntityFlag.Player, false);
                    client.Entity.Name = reader.ReadString("Name");
                    client.Entity.lacb = reader.ReadUInt32("lacb"); 
                //    client.Entity.NobalityDonation = reader.ReadUInt64("Donation");
                    if (client.Entity.Name.Length > 15)
                        client.Entity.Name = client.Entity.Name.Substring(0, 15);
                    client.HeadgearClaim = reader.ReadBoolean("HeadgearClaim");
                    client.Entity.Spouse = reader.ReadString("Spouse");
                    client.Entity.Owner = client;
                    client.Voted = reader.ReadBoolean("VotePoint");
                    client.Entity.MonstersPoints = reader.ReadUInt32("MonstersPoints");
                    client.Entity.DarkPoints = reader.ReadUInt32("DarkPoints");
                    client.Entity.OnlinePointStamp = Time32.Now;
                    client.Entity.CpsPointStamp = Time32.Now;
                    client.Entity.AddFlower = reader.ReadUInt32("Flower");
                    client.MoneySave = reader.ReadUInt32("MoneySave");
                    client.Entity.Experience = reader.ReadUInt64("Experience");
                    client.Entity.Money = reader.ReadUInt32("Money");
                    client.Entity.ConquerPoints = reader.ReadUInt32("ConquerPoints");
                    client.Entity.BoundCps = reader.ReadUInt32("boundcps");
                    client.Entity.ClaimedSTeamPK = reader.ReadBoolean("ClaimedSTeamPK");
                    client.Entity.ClaimedTeamPK = reader.ReadBoolean("ClaimedTeamPK");
                    client.Entity.ClaimedElitePk = reader.ReadBoolean("ClaimedElitePk");
                    client.Entity.TreasuerPoints = reader.ReadUInt32("TreasuerPoints");                   
                    client.Entity.UID = reader.ReadUInt32("UID");
                    client.Entity.MyAchievement = new Game.Achievement(client.Entity);
                    client.Entity.MyAchievement.Load(reader.ReadString("Achievement"));
                    client.Entity.Hitpoints = reader.ReadUInt32("Hitpoints");
                    client.Entity.BankCps = reader.ReadUInt32("BankCps");
                    client.Entity.QuizPoints = reader.ReadUInt32("QuizPoints");
                    client.Entity.Body = reader.ReadUInt16("Body");
                    client.Entity.TotalPerfectionScore = reader.ReadUInt32("TotalPerfectionScore");
                    client.Entity.Face = reader.ReadUInt16("Face");
                    //client.Entity.TitlePoints = reader.ReadInt32("TitlePoints");
                    client.Entity.DeputyLeader = reader.ReadUInt32("DeputyLeader");
                    client.Entity.Strength = reader.ReadUInt16("Strength");
                    client.Entity.Titles = new ConcurrentDictionary<TitlePacket.Titles, DateTime>();
                    client.Entity.MyTitle = (TitlePacket.Titles)reader.ReadUInt32("My_Title");
                    client.Entity.Agility = reader.ReadUInt16("Agility");
                    client.Entity.Spirit = reader.ReadUInt16("Spirit");
                    client.Entity.Vitality = reader.ReadUInt16("Vitality");
                    client.Entity.Atributes = reader.ReadUInt16("Atributes");
                    SetFlowers(client);
                    client.ElitePKStats = new ElitePK.FighterStats(client.Entity.UID, client.Entity.Name, client.Entity.Mesh);
                    client.Entity.SubClass = reader.ReadByte("SubClass");
                    client.Entity.SubClassLevel = reader.ReadByte("SubClassLevel");
                    client.Entity.SubClasses.Active = client.Entity.SubClass;
                    client.Entity.SubClassesActive = client.Entity.SubClass;
                    client.Entity.SubClasses.StudyPoints = reader.ReadUInt16("StudyPoints");
                    client.VirtuePoints = (uint)reader.ReadUInt32("VirtuePoints");
                    client.Entity.Mana = reader.ReadUInt16("Mana");
                    client.Entity.HairStyle = reader.ReadUInt16("HairStyle");
                    client.Entity.OnlinePoints = reader.ReadUInt32("OnlinePoints");
                    //client.Entity.BoundCps = reader.("BoundCPS");
                    client.Entity.killerpoints = reader.ReadUInt32("killerpoints");
                    client.Entity.OnlinePointStamp = Time32.Now;
                    client.Entity.CpsPointStamp = Time32.Now;
                    client.Entity.MapID = reader.ReadUInt16("MapID");
                    client.VendingDisguise = reader.ReadUInt16("VendingDisguise");
                    client.SpiritBeadQ.CanAccept = !Convert.ToBoolean(reader.ReadUInt32("CanAcceptSpiritBead"));
                    client.SpiritBeadQ.Bead = (uint)reader.ReadUInt32("SpiritQuestBead");
                    client.SpiritBeadQ.CollectedSpirits = (uint)reader.ReadUInt32("CollectedSpirits");
                    client.Entity.CountryID = (Game.Enums.CountryID)reader.ReadUInt32("CountryID");
                    if (client.VendingDisguise == 0)
                        client.VendingDisguise = 223;
                    client.Entity.X = reader.ReadUInt16("X");
                    client.Entity.Y = reader.ReadUInt16("Y");
                    if (Constants.EtaleMaps.Contains(client.Entity.MapID))
                    {
                        client.Entity.MapID = 1002;
                        client.Entity.X = 300;
                        client.Entity.Y = 278;
                    }  
                    if (client.Map.BaseID == 1844 || client.Entity.MapID == 1950
                     || client.Entity.MapID == 3333 || client.Entity.MapID == 1090
                      || client.Entity.MapID == 5560 || client.Entity.MapID == 5560
                     || client.Entity.MapID == 5540 || client.Entity.MapID == 5540
                      || client.Entity.MapID == 5452 || client.Entity.MapID == 5452
                     || client.Entity.MapID == 5450 || client.Entity.MapID == 5450
                     || client.Entity.MapID == 4021 || client.Entity.MapID == 3355
                     || client.Entity.MapID == 5561 || client.Entity.MapID == 5561
                     || client.Entity.MapID == 5453 || client.Entity.MapID == 5453
                     || client.Entity.MapID == 4022 || client.Entity.MapID == 4023
                     || client.Entity.MapID == 4024 || client.Entity.MapID == 4025
                     || client.Entity.MapID == 1508 || client.Entity.MapID == 1518
                     || client.Entity.MapID == 7001 || client.Entity.MapID == 1801
                     || client.Entity.MapID == 8877 || client.Entity.MapID == 3333
                     || client.Entity.MapID == 3355 || client.Entity.MapID == 3377
                     || client.Entity.MapID == 4562 || client.Entity.MapID == 4662
                     || client.Entity.MapID == 8883 || client.Entity.MapID == 11017
                     || client.Entity.MapID == 1731 || client.Entity.MapID == 1732
                     || client.Entity.MapID == 3842 || client.Entity.MapID == 3820
                     || client.Entity.MapID == 8839 || client.Entity.MapID == 1826
                     || client.Entity.MapID == 3844 || client.Entity.MapID == 3845 || client.Entity.MapID == 1765 || client.Entity.MapID == 1507
                     || client.Entity.MapID == 3055 || client.Entity.MapID == 38200)
                    {
                        client.Entity.MapID = 1002;
                        client.Entity.X = 300;
                        client.Entity.Y = 278;
                    }  
                    client.NecklaceClaim = reader.ReadBoolean("NecklaceClaim");
                    client.ArmorClaim = reader.ReadBoolean("ArmorClaim");
                    client.WeaponClaim = reader.ReadBoolean("WeaponClaim");
                    client.RingClaim = reader.ReadBoolean("RingClaim");
                    client.BootsClaim = reader.ReadBoolean("BootsClaim");
                    client.FanClaim = reader.ReadBoolean("FanClaim");
                    client.TowerClaim = reader.ReadBoolean("TowerClaim");
                    client.HeadgearClaim = reader.ReadBoolean("HeadgearClaim");
                    client.InLottery = reader.ReadBoolean("InLottery");
                    client.LotteryEntries = reader.ReadByte("LotteryEntries");                    
                    client.LastLotteryEntry = DateTime.FromBinary(reader.ReadInt64("LastLotteryEntry"));                    
                    client.Entity.PreviousMapID = reader.ReadUInt16("PreviousMapID");
                    client.Entity.PKPoints = reader.ReadUInt16("PKPoints");
                    client.Entity.Class = reader.ReadByte("Class");
                    if (reader.ReadByte("Class") >= 160 && reader.ReadByte("Class") <= 165)
                        client.Entity.Windwalker = reader.ReadByte("Windwalker");
                    client.Entity.Reborn = reader.ReadByte("Reborn");
                    client.Entity.Level = reader.ReadByte("Level");
                    client.Entity.BlackList = new List<string>();

                    var BlackList = reader.ReadString("BlackList").Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var person in BlackList)
                    {
                        if (person != null && person != "" && person != "@@")
                            client.Entity.BlackList.Add(person);
                    }
                    client.Entity.FirstRebornClass = reader.ReadByte("FirstRebornClass");
                    client.Entity.SecondRebornClass = reader.ReadByte("SecondRebornClass");
                    client.Entity.FirstRebornLevel = reader.ReadByte("FirstRebornLevel");
                    client.Entity.SecondRebornLevel = reader.ReadByte("SecondRebornLevel");
                    client.LastDragonBallUse = DateTime.FromBinary(reader.ReadInt64("LastDragonBallUse"));
                    client.LastResetTime = DateTime.FromBinary(reader.ReadInt64("LastResetTime"));
                    client.Entity.EnlightenPoints = reader.ReadUInt16("EnlightenPoints");
                    client.Entity.EnlightmentTime = reader.ReadUInt16("EnlightmentWait");
                    if (client.Entity.EnlightmentTime > 0)
                    {
                        if (client.Entity.EnlightmentTime % 20 > 0)
                        {
                            client.Entity.EnlightmentTime -= (ushort)(client.Entity.EnlightmentTime % 20);
                            client.Entity.EnlightmentTime += 20;
                        }
                    }
                    client.Entity.ReceivedEnlightenPoints = reader.ReadByte("EnlightsReceived");
                    client.Entity.DoubleExperienceTime = reader.ReadUInt16("DoubleExpTime");
                    client.DoubleExpToday = reader.ReadBoolean("DoubleExpToday");
                    client.Entity.HeavenBlessing = reader.ReadUInt32("HeavenBlessingTime");
                    client.Entity.VIPLevel = reader.ReadByte("VIPLevel");
                    client.Entity.PrevX = reader.ReadUInt16("PreviousX");
                    client.Entity.PrevY = reader.ReadUInt16("PreviousY");
                    client.ExpBalls = reader.ReadByte("ExpBalls");
                    client.Entity.OnlinePoints = reader.ReadUInt32("OnlinePoints");
                    client.Entity.OnlinePointStamp = Time32.Now;

                    client.Entity.ClanId = reader.ReadUInt32("ClanId");
                    client.Entity.ClanRank = (Clan.Ranks)reader.ReadUInt32("ClanRank");

                    UInt64 lastLoginInt = reader.ReadUInt32("LastLogin");
                    if (lastLoginInt != 0)
                        client.Entity.LastLogin = Kernel.FromDateTimeInt(lastLoginInt);
                    else
                        client.Entity.LastLogin = DateTime.Now;

                   

                    if (client.Entity.MapID == 601)
                        client.OfflineTGEnterTime = DateTime.FromBinary(reader.ReadInt64("OfflineTGEnterTime"));
                    Game.ConquerStructures.Nobility.Sort(client.Entity.UID);

                    if (Kernel.Guilds.ContainsKey(reader.ReadUInt32("GuildID")))
                    {
                        client.Guild = Kernel.Guilds[reader.ReadUInt32("GuildID")];
                        if (client.Guild.Members.ContainsKey(client.Entity.UID))
                        {
                            client.AsMember = client.Guild.Members[client.Entity.UID];
                            client.AsMember.ArsenalDonation = client.GetArsenalDonation();
                            client.AsMember.LastLogin = reader.ReadUInt32("GuildLastLogin");
                            if (client.AsMember.GuildID == 0)
                            {
                                client.AsMember = null;
                                client.Guild = null;
                            }
                            else
                            {
                                client.Entity.GuildID = (ushort)client.Guild.ID;
                                client.Entity.GuildRank = (ushort)client.AsMember.Rank;
                            }
                        }
                        else
                            client.Guild = null;
                    }
                    if (!Game.ConquerStructures.Nobility.Board.TryGetValue(client.Entity.UID, out client.NobilityInformation))
                    {
                        client.NobilityInformation = new MTA.Game.ConquerStructures.NobilityInformation();
                        client.NobilityInformation.EntityUID = client.Entity.UID;
                        client.NobilityInformation.Name = client.Entity.Name;
                        client.NobilityInformation.Donation = 0;
                        client.NobilityInformation.Rank = MTA.Game.ConquerStructures.NobilityRank.Serf;
                        client.NobilityInformation.Position = -1;
                        client.NobilityInformation.Gender = 1;
                        client.NobilityInformation.Mesh = client.Entity.Mesh;
                        if (client.Entity.Body % 10 >= 3)
                            client.NobilityInformation.Gender = 0;
                    }
                    client.Entity.NobilityRank = client.NobilityInformation.Rank;

                    if (DateTime.Now.DayOfYear != client.LastResetTime.DayOfYear)
                    {
                        client.ChiPoints += 500;
                        if (client.ChiPoints > 4000) client.ChiPoints = 4000;
                        if (client.Entity.Level >= 90)
                        {
                            client.Entity.EnlightenPoints = 100;
                            if (client.Entity.NobilityRank == MTA.Game.ConquerStructures.NobilityRank.Knight ||
                                client.Entity.NobilityRank == MTA.Game.ConquerStructures.NobilityRank.Baron)
                                client.Entity.EnlightenPoints += 100;
                            else if (client.Entity.NobilityRank == MTA.Game.ConquerStructures.NobilityRank.Earl ||
                                client.Entity.NobilityRank == MTA.Game.ConquerStructures.NobilityRank.Duke)
                                client.Entity.EnlightenPoints += 200;
                            else if (client.Entity.NobilityRank == MTA.Game.ConquerStructures.NobilityRank.Prince)
                                client.Entity.EnlightenPoints += 300;
                            else if (client.Entity.NobilityRank == MTA.Game.ConquerStructures.NobilityRank.King)
                                client.Entity.EnlightenPoints += 400;
                            if (client.Entity.VIPLevel != 0)
                            {
                                if (client.Entity.VIPLevel <= 3)
                                    client.Entity.EnlightenPoints += 100;
                                else if (client.Entity.VIPLevel <= 5)
                                    client.Entity.EnlightenPoints += 200;
                                else if (client.Entity.VIPLevel == 6)
                                    client.Entity.EnlightenPoints += 300;
                            }
                        }
                        client.Entity.ReceivedEnlightenPoints = 0;
                        client.DoubleExpToday = false;
                        client.ExpBalls = 0;
                        client.LotteryEntries = 0;
                        client.SpiritBeadQ.Reset(false, true);
                        client.LastResetTime = DateTime.Now;
                        ResetExpball(client);
                        ResetLottery(client);
                    }
                    #region Team Arena
                    Game.TeamArena.ArenaStatistics.TryGetValue(client.Entity.UID, out client.TeamArenaStatistic);
                    if (client.TeamArenaStatistic == null)
                    {
                        client.TeamArenaStatistic = new MTA.Network.GamePackets.TeamArenaStatistic(true);
                        client.TeamArenaStatistic.EntityID = client.Entity.UID;
                        client.TeamArenaStatistic.Name = client.Entity.Name;
                        client.TeamArenaStatistic.Level = client.Entity.Level;
                        client.TeamArenaStatistic.Class = client.Entity.Class;
                        client.TeamArenaStatistic.Model = client.Entity.Mesh;
                        TeamArenaTable.InsertArenaStatistic(client);
                        client.TeamArenaStatistic.Status = Network.GamePackets.TeamArenaStatistic.NotSignedUp;
                        if (Game.TeamArena.ArenaStatistics.ContainsKey(client.Entity.UID))
                            Game.TeamArena.ArenaStatistics.Remove(client.Entity.UID);
                        Game.TeamArena.ArenaStatistics.Add(client.Entity.UID, client.TeamArenaStatistic);
                    }
                    else if (client.TeamArenaStatistic.EntityID == 0)
                    {
                        client.TeamArenaStatistic = new MTA.Network.GamePackets.TeamArenaStatistic(true);
                        client.TeamArenaStatistic.EntityID = client.Entity.UID;
                        client.TeamArenaStatistic.Name = client.Entity.Name;
                        client.TeamArenaStatistic.Level = client.Entity.Level;
                        client.TeamArenaStatistic.Class = client.Entity.Class;
                        client.TeamArenaStatistic.Model = client.Entity.Mesh;
                        TeamArenaTable.InsertArenaStatistic(client);
                        client.TeamArenaStatistic.Status = Network.GamePackets.TeamArenaStatistic.NotSignedUp;
                        if (Game.TeamArena.ArenaStatistics.ContainsKey(client.Entity.UID))
                            Game.TeamArena.ArenaStatistics.Remove(client.Entity.UID);
                        Game.TeamArena.ArenaStatistics.Add(client.Entity.UID, client.TeamArenaStatistic);
                    }
                    else
                    {
                        client.TeamArenaStatistic.Level = client.Entity.Level;
                        client.TeamArenaStatistic.Class = client.Entity.Class;
                        client.TeamArenaStatistic.Model = client.Entity.Mesh;
                        client.TeamArenaStatistic.Name = client.Entity.Name;
                    }
                    Game.TeamArena.Clear(client);
                    #endregion
                    #region Arena
                    Game.Arena.ArenaStatistics.TryGetValue(client.Entity.UID, out client.ArenaStatistic);
                    if (client.ArenaStatistic == null)
                    {
                        client.ArenaStatistic = new MTA.Network.GamePackets.ArenaStatistic(true);
                        client.ArenaStatistic.EntityID = client.Entity.UID;
                        client.ArenaStatistic.Name = client.Entity.Name;
                        client.ArenaStatistic.Level = client.Entity.Level;
                        client.ArenaStatistic.Class = client.Entity.Class;
                        client.ArenaStatistic.Model = client.Entity.Mesh;
                        client.ArenaPoints = ArenaTable.ArenaPointFill(client.Entity.Level);
                        client.ArenaStatistic.LastArenaPointFill = DateTime.Now;
                        ArenaTable.InsertArenaStatistic(client);
                        client.ArenaStatistic.Status = Network.GamePackets.ArenaStatistic.NotSignedUp;
                        if (Game.Arena.ArenaStatistics.ContainsKey(client.Entity.UID))
                            Game.Arena.ArenaStatistics.Remove(client.Entity.UID);
                        Game.Arena.ArenaStatistics.Add(client.Entity.UID, client.ArenaStatistic);
                    }
                    else if (client.ArenaStatistic.EntityID == 0)
                    {
                        client.ArenaStatistic = new MTA.Network.GamePackets.ArenaStatistic(true);
                        client.ArenaStatistic.EntityID = client.Entity.UID;
                        client.ArenaStatistic.Name = client.Entity.Name;
                        client.ArenaStatistic.Level = client.Entity.Level;
                        client.ArenaStatistic.Class = client.Entity.Class;
                        client.ArenaStatistic.Model = client.Entity.Mesh;
                        client.ArenaPoints = ArenaTable.ArenaPointFill(client.Entity.Level);
                        client.ArenaStatistic.LastArenaPointFill = DateTime.Now;
                        ArenaTable.InsertArenaStatistic(client);
                        client.ArenaStatistic.Status = Network.GamePackets.ArenaStatistic.NotSignedUp;
                        if (Game.Arena.ArenaStatistics.ContainsKey(client.Entity.UID))
                            Game.Arena.ArenaStatistics.Remove(client.Entity.UID);
                        Game.Arena.ArenaStatistics.Add(client.Entity.UID, client.ArenaStatistic);
                    }
                    else
                    {
                        client.ArenaStatistic.Level = client.Entity.Level;
                        client.ArenaStatistic.Class = client.Entity.Class;
                        client.ArenaStatistic.Model = client.Entity.Mesh;
                        client.ArenaStatistic.Name = client.Entity.Name;
                    }
                    client.ArenaPoints = client.ArenaStatistic.ArenaPoints;
                    client.CurrentHonor = client.ArenaStatistic.CurrentHonor;
                    client.HistoryHonor = client.ArenaStatistic.HistoryHonor;
                    Game.Arena.Clear(client);
                    #endregion
                    #region Champion
                    Game.Champion.ChampionStats.TryGetValue(client.Entity.UID, out client.ChampionStats);
                    if (client.ChampionStats == null)
                    {
                        client.ChampionStats = new MTA.Network.GamePackets.ChampionStatistic(true);
                        client.ChampionStats.UID = client.Entity.UID;
                        client.ChampionStats.Name = client.Entity.Name;
                        client.ChampionStats.Level = client.Entity.Level;
                        client.ChampionStats.Class = client.Entity.Class;
                        client.ChampionStats.Model = client.Entity.Mesh;
                        client.ChampionStats.Points = 0;
                        client.ChampionStats.LastReset = DateTime.Now;
                        ChampionTable.InsertStatistic(client);
                        if (Game.Champion.ChampionStats.ContainsKey(client.Entity.UID))
                            Game.Champion.ChampionStats.Remove(client.Entity.UID);
                        Game.Champion.ChampionStats.Add(client.Entity.UID, client.ChampionStats);
                    }
                    else if (client.ChampionStats.UID == 0)
                    {
                        client.ChampionStats = new Network.GamePackets.ChampionStatistic(true);
                        client.ChampionStats.UID = client.Entity.UID;
                        client.ChampionStats.Name = client.Entity.Name;
                        client.ChampionStats.Level = client.Entity.Level;
                        client.ChampionStats.Class = client.Entity.Class;
                        client.ChampionStats.Model = client.Entity.Mesh;
                        client.ChampionStats.Points = 0;
                        client.ChampionStats.LastReset = DateTime.Now;
                        ArenaTable.InsertArenaStatistic(client);
                        client.ArenaStatistic.Status = Network.GamePackets.ArenaStatistic.NotSignedUp;
                        if (Game.Champion.ChampionStats.ContainsKey(client.Entity.UID))
                            Game.Champion.ChampionStats.Remove(client.Entity.UID);
                        Game.Champion.ChampionStats.Add(client.Entity.UID, client.ChampionStats);
                    }
                    else
                    {
                        client.ChampionStats.Level = client.Entity.Level;
                        client.ChampionStats.Class = client.Entity.Class;
                        client.ChampionStats.Model = client.Entity.Mesh;
                        client.ChampionStats.Name = client.Entity.Name;
                        if (client.ChampionStats.LastReset.DayOfYear != DateTime.Now.DayOfYear)
                            ChampionTable.Reset(client.ChampionStats);
                    }
                    Game.Champion.Clear(client);
                    #endregion
                    Game.IJiangHu Jiang;
                    if (Game.JiangHu.JiangHuClients.TryGetValue(client.Entity.UID, out Jiang))
                    {
                        client.Entity.MyJiang = Jiang as Game.JiangHu;
                        client.Entity.MyJiang.TimerStamp = DateTime.Now;
                        client.Entity.MyJiang.Level = client.Entity.Level;
                    }
                    client.Entity.LoadTopStatus();
                    client.Entity.FullyLoaded = true;
                    return true;
                }
                else
                    return false;
            }
        }

        public static void UpdateData(Client.GameState client, string column, object value)
        {
            if (client.TransferedPlayer) return;
            UpdateData(client.Entity.UID, column, value);
        }
        public static void UpdateData(uint UID, string column, object value)
        {
            if (value is Boolean)
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                    cmd.Update("entities").Set(column, (Boolean)value).Where("UID", UID)
                        .Execute();
            }
            else
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                    cmd.Update("entities").Set(column, value.ToString()).Where("UID", UID)
                        .Execute();
            }
        }
        public static void UpdateGuildRank(uint UID, MTA.Game.Enums.GuildMemberRank rank)
        {
            UpdateData(UID, "GuildRank", (int)rank);
        }
        public static void UpdateOnlineStatus(Client.GameState client, bool online, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            if (online || (!online && client.DoSetOffline))
            {
                UpdateData(client, "Online", online);
            }
        }
        public static void UpdateOnlineStatus(Client.GameState client, bool online)
        {
            if (online || (!online && client.DoSetOffline))
            {
                UpdateData(client, "Online", online);
            }
        }
        public static void LoginNow(Client.GameState client)
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.UPDATE);
            cmd.Update("entities").Set("GuildLastLogin", Network.PacketHandler.UnixTimestamp).Where("UID", client.Entity.UID).Execute();
        }
        public static void UpdateCps(Client.GameState client)
        {
            UpdateData(client, "ConquerPoints", client.Entity.ConquerPoints);
        }
        public static void UpdatebCps(Client.GameState client)
        {
            UpdateData(client, "boundcps", client.Entity.BoundCps);
        } 
        public static void UpdateMoney(Client.GameState client)
        {
            UpdateData(client, "Money", client.Entity.Money);
        }
        public static void UpdateLevel(Client.GameState client)
        {
            UpdateData(client, "Level", client.Entity.Level);
        }
        public static void UpdateGuildID(Client.GameState client)
        {
            UpdateData(client, "guildid", client.Entity.GuildID);
        }
        public static void UpdateClanID(Client.GameState client)
        {
            UpdateData(client, "ClanId", client.Entity.ClanId);
        }
        public static void RemoveClan(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("entities").Set("ClanId", 0).Set("ClanDonation", 0).Set("ClanRank", 0)
                    .Where("ClanId", client.Entity.ClanId).Execute();
        }
        public static void RemoveClanMember(string name)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("entities").Set("ClanId", 0).Set("ClanDonation", 0).Set("ClanRank", 0).Where("Name", name).Execute();
        }
        public static ushort GetClass(string Name)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("Name", Name))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    return reader.ReadUInt16("Class");
                }
            }
            return 0;
        }
        public static void UpdateClanRank(Client.GameState client)
        {
            UpdateData(client, "ClanRank", (uint)client.Entity.ClanRank);
        }
        public static void UpdateClanRank(uint UID, uint rank)
        {
            UpdateData(UID, "ClanRank", rank);
        }

        public static void UpdateClanDonation(Client.GameState client)
        {
            UpdateData(client, "clandonation", (uint)client.Entity.ClanRank);
        }
        public static void UpdateGuildRank(Client.GameState client)
        {
            UpdateData(client, "GuildRank", client.Entity.GuildRank);
        }
        public static void UpdateSkillExp(Client.GameState client, uint spellid, uint exp)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("skills").Set("Experience", exp).Where("EntityID", client.Entity.UID).And("ID", spellid).Execute();
        }
        public static void ResetLottery(Client.GameState client)
        {
            UpdateData(client, "LotteryEntries", 0);
        }
        public static void ResetExpball(Client.GameState client)
        {
            UpdateData(client, "ExpBalls", 0);
        }
        public static bool SaveEntity(Client.GameState c, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            if (c.Fake) return true;
            if (c.TransferedPlayer) return true;
            Game.Entity e = c.Entity;
            if (e.JustCreated) return true;
            #region BlackList
            string Persons = "";
            if (e.BlackList.Count > 0)
            {
                foreach (var person in e.BlackList)
                {
                    Persons += person + "@@";
                }
            }
            #endregion
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("entities"))
            {
                cmd.Set("WarehousePW", c.WarehousePW)
                    .Set("OnlinePoints", e.OnlinePoints)
                    .Set("killerpoints", e.killerpoints)
                    .Set("Spouse", e.Spouse)
                    .Set("lacb", e.lacb)
                      .Set("ClaimedTeamPK", e.ClaimedTeamPK)
                    .Set("ClaimedSTeamPK", e.ClaimedSTeamPK)
                    .Set("ClaimedElitePk", e.ClaimedElitePk)
                    .Set("Money", e.Money)
                    .Set("BlackList", Persons)
                    .Set("Experience", e.Experience)
                    .Set("BankCps", e.BankCps)
                //    .Set("TitlePoints", e.TitlePoints) 
                    .Set("ConquerPoints", e.ConquerPoints)
                   .Set("boundcps", e.BoundCps)   
                    .Set("MonstersPoints", e.MonstersPoints)
                    .Set("DarkPoints", e.DarkPoints)
                 //    .Set("Donation", e.NobalityDonation)
                    .Set("Body", e.Body)
                    //.Set("vote", e.Body)
                    .Set("DeputyLeader", e.DeputyLeader)
                    .Set("Face", e.Face)
                    .Set("Class", e.Class)
                    .Set("Reborn", e.Reborn)
                    .Set("Level", e.Level)
                    .Set("Windwalker", e.Windwalker)
                    .Set("TotalPerfectionScore", e.TotalPerfectionScore)
                    .Set("Status", e.Status)
                    .Set("Status2", e.Status2)
                    .Set("Status3", e.Status3)
                    .Set("Status4", e.Status4)
                    .Set("My_Title", (byte)e.MyTitle)
                    .Set("StudyPoints", e.SubClasses.StudyPoints)
                    .Set("HairStyle", e.HairStyle)
                    .Set("EnlightsReceived", e.ReceivedEnlightenPoints)
                    .Set("PKPoints", e.PKPoints)
                    .Set("QuizPoints", e.QuizPoints)
                    .Set("OnlinePoints", e.OnlinePoints)
                    .Set("ExpBalls", c.ExpBalls)
                    .Set("MoneySave", c.MoneySave)
                    .Set("Hitpoints", e.Hitpoints)                    
                    .Set("LastDragonBallUse", c.LastDragonBallUse.Ticks)
                    .Set("Strength", e.Strength)
                    .Set("Agility", e.Agility)
                    .Set("Spirit", e.Spirit)
                    .Set("Vitality", e.Vitality)
                    .Set("PreviousX", e.PrevX)
                    .Set("PreviousY", e.PrevY)
                    .Set("Atributes", e.Atributes)
                    .Set("Mana", e.Mana)
                    .Set("VIPLevel", e.VIPLevel)
                    .Set("MapID", e.MapID)
                    .Set("X", e.X)
                    .Set("Y", e.Y)
                    .Set("VirtuePoints", c.VirtuePoints)
                    .Set("PreviousY", e.PreviousMapID)
                    .Set("EnlightenPoints", e.EnlightenPoints)
                    .Set("LastResetTime", c.LastResetTime.Ticks)
                    .Set("DoubleExpTime", e.DoubleExperienceTime)
                    .Set("DoubleExpToday", c.DoubleExpToday)
                    .Set("HeavenBlessingTime", e.HeavenBlessing)
                    .Set("InLottery", c.InLottery)
                    .Set("LotteryEntries", c.LotteryEntries)
                    .Set("LastLotteryEntry", c.LastLotteryEntry.Ticks)
                    .Set("HeadgearClaim", c.HeadgearClaim)
                    .Set("NecklaceClaim", c.NecklaceClaim)
                    .Set("ArmorClaim", c.ArmorClaim)
                    .Set("WeaponClaim", c.WeaponClaim)
                    .Set("RingClaim", c.RingClaim)
                    .Set("Flower", c.Entity.AddFlower)
                    .Set("BootsClaim", c.BootsClaim)
                    .Set("TowerClaim", c.TowerClaim)
                    .Set("FanClaim", c.FanClaim)
                    .Set("ChatBanTime", c.ChatBanTime.Ticks)
                    .Set("ChatBanLasts", c.ChatBanLasts)
                    .Set("ChatBanned", c.ChatBanned)
                    .Set("BlessTime", c.BlessTime)
                    .Set("FirstRebornClass", e.FirstRebornClass)
                    .Set("SecondRebornClass", e.SecondRebornClass)
                    .Set("FirstRebornLevel", e.FirstRebornLevel)
                    .Set("SecondRebornLevel", e.SecondRebornLevel)
                    .Set("EnlightmentWait", e.EnlightmentTime)
                    .Set("LastLogin", e.LastLogin.Ticks)
                    .Set("CountryID", (ushort)e.CountryID)
                    .Set("Achievement", e.MyAchievement.ToString())
                    .Set("ClanId", (uint)e.ClanId)
                    .Set("ClanRank", (uint)e.ClanRank);
                if (e.MapID == 601)
                    cmd.Set("OfflineTGEnterTime", c.OfflineTGEnterTime.Ticks);
                else
                    cmd.Set("OfflineTGEnterTime", "0");

                e.LastLogin = DateTime.Now;

                if (c.AsMember != null)
                {
                    cmd.Set("GuildID", c.AsMember.GuildID)
                    .Set("GuildRank", (ushort)c.AsMember.Rank)
                    .Set("GuildSilverDonation", c.AsMember.SilverDonation)
                    .Set("GuildConquerPointDonation", c.AsMember.ConquerPointDonation)
                    .Set("GuildLilies", c.AsMember.Lilies)
                    .Set("GuildRouses", c.AsMember.Rouses)
                    .Set("GuildOrchids", c.AsMember.Orchids)
                    .Set("GuildTulips", c.AsMember.Tulips)
                    .Set("Exploits", c.AsMember.Exploits)
                    .Set("GuildPkDonation", c.AsMember.PkDonation)
                          .Set("CTFCpsReward", c.AsMember.CTFCpsReward)
                                .Set("CTFSilverReward", c.AsMember.CTFSilverReward)
                    .Set("GuildLastlod", (ulong)DateTime.Now.Ticks);
                    c.AsMember.LastLogin = (ulong)DateTime.Now.Ticks;
                }
                else
                {
                    cmd.Set("GuildID", 0)
                    .Set("GuildRank", (ushort)0)
                    .Set("GuildSilverDonation", 0)
                    .Set("GuildConquerPointDonation", 0);
                }
                cmd.Where("UID", e.UID);
                cmd.Execute();
            }
            return true;
        }
        public static bool SaveEntity(Client.GameState c)
        {
            using (var conn = DataHolder.MySqlConnection)
            {
                conn.Open();
                return SaveEntity(c, conn);
            }
        }
        static bool InvalidCharacters(string Name)
        {
            foreach (char c in Name)
            {
                if (Kernel.InvalidCharacters.Contains(c) || (byte)c < 48)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CreateEntity(ref Client.GameState client)
        {
            using (var rdr = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("name", client.Entity.Name)))
            {
                if (rdr.Read())
                {
                    client.Entity.Name = client.Entity.Name + "+ Z";
                }
            }
            while (true)
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("uid", client.Entity.UID))
                using (var reader = cmd.CreateReader())
                {
                    if (reader.Read())
                        client.Entity.UID = Program.EntityUID.Next;
                    else
                        break;
                }
            }
            while (true)
            {
                try
                {
                    using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                        cmd.Insert("entities").Insert("Name", client.Entity.Name).Insert("Owner", client.Account.Username).Insert("Class", client.Entity.Class).Insert("UID", client.Entity.UID)
                            .Insert("Hitpoints", client.Entity.Hitpoints).Insert("Mana", client.Entity.Mana).Insert("Body", client.Entity.Body)
                            .Insert("Face", client.Entity.Face).Insert("HairStyle", client.Entity.HairStyle).Insert("Strength", client.Entity.Strength)
                            .Insert("WarehousePW", "").Insert("Agility", client.Entity.Agility).Insert("Vitality", client.Entity.Vitality).Insert("Spirit", client.Entity.Spirit)
                            .Execute();
                    break;
                }
                catch
                {
                    client.Entity.UID = Program.EntityUID.Next;
                }
            }

            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration").Set("EntityID", client.Entity.UID).Where("Server", Constants.ServerName))
                cmd.Execute();
            client.Account.EntityID = client.Entity.UID;
            return true;
        }
        public static bool CreateEntity(Network.GamePackets.EnitityCreate eC, Client.GameState client, ref string message)
        {
            if (eC.Name.Length > 16)
                eC.Name = eC.Name.Substring(0, 16);
            if (eC.Name == "")
                return false;
            if (eC.Name == "ChestDemon") // Golden Secret
            {
                message = "Invalid characters inside the name.";
                return false;
            }
            if (eC.Name == "ChestDemon") // Golden Secret
            {
                message = "Invalid characters inside the name.";
                return false;
            }
            if (eC.Name == "[gm]") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "{gm}") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "|gm|") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "(gm)") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "<gm>") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "IgmI") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "lgml") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "[GM]") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "{GM}") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "|GM|") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "(GM)") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "<GM>") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "IGMI") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (eC.Name == "lGMl") // Golden Secret
            {
                message = "Eh ally bt3melo dah ya kosmak.";
                return false;
            }
            if (InvalidCharacters(eC.Name))
            {
                message = "Invalid characters inside the name.";
                return false;
            }
            using (var rdr = new MySqlReader(new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("name", eC.Name)))
            {
                if (rdr.Read())
                {
                    message = "The chosen name is already in use.";
                    return false;
                }
            }
            client.Entity = new Game.Entity(Game.EntityFlag.Player, false);
            client.Entity.Name = eC.Name;
            switch (eC.Class)
            {
                case 0:
                case 1: eC.Class = 100; break;
                case 2:
                case 3: eC.Class = 10; break;
                case 4:
                case 5: eC.Class = 40; break;
                case 6:
                case 7: eC.Class = 20; break;
                case 8:
                case 9: eC.Class = 50; break;
                case 10:
                case 11: eC.Class = 60; break;
                case 12:
                case 13: eC.Class = 70; break;
                case 14:
                case 15: eC.Class = 80; break;
                case 16:
                case 17:
                    {
                        eC.Class = 160;
                        client.Entity.Windwalker = 0;
                        break;
                    }
                case 18:
                case 19:
                    {
                        eC.Class = 160;
                        client.Entity.Windwalker = 8;
                        break;
                    }
                default: { Console.WriteLine("Error Class = " + eC.Class); } break;
            }
            DataHolder.GetStats(eC.Class, 1, client);
            client.Entity.Class = eC.Class;
            client.CalculateStatBonus();
            client.CalculateHPBonus();
            client.Entity.Hitpoints = client.Entity.MaxHitpoints;
            client.Entity.Mana = (ushort)(client.Entity.Spirit * 5);
            client.Entity.Body = eC.Body;
            if (eC.Body == 1003 || eC.Body == 1004)
                client.Entity.Face = (ushort)Kernel.Random.Next(1, 50);
            else
                client.Entity.Face = (ushort)Kernel.Random.Next(201, 250);
            byte Color = (byte)Kernel.Random.Next(4, 8);
            client.Entity.HairStyle = (ushort)(Color * 100 + 10 + (byte)Kernel.Random.Next(4, 9));
            client.Entity.UID = Program.EntityUID.Next;
            client.Entity.JustCreated = true;

            while (true)
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("uid", client.Entity.UID))
                using (var reader = cmd.CreateReader())
                {
                    if (reader.Read())
                        client.Entity.UID = Program.EntityUID.Next;
                    else
                        break;
                }
            }
            while (true)
            {
                try
                {
                    using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                        cmd.Insert("entities").Insert("Name", eC.Name).Insert("Owner", client.Account.Username).Insert("Class", eC.Class).Insert("UID", client.Entity.UID)
                            .Insert("Hitpoints", client.Entity.Hitpoints).Insert("Mana", client.Entity.Mana).Insert("Body", client.Entity.Body)
                            .Insert("Face", client.Entity.Face).Insert("HairStyle", client.Entity.HairStyle).Insert("Strength", client.Entity.Strength)
                            .Insert("WarehousePW", "").Insert("Agility", client.Entity.Agility).Insert("Vitality", client.Entity.Vitality).Insert("Spirit", client.Entity.Spirit)
                            .Insert("Windwalker", client.Entity.Windwalker)
                            .Execute();

                    message = "ANSWER_OK";
                    break;
                }
                catch
                {
                    client.Entity.UID = Program.EntityUID.Next;
                }
            }

            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration").Set("EntityID", client.Entity.UID).Where("Server", Constants.ServerName))
                cmd.Execute();
            client.Account.EntityID = client.Entity.UID;
            client.Account.Save();
            return true;
        }
        public static void UpdateBankCps(Client.GameState client)
        {
            UpdateData(client, "BankCps", client.Entity.BankCps);
        }
        public static void UpdateTreasuerPoints(Client.GameState gameState)
        {
            UpdateData(gameState, "TreasuerPoints", gameState.Entity.TreasuerPoints);
        }

    }
}
