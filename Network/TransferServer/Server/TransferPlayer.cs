using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Network.GamePackets;
using MTA.Game;
using System.Collections.Concurrent;
using MTA.Interfaces;
using MTA.Network;
using System.IO;
using MTA.Game.ConquerStructures;

namespace MTA.TransferServer
{  
    public partial class TransferPlayer
    {
        public TransferPlayer()
        {

        }
        public void Deserialize(byte[] buffer)
        {
            packet = buffer;
        }
        public byte[] packet;
        public uint UID
        {
            get
            {
                return BitConverter.ToUInt32(packet, 78);
            }
        }
        public string FromServer
        {
            get
            {
                return PacketHandler.ReadString(packet, 161, packet[160]);
            }
        }
        public string Name
        {
            get
            {
               // return "Tra_" + Key;
                return PacketHandler.ReadString(packet, 181, packet[180]);
            }
        }
        public uint Money
        {
            get
            {
                return BitConverter.ToUInt32(packet, 6);
            }
        }
        public uint Cpc { get { return BitConverter.ToUInt32(packet, 10); } }
        public uint Class { get { return BitConverter.ToUInt16(packet, 14); } }
        public uint SecoundClass { get { return BitConverter.ToUInt16(packet, 16); } }
        public uint FirstClass { get { return BitConverter.ToUInt16(packet, 18); } }
        public uint Reborn { get { return BitConverter.ToUInt32(packet, 22); } }        
        public uint Haire { get { return BitConverter.ToUInt32(packet, 30); } }
        public uint Body { get { return BitConverter.ToUInt32(packet, 34); } }
        public uint Level { get { return BitConverter.ToUInt32(packet, 38); } }
        public uint Str { get { return BitConverter.ToUInt32(packet, 42); } }
        public uint Agi { get { return BitConverter.ToUInt32(packet, 46); } }
        public uint Spi { get { return BitConverter.ToUInt32(packet, 50); } }
        public uint Vit { get { return BitConverter.ToUInt32(packet, 54); } }
        public uint Atr { get { return BitConverter.ToUInt32(packet, 58); } }
        public uint Face { get { return BitConverter.ToUInt32(packet, 62); } }
        public uint QuizPoints { get { return BitConverter.ToUInt32(packet, 66); } }
        public uint Vip { get { return BitConverter.ToUInt32(packet, 70); } }
        public uint Study { get { return BitConverter.ToUInt32(packet, 74); } }
        public ulong NobilityInformation { get { return BitConverter.ToUInt64(packet, 82); } }
        public NobilityRank NobilityRank { get { return (NobilityRank)packet[90]; } }

        public ushort GuildID { get { return BitConverter.ToUInt16(packet, 100); } }
        public ushort GuildRank { get { return BitConverter.ToUInt16(packet, 102); } }
        public string GuildName { get { return PacketHandler.ReadString(packet, 104, 16); } }
        public string GuildLeaderName { get { return PacketHandler.ReadString(packet, 120, 16); } }
        
        public Database.AccountTable.AccountState State
        {
            get
            {
                return (Database.AccountTable.AccountState)packet[23999];
            }
        }
        public string username
        {
            get
            {
                return PacketHandler.ReadString(packet, 24001 , packet[24000]);
            }
        }
        public string password
        {
            get
            {
                return PacketHandler.ReadString(packet, 24017, packet[24016]);
            }
        }
        public string ip
        {
            get
            {
                return PacketHandler.ReadString(packet, 24033, packet[24032]);
            }
        }
        public string Jiang
        {
            get
            {
                return Program.Encoding.GetString(packet, 20004, BitConverter.ToUInt16(packet, 20000)).Replace("\0", "");
            }
        }
        public SafeDictionary<ushort, Interfaces.IProf> Proficiencies = new SafeDictionary<ushort, Interfaces.IProf>();
        public SafeDictionary<ushort, Interfaces.ISkill> Spells = new SafeDictionary<ushort, Interfaces.ISkill>();
        public Dictionary<uint, Network.GamePackets.ConquerItem> Items = new Dictionary<uint, Network.GamePackets.ConquerItem>();
        public List<ChiPowerStructure> ChiPowers = new List<ChiPowerStructure>();
        public void Clone(ref MTA.Client.GameState client)
        {
            var Player = this;
            client.ReadyToPlay();            
            client.Account = new Database.AccountTable(null);
            client.Account.State = State;
            client.Account.Username = username;
            client.Account.Password = password;
            client.Account.IP = ip;           
            client.Account.EntityID = Player.UID;
             
            client.Entity = new Game.Entity(Game.EntityFlag.Player, false);
            client.Entity.Name = Player.Name;
            client.Entity.Spouse = "None";
            client.Entity.UID = Player.UID;
            client.Entity.Money = Player.Money;
            client.Entity.MyFlowers = new Game.Features.Flowers(client.Entity.UID, client.Entity.Name);
            client.Entity.Owner = client;
            client.Entity.MyAchievement = new Game.Achievement(client.Entity);
            client.Entity.Titles = new ConcurrentDictionary<TitlePacket.Titles, DateTime>();
            client.ElitePKStats = new ElitePK.FighterStats(client.Entity.UID, client.Entity.Name, client.Entity.Mesh);
            client.Entity.ConquerPoints = Player.Cpc;
            client.Entity.Class = (byte)Player.Class;
            client.Entity.SecondRebornClass = (byte)Player.SecoundClass;
            client.Entity.FirstRebornClass = (byte)Player.FirstClass;
            client.Entity.Reborn = (byte)Player.Reborn;
            client.Entity.HairStyle = (ushort)Player.Haire;
            client.Entity.Body = (ushort)Player.Body;
            client.Entity.Level = (byte)Player.Level;
            client.Entity.Strength = (ushort)Player.Str;
            client.Entity.Agility = (ushort)Player.Agi;
            client.Entity.Spirit = (ushort)Player.Spi;
            client.Entity.Vitality = (ushort)Player.Vit;
            client.Entity.Atributes = (ushort)Player.Atr;
            client.Entity.Face = (ushort)Player.Face;
            client.Entity.QuizPoints = (ushort)Player.QuizPoints;
            client.Entity.VIPLevel = (byte)Player.Vip;
            if (client.Entity.SubClasses != null)
                client.Entity.SubClasses.StudyPoints = (ushort)Player.Study;


            client.Variables = new VariableVault();
            client.Friends = new SafeDictionary<uint, Game.ConquerStructures.Society.Friend>();
            client.Enemy = new SafeDictionary<uint, Game.ConquerStructures.Society.Enemy>();
            client.ChiPowers = new List<ChiPowerStructure>();
            if (ChiPowers != null)
                client.ChiPowers = ChiPowers;
            client.ChiData = new MTA.Database.ChiTable.ChiData() { UID = client.Entity.UID, Name = client.Entity.Name, Powers = client.ChiPowers };

            client.NobilityInformation = new MTA.Game.ConquerStructures.NobilityInformation();
            client.NobilityInformation.EntityUID = client.Entity.UID;
            client.NobilityInformation.Name = client.Entity.Name;
            client.Entity.NobalityDonation = client.NobilityInformation.Donation = NobilityInformation;
            client.Entity.NobilityRank = client.NobilityInformation.Rank = NobilityRank;
            client.NobilityInformation.Position = -1;
            client.NobilityInformation.Gender = 1;
            client.NobilityInformation.Mesh = client.Entity.Mesh;
            if (client.Entity.Body % 10 >= 3)
                client.NobilityInformation.Gender = 0;


            client.TeamArenaStatistic = new MTA.Network.GamePackets.TeamArenaStatistic(true);
            client.TeamArenaStatistic.EntityID = client.Entity.UID;
            client.TeamArenaStatistic.Name = client.Entity.Name;
            client.TeamArenaStatistic.Level = client.Entity.Level;
            client.TeamArenaStatistic.Class = client.Entity.Class;
            client.TeamArenaStatistic.Model = client.Entity.Mesh;
            client.TeamArenaStatistic.Status = Network.GamePackets.TeamArenaStatistic.NotSignedUp;

            client.ArenaStatistic = new MTA.Network.GamePackets.ArenaStatistic(true);
            client.ArenaStatistic.EntityID = client.Entity.UID;
            client.ArenaStatistic.Name = client.Entity.Name;
            client.ArenaStatistic.Level = client.Entity.Level;
            client.ArenaStatistic.Class = client.Entity.Class;
            client.ArenaStatistic.Model = client.Entity.Mesh;
            client.ArenaStatistic.LastArenaPointFill = DateTime.Now;
            client.ArenaStatistic.Status = Network.GamePackets.ArenaStatistic.NotSignedUp;

            client.ChampionStats = new MTA.Network.GamePackets.ChampionStatistic(true);
            client.ChampionStats.UID = client.Entity.UID;
            client.ChampionStats.Name = client.Entity.Name;
            client.ChampionStats.Level = client.Entity.Level;
            client.ChampionStats.Class = client.Entity.Class;
            client.ChampionStats.Model = client.Entity.Mesh;
            client.ChampionStats.Points = 0;
            client.ChampionStats.LastReset = DateTime.Now;
                       
            if (Jiang != "")
            {
                client.Entity.MyJiang = new Game.JiangHu(0);
                client.Entity.MyJiang.Load(Jiang);
            }

            client.Entity.FullyLoaded = true;
            client.ClaimableItem = new SafeDictionary<uint, DetainedItem>();
            client.DeatinedItem = new SafeDictionary<uint, DetainedItem>();
            client.Spells = new SafeDictionary<ushort, ISkill>();
            client.Proficiencies = new SafeDictionary<ushort, IProf>();
            client.Partners = new SafeDictionary<uint, Game.ConquerStructures.Society.TradePartner>();
            client.Apprentices = new SafeDictionary<uint, Game.ConquerStructures.Society.Apprentice>();

            if (Player.Proficiencies.Count > 0)
                client.Proficiencies = Player.Proficiencies;

            if (Player.Spells.Count > 0)
                client.Spells = Player.Spells;

            if (Player.Items.Count > 0)
            {
                foreach (var item in Player.Items.Values)
                {
                    item.UID = item.UID;
                    if (item.Purification.Available)
                    {
                        var ps = item.Purification;
                        ps.ItemUID = item.UID;
                        item.Purification = ps;
                    }
                    if (item.ExtraEffect.Available)
                    {
                        var ps = item.ExtraEffect;
                        ps.ItemUID = item.UID;
                        item.ExtraEffect = ps;
                    }                    
                    switch (item.Position)
                    {
                        case 0: client.Inventory.Add(item, Game.Enums.ItemUse.None); break;
                        default:
                            if (item.Position > 40) { client.Inventory.Add(item, Game.Enums.ItemUse.None); break; }

                            if (client.Equipment.Free((byte)item.Position))
                                client.Equipment.Add(item, Game.Enums.ItemUse.None);
                            else
                            {
                                if (client.Inventory.Count < 40)
                                {
                                    item.Position = 0;
                                    client.Inventory.Add(item, Game.Enums.ItemUse.None);
                                    if (client.Warehouses[MTA.Game.ConquerStructures.Warehouse.WarehouseID.Market].Count < 20)
                                        client.Warehouses[MTA.Game.ConquerStructures.Warehouse.WarehouseID.Market].Add(item);

                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}
