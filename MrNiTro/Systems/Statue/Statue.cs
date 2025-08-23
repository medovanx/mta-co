using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace MTA.Game
{
    public class Statue : Network.Writer
    {
        public static ConcurrentDictionary<uint, Statue> Statues = new ConcurrentDictionary<uint, Statue>();

        public byte[] SpawnPacket = null;

        public uint UID;

        public Statue(byte[] array, uint uid = 105175, uint action = Enums.ConquerAction.Sit, byte facing = (byte)Enums.ConquerAngle.South, ushort xx = 313, ushort yy = 280, bool war = false)
        {
            UID = uid;
            if (war)
            {
                do
                {
                    Statues.Remove(UID);
                }
                while (Statues.ContainsKey(UID));
            }

            SpawnPacket = new byte[array.Length];
            for (ushort x = 0; x < array.Length; x++)
                SpawnPacket[x] = array[x];
            WriteByte(facing, Game.Entity._Facing, SpawnPacket);
            WriteUInt32(action, Game.Entity._Action, SpawnPacket);

            WriteUInt16(1000, Game.Entity._GuildRank, SpawnPacket);
            WriteUInt32(105175, Game.Entity._UID, SpawnPacket);
            //clear all flags
            WriteUInt64(0, Game.Entity._StatusFlag, SpawnPacket);
            WriteUInt64(0, Game.Entity._StatusFlag2, SpawnPacket);
            WriteUInt64(0, Game.Entity._StatusFlag2, SpawnPacket);

            WriteUInt32(0, Game.Entity._Hitpoints, SpawnPacket);
            WriteUInt32(0, Game.Entity._GuildID, SpawnPacket);

            X = 308;
            Y = 351;

            if (array.Length > 200)
            {
                if (!Statues.ContainsKey(UID))
                    Statues.TryAdd(UID, this);
                else
                {
                    Statues[UID] = this;
                }
            }
            foreach (var client in Kernel.GamePool.Values)
            {
                if (Kernel.GetDistance(X, Y, client.Entity.X, client.Entity.Y) < 16 && client.Entity.MapID == 1002)
                {
                    client.Send(this.SpawnPacket);
                }
            }
        }


        ushort _x, _y;
        public ushort X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
                WriteUInt16(value, Game.Entity._X, SpawnPacket);
            }
        }
        public ushort Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
                WriteUInt16(value, Game.Entity._Y, SpawnPacket);
            }
        }
        public static bool operator >(Statue statue, Client.GameState client)
        {
            if (!client.Screen.Statue.ContainsKey(statue.UID))
            {
                if (Kernel.GetDistance(statue.X, statue.Y, client.Entity.X, client.Entity.Y) < 16 && client.Entity.MapID == 1002)
                {
                    if (statue.SpawnPacket.Length > 200)//check if is created!
                    {
                        client.Send(statue.SpawnPacket);
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool operator <(Statue statue, Client.GameState client)
        {
            if (Kernel.GetDistance(statue.X, statue.Y, client.Entity.X, client.Entity.Y) >= 16 && client.Entity.MapID == 1002)
            {
                return true;
            }
            return false;
        }
    }
}
