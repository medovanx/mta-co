using System;
using MTA.Client;

namespace MTA.Network.GamePackets {
    public class NpcSpawn : Writer, Interfaces.IPacket, Interfaces.INpc, Interfaces.IMapObject {
        private byte[] _buffer;

        public NpcSpawn(bool created = true) {
            if (!created) return;
            _buffer = new byte[36];
            WriteUInt16(28, 0, _buffer);
            WriteUInt16(2030, 2, _buffer);
            WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, _buffer);
            // WriteUInt16(1, 22, Buffer);
        }

        public uint UID {
            get => BitConverter.ToUInt32(_buffer, 8);
            set => WriteUInt32(value, 8, _buffer);
        }

        public ushort X {
            get => BitConverter.ToUInt16(_buffer, 16);
            set => WriteUInt16(value, 16, _buffer);
        }

        public ushort Y {
            get => BitConverter.ToUInt16(_buffer, 18);
            set => WriteUInt16(value, 18, _buffer);
        }

        public ushort Mesh {
            get => BitConverter.ToUInt16(_buffer, 20);
            set => WriteUInt16(value, 20, _buffer);
        }

        public Game.Enums.NpcType Type {
            get => (Game.Enums.NpcType)_buffer[22];
            set => _buffer[22] = (byte)value;
        }

        public string Name {
            get => _name;
            set {
                _name = value;

                byte[] buffer = new byte[90];
                _buffer.CopyTo(buffer, 0);
                WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                buffer[32] = 1;
                WriteStringWithLength(value, 33, buffer);
                _buffer = buffer;
            }
        }

        public _String Effect { get; set; }

        public ushort MapID { get; set; }

        public Game.MapObjectType MapObjType => Game.MapObjectType.Npc;

        public GameState Owner => null;

        public byte[] SpawnPacket;
        private string _name;
        public string effect { get; set; }

        public void SendSpawn(GameState client, bool checkScreen) {
            if (!client.Screen.Add(this) && checkScreen) return;
            client.Send(_buffer);
            if (!string.IsNullOrEmpty(effect)) {
                client.SendScreen(new _String(true) {
                    UID = UID,
                    TextsCount = 22,
                    Type = 10,
                    Texts = { effect }
                });
            }
        }

        public void SendSpawn(GameState client) {
            SendSpawn(client, false);
            if (!string.IsNullOrEmpty(effect)) {
                client.SendScreen(new _String(true) {
                    UID = UID,
                    TextsCount = 22,
                    Type = 10,
                    Texts = { effect }
                });
            }
        }

        public byte[] ToArray() {
            return _buffer;
        }

        public void Deserialize(byte[] buffer) {
            _buffer = buffer;
        }

        public void Send(GameState client) {
            SendSpawn(client, false);
        }
    }
}