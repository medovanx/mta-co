namespace MTA.Network.GamePackets
{
    class PrizeClaimb : Writer, Interfaces.IPacket
    {
        byte[] packet;
        public PrizeClaimb(bool Create)
        {
            if (Create)
            {
                packet = new byte[28];
                WriteInt32(packet.Length - 8, 0, packet);
                WriteUInt16(1036, 2, packet);
            }
        }
        public byte Type
        {
            get { return packet[8]; }
            set { packet[8] = value; }
        }

        public uint OnlineTrainingExper
        {
            get { return BitConverter.ToUInt32(packet, 14); }
            set { WriteUInt32(value, 14, packet); }
        }
        public uint HuntingExper
        {
            get { return BitConverter.ToUInt32(packet, 18); }
            set { WriteUInt32(value, 18, packet); }
        }
        public void Deserialize(byte[] buffer)
        {
            packet = buffer;
        }
        public byte[] ToArray()
        {
            return packet;
        }
        public void Send(Client.GameState client)
        {
            client.Send(packet);
        }
    }
}