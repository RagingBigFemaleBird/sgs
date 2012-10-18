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
        public Game game;
        public ServerHandler()
        {
            semIn = new Semaphore(0, int.MaxValue);
            semOut = new Semaphore(0, int.MaxValue);
            semAccess = new Semaphore(1, 1);
            queueIn = new Queue<object>();
            queueOut = new Queue<object>();
            stream = null;
            client = null;
            commId = 0;
            threadServer = null;
            threadClient = null;
            game = null;
        }
    }

    public class Server
    {
        private int maxClients;

        public int MaxClients
        {
            get { return maxClients; }
        }

        Game game;
        ServerHandler[] handlers;
        /// <summary>
        /// Initialize and start the server.
        /// </summary>
        public Server(Game game, int capacity)
        {
            maxClients = capacity;
            handlers = new ServerHandler[capacity];
            for (int i = 0; i < capacity; i++)
            {
                handlers[i] = new ServerHandler();
            }
            this.game = game;
            Trace.TraceInformation("Server initialized with capacity {0}", capacity);
        }

        /// <summary>
        /// Ready the server. Block if require more clients to connect.
        /// </summary>
        public void Ready()
        {
            Listener();
        }

        public void ExpectNext(int clientId, int timeOutSeconds)
        {
            while (true)
            {
                handlers[clientId].semIn.WaitOne();
                handlers[clientId].semAccess.WaitOne();
                object o = handlers[clientId].queueIn.Dequeue();
                handlers[clientId].semAccess.Release(1);
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

        public Card GetCard(int clientId, int timeOutSeconds)
        {
            handlers[clientId].semIn.WaitOne();
            handlers[clientId].semAccess.WaitOne();
            object o = handlers[clientId].queueIn.Dequeue();
            handlers[clientId].semAccess.Release(1);
            if (o is Card)
            {
                return (Card)o;
            }
            Trace.TraceWarning("Expected Card but type is {0}", o.GetType());
            return null;
        }

        public Player GetPlayer(int clientId, int timeOutSeconds)
        {
            handlers[clientId].semIn.WaitOne();
            handlers[clientId].semAccess.WaitOne();
            object o = handlers[clientId].queueIn.Dequeue();
            handlers[clientId].semAccess.Release(1);
            if (o is Player)
            {
                return (Player)o;
            }
            Trace.TraceWarning("Expected Player but type is {0}", o.GetType());
            return null;
        }

        public int? GetInt(int clientId, int timeOutSeconds)
        {
            handlers[clientId].semIn.WaitOne();
            handlers[clientId].semAccess.WaitOne();
            object o = handlers[clientId].queueIn.Dequeue();
            handlers[clientId].semAccess.Release(1);
            if (o is int)
            {
                return (int)o;
            }
            Trace.TraceWarning("Expected int but type is {0}", o.GetType());
            return null;
        }

        public void SendObject(int clientId, Object o)
        {
            handlers[clientId].semAccess.WaitOne();
            handlers[clientId].queueOut.Enqueue(o);
            handlers[clientId].semAccess.Release(1);
            handlers[clientId].semOut.Release(1);
        }

        private void Listener()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            TcpListener listener = new TcpListener(ep);
            listener.Start();
            int i = 0;
            Trace.TraceInformation("Listener Started");
            while (i < maxClients)
            {
                handlers[i].game = game;
                handlers[i].client = listener.AcceptTcpClient();
                Trace.TraceInformation("Client connected");
                handlers[i].stream = handlers[i].client.GetStream();
                handlers[i].threadServer = new Thread((ParameterizedThreadStart)((o) => { ServerThread(handlers[(int)o].stream, handlers[(int)o].semIn, handlers[(int)o].semAccess, handlers[(int)o].queueIn); }));
                handlers[i].threadServer.Start(i);
                handlers[i].threadClient = new Thread((ParameterizedThreadStart)((o) => { ClientThread(handlers[(int)o].stream, handlers[(int)o].semOut, handlers[(int)o].semAccess, handlers[(int)o].queueOut); }));
                handlers[i].threadClient.Start(i);
                i++;
            }
            Trace.TraceInformation("Server ready");
        }

        private void ServerThread(NetworkStream stream, Semaphore semWake, Semaphore semAccess, Queue<object> queueIn)
        {
            ItemReceiver r = new ItemReceiver(stream);
            game.RegisterCurrentThread();
            while (true)
            {
                object o = r.Receive();
                if (o is int)
                {
                    Trace.TraceInformation("Received a {0}", (int)o);
                }
                {
                    Trace.TraceInformation("Received a {0}", o.GetType().Name);
                }
                semAccess.WaitOne();
                queueIn.Enqueue(o);
                semAccess.Release(1);
                semWake.Release(1);
            }
        }
        private void ClientThread(NetworkStream stream, Semaphore semWake, Semaphore semAccess, Queue<object> queueOut)
        {
            ItemSender r = new ItemSender(stream);
            game.RegisterCurrentThread();
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
