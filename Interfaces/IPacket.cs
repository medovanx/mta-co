using System;

namespace MTA.Interfaces
{
    public interface IPacket
    {
        byte[] ToArray();
        void Deserialize(byte[] buffer);
        void Send(Client.GameState client);
    }
}
