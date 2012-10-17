using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Network
{
    public class Server
    {
        private int maxClients;
        public NetworkStream stream;

        /// <summary>
        /// Initialize and start the server.
        /// </summary>
        public Server(int capacity)
        {
            maxClients = capacity;
            stream = null;
        }

        /// <summary>
        /// Ready the server. Block if require more clients to connect.
        /// </summary>
        public void Ready()
        {
            Thread t = new Thread(Listener);
            t.Start();
        }

        private void Listener()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            TcpListener listener = new TcpListener(ep);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();
            stream = client.GetStream();
            ItemReceiver r = new ItemReceiver(stream);
            while (true) Console.Write("{0}, ", r.Receive());
        }

    }
}
