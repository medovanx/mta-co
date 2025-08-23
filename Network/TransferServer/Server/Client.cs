using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using MTA.Network.Sockets;
using System.IO;
using MTA.Network;
using MTA.Network.AuthPackets;
using MTA.Database;
using MTA.Game;
using MTA.Game.ConquerStructures;
using MTA.Client;

namespace MTA.TransferServer
{
    public class Client
    {
        public struct TranServer
        {
            public string ip;
            public string servername;
            public byte ID;
            public int port;
        }
        public static SafeDictionary<string, TranServer> TranServers = new SafeDictionary<string, TranServer>();
        public static Network.Sockets.ServerSocket WebSocket;
        public static object SynRoot = new object();
        public static void Create()
        {
            WebSocket = new ServerSocket();
            WebSocket.OnClientConnect += GameServer_OnClientConnect;
            WebSocket.OnClientReceive += GameServer_OnClientReceive;
            WebSocket.OnClientDisconnect += GameServer_OnClientDisconnect;
            WebSocket.Enable(Program.TransferServerPort, "0.0.0.0", true);
        }
        public static void Disable()
        {
            WebSocket.Disable();
        }

        static void GameServer_OnClientConnect(ClientWrapper obj)
        {
            if (TranServers.Count == 0)
                return;
            Obj client = new Obj(obj);
        }
        static void GameServer_OnClientDisconnect(ClientWrapper obj)
        {
            var client = obj.Connector as Obj;
            client.Disconnect();
        }
        static void GameServer_OnClientReceive(byte[] buffer, int length, ClientWrapper obj)
        {
            var client = obj.Connector as Obj;
            client.Packets.Enqueue(buffer, length);
            while (client.Packets.CanDequeue())
            {
                byte[] packet = client.Packets.Dequeue();
                uint ID = BitConverter.ToUInt16(packet, 2);
                switch (ID)
                {
                    case 5050:
                        {

                            var servername = Program.Encoding.GetString(packet, 24, 16);
                            servername = servername.Replace("\0", "");
                            TranServer server;
                            if (TranServers.TryGetValue(servername, out server))
                            {
                                var uid = BitConverter.ReadUint(packet, 4);
                                var user = Program.Encoding.GetString(packet, 8, 16);
                                user = user.Replace("\0", "");

                                var account = new AccountTable(user);

                                var reply = Reply.Verified;
                                if (account.exists)
                                    if (account.EntityID != 0)
                                        reply = Reply.Refused;

                                var bytes = new CheckUserReply(uid, user, reply).GetArray();
                                Send(bytes, server);
                                Console.WriteLine("Reply Send of user : " + user);
                            }
                            else
                                Console.WriteLine("Invaild ServerName : " + servername);
                            break;
                        }
                    case 5051:
                        {
                            var servername = Program.Encoding.GetString(packet, 28, 16);
                            servername = servername.Replace("\0", "");
                            TranServer server;
                            if (TranServers.TryGetValue(servername, out server))
                            {
                                var uid = BitConverter.ReadUint(packet, 4);
                                var reply = (Reply)BitConverter.ReadUint(packet, 8);
                                var user = Program.Encoding.GetString(packet, 12, 16);
                                user = user.Replace("\0", "");

                                if (reply == Reply.Refused)
                                {
                                    if (Kernel.GamePool.ContainsKey(uid))
                                    {
                                        var player = Kernel.GamePool[uid];
                                        player.MessageBox("Transfer Failed. For User :" + user + " Change it From site Game ");
                                        Console.WriteLine("Transfer Failed. For User :" + user + " Change it From site Game ");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Transfer Available. For User :" + user);
                                    var fClient = new GameState(null);
                                    if (Kernel.GamePool.ContainsKey(uid))
                                    {
                                        fClient = Kernel.GamePool[uid];
                                        fClient.MessageBox("Transfer Available. For User :" + user);
                                    }
                                    else
                                    {
                                        var account = new AccountTable(user);
                                        fClient.Fake = false;
                                        fClient.FakeLoad(uid, false);
                                        fClient.Account = account;
                                    }
                                    byte[] CreateTransfer = new MTA.TransferServer.Transfer(fClient, Constants.ServerName).GetArray();
                                    Send(CreateTransfer, server);
                                }
                            }
                            else
                                Console.WriteLine("Invaild ServerName : " + servername);
                            break;
                        }
                    case 5052:
                        {
                            TransferPlayer player = new TransferPlayer();
                            player.Deserialize(packet);
                            var servername = player.FromServer.Replace("\0", "");
                            TranServer server;
                            if (TranServers.TryGetValue(servername, out server))
                            {
                                Console.WriteLine("start Transfer For User :" + player.username);
                                #region spells
                                uint SpellsSize = BitConverter.ToUInt32(packet, 11000);
                                byte[] SpellsPacket = new byte[SpellsSize];
                                for (ushort x = 0; x < SpellsSize; x++)
                                {
                                    SpellsPacket[x] = packet[11004 + x];
                                }
                                using (var stream = new MemoryStream(SpellsPacket))
                                using (var hreader = new BinaryReader(stream))
                                {
                                    List<Interfaces.ISkill> spellscolletion;
                                    new SkillTable().GetSkill(hreader, out spellscolletion);
                                    foreach (var spell in spellscolletion)
                                    {
                                        spell.Available = false;
                                        if (!player.Spells.ContainsKey(spell.ID))
                                            player.Spells.Add(spell.ID, spell);
                                    }
                                }
                                #endregion
                                #region profs
                                uint ProfsSize = BitConverter.ToUInt32(packet, 12000);
                                byte[] ProfsPacket = new byte[ProfsSize];
                                for (ushort x = 0; x < ProfsSize; x++)
                                {
                                    ProfsPacket[x] = packet[12004 + x];
                                }

                                using (var stream = new MemoryStream(ProfsPacket))
                                using (var hreader = new BinaryReader(stream))
                                {
                                    List<Network.GamePackets.Proficiency> profscolletion;
                                    new SkillTable().GetProf(hreader, out profscolletion);
                                    foreach (var proficiency in profscolletion)
                                    {
                                        proficiency.Available = false;
                                        if (!player.Proficiencies.ContainsKey(proficiency.ID))
                                            player.Proficiencies.Add(proficiency.ID, proficiency);
                                    }
                                }
                                #endregion
                                #region items
                                // Dictionary<uint, Network.GamePackets.ConquerItem> Items = new Dictionary<uint, Network.GamePackets.ConquerItem>();

                                uint ItemCount = BitConverter.ToUInt32(packet, 200);

                                byte[] ItemArray = new byte[ItemCount * 60];
                                for (ushort x = 0; x < ItemArray.Length; x++)
                                {
                                    ItemArray[x] = packet[x + 204];
                                }
                                using (var stream = new MemoryStream((ItemArray)))
                                using (var hreader = new BinaryReader(stream))
                                {
                                    for (ushort x = 0; x < ItemCount; x++)
                                    {
                                        hreader.BaseStream.Seek(60 * x, SeekOrigin.Begin);
                                        var item = new Database.ConquerItemTable().ReadItem(hreader);
                                        if (!player.Items.ContainsKey(item.UID))
                                            player.Items.Add(item.UID, item);
                                    }
                                }
                                Dictionary<uint, Network.GamePackets.ItemAdding.Purification_> Purifications = new Dictionary<uint, Network.GamePackets.ItemAdding.Purification_>();
                                Dictionary<uint, Network.GamePackets.ItemAdding.Refinery_> Refinary = new Dictionary<uint, Network.GamePackets.ItemAdding.Refinery_>();
                                uint count = 0;

                                byte[] Artefacts = new byte[BitConverter.ToUInt32(packet, 14000)];
                                for (ushort x = 0; x < Artefacts.Length; x++)
                                {
                                    byte type = packet[14004 + count];
                                    count++;
                                    if (type == 0)
                                    {
                                        uint ItemUID = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        uint PurificationItemID = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        uint PurificationDuration = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        uint PurificationLevel = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        long Ticks = (long)BitConverter.ToUInt64(packet, (int)(14004 + count));
                                        count += 8;
                                        Network.GamePackets.ItemAdding.Purification_ purif = new Network.GamePackets.ItemAdding.Purification_();
                                        purif.Available = true;
                                        purif.ItemUID = ItemUID;
                                        purif.PurificationItemID = PurificationItemID;
                                        purif.PurificationDuration = PurificationDuration;
                                        purif.PurificationLevel = PurificationLevel;
                                        purif.AddedOn = DateTime.FromBinary(Ticks);
                                        if (!Purifications.ContainsKey(purif.ItemUID))
                                            Purifications.Add(purif.ItemUID, purif);
                                    }
                                    else
                                    {
                                        uint ItemUID = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        uint EffectID = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        uint EffectLevel = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        uint EffectPercent = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        uint EffectDuration = BitConverter.ToUInt32(packet, (int)(14004 + count));
                                        count += 4;
                                        long Ticks = (long)BitConverter.ToUInt64(packet, (int)(14004 + count));
                                        count += 8;
                                        Network.GamePackets.ItemAdding.Refinery_ refin = new Network.GamePackets.ItemAdding.Refinery_();
                                        refin.Available = true;
                                        refin.ItemUID = ItemUID;
                                        refin.EffectID = EffectID;
                                        refin.EffectLevel = EffectLevel;
                                        refin.EffectPercent = EffectPercent;
                                        refin.EffectDuration = EffectDuration;
                                        refin.AddedOn = DateTime.FromBinary(Ticks);
                                        if (!Refinary.ContainsKey(refin.ItemUID))
                                            Refinary.Add(refin.ItemUID, refin);
                                    }
                                }

                                foreach (var item in player.Items.Values)
                                {
                                    if (Purifications.ContainsKey(item.UID))
                                    {
                                        item.Purification = new Network.GamePackets.ItemAdding.Purification_();
                                        var purif = Purifications[item.UID];
                                        purif.ItemUID = item.UID;
                                        item.Purification = purif;
                                    }
                                    if (Refinary.ContainsKey(item.UID))
                                    {
                                        item.ExtraEffect = new Network.GamePackets.ItemAdding.Refinery_();
                                        var refi = Refinary[item.UID];
                                        refi.ItemUID = item.UID;
                                        item.ExtraEffect = refi;
                                    }
                                    item.UID = item.UID;
                                }

                                #endregion
                                if (player.Jiang != null && player.Jiang != "")
                                {
                                    Game.JiangHu jiang = new Game.JiangHu(0);
                                    jiang.Load(player.Jiang, player.UID);
                                }
                                #region chi
                                uint chiSize = BitConverter.ToUInt32(packet, 21000);
                                byte[] chiPacket = new byte[chiSize];
                                for (ushort x = 0; x < chiSize; x++)
                                {
                                    chiPacket[x] = packet[21004 + x];
                                }
                                using (var stream = new MemoryStream(chiPacket))
                                using (var reader = new BinaryReader(stream))
                                {
                                    int chicount = reader.ReadByte();
                                    for (int i = 0; i < chicount; i++)
                                    {
                                        var power = new ChiPowerStructure().Deserialize(reader);
                                        if (power.Power == (Enums.ChiPowerType)(i + 1))
                                            player.ChiPowers.Add(power);
                                    }
                                }
                                #endregion

                                /// save this data 
                                /// 
                                var Client = new GameState(null);
                                player.Clone(ref Client);
                                Client.Fake = false;
                                Client.JustCreated = false;
                                Client.Account.Username = Client.Account.Username.Replace("\0", "");
                                Client.Account.Password = Client.Account.Password.Replace("\0", "");
                                var account = Client.Account;



                                EntityTable.CreateEntity(ref Client);
                                Console.WriteLine("Saving UID : [" + player.UID + "] " + Client.Entity.UID);
                                Console.WriteLine("Saving Entity");
                                EntityTable.SaveEntity(Client);
                                Console.WriteLine("Saving items");
                                #region items
                                for (int ii = 0; ii < Client.Inventory.Objects.Length; ii++)
                                {
                                    var item = Client.Inventory.Objects[ii];
                                    if (item == null)
                                        continue;
                                    item.UID = Program.NextItemID;
                                    var pur = item.Purification;
                                    pur.ItemUID = item.UID;
                                    item.Purification = pur;
                                    var ext = item.ExtraEffect;
                                    ext.ItemUID = item.UID;
                                    item.ExtraEffect = ext;
                                    ConquerItemTable.AddItem(ref item, Client);
                                    if (item.Purification.Available)
                                        ItemAddingTable.AddPurification(item.Purification);
                                    if (item.ExtraEffect.Available)
                                        ItemAddingTable.AddExtraEffect(item.ExtraEffect);
                                }
                                for (int ii = 0; ii < Client.Equipment.Objects.Length; ii++)
                                {
                                    var item = Client.Equipment.Objects[ii];
                                    if (item == null)
                                        continue;
                                    item.UID = Program.NextItemID;
                                    var pur = item.Purification;
                                    pur.ItemUID = item.UID;
                                    item.Purification = pur;
                                    var ext = item.ExtraEffect;
                                    ext.ItemUID = item.UID;
                                    item.ExtraEffect = ext;
                                    ConquerItemTable.AddItem(ref item, Client);
                                    if (item.Purification.Available)
                                        ItemAddingTable.AddPurification(item.Purification);
                                    if (item.ExtraEffect.Available)
                                        ItemAddingTable.AddExtraEffect(item.ExtraEffect);
                                }
                                if (Client.Warehouses != null)
                                {
                                    foreach (var ware in Client.Warehouses.Values)
                                    {
                                        for (int ii = 0; ii < ware.Objects.Length; ii++)
                                        {
                                            var item = ware.Objects[ii];
                                            if (item == null)
                                                continue;
                                            item.UID = Program.NextItemID;
                                            var pur = item.Purification;
                                            pur.ItemUID = item.UID;
                                            item.Purification = pur;
                                            var ext = item.ExtraEffect;
                                            ext.ItemUID = item.UID;
                                            item.ExtraEffect = ext;
                                            ConquerItemTable.AddItem(ref item, Client);
                                            if (item.Purification.Available)
                                                ItemAddingTable.AddPurification(item.Purification);
                                            if (item.ExtraEffect.Available)
                                                ItemAddingTable.AddExtraEffect(item.ExtraEffect);
                                        }
                                    }
                                }
                                #endregion
                                Console.WriteLine("Saving profs");
                                SkillTable.SaveProficiencies(Client);
                                Console.WriteLine("Saving spells");
                                SkillTable.SaveSpells(Client);

                                Console.WriteLine("Saving chi");
                                var command = new MySqlCommand(MySqlCommandType.INSERT);
                                command.Insert("chi").Insert("uid", Client.Entity.UID).Insert("name", Client.Entity.Name);
                                command.Execute();

                                ChiTable.Save(Client);

                                Console.WriteLine("Saving JiangHu");
                                if (Client.Entity.MyJiang != null)
                                {
                                    Client.Entity.MyJiang.UID = Client.Entity.UID;
                               //     Database.JiangHu.New();
                                    Database.JiangHu.SaveJiangHu();
                                    if (!Game.JiangHu.JiangHuClients.ContainsKey(Client.Entity.UID))
                                        Game.JiangHu.JiangHuClients.TryAdd(Client.Entity.UID, Client.Entity.MyJiang);
                                }

                                Console.WriteLine("Saving Account with new UID " + Client.Entity.UID);
                                Client.Account.EntityID = Client.Entity.UID;
                                var acc = new AccountTable(Client.Account.Username);
                                if (!acc.exists)
                                    Client.Account.Insert();

                                Client.Account.Save();

                                byte[] DoneTransfer = new MTA.TransferServer.DoneUser(player.UID, player.username).GetArray();
                                Send(DoneTransfer, server);     
                                Console.WriteLine("Saving UID : " + account.EntityID + " Done.");
                            }
                            else
                                Console.WriteLine("Invaild ServerName : " + servername);
                            break;
                        }
                    case 5053:
                        {
                            var servername = Program.Encoding.GetString(packet, 24, 16);
                            servername = servername.Replace("\0", "");
                            TranServer server;
                            if (TranServers.TryGetValue(servername, out server))
                            {
                                var uid = BitConverter.ReadUint(packet, 4);
                                var user = Program.Encoding.GetString(packet, 8, 16);
                                user = user.Replace("\0", "");
                                var fClient = new GameState(null);
                                if (Kernel.GamePool.ContainsKey(uid))
                                {
                                    fClient = Kernel.GamePool[uid];
                                    fClient.MessageBox("Transfer Available. For User :" + user);
                                }
                                else
                                {
                                    var account = new AccountTable(user);
                                    fClient.Fake = false;
                                    fClient.FakeLoad(uid, false);
                                    fClient.Account = account;
                                }
                                // if (!Kernel.TransferdPlayersQueue.ContainsKey(fClient.Entity.UID))
                                {
                                    fClient.Account.State = AccountTable.AccountState.Transfered;
                                    fClient.Account.Save();
                                    fClient.Account.EntityID = 0;
                                    fClient.Account.Save();
                                    fClient.Disconnect();
                                    //  Kernel.TransferdPlayersQueue.Add(fClient.Entity.UID, fClient);                                    
                                }
                            }
                            else
                                Console.WriteLine("Invaild ServerName : " + servername);
                            break;
                        }

                }
            }
        }

        private static void Send(byte[] bytes, TranServer server)
        {
            try
            {
                System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                socket.Connect(server.ip, server.port);
                socket.SendBufferSize = ushort.MaxValue;
                if (socket.Connected)
                {
                    socket.SendTo(bytes, socket.RemoteEndPoint);
                }
            }
            catch (System.Net.Sockets.SocketException e) { Console.WriteLine(e.Message); }
        }

        internal static bool CheckServer(uint uid, string user, string servername)
        {
            Console.WriteLine("Checking Server Transfer For " + user);
            TranServer server;
            if (TranServers.TryGetValue(servername, out server))
            {
                byte[] Check = new TransferServer.CheckUser(uid, user).GetArray();
                try
                {
                    System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                    socket.Connect(server.ip, server.port);
                    socket.SendBufferSize = ushort.MaxValue;
                    if (socket.Connected)
                    {
                        socket.SendTo(Check, socket.RemoteEndPoint);
                        return true;
                    }
                }
                catch (System.Net.Sockets.SocketException e) { Console.WriteLine(e.Message); return false; }
            }
            return false;
        }
    }
}