using System;
using MTA.Client;
using MTA.Interfaces;

namespace MTA.Network.GamePackets.Roulette {
    public class MsgRouletteTable : IPacket {
        public enum TableType : uint {
            Money = 1,
            ConquerPoints = 2
        }
        //??? Player count on table?

        public byte[] packet;

        public ushort Lenght {
            set {
                packet = new byte[value + 8];
                Writer.WriteUshort((ushort)(packet.Length - 8), 0, packet);
            }
        }

        public ushort PacketID {
            set { Writer.WriteUshort(value, 2, packet); }
        }

        public uint UID {
            get { return BitConverter.ReadUint(packet, 4); }
            set { Writer.WriteUint(value, 4, packet); }
        }

        public uint TableNumber {
            set { Writer.WriteUint(value, 8, packet); }
        }

        public TableType MoneyType {
            get { return (TableType)BitConverter.ReadUint(packet, 12); }
            set { Writer.WriteUint((uint)value, 12, packet); }
        }

        public ushort X {
            get { return BitConverter.ReadUshort(packet, 16); }
            set { Writer.WriteUshort(value, 16, packet); }
        }

        public ushort Y {
            get { return BitConverter.ReadUshort(packet, 18); }
            set { Writer.WriteUshort(value, 18, packet); }
        }

        public ushort Mesh {
            set { Writer.WriteUshort(value, 20, packet); }
        }

        public byte PlayersCount {
            set { Writer.WriteByte(value, 22, packet); }
        }

        public byte[] ToArray() {
            return packet;
        }

        public void Send(GameState client) {
            client.Send(ToArray());
        }


        public void Deserialize(byte[] buffer) {
            throw new NotImplementedException();
        }

        public static MsgRouletteTable Create() {
            MsgRouletteTable packet = new MsgRouletteTable();
            packet.Lenght = 23; //???    
            packet.PacketID = GamePackets.MsgRouletteTable;
            return packet;
        }
    }
}