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
                    return true;
                }
            }
            return false;
        }

        public bool ExpectNext(int clientId, int timeOutSeconds)
        {
            Trace.TraceInformation("Expecting commId for {0} is {1}", clientId, handlers[clientId].commId);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                object o;
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

        public void SendObject(int clientId, Object o)
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

        // todo: this function not working
        public void SendInterruptedObject(int clientId, Object o)
        {
            if (o is Card)
            {
                Card card = o as Card;
                CardItem item = new CardItem();
                item.playerId = Game.CurrentGame.Players.IndexOf(card.Place.Player);
                item.deck = card.Place.DeckType;
                item.place = Game.CurrentGame.Decks[card.Place.Player, card.Place.DeckType].IndexOf(card);
                Trace.Assert(item.place >= 0);
                item.Id = card.Id;
                item.rank = card.Rank;
                item.suit = (int)card.Suit;
                CommandItem citem = new CommandItem();
                citem.command = Command.Interrupt;
                citem.data = 1;
                citem.obj = item;
                Trace.TraceInformation("Interrupted, sending a {0} to {1}", citem, clientId);
                handlers[clientId].semAccess.WaitOne();
                handlers[clientId].queueOut.Enqueue(citem);
                handlers[clientId].semAccess.Release(1);
                handlers[clientId].semOut.Release(1);
            }
            else
            {
                Trace.Assert(false);
            }
        }

        private void Listener()
        {
            IPHostEntry host;
            IPAddress localIP = null;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip;
                }
            }
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            TcpListener listener = new TcpListener(ep);
            listener.Start();
            int i = 0;
            Trace.TraceInformation("Listener Started on {0}", localIP.ToString());
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
                SendObject(i, i);
                i++;
            }
            Trace.TraceInformation("Server ready");
        }

        private void ServerThread(Stream stream, Semaphore semWake, Semaphore semAccess, Queue<object> queueIn)
        {
            ItemReceiver r = new ItemReceiver(stream);
            game.RegisterCurrentThread();
            while (true)
            {
                object o;
                do
                {
                    o = r.Receive();
                    if (o is int)
                    {
                        Trace.TraceInformation("{0} Received a {1}", Thread.CurrentThread.Name, (int)o);
                    }
                    {
                        Trace.TraceInformation("{0} Received a {1}", Thread.CurrentThread.Name, o.GetType().Name);
                    }
                } while (HandleInterrupt(o));
                semAccess.WaitOne();
                queueIn.Enqueue(o);
                semAccess.Release(1);
                semWake.Release(1);
            }
        }
        private void ClientThread(Stream stream, Semaphore semWake, Semaphore semAccess, Queue<object> queueOut)
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
