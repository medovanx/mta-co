using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class FloorItem : Writer, Interfaces.IPacket, Interfaces.IMapObject
    {
        public const byte Drop = 1,
            Remove = 2,
            Animation = 3,
            DropDetain = 4,
            Effect = 10,
            RemoveEffect = 12;
        public const uint
            FlameLotus = 940,
            AuroraLotus = 930,
           DaggerStorm = 1027,
            Twilight = 40,
            FuryofEgg = 41,
            ShacklingIce = 42;
        public static Counter FloorUID = new Counter(0);
        byte[] Buffer;
        Client.GameState owner;
        ushort mapid;
        public Time32 OnFloor, UseTime;
        public bool PickedUpAlready = false;
        public uint Value;
        public FloorValueType ValueType;
        private ConquerItem item;
        public bool Shake = false;
        public bool Zoom = false;
        public bool Darkness = false;

        public void AppendFlags()
        {
            WriteInt32((Shake ? 1 : 0) | (Zoom ? 2 : 0) | (Darkness ? 4 : 0), 8, Buffer);
        }

        public FloorItem(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[109 + 8];
                WriteUInt16(109, 0, Buffer);
                WriteUInt16(1101, 2, Buffer);
                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
                Value = 0;
                ValueType = FloorValueType.Item;
            }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public uint ItemID
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }
        public ushort X
        {
            get { return BitConverter.ToUInt16(Buffer, 16); }
            set { WriteUInt16(value, 16, Buffer); }
        }
        public ushort Y
        {
            get { return BitConverter.ToUInt16(Buffer, 18); }
            set { WriteUInt16(value, 18, Buffer); }
        }
        public Game.Enums.Color ItemColor
        {
            get { return (Game.Enums.Color)BitConverter.ToUInt16(Buffer, 20); }
            set { WriteUInt16((ushort)value, 20, Buffer); }
        }
        public byte Type
        {
            get { return Buffer[22]; }
            set { Buffer[22] = value; }
        }
        public ushort MaxLife
        {
            get { return BitConverter.ToUInt16(Buffer, 20); }
            set { WriteUInt16((ushort)value, 20, Buffer); }
        }
        public uint Life
        {
            get { return BitConverter.ToUInt32(Buffer, 23); }
            set { WriteUInt32(value, 23, Buffer); }
        }
        public byte mColor
        {
            get { return Buffer[27]; }
            set { Buffer[27] = value; }
        }
        public uint OwnerUID
        {
            get { return BitConverter.ToUInt32(Buffer, 28); }
            set { WriteUInt32(value, 28, Buffer); }
        }
        public uint OwnerGuildUID
        {
            get { return BitConverter.ToUInt32(Buffer, 32); }
            set { WriteUInt32(value, 32, Buffer); }
        }
        public byte FlowerType
        {
            get { return Buffer[36]; }
            set { Buffer[36] = value; }
        }
        public ulong Timer
        {
            get { return BitConverter.ToUInt64(Buffer, 37 + 8); }
            set { WriteUInt64(value, 37 + 8, Buffer); }
        }
        public string Name
        {
            get { return Program.Encoding.GetString(Buffer, 45 + 8, 16); }
            set { WriteString(value, 45 + 8, Buffer); }
        }
        public MTA.Game.MapObjectType MapObjType
        {
            get { return MTA.Game.MapObjectType.Item; }
            set { }
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
        public void SendSpawn(Client.GameState client)
        {
            SendSpawn(client, false);
        }
        public Client.GameState Owner
        {
            get { return owner; }
            set { owner = value; }
        }
        public ConquerItem Item
        {
            get { return item; }
            set { item = value; }
        }
        public ushort MapID
        {
            get { return mapid; }
            set { mapid = value; }
        }
        public void SendSpawn(Client.GameState client, bool checkScreen)
        {
            if (client.Screen.Add(this) || !checkScreen)
            {
                client.Send(Buffer);
            }
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public enum FloorValueType { Item, Money, ConquerPoints }
    }
}
