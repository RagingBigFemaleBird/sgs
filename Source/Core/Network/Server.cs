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
                ServerGamer gamer = new ServerGamer() { Game = game, OnlineStatus = Network.OnlineStatus.Offline };
                gamer.OnDisconnected += Server_OnDisconnected;
                gamer.StartSender();
                Gamers.Add(gamer);                
            }            
            this.game = game;
            Trace.TraceInformation("Server initialized with capacity {0}", capacity);
        }

        public void SetOnlineStatus(int id, OnlineStatus status)
        {
            Trace.Assert(id >= 0);
            if (id < 0) return;
            if (game != null && game.Players.Count > id)
            {
                game.Players[id].OnlineStatus = status;
            }
            Gamers[id].OnlineStatus = status;            
            for (int i = 0; i < MaxClients; i++)
            {                
                Gamers[i].SendAsync(new OnlineStatusUpdate() { PlayerId = id, OnlineStatus = status } );
            }             
        }

        bool killServer;

        void Server_OnDisconnected(ServerGamer gamer)
        {
            if (!gamer.IsSpectator)
            {
                SetOnlineStatus(Gamers.IndexOf(gamer), OnlineStatus.Offline);
            }
            if (!Gamers.Any(hdl => !hdl.IsSpectator && hdl.IsConnected))
            {
                killServer = true;
            }
        }

        public bool IsDisconnected(int id)
        {
            return !Gamers[id].IsConnected;
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
                if (!Gamers.Any(g => g.OnlineStatus == OnlineStatus.Offline)) break;
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
            if (killServer) return;
            Trace.TraceInformation("Client connected");
            var stream = client.GetStream();
            LoginToken? token = _ReadLoginToken(stream, 4000);            
            var timeOut = stream.ReadTimeout;
            if (token == null) return;

            Account theAccount = game.Settings.Accounts.FirstOrDefault
                (a => a.LoginToken.TokenString == token.Value.TokenString);
            int indexC;
            
            if (theAccount != null)
            {
                indexC = game.Settings.Accounts.IndexOf(theAccount);
            }
            else
            {
                indexC = numberOfGamers;                
            }
            try
            {
                var packet = new ConnectionResponse() { SelfId = indexC, Settings = game.Settings };
                Serializer.SerializeWithLengthPrefix<GameDataPacket>(stream, packet, PrefixStyle.Base128);
                stream.Flush();
            }
            catch (IOException)
            {
                try { stream.Close(); }
                catch (Exception) { }
                return;
            }
            ServerGamer gamer = Gamers[indexC];         
            gamer.AddStream(stream);
            gamer.IsSpectator = (indexC == numberOfGamers);
            if (indexC != numberOfGamers)
            {
                gamer.StartReceiver();
                SetOnlineStatus(indexC, OnlineStatus.Online);
            }       
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

        public void SendPacket(int clientId, GameDataPacket packet)
        {
            if (killServer && Thread.CurrentThread == game.MainThread)
            {
                throw new GameOverException() { EveryoneQuits = true };
            }
            Gamers[clientId].SendAsync(packet);
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
            foreach (var gamer in Gamers)
            {
                gamer.Stop();
            }
            Thread.Sleep(3000);
            foreach (var gamer in Gamers)
            {
                if (!gamer.IsStopped)
                {
                    gamer.Abort();
                }
            }            
        }

    }
}
