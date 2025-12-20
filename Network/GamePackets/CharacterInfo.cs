using System;
namespace MTA.Network.GamePackets
{
    public unsafe class CharacterInfo : Writer, Interfaces.IPacket
    {
        private Client.GameState client;
        public CharacterInfo(Client.GameState _client)
        {
            client = _client;
        }
        public void Deserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }
        public byte[] ToArray()
        {
            byte[] Packet = new byte[144 + 8 + client.Entity.Spouse.Length + client.Entity.Name.Length];
            Write((ushort)(Packet.Length - 8), 0, Packet);
            Write(1006, 2, Packet);
            Write((uint)Time32.timeGetTime().GetHashCode(), 4, Packet);
            Write(client.Entity.UID, 8, Packet);
            Write(client.Entity.Mesh, 14, Packet);
            Write(client.Entity.HairStyle, 18, Packet);
            Write(client.Entity.Money, 20, Packet);
            Write(client.Entity.ConquerPoints, 28, Packet);
            Write(client.Entity.Experience, 32, Packet);
            Write(client.Entity.Strength, 60, Packet);
            Write(client.Entity.Agility, 62, Packet);
            Write(client.Entity.Vitality, 64, Packet);
            Write(client.Entity.Spirit, 66, Packet);
            Write(client.Entity.Atributes, 68, Packet);
            Write(client.Entity.Hitpoints, 70, Packet);
            Write(client.Entity.Mana, 74, Packet);
            Write(client.Entity.PKPoints, 72, Packet);
            Packet[78] = client.Entity.Level;
            Packet[79] = client.Entity.Class;
            Packet[80] = client.Entity.FirstRebornClass;
            Packet[81] = client.Entity.SecondRebornClass;
            Packet[83] = client.Entity.Reborn;
            Write((ushort)(client.Entity.EnlightenPoints * 100), 93, Packet);
            Write(client.Entity.BoundCps, 107, Packet);
            Write(client.Entity.SubClasses.Active, 111, Packet);
            Write(client.Entity.SubClasses.GetHashPoint(), 112, Packet);
            Write((UInt16)client.Entity.MyTitle, 105, Packet);
            Write((ushort)client.Entity.CountryID, 124, Packet);
            Write(3, 130 + 8, Packet);
            Packet[131 + 8] = (byte)client.Entity.Name.Length;
            Write(client.Entity.Name, 132 + 8, Packet);
            Write((byte)client.Entity.Spouse.Length, 133 + 8 + client.Entity.Name.Length, Packet);
            Write(client.Entity.Spouse, 134 + 8 + client.Entity.Name.Length, Packet);
            if (client.Entity.Class is >= 160 and <= 165)
                WriteUInt32(client.Entity.Windwalker, 89, Packet);
            return Packet;
        }
        public void Send(Client.GameState client)
        {
            client.Send(ToArray());
        }
    }
}
