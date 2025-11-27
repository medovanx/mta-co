using MTA.Game;
using System;

namespace MTA.Network.GamePackets
{
    public class NpcSpawn : Writer, Interfaces.IPacket, Interfaces.INpc, Interfaces.IMapObject
    {
        private byte[] Buffer;
        private ushort _MapID;

        public NpcSpawn(bool Created = true)
        {
            if (Created)
            {
                Buffer = new byte[36];
                WriteUInt16(28, 0, Buffer);
                WriteUInt16(2030, 2, Buffer);
                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
            }
            // WriteUInt16(1, 22, Buffer);
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
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

        public ushort Mesh
        {
            get { return BitConverter.ToUInt16(Buffer, 20); }
            set { WriteUInt16(value, 20, Buffer); }
        }

        public MTA.Game.Enums.NpcType Type
        {
            get { return (MTA.Game.Enums.NpcType)Buffer[22]; }
            set { Buffer[22] = (byte)value; }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;

                byte[] buffer = new byte[90];
                Buffer.CopyTo(buffer, 0);
                WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                buffer[32] = 1;
                WriteStringWithLength(value, 33, buffer);
                Buffer = buffer;
            }
        }

        public _String Effect { get; set; }

        public ushort MapID { get { return _MapID; } set { _MapID = value; } }

        public MTA.Game.MapObjectType MapObjType { get { return MTA.Game.MapObjectType.Npc; } }

        public Client.GameState Owner { get { return null; } }

        public byte[] SpawnPacket;
        private string _Name;
        public string effect { get; set; }
        public void SendSpawn(Client.GameState client, bool checkScreen)
        {
            if (client.Screen.Add(this) || !checkScreen)
            {
                client.Send(Buffer);
                if (effect != "" && effect != null)
                {
                    client.SendScreen(new _String(true)
                    {
                        UID = UID,
                        TextsCount = 22,
                        Type = 10,
                        Texts = { effect }
                    }, true);
                }
            }
        }
        public void SendSpawn(Client.GameState client)
        {
            SendSpawn(client, false);
            if (effect != "" && effect != null)
            {
                client.SendScreen(new _String(true)
                {
                    UID = UID,
                    TextsCount = 22,
                    Type = 10,
                    Texts = { effect }
                }, true);
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
        public void Send(Client.GameState client)
        {
            SendSpawn(client, false);
        }
    }
}
