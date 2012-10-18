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
    public class ServerHandler
    {
        public Semaphore semIn;
        public Semaphore semOut;
        public Semaphore semAccess;
        public Queue<object> queueIn;
        public Queue<object> queueOut;
        public NetworkStream stream;
        public TcpClient client;
        public Thread threadServer;
        public Thread threadClient;
        public int commId;
        public ServerHandler()
        {
            semIn = new Semaphore(0, 1);
            semOut = new Semaphore(0, 1);
            semAccess = new Semaphore(1, 1);
            queueIn = new Queue<object>();
            queueOut = new Queue<object>();
            stream = null;
            client = null;
            commId = 0;
        }
    }

    public class Server
    {
        private int maxClients;
        ServerHandler[] handlers;
        bool ready;
        /// <summary>
        /// Initialize and start the server.
        /// </summary>
        public Server(int capacity)
        {
            maxClients = capacity;
            handlers = new ServerHandler[capacity];
            for (int i = 0; i < capacity; i++)
            {
                handlers[i] = new ServerHandler();
            }
            ready = false;
        }

        /// <summary>
        /// Ready the server. Block if require more clients to connect.
        /// </summary>
        public void Ready()
        {
            Listener();
            ready = true;
        }

        public void ExpectNext(int clientId, int timeOutSeconds)
        {
            while (true)
            {
                handlers[clientId].semIn.WaitOne();
                object o = handlers[clientId].queueIn.Dequeue();
                if (o is CommandItem)
                {
                    CommandItem i = (CommandItem)o;
                    if (i.command == Command.QaId && i.data == handlers[clientId].commId)
                    {
                        handlers[clientId].commId++;
                        break;
                    }
                }
                Trace.TraceInformation("Skipping garbage {0}", o.GetType());
            }
        }

        public Card getCard(int clientId, int timeOutSeconds)
        {
            handlers[clientId].semIn.WaitOne();
            object o = handlers[clientId].queueIn.Dequeue();
            if (o is Card)
            {
                return (Card)o;
            }
            Trace.TraceWarning("Expected Card but type is {0}", o.GetType());
            return null;
        }

        public Player getPlayer(int clientId, int timeOutSeconds)
        {
            handlers[clientId].semIn.WaitOne();
            object o = handlers[clientId].queueIn.Dequeue();
            if (o is Player)
            {
                return (Player)o;
            }
            Trace.TraceWarning("Expected Player but type is {0}", o.GetType());
            return null;
        }

        private void Listener()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            TcpListener listener = new TcpListener(ep);
            listener.Start();
            int i = 0;
            while (i < maxClients)
            {
                handlers[i].client = listener.AcceptTcpClient();
                handlers[i].stream = handlers[i].client.GetStream();
                i++;
                handlers[i].threadServer = new Thread(() => { ServerThread(handlers[i].stream, handlers[i].semIn, handlers[i].semAccess, handlers[i].queueIn); });
                handlers[i].threadServer.Start();
                handlers[i].threadClient = new Thread(() => { ClientThread(handlers[i].stream, handlers[i].semOut, handlers[i].semAccess, handlers[i].queueOut); });
                handlers[i].threadClient.Start();
            }
        }

        private void ServerThread(NetworkStream stream, Semaphore semWake, Semaphore semAccess, Queue<object> queueIn)
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
        private void ClientThread(NetworkStream stream, Semaphore semWake, Semaphore semAccess, Queue<object> queueOut)
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
