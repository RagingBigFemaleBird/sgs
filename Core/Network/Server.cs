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
using System.Diagnostics;

namespace Sanguosha.Core.Network
{
    public class Server
    {
        private int maxClients;
        private NetworkStream[] streams;
        private TcpClient[] clients;
        private Thread[] threadsServer;
        private Thread[] threadsClient;
        bool ready;
        /// <summary>
        /// Initialize and start the server.
        /// </summary>
        public Server(int capacity)
        {
            maxClients = capacity;
            streams = null;
            ready = false;
        }

        /// <summary>
        /// Ready the server. Block if require more clients to connect.
        /// </summary>
        public void Ready()
        {
            streams = new NetworkStream[maxClients];
            clients = new TcpClient[maxClients];
            Listener();
            ready = true;
        }

        private void Listener()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            TcpListener listener = new TcpListener(ep);
            listener.Start();
            int i = 0;
            while (i < maxClients)
            {
                clients[i] = listener.AcceptTcpClient();
                streams[i] = clients[i].GetStream();
                i++;
            }
        }

        public void SpawnServers(Semaphore[] semWake, Semaphore[] semData, Semaphore[] semAccess, Queue<object>[] queueIn, Queue<object>[] queueOut)
        {
            if (!ready)
            {
                return;
            }
            if (semWake.Count() != maxClients || semData.Count() != maxClients || semAccess.Count() != maxClients || queueIn.Count() != maxClients || queueOut.Count() != maxClients)
            {
                Trace.TraceError("Attempting to start server with incorrect parameters.");
                return;
            }
            threadsServer = new Thread[maxClients];
            threadsClient = new Thread[maxClients];
            for (int i = 0; i < maxClients; i++)
            {
                threadsServer[i] = new Thread(() => { ServerThread(streams[i], semWake[i], semAccess[i], queueIn[i]); });
                threadsServer[i].Start();
                threadsClient[i] = new Thread(() => { ClientThread(streams[i], semData[i], semAccess[i], queueOut[i]); });
                threadsClient[i].Start();
            }
        }

        public void ServerThread(NetworkStream stream, Semaphore semWake, Semaphore semAccess, Queue<object> queueIn)
        {
            ItemReceiver r = new ItemReceiver(stream);
            while (true)
            {
                object o = r.Receive();
                semWake.Release(1);
                semAccess.WaitOne();
                queueIn.Enqueue(o);
                semAccess.Release(1);
            }
        }
        public void ClientThread(NetworkStream stream, Semaphore semWake, Semaphore semAccess, Queue<object> queueOut)
        {
            ItemSender r = new ItemSender(stream);
            while (true)
            {
                semWake.WaitOne();
                object o;
                semAccess.WaitOne();
                o = queueOut.Dequeue();
                semAccess.Release(1);
                if (!r.Send(o))
                {
                    Trace.TraceError("Network failure @ send");
                }
            }
        }
    }
}
