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

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException" />
        /// <exception cref="System.Net.Sockets.SocketException" />
        public void Start()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpString), PortNumber);
            TcpClient client = new TcpClient();
            client.Connect(ep);
            stream = client.GetStream();
            receiver = new ItemReceiver(stream);
            sender = new ItemSender(stream);
            commId = 0;
        }

        public object Receive()
        {
            retry:
            object o = receiver.Receive();
            if (o is CommandItem)
            {
                CommandItem item = (CommandItem)o;
                if (item.command == Command.Interrupt)
                {
                    object o2 = receiver.Receive();
                    Trace.Assert(o2 is InterruptedObject);
                    InterruptedObject obj = (InterruptedObject)o2;
                    Trace.Assert(obj.obj is CardItem);
                    CardItem i = (CardItem)obj.obj;
                    if (i.Id >= 0)
                    {
                        Trace.TraceInformation("Identify {0}{1}{2} is {3}{4}{5}", i.playerId, i.deck, i.place, GameEngine.CardSet[i.Id].Suit, GameEngine.CardSet[i.Id].Rank, GameEngine.CardSet[i.Id].Type.CardType);
                        if (i.playerId < 0)
                        {
                            DeckPlace place = Game.CurrentGame.Decks[null, i.deck][i.place].Place;
                            Game.CurrentGame.Decks[null, i.deck][i.place].CopyFrom(GameEngine.CardSet[i.Id]);
                            Game.CurrentGame.Decks[null, i.deck][i.place].Place = place;
                            Game.CurrentGame.Decks[null, i.deck][i.place].Id = i.Id;
                        }
                        else
                        {
                            DeckPlace place = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place].Place;
                            Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place].CopyFrom(GameEngine.CardSet[i.Id]);
                            Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place].Place = place;
                            Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place].Id = i.Id;
                        }
                    }
                    goto retry;
                }
            }
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
                    Trace.TraceInformation("Identify {0}{1}{2} is {3}{4}{5}", i.playerId, i.deck, i.place, GameEngine.CardSet[i.Id].Suit, GameEngine.CardSet[i.Id].Rank, GameEngine.CardSet[i.Id].Type.CardType);
                    if (i.playerId < 0)
                    {
                        DeckPlace place = Game.CurrentGame.Decks[null, i.deck][i.place].Place;
                        Game.CurrentGame.Decks[null, i.deck][i.place].CopyFrom(GameEngine.CardSet[i.Id]);
                        Game.CurrentGame.Decks[null, i.deck][i.place].Place = place;
                        Game.CurrentGame.Decks[null, i.deck][i.place].AddtionalGenericType = i.additionalType;
                    }
                    else
                    {
                        DeckPlace place = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place].Place;
                        Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place].CopyFrom(GameEngine.CardSet[i.Id]);
                        Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place].Place = place;
                        Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place].AddtionalGenericType = i.additionalType;
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
            else if (o is SkillItem)
            {
                SkillItem i = (SkillItem)o;
                foreach (var skill in Game.CurrentGame.Players[i.playerId].ActionableSkills)
                {
                    if (skill.GetType().Name.Equals(i.name))
                    {
                        return skill;
                    }
                }
                Trace.TraceWarning("Client seem to be sending invalid skills. DDOS?");
                return null;
            }
            return o;
        }

        public void AnswerNext()
        {
            sender.Send(new CommandItem() { command = Command.QaId, data = commId });
        }

        public void NextComm()
        {
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
                item.additionalType = card.AddtionalGenericType;
                Trace.Assert(item.place >= 0);
                o = item;
            }
            if (o is ISkill)
            {
                ISkill skill = o as ISkill;
                SkillItem item = new SkillItem();
                item.playerId = skill.Owner.Id;
                item.name = skill.GetType().Name;
                o = item;
            }
            sender.Send(o);
        }
    }
}
