using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MTA.Client;
using MTA.Database;
using MTA.Game;
using MTA.Game.ConquerStructures;
using MTA.Game.ConquerStructures.Society;
using MTA.Game.Features.Reincarnation;
using MTA.Interfaces;
using MTA.Network;
using MTA.WebServer;
using ProtoBuf;

namespace MTA {
    public class Kernel {
        public static SafeDictionary<string, string> Translated = new SafeDictionary<string, string>();
        public static List<uint> Members30Guilds = new List<uint>();
        public static List<string> WarLegendsJoin = new List<string>();

        public static ConcurrentDictionary<uint, TransferPlayer> TransferPool2 =
            new ConcurrentDictionary<uint, TransferPlayer>();

        public static List<uint>
            TransferredPlayers = []; // new ConcurrentDictionary<uint, WebServer.TransferPlayer>();    

        public static ConcurrentDictionary<uint, TransferPlayer> TransferPool =
            new ConcurrentDictionary<uint, TransferPlayer>();

        public static Dictionary<UInt32, Refinery.RefineryBoxes> DatabaseRefineryBoxes =
            new Dictionary<UInt32, Refinery.RefineryBoxes>();

        public static Dictionary<UInt32, Refinery.RefineryItem> DatabaseRefinery =
            new Dictionary<UInt32, Refinery.RefineryItem>();

        public static uint MaxRoses = 100;
        public static uint MaxLilies = 999;
        public static uint MaxOrchids = 500;
        public static uint MaxTulips = 50;
        private static Int64 _randSeed = 3721;

        [DllImport("winmm", EntryPoint = "timeGetTime", ExactSpelling = true, CharSet = CharSet.Ansi,
            SetLastError = true)]
        private static extern long timeGetTime();

        public static int RandGet(int nMax, bool bReset = false) {
            if (bReset)
                _randSeed = timeGetTime();
            const long x = 0xffffffff;
            _randSeed *= 134775813;
            _randSeed += 1;
            _randSeed = _randSeed % x;
            var i = _randSeed / (double)0xffffffff;
            var final = (ulong)(nMax * i);

            return (int)final;
        }

        public static ConcurrentDictionary<uint, Entity> BlackSpoted = new ConcurrentDictionary<uint, Entity>();

        //  public static SafeDictionary<uint, Game.Features.Flowers.Flowers> AllFlower = new SafeDictionary<uint, Game.Features.Flowers.Flowers>(1000);
        public static Dictionary<uint, Clan> Clans = new Dictionary<uint, Clan>(100000);

        public static Dictionary<uint, ReincarnateInfo>
            ReincarnatedCharacters = new Dictionary<uint, ReincarnateInfo>();

        public static ConcurrentDictionary<uint, AccountTable> AwaitingPool =
            new ConcurrentDictionary<uint, AccountTable>();

        public static ConcurrentDictionary<uint, GameState> GamePool = new ConcurrentDictionary<uint, GameState>();

        public static ConcurrentDictionary<uint, GameState>
            DisconnectPool = new ConcurrentDictionary<uint, GameState>();

        public static QuizShow? QuizShow;
        public static SafeDictionary<ushort, Map> Maps = new SafeDictionary<ushort, Map>(10000);
        public static SafeDictionary<uint, Guild> Guilds = new SafeDictionary<uint, Guild>(100000);
        public static Dictionary<uint, PokerTables> PokerTables = new Dictionary<uint, PokerTables>(50);

        public static List<char> InvalidCharacters =
            [' ', '[', '{', '}', '(', ')', ']', '#', '*', '\\', '/', '<', '>', '"', '|', '='];

        public static List<string> Insults = [
            "bitch", "noob", "n00b",
        ];

        public static FastRandom Random = new FastRandom();
        public static int BoundId = 45;
        public static int BoundIdEnd = 46;
        public static bool Spawn1 = false;

        #region 7bit

        public static byte[] FinalizeProtoBuf(object proto, ushort packetid) {
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, proto);
            var protobuff = ms.ToArray();
            var buffer = new byte[12 + protobuff.Length];
            Buffer.BlockCopy(protobuff, 0, buffer, 4, protobuff.Length);
            Writer.Write(buffer.Length - 8, 0, buffer);
            Writer.Write(packetid, 2, buffer);
            return buffer;
        }

        public static byte[] CreateProtocolBuffer(params uint[] values) {
            List<byte> ptr = [
                8
            ];
            for (int x = 0; x < values.Length; x++) {
                uint value = values[x];
                while (value > 0x7F) {
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

        public static uint[] Read7BitEncodedInt(byte[] buffer) {
            List<uint> ptr2 = new List<uint>();

            for (int i = 0; i < buffer.Length;) {
                if (i + 2 <= buffer.Length) {
                    int tmp = buffer[i++];

                    if (tmp % 8 == 0)
                        while (true) {
                            if (i + 1 > buffer.Length) break;
                            tmp = buffer[i++];
                            if (tmp < 128) {
                                ptr2.Add((uint)tmp);
                                break;
                            }

                            int result = tmp & 0x7f;
                            if ((tmp = buffer[i++]) < 128) {
                                result |= tmp << 7;
                                ptr2.Add((uint)result);
                                break;
                            }

                            result |= (tmp & 0x7f) << 7;
                            if ((tmp = buffer[i++]) < 128) {
                                result |= tmp << 14;
                                ptr2.Add((uint)result);
                                break;
                            }

                            result |= (tmp & 0x7f) << 14;
                            if ((tmp = buffer[i++]) < 128) {
                                result |= tmp << 21;
                                ptr2.Add((uint)result);
                                break;
                            }

                            result |= (tmp & 0x7f) << 21;
                            result |= (buffer[i++]) << 28;
                            ptr2.Add((uint)result);
                            break;
                        }
                }
                else break;
            }

            return ptr2.ToArray();
        }

        #endregion

        public static short GetDistance(ushort startX, ushort startY, ushort endX, ushort endY) {
            return (short)Math.Sqrt((startX - endX) * (startX - endX) + (startY - endY) * (startY - endY));
        }

        public static double GetDDistance(ushort startX, ushort startY, ushort endX, ushort endY) {
            return Math.Sqrt((startX - endX) * (startX - endX) + (startY - endY) * (startY - endY));
        }

        public static bool ChanceSuccess(double chance) {
            var num2 = (Random.Next(0x989680) / 10000000.0) * 100.0;
            return (chance >= num2);
        }

        public static int GetDegree(int startX, int endX, int startY, int endY) {
            double addX = endX - startX;
            double addY = endY - startY;
            var r = Math.Atan2(addY, addX);
            if (r < 0) r += Math.PI * 2;
            var direction = (int)(360 - (r * 180 / Math.PI));
            return direction;
        }

        public static ulong ToDateTimeInt(DateTime dt) {
            return ulong.Parse(dt.ToString("yyyyMMddHHmmss"));
        }

        public static DateTime FromDateTimeInt(UInt64 val) {
            return new DateTime(
                (int)(val / 10000000000),
                (int)((val % 10000000000) / 100000000),
                (int)((val % 100000000) / 1000000),
                (int)((val % 1000000) / 10000),
                (int)((val % 10000) / 100),
                (int)(val % 100));
        }

        public static ulong TqTimer(DateTime timer) {
            var year = 10000000000000 * (ulong)(timer.Year - 1900);
            var month = 100000000000 * (ulong)(timer.Month - 1);
            var dayOfYear = 100000000 * (ulong)(timer.DayOfYear - 1);
            var day = (ulong)(timer.Day * 1000000);
            var hour = (ulong)(timer.Hour * 10000);
            var minute = (ulong)(timer.Minute * 100);
            var second = (ulong)(timer.Second);
            return year + month + dayOfYear + day + hour + minute + second;
        }

        public static Enums.ConquerAngle GetAngle(ushort x, ushort y, ushort x2, ushort y2) {
            double addX = x2 - x;
            double addY = y2 - y;
            double r = Math.Atan2(addY, addX);

            if (r < 0) r += Math.PI * 2;

            var direction = 360 - (r * 180 / Math.PI);

            byte dir = (byte)((7 - (Math.Floor(direction) / 45 % 8)) - 1 % 8);
            return (Enums.ConquerAngle)(byte)(dir % 8);
        }

        public static Boolean ValidClanName(String name) {
            lock (Clans) {
                foreach (Clan clans in Clans.Values) {
                    if (clans.Name == name)
                        return false;
                }
            }

            return true;
        }

        public static void SendWorldMessage(IPacket packet) {
            foreach (var client in Program.Values) {
                client.Send(packet);
            }
        }

        public static void SendWorldMessage(IPacket message, GameState[] to) {
            foreach (var client in to) {
                client.Send(message);
            }
        }

        public static void Execute(Action<GameState> action) {
            foreach (var client in Program.Values) {
                action(client);
            }
        }

        public static void SendWorldMessage(IPacket message, GameState[] to, uint exceptuid) {
            foreach (var client in to) {
                if (client.Entity.UID != exceptuid) {
                    client.Send(message);
                }
            }
        }

        public static void SendWorldMessage(IPacket message, GameState[] to, ushort mapid) {
            foreach (var client in to) {
                if (client.Map.ID == mapid) {
                    client.Send(message);
                }
            }
        }

        public static void SendWorldMessage(IPacket message, GameState[] to, ushort mapid, uint exceptuid) {
            foreach (var client in to) {
                if (client.Map.ID != mapid) continue;
                if (client.Entity.UID != exceptuid) {
                    client.Send(message);
                }
            }
        }

        public static void SendScreen(IMapObject obj, IPacket packet) {
            var values = Program.Values;
            foreach (var pClient in values) {
                if (!pClient.Socket.Alive) continue;
                if (pClient.Entity.MapID != obj.MapID) continue;
                if (GetDistance(pClient.Entity.X, pClient.Entity.Y, obj.X, obj.Y) > Constants.pScreenDistance) continue;
                pClient.Send(packet);
            }
        }

        public static uint MaxJumpTime(short distance) {
            var x = 400 * (uint)distance / 10;
            return x;
        }

        public static bool Rate(int value) {
            return value > Random.Next() % 100;
        }

        public static bool Rate(double percent) {
            if (percent == 0) return false;
            while ((int)percent > 0) percent /= 10f;
            int discriminant = 1;
            percent = Math.Round(percent, 4);
            const double tolerance = 0.0001; // Tolerance for floating point comparison
            while (Math.Abs(percent - Math.Ceiling(percent)) > tolerance) {
                percent *= 10;
                discriminant *= 10;
                percent = Math.Round(percent, 4);
            }

            return Rate((int)percent, discriminant);
        }

        public static bool Rate(int value, int discriminant) {
            return value > Random.Next() % discriminant;
        }

        public static bool Rate(ulong value) {
            return Rate((int)value);
        }

        public static int RandFromGivingNums(params int[] nums) {
            return nums[Random.Next(0, nums.Length)];
        }

        internal static void SendSpawn(StaticEntity item) {
            foreach (var client in Program.Values)
                if (client.Map.ID == item.MapID)
                    if (GetDistance(item.X, item.Y, client.Entity.X, client.Entity.Y) <= Constants.pScreenDistance)
                        item.SendSpawn(client);
        }


        public static bool ChanceSuccess(int percent) {
            if (percent == 0)
                return false;

            return (Random.Next(0, 100) < percent);
        }

        public static Enums.ConquerAngle GetFacing(short angle) {
            sbyte cAngle = (sbyte)((angle / 46) - 1);
            return (cAngle == -1) ? Enums.ConquerAngle.South : (Enums.ConquerAngle)cAngle;
        }

        public static short GetAngleX(ushort x, ushort y, ushort x2, ushort y2) {
            double r = Math.Atan2(y2 - y, x2 - x);
            if (r < 0)
                r += Math.PI * 2;
            return (short)Math.Round(r * 180 / Math.PI);
        }

        public static GameState[]? Values;


        internal static void SendWorldMessage(byte[] p) {
            foreach (var client in Program.Values) {
                client.Send(p);
            }
        }
    }
}