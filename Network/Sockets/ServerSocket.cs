using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace MTA.Network.Sockets
{
    public class ServerSocket
    {
        public event Action<ClientWrapper> OnClientConnect, OnClientDisconnect;
        public event Action<byte[], int, ClientWrapper> OnClientReceive;

        private ConcurrentDictionary<int, int> BruteforceProtection;
        private const int TimeLimit = 1000 * 15; // 1 connection every 10 seconds for one ip
        private object SyncRoot;

        private Socket Connection;
        private ushort port;
        private string ipString;
        private bool enabled;
        private Thread thread;
        public ServerSocket()
        {
            Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SyncRoot = new object();
            thread = new Thread(doSyncAccept);
            thread.Start();
        }

        public void Enable(ushort port, string ip, bool BigSend = false)
        {
            ipString = ip;
            this.port = port;
            Connection.Bind(new IPEndPoint(IPAddress.Parse(ipString), this.port));
            Connection.Listen((int)SocketOptionName.MaxConnections);
            if (BigSend)
            {
                Connection.ReceiveBufferSize = ushort.MaxValue;
                Connection.SendBufferSize = ushort.MaxValue;
            }
            enabled = true;
            BruteforceProtection = new ConcurrentDictionary<int, int>();
        }

        public bool PrintoutIPs = false;
        private void doSyncAccept()
        {
            while (true)
            {
                if (enabled)
                {
                    try
                    {
                        ProcessSocket(Connection.Accept());
                    }
                    catch { }
                }
                Thread.Sleep(1);
            }
        }
        private void doAsyncAccept(IAsyncResult res)
        {
            try
            {
                Socket socket = Connection.EndAccept(res);
                ProcessSocket(socket);
                Connection.BeginAccept(doAsyncAccept, null);
            }
            catch
            {

            }
        }

        private void ProcessSocket(Socket socket)
        {
            try
            {
                string ip = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
                int ipHash = ip.GetHashCode();
                ClientWrapper wrapper = new ClientWrapper();
                wrapper.Create(socket, this, OnClientReceive);
                wrapper.Alive = true;
                wrapper.IP = ip;
                if (OnClientConnect != null) OnClientConnect(wrapper);
            }
            catch
            {

            }
        }

        public void Reset()
        {
            Disable();
            Enable();
        }

        public void Disable()
        {
            enabled = false;
            Connection.Close(1);
        }

        public void Enable()
        {
            if (!enabled)
            {
                Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Connection.Bind(new IPEndPoint(IPAddress.Parse(ipString), port));
                Connection.Listen((int)SocketOptionName.MaxConnections);
                enabled = true;
                //this.Connection.BeginAccept(doAsyncAccept, null);
            }
        }

        public void InvokeDisconnect(ClientWrapper Client)
        {
            if (OnClientDisconnect != null)
                OnClientDisconnect(Client);
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }
    }
}

