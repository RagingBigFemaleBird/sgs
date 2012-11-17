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

        bool isReplay;
        Stream replayFile;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException" />
        /// <exception cref="System.Net.Sockets.SocketException" />
        public void Start(bool isReplay = false, Stream replayStream = null)
        {
            this.isReplay = isReplay;
            replayFile = replayStream;
            if (isReplay)
            {
                receiver = new ItemReceiver(replayFile);
                sender = new ItemSender(Stream.Null);
            }
            else
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpString), PortNumber);
                TcpClient client = new TcpClient();
                client.Connect(ep);
                NetworkStream stream = client.GetStream();
                receiver = new ItemReceiver(stream, replayFile);
                sender = new ItemSender(stream);
            }
            commId = 0;
        }

        private void _DeserializeCardItem(Card gameCard, int cardId)
        {
            var place = gameCard.Place;
            gameCard.CopyFrom(GameEngine.CardSet[cardId]);
            gameCard.Place = place;
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

        public object Receive()
        {
            if (isReplay)
            {
                Thread.Sleep(100);
            }
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
                        Trace.TraceInformation("Identify {0}{1}{2} is {3}{4}{5}", i.playerId, i.deck, i.place, i.suit, i.rank, i.type);
                        Card gameCard;
                        if (i.playerId < 0)
                        {
                            gameCard = Game.CurrentGame.Decks[null, i.deck][i.place];
                        }
                        else
                        {
                            gameCard = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place];
                        }
                        _DeserializeCardItem(gameCard, i.Id);
                        gameCard.Rank = i.rank;
                        gameCard.Suit = (SuitType)i.suit;
                        if (i.type != null) gameCard.Type = _DeserializeType(i.type, i.typeHorseName);
                        gameCard.AdditionalType = _DeserializeType(i.additionalType, i.additionalTypeHorseName);
                    }
                    return Receive();
                }
            }
            else if (o is PlayerItem)
            {
                PlayerItem i = (PlayerItem)o;
                o = Game.CurrentGame.Players[i.id];
            }
            else if (o is int)
            {
                Trace.TraceInformation("Received a {0}", (int)o);
            }
            else if (o is CardItem)
            {
                CardItem i = (CardItem)o;
                if (i.Id >= 0)
                {
                    Trace.TraceInformation("Identify {0}{1}{2} is {3}{4}{5}", i.playerId, i.deck, i.place, i.suit, i.rank, i.type);
                    Card gameCard;
                    if (i.playerId < 0)
                    {                        
                        gameCard = Game.CurrentGame.Decks[null, i.deck][i.place];
                    }
                    else
                    {
                        gameCard = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place];
                    }
                    _DeserializeCardItem(gameCard, i.Id);
                    gameCard.Rank = i.rank;
                    gameCard.Suit = (SuitType)i.suit;
                    if (i.type != null) gameCard.Type = _DeserializeType(i.type, i.typeHorseName);
                    gameCard.AdditionalType = _DeserializeType(i.additionalType, i.additionalTypeHorseName);
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
                _SerializeType(ref item.type, ref item.typeHorseName, card.Type);
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
