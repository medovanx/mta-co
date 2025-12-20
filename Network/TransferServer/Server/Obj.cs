using MTA.Network.Sockets;

namespace MTA.TransferServer
{
    public class Obj
    {
        public Network.BigConcurrentPacketQueue Packets;
        private ClientWrapper sock;
        public Obj(ClientWrapper obj)
        {
            sock = obj;
            sock.Connector = this;
            Packets = new Network.BigConcurrentPacketQueue(0);
        }

        public void Disconnect()
        {
            sock.Disconnect();
        }
    }
}
