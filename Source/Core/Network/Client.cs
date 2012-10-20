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
    public class Client
    {
        NetworkStream stream;
        ItemReceiver receiver;
        ItemSender sender;
        int commId;
        public int SelfId { get; set; }
        public void Start()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            TcpClient client = new TcpClient();
            client.Connect(ep);
            stream = client.GetStream();
            receiver = new ItemReceiver(stream);
            sender = new ItemSender(stream);
            commId = 0;
        }

        public object Receive()
        {
            object o = receiver.Receive();
            if (o is int)
            {
                Trace.TraceInformation("Received a {0}", (int)o);
            }
            else
            {
                Trace.TraceInformation("Received a {0}", o.GetType().Name);
            }
            if (o is CardItem)
            {
                CardItem i = (CardItem)o;
                if (i.Id >= 0)
                {
                    Trace.TraceInformation("Identify {0}{1}{2} is {3}{4}{5}", i.playerId, i.deck, i.place, Game.CurrentGame.SlaveCardSet[i.Id].Suit, Game.CurrentGame.SlaveCardSet[i.Id].Rank, Game.CurrentGame.SlaveCardSet[i.Id].Type.CardType);
                    if (i.playerId < 0)
                    {
                        Card c = new Card(Game.CurrentGame.SlaveCardSet[i.Id]);
                        c.Place = new DeckPlace(null, i.deck);
                        Game.CurrentGame.Decks[null, i.deck][i.place] = c;
                    }
                    else
                    {
                        Card c = new Card(Game.CurrentGame.SlaveCardSet[i.Id]);
                        c.Place = new DeckPlace(Game.CurrentGame.Players[i.playerId], i.deck);
                        Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place] = c;
                    }
                }
                if (i.playerId < 0)
                {
                    o = Game.CurrentGame.Decks[null, i.deck][i.place];
                }
                else
                {
                    o = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place];
                }
                Trace.Assert(o != null);
                return o as Card;
            }
            return o;
        }

        public void AnswerNext()
        {
            sender.Send(new CommandItem() { command = Command.QaId, data = commId });
            commId++;
        }

        public void AnswerItem(object o)
        {
            if (o is Card)
            {
                Card card = o as Card;
                CardItem item = new CardItem();
                item.playerId = Game.CurrentGame.Players.IndexOf(card.Place.Player);
                item.deck = card.Place.DeckType;
                item.place = Game.CurrentGame.Decks[card.Place.Player, card.Place.DeckType].IndexOf(card);
                Trace.Assert(item.place >= 0);
                o = item;
            }
            sender.Send(o);
        }
    }
}
