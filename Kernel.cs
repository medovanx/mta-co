using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MTA.Database;
using System.Collections.Concurrent;
using MTA.Game;

namespace MTA
{
    public class Kernel
    {
        public static SafeDictionary<string, string> Translateed = new SafeDictionary<string, string>();
        public static List<uint> Members30Guilds = new List<uint>();
        public static List<string> WarLegendsJoin = new List<string>();
        public static ConcurrentDictionary<uint, WebServer.TransferPlayer> TransferPool2 = new ConcurrentDictionary<uint, WebServer.TransferPlayer>();
        public static List<uint> TransferdPlayers = new List<uint>();// new ConcurrentDictionary<uint, WebServer.TransferPlayer>();    
        public static ConcurrentDictionary<uint, WebServer.TransferPlayer> TransferPool = new ConcurrentDictionary<uint, WebServer.TransferPlayer>();
        public static Dictionary<UInt32, Refinery.RefineryBoxes> DatabaseRefineryBoxes =
                                                    new Dictionary<UInt32, Refinery.RefineryBoxes>();
        public static Dictionary<UInt32, Refinery.RefineryItem> DatabaseRefinery =
                                                    new Dictionary<UInt32, Refinery.RefineryItem>();

        public static uint MaxRoses = 100;
        public static uint MaxLilies = 999;
        public static uint MaxOrchads = 500;
        public static uint MaxTulips = 50;
        private static Int64 RandSeed = 3721;
        [DllImport("winmm", EntryPoint = "timeGetTime", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]

        private static extern long timeGetTime();
        public static int RandGet(int nMax, bool bReset = false)
        {
            if (bReset)
                RandSeed = timeGetTime();

            Int64 x = 0xffffffff;
            double i;
            ulong final;

            RandSeed *= ((Int64)134775813);
            RandSeed += 1;
            RandSeed = RandSeed % x;
            i = ((double)RandSeed) / (double)0xffffffff;
            final = (ulong)(nMax * i);

            return (int)final;
        }
        public static ConcurrentDictionary<uint, Game.Entity> BlackSpoted = new ConcurrentDictionary<uint, Game.Entity>();
        //  public static SafeDictionary<uint, Game.Features.Flowers.Flowers> AllFlower = new SafeDictionary<uint, Game.Features.Flowers.Flowers>(1000);
        public static Dictionary<uint, Clan> Clans = new Dictionary<uint, Clan>(100000);
        public static Dictionary<uint, Game.Features.Reincarnation.ReincarnateInfo> ReincarnatedCharacters = new Dictionary<uint, Game.Features.Reincarnation.ReincarnateInfo>();

        public static ConcurrentDictionary<uint, Database.AccountTable> AwaitingPool = new ConcurrentDictionary<uint, Database.AccountTable>();

        public static ConcurrentDictionary<uint, Client.GameState> GamePool = new ConcurrentDictionary<uint, Client.GameState>();
        public static ConcurrentDictionary<uint, Client.GameState> DisconnectPool = new ConcurrentDictionary<uint, Client.GameState>();
        public static Game.ConquerStructures.QuizShow QuizShow;
        public static SafeDictionary<ushort, Game.Map> Maps = new SafeDictionary<ushort, Game.Map>(10000);
        public static SafeDictionary<uint, Game.ConquerStructures.Society.Guild> Guilds = new SafeDictionary<uint, MTA.Game.ConquerStructures.Society.Guild>(100000);
        public static Dictionary<uint, Database.PokerTables> PokerTables = new Dictionary<uint, Database.PokerTables>(50);
        public static List<char> InvalidCharacters = new List<char>() { ' ', '[', '{', '}', '(', ')', ']', '#', '*', '\\', '/', '<', '>', '"', '|', '=' };

        public static List<string> Insults = new List<string>() { "k o s", "3ars", "fuck", "abn", "naka", "Dick", "head", "mother", "fucker", "Kick", "Fuck ur self", "5od yad", "abok", "omak", "cock", "M3rsen", "pussy", "son of bitch", "kos", "omk", " k o s", "mtnak", "sharmot", "5owl", "5awl", "zanya", "3rs", "hanekak", "Dana hanekak", "Den", "Sharmota", "Kosomen omak", "Kosomen", "Mayten", "a7a", "a7eh", "fuck", "a 7 a", "a7 a" };

        public static FastRandom Random = new FastRandom();
        public static int boundID = 45;
        public static int boundIDEnd = 46;
        public static bool Spawn1 = false;
        #region 7bit
        public static byte[] FinalizeProtoBuf(object proto, ushort packetid)
        {
            byte[] protobuff;
            using (var ms = new System.IO.MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, proto);
                protobuff = ms.ToArray();
                byte[] buffer;
                buffer = new byte[12 + protobuff.Length];
                System.Buffer.BlockCopy(protobuff, 0, buffer, 4, protobuff.Length);
                Network.Writer.Write(buffer.Length - 8, 0, buffer);
                Network.Writer.Write(packetid, 2, buffer);
                return buffer;
            }
        }
        public static byte[] CreateProtocolBuffer(params uint[] values)
        {
            List<byte> ptr = new List<byte>();
            ptr.Add(8);
            for (int x = 0; x < values.Length; x++)
            {
                uint value = values[x];
                while (value > 0x7F)
                {
                    ptr.Add((byte)((value & 0x7F) | 0x80));
                    value >>= 7;
                }
                ptr.Add((byte)(value & 0x7F));
                ptr.Add((byte)(8 * (x + 2)));
                if (x + 1 == values.Length)
                    break;
            }
            return ptr.ToArray();
        }
        public static uint[] Read7BitEncodedInt(byte[] buffer)
        {
            List<uint> ptr2 = new List<uint>();

            for (int i = 0; i < buffer.Length;)
            {
                if (i + 2 <= buffer.Length)
                {
                    int tmp = buffer[i++];

                    if (tmp % 8 == 0)
                        while (true)
                        {
                            if (i + 1 > buffer.Length) break;
                            tmp = buffer[i++];
                            if (tmp < 128)
                            {
                                ptr2.Add((uint)tmp);
                                break;
                            }
                            else
                            {
                                int result = tmp & 0x7f;
                                if ((tmp = buffer[i++]) < 128)
                                {
                                    result |= tmp << 7;
                                    ptr2.Add((uint)result);
                                    break;
                                }
                                else
                                {
                                    result |= (tmp & 0x7f) << 7;
                                    if ((tmp = buffer[i++]) < 128)
                                    {
                                        result |= tmp << 14;
                                        ptr2.Add((uint)result);
                                        break;
                                    }
                                    else
                                    {
                                        result |= (tmp & 0x7f) << 14;
                                        if ((tmp = buffer[i++]) < 128)
                                        {
                                            result |= tmp << 21;
                                            ptr2.Add((uint)result);
                                            break;
                                        }
                                        else
                                        {
                                            result |= (tmp & 0x7f) << 21;
                                            result |= (tmp = buffer[i++]) << 28;
                                            ptr2.Add((uint)result);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                }
                else break;
            }
            return ptr2.ToArray();
        }
        #endregion
        public static short GetDistance(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            return (short)Math.Sqrt((X - X2) * (X - X2) + (Y - Y2) * (Y - Y2));
        }
        public static double GetDDistance(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            return Math.Sqrt((X - X2) * (X - X2) + (Y - Y2) * (Y - Y2));
        }
        public static bool ChanceSuccess(double Chance)
        {
            double num2 = (((double)Random.Next(0x989680)) / 10000000.0) * 100.0;
            return (Chance >= num2);
        }
        public static int GetDegree(int X, int X2, int Y, int Y2)
        {
            int direction = 0;

            double AddX = X2 - X;
            double AddY = Y2 - Y;
            double r = (double)Math.Atan2(AddY, AddX);
            if (r < 0) r += (double)Math.PI * 2;

            direction = (int)(360 - (r * 180 / Math.PI));

            return direction;
        }
        public static UInt64 ToDateTimeInt(DateTime dt)
        {
            return UInt64.Parse(dt.ToString("yyyyMMddHHmmss"));
        }
        public static DateTime FromDateTimeInt(UInt64 val)
        {
            return new DateTime(
                (Int32)(val / 10000000000),
                (Int32)((val % 10000000000) / 100000000),
                (Int32)((val % 100000000) / 1000000),
                (Int32)((val % 1000000) / 10000),
                (Int32)((val % 10000) / 100),
                (Int32)(val % 100));
        }
        public static ulong TqTimer(DateTime timer)
        {
            var year = (ulong)(10000000000000 * (ulong)(timer.Year - 1900));
            var month = (ulong)(100000000000 * (ulong)(timer.Month - 1));
            var dayofyear = (ulong)(100000000 * (ulong)(timer.DayOfYear - 1));
            var day = (ulong)(timer.Day * 1000000);
            var Hour = (ulong)(timer.Hour * 10000);
            var Minute = (ulong)(timer.Minute * 100);
            var Second = (ulong)(timer.Second);
            return (ulong)(year + month + dayofyear + day + Hour + Minute + Second);
        }
        public static Game.Enums.ConquerAngle GetAngle(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            double direction = 0;

            double AddX = X2 - X;
            double AddY = Y2 - Y;
            double r = (double)Math.Atan2(AddY, AddX);

            if (r < 0) r += (double)Math.PI * 2;

            direction = 360 - (r * 180 / (double)Math.PI);

            byte Dir = (byte)((7 - (Math.Floor(direction) / 45 % 8)) - 1 % 8);
            return (Game.Enums.ConquerAngle)(byte)((int)Dir % 8);
        }
        public static Boolean ValidClanName(String name)
        {
            lock (Clans)
            {
                foreach (Clan clans in Clans.Values)
                {
                    if (clans.Name == name)
                        return false;
                }
            }
            return true;
        }
        public static void SendWorldMessage(Interfaces.IPacket packet)
        {
            foreach (Client.GameState client in Program.Values)
            {
                if (client != null)
                {
                    client.Send(packet);
                }
            }
        }
        public static void SendWorldMessage(Interfaces.IPacket message, Client.GameState[] to)
        {
            foreach (Client.GameState client in to)
            {
                if (client != null)
                {
                    client.Send(message);
                }
            }
        }
        public static void Execute(Action<Client.GameState> action)
        {
            foreach (Client.GameState client in Program.Values)
            {
                if (client != null)
                {
                    action(client);
                }
            }
        }
        public static void SendWorldMessage(Interfaces.IPacket message, Client.GameState[] to, uint exceptuid)
        {
            foreach (Client.GameState client in to)
            {
                if (client != null)
                {
                    if (client.Entity.UID != exceptuid)
                    {
                        client.Send(message);
                    }
                }
            }
        }

        public static void SendWorldMessage(Interfaces.IPacket message, Client.GameState[] to, ushort mapid)
        {
            foreach (Client.GameState client in to)
            {
                if (client != null)
                {
                    if (client.Map.ID == mapid)
                    {
                        client.Send(message);
                    }
                }
            }
        }

        public static void SendWorldMessage(Interfaces.IPacket message, Client.GameState[] to, ushort mapid, uint exceptuid)
        {
            foreach (Client.GameState client in to)
            {
                if (client != null)
                {
                    if (client.Map.ID == mapid)
                    {
                        if (client.Entity.UID != exceptuid)
                        {
                            client.Send(message);
                        }
                    }
                }
            }
        }

        public static void SendScreen(Interfaces.IMapObject obj, Interfaces.IPacket packet)
        {
            var Values = Program.Values;
            foreach (var pClient in Values)
            {
                if (pClient == null) continue;
                if (!pClient.Socket.Alive) continue;
                if (pClient.Entity.MapID != obj.MapID) continue;
                if (Kernel.GetDistance(pClient.Entity.X, pClient.Entity.Y, obj.X, obj.Y) > Constants.pScreenDistance) continue;
                pClient.Send(packet);
            }
        }

        public static uint maxJumpTime(short distance)
        {
            uint x = 0;
            x = 400 * (uint)distance / 10;
            return x;
        }
        public static bool Rate(int value)
        {
            return value > Random.Next() % 100;
        }
        public static bool Rate(double percent)
        {
            if (percent == 0) return false;
            while ((int)percent > 0) percent /= 10f;
            int discriminant = 1;
            percent = Math.Round(percent, 4);
            while (percent != Math.Ceiling(percent))
            {
                percent *= 10;
                discriminant *= 10;
                percent = Math.Round(percent, 4);
            }
            return Kernel.Rate((int)percent, discriminant);
        }
        public static bool Rate(int value, int discriminant)
        {
            return value > Random.Next() % discriminant;
        }
        public static bool Rate(ulong value)
        {
            return Rate((int)value);
        }
        public static int RandFromGivingNums(params int[] nums)
        {
            return nums[Random.Next(0, nums.Length)];
        }

        internal static void SendSpawn(Game.StaticEntity item)
        {
            foreach (Client.GameState client in Program.Values)
                if (client != null)
                    if (client.Map.ID == item.MapID)
                        if (GetDistance(item.X, item.Y, client.Entity.X, client.Entity.Y) <= Constants.pScreenDistance)
                            item.SendSpawn(client);
        }


        public static bool ChanceSuccess(int percent)
        {
            if (percent == 0)
                return false;

            return (Random.Next(0, 100) < percent);
        }
        public static Enums.ConquerAngle GetFacing(short angle)
        {
            sbyte c_angle = (sbyte)((angle / 46) - 1);
            return (c_angle == -1) ? Enums.ConquerAngle.South : (Enums.ConquerAngle)c_angle;
        }
        public static short GetAnglex(ushort X, ushort Y, ushort x2, ushort y2)
        {
            double r = Math.Atan2(y2 - Y, x2 - X);
            if (r < 0)
                r += Math.PI * 2;
            return (short)Math.Round(r * 180 / Math.PI);
        }
        public static Client.GameState[] Values;


        internal static void SendWorldMessage(byte[] p)
        {
            foreach (Client.GameState client in Program.Values)
            {
                client.Send(p);
            }
        }
    }
}
