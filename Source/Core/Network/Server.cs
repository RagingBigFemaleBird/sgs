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
using System.ComponentModel;
using ProtoBuf;

namespace Sanguosha.Core.Network
{
    public class Server
    {
        //The following invariant MUST hold: the first <numberOfGamers> handlers are for gamers. The last one is for spectators.
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
            for (int i = 0; i <= capacity; i++)
            {
                Gamers.Add(new ServerGamer());
                Gamers[i].Game = game;                
                Gamers[i].ConnectionStatus = ConnectionStatus.Disconnected;
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
            connectThread = new Thread(ConnectionListener) { IsBackground = true };
            connectThread.Start();
            for (int i = 0; i < 50; i++)
            {
                if (Gamers.Count(g => g.ConnectionStatus != ConnectionStatus.Disconnected) == 1) break;
                Thread.Sleep(100);
            }
        }

        Thread connectThread;

        private LoginToken? _ReadLoginToken(NetworkStream stream, int waitTimeMSecs)
        {
            if (stream == null) return null;
            int timeOut = stream.ReadTimeout;
            stream.ReadTimeout = waitTimeMSecs;
            ConnectionRequest packet;
            try
            {
                packet = Serializer.DeserializeWithLengthPrefix<GameDataPacket>(stream, PrefixStyle.Base128) as ConnectionRequest;
            }
            catch (Exception)
            {
                return null;
            }
            var connectionRequest = packet;
            if (connectionRequest == null) return null;
            stream.ReadTimeout = timeOut;
            return packet.Token;
        }

        private void _ConnectClient(TcpClient client)
        {
            Trace.TraceInformation("Client connected");
            var stream = client.GetStream();

            LoginToken? token = _ReadLoginToken(stream, 4000);            
            var timeOut = stream.ReadTimeout;
            if (token == null) return;

            Account theAccount = game.Settings.Accounts.FirstOrDefault
                (a => a.LoginToken.TokenString == token.Value.TokenString);
            int indexC;
            // bool spectatorJoining;
            if (theAccount != null)
            {
                // spectatorJoining = false;
                indexC = game.Settings.Accounts.IndexOf(theAccount);
            }
            else
            {
                // spectatorJoining = true;
                indexC = numberOfGamers;                
            }                                   
            ServerGamer gamer = Gamers[indexC];
            gamer.AddStream(stream);
            gamer.ConnectionStatus = ConnectionStatus.Connected;
            gamer.TcpClient = client;
            gamer.StartListening();
        }

        void ConnectionListener()
        {
            Trace.TraceInformation("Reconnection listener Started on {0} : {1}", ipAddress.ToString(), IpPort);
            int errorCount = 0;
            while (true)
            {
                TcpClient client = null;
                try
                {
                    client = listener.AcceptTcpClient();
                }
                catch (Exception)
                {                    
                    if (errorCount++ > 10) return;
                    continue;
                }
                
                if (client == null) continue;                
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (o, e) =>
                {
                    _ConnectClient(client);
                };
                worker.RunWorkerAsync();
            }
        }

        public void SendObject(int clientId, GameDataPacket o)
        {
            if (!Gamers.Any(hdl => hdl.ConnectionStatus == ConnectionStatus.Connected))
            {
                throw new GameOverException();
            }
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
            connectThread.Abort();
        }

    }
}
