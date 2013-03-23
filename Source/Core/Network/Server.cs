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
using Sanguosha.Core.UI;
using System.Runtime.Serialization.Formatters.Binary;
using Sanguosha.Lobby.Core;
using Sanguosha.Core.Utils;

namespace Sanguosha.Core.Network
{
    public class Server
    {
        //The following invariant MUST hold: the first <numberOfGamers> handers are for gamers. The last one is for spectators.
        private int numberOfGamers;

        public int MaxClients
        {
            get { return numberOfGamers + 1; }
        }

        Game game;
        List<NetworkGamer> handlers;

        public List<NetworkGamer> Handlers
        {
            get { return handlers; }
        }
        /// <summary>
        /// Initialize and start the server.
        /// </summary>
        public Server(Game game, int capacity, IPAddress address)
        {
            isStopped = false;
            ipAddress = address;
            IPEndPoint ep = new IPEndPoint(ipAddress, 0);
            listener = new TcpListener(ep);
            listener.Start();
            ipPort = ((IPEndPoint)listener.LocalEndpoint).Port;
            numberOfGamers = capacity;
            handlers = new List<NetworkGamer>();
            for (int i = 0; i < capacity; i++)
            {
                handlers.Add(new NetworkGamer());
            }
            this.game = game;
            Trace.TraceInformation("Server initialized with capacity {0}", capacity);
        }

        public bool IsDisconnected(int id)
        {
            return handlers[id].ConnectionStatus != ConnectionStatus.Connected;
        }

        /// <summary>
        /// Ready the server. Block if require more clients to connect.
        /// </summary>
        public void Start()
        {
            Trace.TraceInformation("Listener Started on {0} : {1}", ipAddress.ToString(), IpPort);
            int To = 8000;
            game.Settings.Accounts = new List<Account>();
            int i;
            for (i = 0; i < numberOfGamers; i++)
            {
                try
                {
                    while (!listener.Pending() && To > 0)
                    {
                        To -= 200;
                        Thread.Sleep(200);
                    }
                    if (To <= 0) break;
                    handlers[i].TcpClient = listener.AcceptTcpClient();
                }
                catch (Exception)
                {
                    break;
                }
                Trace.TraceInformation("Client connected");
                handlers[i].DataStream = handlers[i].TcpClient.GetStream();
                handlers[i].DataStream.ReadTimeout = 2500;
                if (game.Configuration != null)
                {
                    object item;
                    try
                    {
                        item = (new ItemReceiver(handlers[i].DataStream)).Receive();
                    }
                    catch (Exception)
                    {
                        item = null;
                    }
                    if (!(item is LoginToken) ||
                     !game.Configuration.LoginTokens.Any(id => id.TokenString == ((LoginToken)item).TokenString))
                    {
                        handlers[i].client.Close();
                        i--; 
                        continue;
                    }
                    int index;
                    for (index = 0; index < game.Configuration.LoginTokens.Count; index++)
                    {
                        if (game.Configuration.LoginTokens[index].TokenString == ((LoginToken)item).TokenString)
                        {
                            if (game.Settings.Accounts.Contains(game.Configuration.Accounts[index]))
                            {
                                handlers[i].client.Close();
                                i--;
                                continue;
                            }
                            game.Settings.Accounts.Add(game.Configuration.Accounts[index]);
                        }
                    }
                }
                handlers[i].stream.ReadTimeout = Timeout.Infinite;
                handlers[i].stream = new RecordTakingOutputStream(handlers[i].stream);
                handlers[i].sender = new ItemSender(handlers[i].stream);
                handlers[i].receiver = new ItemReceiver(handlers[i].stream);
                handlers[i].threadServer = new Thread((ParameterizedThreadStart)((o) => 
                {
                    ServerThread(handlers[(int)o]);
                })) { IsBackground = true };
                handlers[i].threadServer.Start(i);
                handlers[i].threadClient = new Thread((ParameterizedThreadStart)((o) => 
                {
                    ClientThread(handlers[(int)o]); 
                })) { IsBackground = true };
                handlers[i].threadClient.Start(i);
            }
            List<Account> remainingDisconnected = null;
            if (game.Configuration != null)
            {
                remainingDisconnected = new List<Account>(from acc in game.Configuration.Accounts where !game.Settings.Accounts.Contains(acc) select acc);
            }
            for (; i < numberOfGamers; i++)
            {
                handlers[i].game = game;
                handlers[i].stream = Stream.Null;
                handlers[i].disconnected = true;
                if (game.Configuration != null)
                {
                    game.Settings.Accounts.Add(remainingDisconnected.First());
                    remainingDisconnected.RemoveAt(0);
                }
                handlers[i].stream = new RecordTakingOutputStream(handlers[i].stream);
                handlers[i].sender = new ItemSender(handlers[i].stream);
                handlers[i].receiver = new ItemReceiver(handlers[i].stream);
                handlers[i].threadServer = new Thread((ParameterizedThreadStart)((o) =>
                {
                    ServerThread(handlers[(int)o]);
                })) { IsBackground = true };
                handlers[i].threadServer.Start(i);
                handlers[i].threadClient = new Thread((ParameterizedThreadStart)((o) =>
                {
                    ClientThread(handlers[(int)o]);
                })) { IsBackground = true };
                handlers[i].threadClient.Start(i);
            }
            var spectatorHandler = new ServerHandler();
            spectatorHandler.game = game;
            spectatorHandler.stream = new ReplaySplitterStream();
            spectatorHandler.receiver = new ItemReceiver(spectatorHandler.stream);
            spectatorHandler.sender = new ItemSender(spectatorHandler.stream);
            spectatorHandler.disconnected = true;
            spectatorHandler.threadServer = null;
            spectatorHandler.threadClient = new Thread((ParameterizedThreadStart)((o) =>
            {
                ClientThread(handlers[(int)o]);
            })) { IsBackground = true };
            handlers.Add(spectatorHandler);
            spectatorHandler.threadClient.Start(handlers.Count - 1);
            Trace.TraceInformation("Server ready");
            reconnectThread = new Thread(ReconnectionListener) { IsBackground = true };
            reconnectThread.Start();
        }
        
        Thread reconnectThread;

        void ReconnectionListener()
        {
            Trace.TraceInformation("Reconnection listener Started on {0} : {1}", ipAddress.ToString(), IpPort);
            while (true)
            {
                try
                {
                    var client = listener.AcceptTcpClient();
                    Trace.TraceInformation("Client connected");
                    var stream = client.GetStream();
                    bool spectatorJoining = false;
                    stream.ReadTimeout = 2000;
                    Account theAccount = null;
                    if (game.Configuration != null)
                    {
                        object item;
                        try
                        {
                            item = (new ItemReceiver(stream)).Receive();
                        }
                        catch (Exception)
                        {
                            item = null;
                        }
                        if (!(item is LoginToken))
                        {
                            client.Close();
                            continue;
                        }
                        if (!game.Configuration.LoginTokens.Any(id => id.TokenString == ((LoginToken)item).TokenString))
                        {
                            spectatorJoining = true;
                        }
                        else
                        {
                            int index;
                            for (index = 0; index < game.Configuration.LoginTokens.Count; index++)
                            {
                                if (game.Configuration.LoginTokens[index].TokenString == ((LoginToken)item).TokenString)
                                {
                                    theAccount = game.Configuration.Accounts[index];
                                }
                            }
                        }
                    }
                    stream.ReadTimeout = Timeout.Infinite;
                    int indexC = game.Settings.Accounts.IndexOf(theAccount);
                    if (spectatorJoining) indexC = numberOfGamers;
                    if (indexC < 0)
                    {
                        client.Close();
                        continue;
                    }
                    lock (handlers[indexC].queueIn) lock (handlers[indexC].queueOut) lock (handlers[indexC].sender) lock (handlers[indexC].receiver)
                    {
                        handlers[indexC].sender.Flush();
                        if (spectatorJoining)
                        {
                            ReplaySplitterStream rpstream = handlers[indexC].stream as ReplaySplitterStream;
                            var tempSender = new ItemSender(stream);
                            tempSender.Send(new CommandItem() { command = Command.Detach, type = ItemType.Int, data = 0 });
                            tempSender.Flush();
                            rpstream.DumpTo(stream);
                            stream.Flush();
                            tempSender.Send(new CommandItem() { command = Command.Attach, type = ItemType.Int, data = 0 });
                            tempSender.Flush();
                            rpstream.AddStream(stream);
                        }
                        else
                        {
                            var newRCStream = new RecordTakingOutputStream(stream);
                            var tempSender = new ItemSender(newRCStream);
                            tempSender.Send(new CommandItem() { command = Command.Detach, type = ItemType.Int, data = 0 });
                            tempSender.Flush();
                            (handlers[indexC].stream as RecordTakingOutputStream).DumpTo(newRCStream);
                            handlers[indexC].disconnected = false;
                            newRCStream.Flush();
                            tempSender.Send(new CommandItem() { command = Command.Attach, type = ItemType.Int, data = 0 });
                            tempSender.Flush();
                            handlers[indexC].stream = newRCStream;
                            handlers[indexC].sender = new ItemSender(handlers[indexC].stream);
                            handlers[indexC].receiver = new ItemReceiver(handlers[indexC].stream);
                        }
                    }

                }
                catch (Exception)
                {
                    return;
                }
            }

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
                    if (i.type == ItemType.CardRearrangement)
                    {
                        for (int ec = 0; ec < MaxClients; ec++)
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
            var o = new CommandItem() { command = Command.Interrupt, type = ItemType.CardUsageResponded };
            o.data = new CardUsageResponded() { playerId = id };
            for (int ec = 0; ec < MaxClients; ec++)
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
                lock (handlers[clientId].queueIn)
                {
                    o = handlers[clientId].queueIn.Dequeue();
                }
                if (o == null)
                {
                    return false;
                }
                if (o is CommandItem)
                {
                    CommandItem i = (CommandItem)o;
                    if (i.command == Command.QaId)
                    {
                        Trace.TraceInformation("Current commId from {0} is {1}", clientId, i.type);
                    }
                    if (!(i.data is int) || (i.command == Command.QaId && (int)i.data == handlers[clientId].commId))
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

        private static int MaximumDelayInBetweenPacket = 2000;

        public Card GetCard(int clientId, int timeOutSeconds)
        {
            object o;
            if (handlers[clientId].disconnected) return null;
            if (!handlers[clientId].semIn.WaitOne(MaximumDelayInBetweenPacket)) return null;
            lock (handlers[clientId].queueIn)
            {
                o = handlers[clientId].queueIn.Dequeue();
            }
            if (o == null)
            {
                Trace.TraceWarning("Expected Card but null");
                return null;
            }
            if (o is CardItem)
            {
                return Translator.DecodeServerCard((CardItem)o, clientId);
            }
            Trace.TraceWarning("Expected Card but type is {0}", o.GetType());
            return null;
        }

        public Player GetPlayer(int clientId, int timeOutSeconds)
        {
            object o;
            if (handlers[clientId].disconnected) return null;
            if (!handlers[clientId].semIn.WaitOne(MaximumDelayInBetweenPacket)) return null;
            lock (handlers[clientId].queueIn)
            {
                o = handlers[clientId].queueIn.Dequeue();
            }
            return o as Player;
        }

        public int? GetInt(int clientId, int timeOutSeconds)
        {
            object o;
            if (handlers[clientId].disconnected) return null;
            if (!handlers[clientId].semIn.WaitOne(MaximumDelayInBetweenPacket)) return null;
            lock (handlers[clientId].queueIn)
            {
                o = handlers[clientId].queueIn.Dequeue();
            }
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
            if (!handlers[clientId].semIn.WaitOne(MaximumDelayInBetweenPacket)) return null;
            lock (handlers[clientId].queueIn)
            {
                o = handlers[clientId].queueIn.Dequeue();
            }
            if (o == null)
            {
                Trace.TraceWarning("Expected skill but null");
                return null;
            }
            if (o is SkillItem)
            {
                return Translator.EncodeSkill((SkillItem)o);
            }
            if (o is CheatSkill)
            {
                return o as CheatSkill;
            }
            Trace.TraceWarning("Expected Skill but type is {0}", o.GetType());
            return null;
        }
        bool isStopped;

        public void SendObject(int clientId, object o)
        {
            //if all clients disconnect. terminate
            if (!isStopped && !handlers.Any(hd => hd.disconnected == false))
            {
                isStopped = true;
                throw new GameOverException();
            }
            if (o is Card)
            {
                var item = Translator.EncodeServerCard(o as Card, clientId);
                o = item;
            }
            if (o is CheatSkill)
            {
            }
            else if (o is ISkill)
            {
                o = Translator.EncodeSkill(o as ISkill);
            }
            Trace.TraceInformation("Sending a {0} to {1}", o, clientId);
            lock (handlers[clientId].queueOut)
            {
                handlers[clientId].queueOut.Enqueue(o);
            }
            handlers[clientId].semOut.Release(1);
        }

        public void Flush(int clientId)
        {
            SendObject(clientId, new FlushObject());
        }

        public void SendInterruptedObject(int clientId, Object o)
        {
            Trace.TraceInformation("Interrupted, sending a {0} to {1}", o.GetType(), clientId);
            lock (handlers[clientId].queueOut)
            {
                handlers[clientId].queueOut.Enqueue(o);
            }
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
            for (int i = 0; i < handlers.Count(); i++)
            {
                SendObject(i, new TerminationObject());
                handlers[i].threadClient.Join();
                if (handlers[i].threadServer != null) handlers[i].threadServer.Abort();
            }
            listener.Stop();
            reconnectThread.Abort();
        }

        private void ServerThread(ServerHandler handler)
        {
            game.RegisterCurrentThread();
            while (true)
            {
                object o;
                if (handler.disconnected) Thread.Sleep(1000);
                lock (handler.receiver)
                {
                    do
                    {
                        o = handler.receiver.Receive();
                        if (o == null) handler.disconnected = true;
                    } while (HandleInterrupt(o));
                }
                if (o != null)
                {
                    lock (handler.queueIn)
                    {
                        handler.queueIn.Enqueue(o);
                    }
                    handler.semIn.Release(1);
                }
            }
        }

        private void ClientThread(ServerHandler handler)
        {
            game.RegisterCurrentThread();
            while (true)
            {
                handler.semOut.WaitOne();
                object o;
                lock (handler.queueOut)
                {
                    o = handler.queueOut.Dequeue();
                }
                lock (handler.sender)
                {
                    if (o is FlushObject)
                    {
                        handler.sender.Flush();
                    }
                    else if (o is TerminationObject)
                    {
                        handler.sender.Flush();
                        return;
                    }
                    else if (!handler.sender.Send(o))
                    {
                        Trace.TraceError("Network failure @ send");
                    }
                }
            }
        }
    }
}
