namespace MTA.Network.GamePackets
{
    using Interfaces;
    using Network;

    public class ItemView : Writer, IPacket
    {
        private byte[] Buffer;
        private Client.GameState client;

        public ItemView(Client.GameState _client)
        {
            client = _client;
        }

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }

        public void Send(Client.GameState client)
        {
            client.Send(ToArray());
        }

        public byte[] ToArray()
        {
            Buffer = new byte[0x54];
            WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
            WriteUInt16(0x3f1, 2, Buffer);
            WriteUInt32(0x2e, 12, Buffer);
            WriteUInt32(client.Equipment.GetGear(1, client), 0x20, Buffer);
            WriteUInt32(client.Equipment.GetGear(2, client), 0x24, Buffer);
            WriteUInt32(client.Equipment.GetGear(3, client), 40, Buffer);
            WriteUInt32(client.Equipment.GetGear(4, client), 0x2c, Buffer);
            WriteUInt32(client.Equipment.GetGear(5, client), 0x30, Buffer);
            WriteUInt32(client.Equipment.GetGear(6, client), 0x34, Buffer);
            WriteUInt32(client.Equipment.GetGear(7, client), 0x38, Buffer);
            WriteUInt32(client.Equipment.GetGear(8, client), 60, Buffer);
            WriteUInt32(client.Equipment.GetGear(9, client), 0x40, Buffer);
            WriteUInt32(client.Equipment.GetGear(10, client), 0x44, Buffer);
            WriteUInt32(client.Equipment.GetGear(11, client), 0x48, Buffer);
            return Buffer;
        }
    }
}

