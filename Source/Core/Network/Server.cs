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

        public List<ServerGamer> Gamers
        {
            get;
            private set;
        }
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
            numberOfGamers = capacity;
            Gamers = new List<ServerGamer>();
            for (int i = 0; i < capacity; i++)
            {
                Gamers.Add(new ServerGamer());
                Gamers[i].Game = game;
                Gamers[i].StartListening();
            }
            this.game = game;
            Trace.TraceInformation("Server initialized with capacity {0}", capacity);
        }

        public bool IsDisconnected(int id)
        {
            return Gamers[id].ConnectionStatus != ConnectionStatus.Connected;
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
                    Gamers[i].TcpClient = listener.AcceptTcpClient();
                }
                catch (Exception)
                {
                    break;
                }
                Trace.TraceInformation("Client connected");
                if (game.Configuration != null)
                {
                    object item = null;
                    Semaphore sem0 = new Semaphore(0, int.MaxValue);
                    GamePacketHandler hd = (o) => { if (o is ConnectionRequest) { item = ((ConnectionRequest)o).token; sem0.Release(1); } };
                    Gamers[i].OnGameDataPacketReceived += hd;
                    Gamers[i].DataStream = new RecordTakingOutputStream(Gamers[i].TcpClient.GetStream());

                    if (!sem0.WaitOne(4000) || !(item is LoginToken) ||
                     !game.Configuration.LoginTokens.Any(id => id.TokenString == ((LoginToken)item).TokenString))
                    {
                        Gamers[i].OnGameDataPacketReceived -= hd;
                        Gamers[i].DataStream = null;
                        Gamers[i].TcpClient.Close();
                        i--;
                        continue;
                    }
                    Gamers[i].OnGameDataPacketReceived -= hd;
                    int index;
                    for (index = 0; index < game.Configuration.LoginTokens.Count; index++)
                    {
                        if (game.Configuration.LoginTokens[index].TokenString == ((LoginToken)item).TokenString)
                        {
                            if (game.Settings.Accounts.Contains(game.Configuration.Accounts[index]))
                            {
                                Gamers[i].TcpClient.Close();
                                i--;
                                continue;
                            }
                            game.Settings.Accounts.Add(game.Configuration.Accounts[index]);
                        }
                    }
                }
                else
                {
                    Gamers[i].DataStream = Gamers[i].TcpClient.GetStream();
                }
            }
            List<Account> remainingDisconnected = null;
            if (game.Configuration != null)
            {
                remainingDisconnected = new List<Account>(from acc in game.Configuration.Accounts where !game.Settings.Accounts.Contains(acc) select acc);
            }
            for (; i < numberOfGamers; i++)
            {
                Gamers[i].DataStream = Stream.Null;
                Gamers[i].ConnectionStatus = ConnectionStatus.Disconnected;
                if (game.Configuration != null)
                {
                    game.Settings.Accounts.Add(remainingDisconnected.First());
                    remainingDisconnected.RemoveAt(0);
                }
            }
            var spectatorHandler = new ServerGamer();
            spectatorHandler.DataStream = new ReplaySplitterStream();
            spectatorHandler.ConnectionStatus = ConnectionStatus.Disconnected;
            Gamers.Add(spectatorHandler);
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
                    ServerGamer tempGamer = new ServerGamer();
                    tempGamer.Game = game;
                    tempGamer.StartListening();
                    tempGamer.Once = true;
                    Account theAccount = null;
                    if (game.Configuration != null)
                    {
                        object item = null;
                        Semaphore sem0 = new Semaphore(0, int.MaxValue);
                        GamePacketHandler hd = (o) => { if (o is ConnectionRequest) { item = ((ConnectionRequest)o).token; sem0.Release(1); } };
                        tempGamer.OnGameDataPacketReceived += hd;
                        tempGamer.DataStream = stream;

                        if (!sem0.WaitOne(4000) || !(item is LoginToken))
                        {
                            tempGamer.OnGameDataPacketReceived -= hd;
                            tempGamer.DataStream = null;
                            tempGamer.TcpClient.Close();
                            continue;
                        }
                        tempGamer.OnGameDataPacketReceived -= hd;
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
                                    if (game.Settings.Accounts.Contains(game.Configuration.Accounts[index]))
                                    {
                                        theAccount = game.Configuration.Accounts[index];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        tempGamer.DataStream = stream;
                    }
                    int indexC = game.Settings.Accounts.IndexOf(theAccount);
                    if (spectatorJoining) indexC = numberOfGamers;
                    if (indexC < 0)
                    {
                        client.Close();
                        continue;
                    }
                    tempGamer.Stop();
                    Gamers[indexC].Lock();
                    try
                    {
                        if (spectatorJoining)
                        {
                            ReplaySplitterStream rpstream = Gamers[indexC].DataStream as ReplaySplitterStream;
                            tempGamer.Send(new UIStatusHint() { IsDetached = true });
                            tempGamer.Flush();
                            rpstream.DumpTo(stream);
                            tempGamer.Flush();
                            tempGamer.Send(new UIStatusHint() { IsDetached = false });
                            tempGamer.Flush();
                            rpstream.AddStream(stream);
                        }
                        else
                        {
                            var oldstream = Gamers[indexC].DataStream as RecordTakingOutputStream;
                            var newRCStream = new RecordTakingOutputStream(stream);
                            Gamers[indexC].DataStream = newRCStream;
                            Gamers[indexC].StartListening();
                            Gamers[indexC].ConnectionStatus = ConnectionStatus.Connected;
                            tempGamer.Send(new UIStatusHint() { IsDetached = true });
                            tempGamer.Flush();
                            (oldstream).DumpTo(newRCStream);
                            newRCStream.Flush();
                            tempGamer.Flush();
                            tempGamer.Send(new UIStatusHint() { IsDetached = false });
                            tempGamer.Flush();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    Gamers[indexC].Unlock();

                }
                catch (Exception)
                {
                }
            }

        }

        public void SendObject(int clientId, GameDataPacket o)
        {
            if (!Gamers.Any(hdl => hdl.ConnectionStatus == ConnectionStatus.Connected)) throw new GameOverException();
            Gamers[clientId].Send(o);
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
            listener.Stop();
            reconnectThread.Abort();
        }

    }
}
