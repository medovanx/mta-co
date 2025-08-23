using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Client;
using System.Collections.Concurrent;
using MTA.Game.ConquerStructures.Society;
using MTA.Network;
using MTA.Database;

namespace MTA.Game
{
    public class CaptureTheFlag
    {
        public class CTF_Rank : Writer
        {
            private byte[] Packet;
            public CTF_Rank(GameState client)
            {
                this.Packet = new byte[354];
                Writer.WriteUInt16(346, 0, this.Packet);
                Writer.WriteUInt16(1063, 2, this.Packet);
                this.Packet[4] = 9;
                this.Packet[6] = 1;
                this.Packet[14] = 8;
                Writer.WriteUInt32(client.Guild.CTFPoints, 18, this.Packet);
            }
            public void Send(GameState client)
            {
                client.Send(this.ToArray());
            }
            public byte[] ToArray()
            {
                ushort num = 30;
                Guild[] array = (
                    from p in Kernel.Guilds.Values
                    where p.CTFPoints != 0u
                    orderby p.CTFPoints descending
                    select p).ToArray<Guild>();
                if (array != null)
                {
                    byte b = 0;
                    while ((int)b < array.Length && b != 9)
                    {
                        Writer.WriteString(array[(int)b].Name, (int)num, this.Packet);
                        num += 16;
                        Writer.WriteUInt32(array[(int)b].CTFPoints, (int)num, this.Packet);
                        num += 4;
                        Writer.WriteUInt32(array[(int)b].MemberCount, (int)num, this.Packet);
                        num += 4;
                        Writer.WriteUInt64((ulong)array[(int)b].CTFdonationSilverold, (int)num, this.Packet);
                        num += 8;
                        Writer.WriteUInt32(array[(int)b].CTFdonationCPsold, (int)num, this.Packet);
                        num += 4;
                        b += 1;
                    }
                }
                return this.Packet;
            }
        }
        public class Base
        {
            public SobNpcSpawn Flag;
            public ConcurrentDictionary<uint, uint> Scores;
            public uint CapturerID;

            public Base(SobNpcSpawn flag)
            {
                Flag = flag;
                Scores = new ConcurrentDictionary<uint, uint>();
                CapturerID = 0;
            }

            public void Capture()
            {
                if (Scores.Count == 0) Scores.Add((uint)0, (uint)0);
                uint guildId = Scores.OrderByDescending(p => p.Value).FirstOrDefault().Key;
                CapturerID = guildId;
                var guild = Kernel.Guilds[guildId];
                Flag.Name = guild.Name;
                Flag.Hitpoints = Flag.MaxHitpoints;
                Kernel.SendScreen(Flag, Flag);
                foreach (var player in Program.Values)
                {
                    if (player.Entity.MapID == MapID)
                    {
                        player.Send(generatePacket2(5, (Flag.UID - 20570)));
                    }
                }
                Scores.Clear();
            }
        }
        public const ushort MapID = 2057;

        private Map Map;
        public Dictionary<uint, Base> Bases;
        public static bool IsWar;
        public static DateTime StartTime;

        public CaptureTheFlag()
        {
            Bases = new Dictionary<uint, Base>();
            if (!Kernel.Maps.ContainsKey((int)2057L))
            {
                new Map(MapID, DMaps.MapPaths[MapID]);
            }
            Map = Kernel.Maps[MapID];
            foreach (var npc in Map.Npcs.Values)
                if (npc is SobNpcSpawn)
                    Bases.Add(npc.UID, new Base((SobNpcSpawn)npc));
            SpawnFlags();
        }

        public void SpawnFlags()
        {
            int toAdd = 6 - Map.StaticEntities.Count;
            for (int i = toAdd; i > 0; i--)
            {
                var coords = Map.RandomCoordinates();
                StaticEntity entity = new StaticEntity((uint)(coords.Item1 * 1000 + coords.Item2), coords.Item1, coords.Item2, MapID);
                //System.Console.WriteLine("X : {0}  | Y : {1}", coords.Item1, coords.Item2);
                entity.DoFlag();
                Map.AddStaticEntity(entity);
            }
        }

        public bool SignUp(GameState client)
        {
            if (client.Entity.GuildID == 0) return false;
            if (client.Guild == null) return false;
            var coords = Map.RandomCoordinates(482, 367, 27);
            client.Entity.Teleport(MapID, coords.Item1, coords.Item2);
            return true;
        }

        public void AroundBase(GameState client)
        {
            if (client.Entity.MapID != MapID) return;
            if (client.Entity.GuildID == 0) return;
            if (client.Guild == null) return;

            foreach (var _base in Bases.Values)
            {
                if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, _base.Flag.X, _base.Flag.Y) == 9)
                {
                    if (_base.CapturerID == client.Entity.GuildID)
                    {
                        if (client.Entity.ContainsFlag2(Update.Flags2.CarryingFlag))
                        {
                            client.Send(generateTimer(0));
                            client.Send(generateEffect(client));
                            client.Entity.RemoveFlag2(Update.Flags2.CarryingFlag);
                            // client.Entity.nRemoveFlag(91);
                            client.Guild.CTFPoints += 15;
                            Program.World.CTF.SendUpdates();
                            client.Send(generatePacket(7, client.Entity.UID));
                            client.AsMember.Exploits += (ushort)(client.Entity.Level / 2);
                        }
                    }
                }
                else
                {
                    foreach (var item in client.Map.StaticEntities.Values)
                    {
                        if (Kernel.GetDistance(item.X, item.Y, client.Entity.X, client.Entity.Y) == 0)
                        {
                            if (client.Entity.ContainsFlag2(Update.Flags2.CarryingFlag))
                            {
                                client.Entity.FlagStamp = Time32.Now;

                                client.Send(Program.World.CTF.generateTimer(60));
                                client.Send(Program.World.CTF.generateEffect(client));
                                client.Guild.CTFPoints += 3;
                                client.Map.RemoveStaticItem(item);
                                client.RemoveScreenSpawn(item, true);
                            }
                        }
                    }
                }
            }
        }

        public static bool Attackable(Game.Entity entity)
        {
            return Kernel.GetDistance(entity.X, entity.Y, 482, 367) > 32;
        }

        public void AddScore(uint damage, Guild guild, SobNpcSpawn attacked)
        {
            if (Bases.ContainsKey(attacked.UID))
            {
                {
                    var _base = Program.World.CTF.Bases[attacked.UID];
                    if (!_base.Scores.ContainsKey(guild.ID))
                        _base.Scores.Add(guild.ID, damage);
                    else
                        _base.Scores[guild.ID] += damage;
                }


            }
        }

        public void FlagOwned(SobNpcSpawn attacked)
        {
            if (Bases.ContainsKey(attacked.UID))
            {
                foreach (var player in Program.Values)
                {
                    if (player.Entity.MapID == MapID)
                    {
                        player.Send(generatePacket(5, (attacked.UID - 20570)));
                    }
                }
            }
        }

        public static void Close()
        {
            foreach (var player in Program.Values)
                if (player.Entity.MapID == MapID)
                    player.Entity.Teleport(1002, 439, 390);

            var array = Kernel.Guilds.Values.Where(p => p.CTFPoints != 0).OrderByDescending(p => p.CTFPoints).ToArray();

            for (int i = 0; i < Math.Min(8, array.Length); i++)
            {
                array[i].CalculateCTFRANK(true);
                Database.GuildTable.SaveCTFPoins(array[i]);

                if (i == 0)
                {
                    array[i].CTFReward += 10;
                    array[i].ConquerPointFund += 3000;
                    array[i].SilverFund += 120000000;
                }
                else if (i == 1)
                {
                    array[i].CTFReward += 9;
                    array[i].ConquerPointFund += 2000;
                    array[i].SilverFund += 100000000;
                }
                else if (i == 2)
                {
                    array[i].CTFReward += 8;
                    array[i].ConquerPointFund += 1000;
                    array[i].SilverFund += 80000000;
                }
                else if (i == 3)
                {
                    array[i].CTFReward += 7;
                    array[i].ConquerPointFund += 600;
                    array[i].SilverFund += 65000000;
                }
                else if (i == 4)
                {
                    array[i].CTFReward += 6;
                    array[i].ConquerPointFund += 500;
                    array[i].SilverFund += 50000000;
                }
                else if (i == 5)
                {
                    array[i].CTFReward += 5;
                    array[i].ConquerPointFund += 400;
                    array[i].SilverFund += 40000000;
                }
                else if (i == 6)
                {
                    array[i].CTFReward += 4;
                    array[i].ConquerPointFund += 300;
                    array[i].SilverFund += 30000000;
                }
                else if (i == 7)
                {
                    array[i].CTFReward += 3;
                    array[i].ConquerPointFund += 200;
                    array[i].SilverFund += 20000000;
                }
                Database.GuildTable.SaveCTFReward(array[i]);
                array[i].CTFdonationCPs = array[i].CTFdonationCPsold;
                array[i].CTFdonationSilver = array[i].CTFdonationSilverold;
                array[i].CTFdonationCPsold = 0;
                array[i].CTFdonationSilverold = 0;
            }
            if (array.Length > 8)
            {
                for (int x = 8; x < array.Length; x++)
                {
                    array[x].CTFPoints = 0;
                    foreach (var meme in array[x].Members.Values)
                    {
                        meme.Exploits = 0;
                        meme.ExploitsRank = 0;
                    }
                }
            }
        }

        public void SendUpdates(GameState client)
        {
            if (Time32.Now > client.CTFUpdateStamp.AddSeconds(5))
            {
                client.CTFUpdateStamp = Time32.Now;
                var buffer = generateCTFRanking4();
                client.Send(buffer);
                foreach (var _base in Bases.Values)
                {
                    if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, _base.Flag.X, _base.Flag.Y) <= 9)
                    {
                        buffer = generateFlagRanking(_base);
                        client.Send(buffer);
                    }
                }
            }
        }

        public byte[] generateCTFRanking4()
        {
            var array = Kernel.Guilds.Values.Where(p => p.CTFPoints != 0).OrderByDescending(p => p.CTFPoints).ToArray();
            return generateList4(2, array, p => p.CTFPoints);
        }

        public byte[] generateCTFRanking()
        {
            var array = Kernel.Guilds.Values.Where(p => p.CTFPoints != 0).OrderByDescending(p => p.CTFPoints).ToArray();
            return generateList(2, array, p => p.CTFPoints);
        }

        private byte[] generateFlagRanking(Base flag)
        {
            var scores = flag.Scores.OrderByDescending(p => p.Value).ToArray();
            var array = new Guild[Math.Min(5, scores.Length)];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Kernel.Guilds[scores[i].Key];
                array[i].CTFFlagScore = scores[i].Value;
            }
            return generateList(1, array, p => p.CTFFlagScore);
        }

        private byte[] generateList(int type, Guild[] array = null, Func<Guild, UInt32> select = null)
        {
            byte[] data = null;
            if (array == null)
                data = new byte[48];
            else
                data = new byte[48 + (array.Length * 24)];
            Writer.WriteInt32(data.Length - 8, 0, data);
            Writer.WriteUInt16(2224, 2, data);
            Writer.WriteInt32(type, 4, data);
            if (array != null)
            {
                Writer.WriteInt32(array.Length, 28, data);
                for (int i = 0; i < array.Length; i++)
                {
                    int offset = 32 + (i * 24);
                    Writer.WriteInt32(i, offset, data); offset += 4;
                    Writer.WriteUInt32(select(array[i]), offset, data); offset += 4;
                    Writer.WriteString(array[i].Name, offset, data); offset += 16;
                }
            }
            return data;
        }
        private byte[] generateList4(int type, Guild[] array = null, Func<Guild, UInt32> select = null)
        {
            byte[] data = null;
            if (array == null)
                data = new byte[48 + 4];
            else
                data = new byte[48 + 4 + (array.Length * 24)];
            Writer.WriteInt32(data.Length - 8, 0, data);
            Writer.WriteUInt16(2224, 2, data);
            Writer.WriteInt32(type, 4 + 4, data);
            if (array != null)
            {
                Writer.WriteInt32(array.Length, 28 + 4, data);
                for (int i = 0; i < array.Length; i++)
                {
                    int offset = 32 + 4 + (i * 24);
                    Writer.WriteInt32(i, offset, data); offset += 4;
                    Writer.WriteUInt32(select(array[i]), offset, data); offset += 4;
                    Writer.WriteString(array[i].Name, offset, data); offset += 16;

                }
            }
            return data;
        }

        public byte[] generateTimer(uint time)
        {
            return generatePacket(8, time);
        }

        public byte[] generateEffect(GameState Client)
        {
            //return generatePacket3(6, 6327607);
            return generatePacket(6, Client.Entity.UID);
        }

        private byte[] generatePacket(int type, uint dwParam)
        {
            byte[] data = new byte[48];
            Writer.WriteInt32(data.Length - 8, 0, data);
            Writer.WriteUInt16(2224, 2, data);
            Writer.WriteInt32(type, 4, data);
            Writer.WriteUInt32(dwParam, 8, data);
            return data;
        }

        public static byte[] generatePacket2(int type, uint dwParam)
        {
            byte[] data = new byte[48];
            Writer.WriteInt32(data.Length - 8, 0, data);
            Writer.WriteUInt16(2224, 2, data);
            Writer.WriteInt32(type, 4, data);
            Writer.WriteUInt32(dwParam, 8, data);
            return data;
        }

        public void SendUpdates()
        {
            foreach (var player in Program.Values)
                if (player.Entity.MapID == MapID)
                    SendUpdates(player);
        }

        public void CloseList(GameState client)
        {
            client.Send(generateList(3));
        }



        public static void CTFGuildsRank(GameState client, byte[] packet)
        {
            var array = Kernel.Guilds.Values.Where(p => p.CTFPoints != 0).OrderByDescending(p => p.CTFPoints).ToArray();

            const byte maxcount = 5;
            byte page = packet[10];

            if (array.Length == 0)
                return;

            byte[] buffer = new byte[908];
            Writer.WriteUInt16(900, 0, buffer);
            Writer.WriteUInt16(1063, 2, buffer);
            Writer.WriteUInt16(0, 4, buffer);
            Writer.WriteUInt32(1, 6, buffer);
            Writer.WriteUInt32((uint)(array.Length), 10, buffer);
            Writer.WriteUInt32((uint)(array.Length), 14, buffer);

            Writer.WriteUInt32(client.Guild.CTFdonationCPsold, 18, buffer);
            Writer.WriteUInt64((ulong)client.Guild.CTFdonationSilverold, 22, buffer);

            int offset = 30;
            for (ushort x = (ushort)(page * maxcount - maxcount); x < page * maxcount; x++)
            {
                if (x >= array.Length) break;
                var guild = array[x];
                Writer.WriteUInt32(guild.CTFdonationCPs, offset, buffer);
                offset += 4;
                Writer.WriteUInt64((ulong)guild.CTFdonationSilver, offset, buffer);
                offset += 8;
                Writer.WriteString(guild.Name, offset, buffer);
                offset += 0x24;
                Writer.WriteUInt32(guild.ID, offset, buffer);
                offset += 4;
            }
            client.Send(buffer);
        }

        public static void CTFGuildsRank2(GameState client, byte[] packet)
        {
            const byte maxcount = 5;
            byte page = packet[10];


            var guild_array = Kernel.Guilds.Values.Where(p => p.CTFPoints != 0).OrderByDescending(p => p.CTFPoints).ToArray();
            if (guild_array.Length == 0)
                return;
            byte[] buffer2 = new byte[0x38c];
            Writer.WriteUInt16(900, 0, buffer2);
            Writer.WriteUInt16(0x427, 2, buffer2);
            Writer.WriteUInt16(0, 4, buffer2);
            Writer.WriteUInt16(page, 6, buffer2);
            Writer.WriteUInt32(10, 10, buffer2);
            Writer.WriteUInt32((uint)guild_array.Length, 14, buffer2);
            client.Guild = client.AsMember.Guild;
            Writer.WriteUInt32(client.Guild.CTFdonationCPsold, 0x12, buffer2);
            Writer.WriteUInt64((ulong)client.Guild.CTFdonationSilverold, 0x16, buffer2);
            ushort offset = 30;

            for (ushort x = (ushort)(page * maxcount - maxcount); x < page * maxcount; x++)
            {
                if (x >= guild_array.Length) break;
                var guild = guild_array[x];
                Writer.WriteUInt32(guild.CTFdonationCPsold, offset, buffer2);
                offset += 4;
                Writer.WriteUInt64((ulong)guild.CTFdonationSilverold, offset, buffer2);
                offset += 8;
                Writer.WriteString(guild.Name, offset, buffer2);
                offset += 0x24;
                Writer.WriteUInt32(guild.ID, offset, buffer2);
                offset += 4;
            }

            client.Send(buffer2);
        }

        public static void CTFsRank(GameState client, byte[] packet)
        {
            var array = Kernel.Guilds.Values.Where(p => p.CTFPoints != 0).OrderByDescending(p => p.CTFPoints).ToArray();
            byte[] Packet = new byte[356];
            Writer.WriteUInt16(348, 0, Packet);
            Writer.WriteUInt16(1063, 2, Packet);
            Packet[4] = 9;//type
            Packet[6] = 1; // count
            Packet[14] = 8; // count
            ushort index = 26;
            Writer.WriteUInt32(client.Entity.UID, 18, Packet);
            for (byte i = 0; i < array.Length; i++)
            {
                if (i >= 8)
                    break;
                if (i >= array.Length)
                    break;
                var guild = array[i];
                Packet[index] = (byte)(i + 1);
                index = (ushort)(index + 4);
                Writer.WriteString(guild.Name, index, Packet);
                index = (ushort)(index + 16);
                Writer.WriteUInt32(guild.CTFPoints, index, Packet);
                index = (ushort)(index + 4);
                Writer.WriteUInt32(guild.MemberCount, index, Packet);
                index = (ushort)(index + 12);
            }
            client.Send(Packet);
        }

        public static void CTFExpolitsRank(GameState client, byte[] packet)
        {
            byte page = packet[10];

            var array = client.Guild.Members.Values.Where(p => p.Exploits != 0).OrderByDescending(p => p.Exploits).ToArray();
            if (array.Length == 0)
                return;

            byte[] buffer = new byte[0x38c];
            Writer.WriteUInt16(900, 0, buffer);
            Writer.WriteUInt16(0x427, 2, buffer);
            Writer.WriteUInt16(8, 4, buffer);
            Writer.WriteUInt32(page, 6, buffer);
            Writer.WriteUInt32((uint)array.Length, 10, buffer);
            Writer.WriteUInt32(8, 14, buffer);
            Writer.WriteUInt32(client.AsMember.Exploits, 0x12, buffer);
            int offset = 30;
            for (ushort x = 0; x < array.Length; x++)//(ushort)(page * maxcount - maxcount); x < page * maxcount; x++)
            {
                if (x > 20)
                    break;
                if (x >= array.Length) break;
                var member = array[x];
                Writer.WriteString(member.Name, offset, buffer);
                offset += 0x10;

                Writer.WriteUInt32(member.Exploits, offset, buffer);
                offset += 4;
            }
            client.Send(buffer);
        }

        public static void CTFExpolitsRank2(GameState client, byte[] packet)
        {
            const byte maxcount = 5;
            byte page = packet[10];

            var array = client.Guild.Members.Values.Where(p => p.Exploits != 0).OrderByDescending(p => p.Exploits).ToArray();
            if (array.Length == 0)
                return;

            byte[] buffer = new byte[0x38c];
            Writer.WriteUInt16(900, 0, buffer);
            Writer.WriteUInt16(0x427, 2, buffer);
            Writer.WriteUInt16(1, 4, buffer);
            Writer.WriteUInt32(page, 6, buffer);
            Writer.WriteUInt32((uint)array.Length, 10, buffer);
            Writer.WriteUInt32(5, 14, buffer);
            Writer.WriteUInt32(client.Guild.CTFdonationCPsold, 0x12, buffer);
            Writer.WriteUInt64((ulong)client.Guild.CTFdonationSilverold, 0x16, buffer);

            Writer.WriteUInt32((ushort)array.Length, 30, buffer);

            int offset = 30;

            for (ushort x = (ushort)(page * maxcount - maxcount); x < page * maxcount; x++)
            {
                if (x >= array.Length) break;
                var member = array[x];
                Writer.WriteUInt32((ushort)(x + 1), offset, buffer);
                offset += 4;

                Writer.WriteUInt32(member.Exploits, offset, buffer);
                offset += 4;
                Writer.WriteUInt32(member.CTFCpsReward, offset, buffer);
                offset += 4;
                Writer.WriteUInt64(member.CTFSilverReward, offset, buffer);
                offset += 8;
                Writer.WriteUInt32(member.ID, offset, buffer);
                offset += 4;
                Writer.WriteString(member.Name, offset, buffer);
                offset += 0x24;
            }
            client.Send(buffer);
        }
    }
}
