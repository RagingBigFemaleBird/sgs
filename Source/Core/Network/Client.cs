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
        ItemReceiver receiver;
        ItemSender sender;
        int commId;
        
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
        public void Start(Stream recordStream = null, LoginToken? token = null)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpString), PortNumber);
            TcpClient client = new TcpClient();
            client.Connect(ep);
            NetworkStream stream = client.GetStream();
            receiver = new ItemReceiver(stream, recordStream);
            sender = new ItemSender(stream);
            if (token != null)
            {
                sender.Send((LoginToken)token);
                sender.Flush();
            }
            commId = 0;
        }

        public ReplayController ReplayController { get; set; }

        public void StartReplay(Stream replayStream)
        {
            receiver = new ItemReceiver(replayStream);
            this.replayStream = replayStream;
            sender = new ItemSender(Stream.Null);
            commId = 0;
            ReplayController = new Utils.ReplayController();
            ReplayController.EvenDelays = true;
        }

        private Stream replayStream;

        public Stream ReplayStream
        {
            get { return replayStream; }
        }

        public Stream RecordStream
        {
            get
            {
                if (receiver != null) return receiver.RecordStream;
                else return null;
            }
            set
            {
                Trace.Assert(receiver != null);
                receiver.RecordStream = value;
            }
        }

        public void MoveHandCard(int from, int to)
        {
            HandCardMovement move = new HandCardMovement();
            move.playerId = SelfId;
            move.from = from;
            move.to = to;
            CommandItem item = new CommandItem();
            item.command = Command.Interrupt;
            item.type = ItemType.HandCardMovement;
            item.data = move;
            sender.Send(item);
            sender.Flush();
        }

        public object Receive()
        {
            object o = receiver.Receive();
#if !DEBUG
            if (o == null)
            {
                //disconnected. For now, end game
                throw new GameOverException();
            }
#endif
            if (o is CommandItem)
            {
                CommandItem item = (CommandItem)o;
                if (item.command == Command.Attach)
                {
                    Game.CurrentGame.IsUiDetached--;
                    return Receive();
                }
                if (item.command == Command.Detach)
                {
                    Game.CurrentGame.IsUiDetached++;
                    return Receive();
                }
                if (item.command == Command.Interrupt)
                {
                    if (item.type == ItemType.CardRearrangement)
                    {
                        CardRearrangement ca = (CardRearrangement)item.data;
                        Game.CurrentGame.NotificationProxy.NotifyCardChoiceCallback(ca);
                    }
                    if (item.type == ItemType.CardUsageResponded)
                    {
                        var cbi = (CardUsageResponded)item.data;
                        Game.CurrentGame.NotificationProxy.NotifyMultipleCardUsageResponded(Game.CurrentGame.Players[cbi.playerId]);
                    }
                    return Receive();
                }
            }
            else if (o is Player)
            {
            }
            else if (o is int)
            {
                Trace.TraceInformation("Received a {0}", (int)o);
            }
            else if (o is CardItem)
            {
                CardItem i = (CardItem)o;
                return Translator.DecodeCard(i, SelfId);
            }
            else if (o is SkillItem)
            {
                return Translator.EncodeSkill((SkillItem)o);
            }
            return o;
        }

        public void AnswerNext()
        {
            sender.Send(new CommandItem() { command = Command.QaId, type = ItemType.Int, data=commId });
        }

        public void NextComm()
        {
            commId++;
        }

        public void AnswerItem(Card card)
        {            
            sender.Send(Translator.EncodeCard(card));
        }

        public void AnswerItem(ISkill skill)
        {
            if (skill is CheatSkill)
            {
                sender.Send(skill);
            }
            else
            {
                sender.Send(Translator.EncodeSkill(skill));
            }
        }

        public void AnswerItem(int i)
        {            
            sender.Send(i);
        }

        public void AnswerItem(Player p)
        {
            sender.Send(p);
        }

        public void Flush()
        {
            sender.Flush();
        }

        public void CardChoiceCallBack(CardRearrangement arrange)
        {
            CommandItem item = new CommandItem();
            item.command = Command.Interrupt;
            item.type = ItemType.CardRearrangement;
            item.data = arrange;
            sender.Send(item);
            sender.Flush();
        }

        public void Stop()
        {
            if (receiver.RecordStream != null)
            {
                receiver.RecordStream.Flush();
                receiver.RecordStream.Close();
            }
        }
    }
}
