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
using System.IO;

namespace Sanguosha.Core.Network
{
    public class ServerHandler
    {
        public Semaphore semIn;
        public Semaphore semOut;
        public Semaphore semAccess;
        public Queue<object> queueIn;
        public Queue<object> queueOut;
        public Stream stream;
        public TcpClient client;
        public Thread threadServer;
        public Thread threadClient;
        public int commId;
        public Game game;
        public bool disconnected;
        public ServerHandler()
        {
            disconnected = false;
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
        class FlushObject
        {
        }

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
        public Server(Game game, int capacity, IPAddress address)
        {
            ipAddress = address;
            IPEndPoint ep = new IPEndPoint(ipAddress, 0);
            listener = new TcpListener(ep);
            listener.Start();
            ipPort = ((IPEndPoint)listener.LocalEndpoint).Port;
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
        public void Start()
        {
            Trace.TraceInformation("Listener Started on {0} : {1}", ipAddress.ToString(), IpPort);
            for (int i = 0; i < maxClients; i++)
            {
                handlers[i].game = game;
                handlers[i].client = listener.AcceptTcpClient();
                Trace.TraceInformation("Client connected");
                handlers[i].stream = handlers[i].client.GetStream();
                handlers[i].threadServer = new Thread((ParameterizedThreadStart)((o) => 
                {
                    ServerThread(handlers[(int)o]);
                    handlers[(int)o].disconnected = true; 
                })) { IsBackground = true };
                handlers[i].threadServer.Start(i);
                handlers[i].threadClient = new Thread((ParameterizedThreadStart)((o) => 
                {
                    ClientThread(handlers[(int)o]); 
                })) { IsBackground = true };
                handlers[i].threadClient.Start(i);
            }
            Trace.TraceInformation("Server ready");
        }

        public void CommIdInc(int clientId)
        {
            handlers[clientId].commId++;
        }

        CardHandler _DeserializeType(Type type, String horseName)
        {
            if (type == null) return null;
            if (horseName == null)
            {
                return Activator.CreateInstance(type) as CardHandler;
            }
            else
            {
                return Activator.CreateInstance(type, horseName) as CardHandler;
            }
        }

        void _SerializeType(ref Type type, ref String horse, CardHandler handler)
        {
            if (handler is RoleCardHandler || handler is Heroes.HeroCardHandler || handler == null)
            {
                type = null;
                horse = null;
                return;
            }
            type = handler.GetType();
            if (handler is OffensiveHorse || handler is DefensiveHorse) horse = handler.CardType;
            else horse = null;
        }

        bool PlayerIdSanityCheck(int id)
        {
            if (id < 0 || id >= Game.CurrentGame.Players.Count)
            {
                return false;
            }
            if (Game.CurrentGame.Players[id].IsDead)
            {
                return false;
            }
            return true;
        }

        bool HandleInterrupt(object o)
        {
            if (o is CommandItem)
            {
                CommandItem i = (CommandItem)o;
                if (i.command == Command.Interrupt)
                {
                    if (i.obj is HandCardMovement)
                    {
                        HandCardMovement move = (HandCardMovement)i.obj;
                        if (PlayerIdSanityCheck(move.playerId))
                        {
                            var deck = Game.CurrentGame.Decks[Game.CurrentGame.Players[move.playerId], DeckType.Hand];
                            if (!(move.to < 0 || move.from < 0 || move.from >= deck.Count || move.to >= deck.Count))
                            {
                                var card1 = deck[move.from];
                                deck.Remove(card1);
                                deck.Insert(move.to, card1);
                            }
                        }
                    }
                    if (i.obj is CardChoiceCallback)
                    {
                        for (int ec = 0; ec < maxClients; ec++)
                        {
                            SendInterruptedObject(ec, o);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public void SendMultipleCardUsageResponded(int id)
        {
            var o = new CommandItem() { command = Command.Interrupt };
            o.obj = new CardUsageResponded() { playerId = id };
            for (int ec = 0; ec < maxClients; ec++)
            {
                SendInterruptedObject(ec, o);
            }
            
        }

        public bool ExpectNext(int clientId, int timeOutSeconds)
        {
            Trace.TraceInformation("Expecting commId for {0} is {1}", clientId, handlers[clientId].commId);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                object o;
                if (handlers[clientId].disconnected) return false;
                if (!handlers[clientId].semIn.WaitOne(timeOutSeconds * 1000))
                {
                    return false;
                }
                handlers[clientId].semAccess.WaitOne();
                o = handlers[clientId].queueIn.Dequeue();
                handlers[clientId].semAccess.Release(1);
                if (o == null)
                {
                    return false;
                }
                if (o is CommandItem)
                {
                    CommandItem i = (CommandItem)o;
                    if (i.command == Command.QaId)
                    {
                        Trace.TraceInformation("Current commId from {0} is {1}", clientId, i.data);
                    }
                    if (i.command == Command.QaId && i.data == handlers[clientId].commId)
                    {
                        break;
                    }
                }
                Trace.TraceInformation("Skipping garbage {0}", o.GetType());
                if (sw.Elapsed.Seconds >= timeOutSeconds)
                {
                    return false;
                }
            }
            return true;
        }

        //todo: timeout for others, when 0, wait on a minimal timeout (1s)?
        public Card GetCard(int clientId, int timeOutSeconds)
        {
            object o;
            if (handlers[clientId].disconnected) return null;
            handlers[clientId].semIn.WaitOne();
            handlers[clientId].semAccess.WaitOne();
            o = handlers[clientId].queueIn.Dequeue();
            handlers[clientId].semAccess.Release(1);
            if (o == null)
            {
                Trace.TraceWarning("Expected Card but null");
                return null;
            }
            if (o is CardItem)
            {
                return Translator.DecodeForServer((CardItem)o, clientId);
            }
            Trace.TraceWarning("Expected Card but type is {0}", o.GetType());
            return null;
        }

        public Player GetPlayer(int clientId, int timeOutSeconds)
        {
            object o;
            if (handlers[clientId].disconnected) return null;
            handlers[clientId].semIn.WaitOne();
            handlers[clientId].semAccess.WaitOne();
            o = handlers[clientId].queueIn.Dequeue();
            handlers[clientId].semAccess.Release(1);
            if (o == null)
            {
                Trace.TraceWarning("Expected Player but null");
                return null;
            }
            if (o is PlayerItem)
            {
                PlayerItem i = (PlayerItem)o;
                return Game.CurrentGame.Players[i.id];
            }
            Trace.TraceWarning("Expected Player but type is {0}", o.GetType());
            return null;
        }

        public int? GetInt(int clientId, int timeOutSeconds)
        {
            object o;
            if (handlers[clientId].disconnected) return null;
            handlers[clientId].semIn.WaitOne();
            handlers[clientId].semAccess.WaitOne();
            o = handlers[clientId].queueIn.Dequeue();
            handlers[clientId].semAccess.Release(1);
            if (o == null)
            {
                Trace.TraceWarning("Expected int but null");
                return null;
            }
            if (o is int)
            {
                return (int)o;
            }
            Trace.TraceWarning("Expected int but type is {0}", o.GetType());
            return null;
        }

        public ISkill GetSkill(int clientId, int timeOutSeconds)
        {
            object o;
            if (handlers[clientId].disconnected) return null;
            handlers[clientId].semIn.WaitOne();
            handlers[clientId].semAccess.WaitOne();
            o = handlers[clientId].queueIn.Dequeue();
            handlers[clientId].semAccess.Release(1);
            if (o == null)
            {
                Trace.TraceWarning("Expected skill but null");
                return null;
            }
            if (o is SkillItem)
            {
                return Translator.Translate((SkillItem)o);
            }
            if (o is CheatSkill)
            {
                return o as CheatSkill;
            }
            Trace.TraceWarning("Expected Skill but type is {0}", o.GetType());
            return null;
        }

        public void SendObject(int clientId, object o)
        {
            if (o is Card)
            {
                var item = Translator.TranslateForServer(o as Card, clientId);
                o = item;
            }
            if (o is CheatSkill)
            {
            }
            else if (o is ISkill)
            {
                o = Translator.Translate(o as ISkill);
            }
            Trace.TraceInformation("Sending a {0} to {1}", o, clientId); 
            handlers[clientId].semAccess.WaitOne();
            handlers[clientId].queueOut.Enqueue(o);
            handlers[clientId].semAccess.Release(1);
            handlers[clientId].semOut.Release(1);
        }

        public void Flush(int clientId)
        {
            SendObject(clientId, new FlushObject());
        }

        public void SendInterruptedObject(int clientId, Object o)
        {
            Trace.TraceInformation("Interrupted, sending a {0} to {1}", o.GetType(), clientId);
            handlers[clientId].semAccess.WaitOne();
            handlers[clientId].queueOut.Enqueue(o);
            handlers[clientId].semAccess.Release(1);
            handlers[clientId].semOut.Release(1);
            Flush(clientId);
        }

        IPAddress ipAddress;

        public IPAddress IpAddress
        {
            get { return ipAddress; }
        }

        int ipPort;

        public int IpPort
        {
            get { return ipPort; }
        }

        TcpListener listener;

        public void Stop()
        {
            foreach (var thread in handlers)
            {
                thread.threadClient.Abort();
                thread.threadServer.Abort();
            }
        }

        private void ServerThread(ServerHandler handler)
        {
            ItemReceiver r = new ItemReceiver(handler.stream);
            game.RegisterCurrentThread();
            while (true)
            {
                object o;
                do
                {
                    o = r.Receive();
                    if (o == null) return;
                } while (HandleInterrupt(o));
                handler.semAccess.WaitOne();
                handler.queueIn.Enqueue(o);
                handler.semAccess.Release(1);
                handler.semOut.Release(1);
            }
        }
        private void ClientThread(ServerHandler handler)
        {
            ItemSender r = new ItemSender(handler.stream);
            game.RegisterCurrentThread();
            while (true)
            {
                handler.semOut.WaitOne();
                object o;
                handler.semAccess.WaitOne();
                o = handler.queueOut.Dequeue();
                handler.semAccess.Release(1);
                if (o is FlushObject)
                {
                    r.Flush();
                }
                else if (!r.Send(o))
                {
                    Trace.TraceError("Network failure @ send");
                }
            }
        }
    }
}
