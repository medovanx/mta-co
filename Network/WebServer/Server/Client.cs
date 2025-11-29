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

namespace MTA.WebServer
{
    public class Client
    {
        public struct TranServer
        {
            public string ip;
            public string servername;
            public uint Key;
            public byte ID;
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
            WebSocket.Enable(Program.WebServerPort, "0.0.0.0", true);
            Console.WriteLine("WebServer online : " + Program.WebServerPort);
        }
        public static void Disable()
        {
            WebSocket.Disable();
        }

        public class Obj
        {
            public Network.BigConcurrentPacketQueue Packets;
            private ClientWrapper sock;
            public Obj(ClientWrapper obj)
            {
                sock = obj;
                sock.Connector = this;
                Packets = new Network.BigConcurrentPacketQueue(0);
            }

            public void Disconnect()
            {
                sock.Disconnect();
            }

        }
        static void GameServer_OnClientConnect(ClientWrapper obj)
        {
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
                uint ID = BitConverter.ToUInt32(packet, 2);
                switch (ID)
                {
                    case 47883:
                        {
                            TranServer Server;
                            if (TranServers.TryGetValue(obj.IP, out Server))
                            {
                                TransferPlayer player = new TransferPlayer();
                                player.Deserialize(packet);
                                player.Server = Server;
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

                                if (!Kernel.TransferPool.ContainsKey(player.UID))
                                    Kernel.TransferPool.Add(player.UID, player);
                                else
                                    Kernel.TransferPool[player.UID] = player;

                                Console.WriteLine("[" + player.UID + "] " + player.username + "has been added to TransferPool.");
                            }
                            else
                                Console.WriteLine("Invaild WebServer Connect IP: " + obj.IP);
                            break;
                        }
                }
            }
        }

        private static string ReadString(byte[] packet, int p, ushort p_2)
        {
            return Program.Encoding.GetString(packet, p, p_2);
        }
        public static uint GetCountSouls(Dictionary<uint, Network.GamePackets.ConquerItem> items)
        {
            uint count = 0;
            foreach (var item in items.Values)
            {
                if (item.ExtraEffect.ItemUID != 0)
                    count++;
                if (item.Purification.ItemUID != 0)
                    count++;
            }
            return count;
        }
    }
}