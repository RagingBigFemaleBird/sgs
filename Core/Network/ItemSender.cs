using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Network
{
    public enum Command
    {
        WhoAmI,
        QaId,
        GameStart,
    }
    [Serializable]
    public struct CardItem
    {
        public int playerId;
        public DeckType deck;
        public int place;
    }
    [Serializable]
    public struct CommandItem
    {
        public Command command;
        public int data;
    }
    [Serializable]
    public struct PlayerItem
    {
        public int id;
    }
    public class ItemSender
    {
        private NetworkStream stream;
        static IFormatter formatter = new BinaryFormatter();
        public ItemSender(NetworkStream s)
        {
            stream = s;
        }

        private void QueueCard(Card card)
        {
            CardItem item = new CardItem();
            item.playerId = Game.CurrentGame.Players.IndexOf(card.Place.Player);
            Trace.Assert(item.playerId >= 0);
            item.deck = card.Place.DeckType;
            item.playerId = Game.CurrentGame.Decks[card.Place.Player, card.Place.DeckType].IndexOf(card);
            Trace.Assert(item.playerId >= 0);
            formatter.Serialize(stream, item);
            stream.Flush();
        }

        private void QueuePlayer(Player player)
        {
            int playerId = Game.CurrentGame.Players.IndexOf(player);
            Trace.Assert(playerId >= 0);
            PlayerItem item = new PlayerItem();
            item.id = playerId;
            formatter.Serialize(stream, item);
            stream.Flush();
        }

        private void QueueInt(int i)
        {
            formatter.Serialize(stream, i);
            stream.Flush();
        }

        private void QueueSkill(ISkill skill)
        {
        }

        private void QueueCommand(CommandItem c)
        {
            formatter.Serialize(stream, c);
            stream.Flush();
        }

        public bool Send(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (o is int)
            {
                QueueInt((int)o);
            }
            else if (o is Player)
            {
                QueuePlayer(o as Player);
            }
            else if (o is Card)
            {
                QueueCard(o as Card);
            }
            else if (o is ISkill)
            {
                QueueSkill(o as ISkill);
            }
            else if (o is CommandItem)
            {
                QueueCommand((CommandItem)o);
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
