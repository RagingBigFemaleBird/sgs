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
using Sanguosha.Lobby.Core;
using Sanguosha.Core.Utils;

namespace Sanguosha.Core.Network
{
    public class Client
    {
        ClientGamer networkService;

        public int SelfId { get; set; }

        private string ipString;

        public string IpString
        {
            get { return ipString; }
            set { ipString = value; }
        }
        private int portNumber;

        public int PortNumber
        {
            get { return portNumber; }
            set { portNumber = value; }
        }

        public Client()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isReplay">Set true if this is client is connected to a replayFile</param>
        /// <param name="replayStream"></param>
        /// <exception cref="System.ArgumentOutOfRangeException" />
        /// <exception cref="System.Net.Sockets.SocketException" />
        public void Start(Stream recordStream, LoginToken? token = null)
        {
            RecordStream = recordStream;
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpString), PortNumber);
            TcpClient client = new TcpClient();
            client.Connect(ep);
            NetworkStream stream = client.GetStream();
            networkService = new ClientGamer();
            networkService.DataStream = new RecordTakingInputStream(stream, recordStream);
            if (token != null)
            {
                networkService.Send(new ConnectionRequest() { Token = (LoginToken)token });
            }
        }

        public ReplayController ReplayController { get; set; }

        public void StartReplay(Stream replayStream)
        {
            this.replayStream = replayStream;
            networkService = new ClientGamer();
            networkService.DataStream = new NullOutputStream(replayStream);
            ReplayController = new Utils.ReplayController();
            ReplayController.EvenDelays = true;
        }

        private Stream replayStream;

        public Stream ReplayStream
        {
            get { return replayStream; }
        }

        public Stream RecordStream { get; set; }

        public void MoveHandCard(int from, int to)
        {
            networkService.Send(new HandCardMovementNotification() { From = from, To = to, PlayerItem = PlayerItem.Parse(SelfId) });
        }

        public void CardChoiceCallBack(CardRearrangement arrange)
        {
            networkService.Send(new CardRearrangementNotification() { CardRearrangement = arrange });
        }

        public void Send(GameDataPacket p)
        {
            networkService.Send(p);
        }

        public object Receive()
        {
            while (true)
            {
                var pkt = networkService.Receive();
                if (pkt is StatusSync)
                {
                    return ((StatusSync)pkt).Status;
                }
                else if (pkt is CardSync)
                {
                    return ((CardSync)pkt).Item.ToCard(SelfId);
                }
                else if (pkt is CardRearrangementNotification)
                {
                    Game.CurrentGame.NotificationProxy.NotifyCardChoiceCallback((pkt as CardRearrangementNotification).CardRearrangement);
                    continue;
                }
                else if (pkt is SeedSync)
                {
                    continue;
                }
                else if (pkt is UIStatusHint)
                {
                    Game.CurrentGame.IsUiDetached = (pkt as UIStatusHint).IsDetached;
                    continue;
                }
                else if (pkt is MultiCardUsageResponded)
                {
                    Game.CurrentGame.NotificationProxy.NotifyMultipleCardUsageResponded((pkt as MultiCardUsageResponded).PlayerItem.ToPlayer());
                    continue;
                }
                else if (pkt is OnlineStatusUpdate)
                {
                    var osu = pkt as OnlineStatusUpdate;
                    if (Game.CurrentGame.Players.Count > osu.PlayerId)
                    {
                        Game.CurrentGame.Players[osu.PlayerId].OnlineStatus = osu.OnlineStatus;
                    }
                    continue;
                }
                else
                {
                    return pkt;
                }
            }
        }

        public void Stop()
        {
            if (RecordStream != null)
            {
                RecordStream.Flush();
                RecordStream.Close();
            }
        }
    }
}
