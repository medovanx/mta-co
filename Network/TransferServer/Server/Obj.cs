using MTA.Network.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
