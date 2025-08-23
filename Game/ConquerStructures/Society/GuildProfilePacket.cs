namespace MTA.Network.GamePackets
{
    using MTA;
    using MTA.Client;
    using MTA.Interfaces;
    using MTA.Network;
    using System;

    public class GuildProfilePacket : Writer, Interfaces.IPacket
    {
        private byte[] Packet;
        public GuildProfilePacket() { }
        public uint Silver
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 8); }
            set { WriteUInt32(value, 8, Packet); }
        }

        public uint Pk
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 20); }
            set { WriteUInt32(value, 20, Packet); }
        }

        public uint Cps
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 12); }
            set { WriteUInt32(value, 12, Packet); }
        }

        public uint Guide
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 16); }
            set { WriteUInt32(value, 16, Packet); }
        }

        public uint Arsenal
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 24); }
            set { WriteUInt32(value, 24, Packet); }
        }

        public uint Rose
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 28); }
            set { WriteUInt32(value, 28, Packet); }
        }

        public uint Orchid
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 32); }
            set { WriteUInt32(value, 32, Packet); }
        }

        public uint Lily
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 36); }
            set { WriteUInt32(value, 36, Packet); }
        }

        public uint Tulip
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 40); }
            set { WriteUInt32(value, 40, Packet); }
        }

        public uint HistorySilvers
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 44); }
            set { WriteUInt32(value, 44, Packet); }
        }

        public uint HistoryCps
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 48); }
            set { WriteUInt32(value, 48, Packet); }
        }

        public uint HistoryGuide
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 52); }
            set { WriteUInt32(value, 52, Packet); }
        }

        public uint HistoryPk
        {
            get { return MTA.BitConverter.ToUInt32(Packet, 56); }
            set { WriteUInt32(value, 56, Packet); }
        }

        public void Send(Client.GameState client)
        {
            client.Send(Packet);
        }
        public void Deserialize(byte[] Data)
        {
            this.Packet = Data;
        }

        public byte[] ToArray()
        {
            return Packet;
        }
    }
}