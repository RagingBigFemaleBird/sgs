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
using System.IO;

namespace Sanguosha.Core.Network
{
    public enum Command
    {
        WhoAmI,
        QaId,
        GameStart,
        Interrupt,
    }
    [Serializable]
    public struct InterruptedObject
    {
        public object obj;
    }
    [Serializable]
    public struct CardItem
    {
        public int playerId;
        public DeckType deck;
        public int place;
        public int Id;
        public CardHandler additionalType;
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
    [Serializable]
    public struct SkillItem
    {
        public int playerId;
        public string name;
    }
    public class ItemSender
    {
        private Stream stream;
        static IFormatter formatter = new BinaryFormatter();
        public ItemSender(Stream s)
        {
            stream = s;
        }

        private void QueueCard(CardItem card)
        {
            formatter.Serialize(stream, card);
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

        private void QueueSkill(SkillItem skill)
        {
            formatter.Serialize(stream, skill);
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
            else if (o is CardItem)
            {
                QueueCard((CardItem)o);
            }
            else if (o is ISkill)
            {
                QueueSkill(o as ISkill);
            }
            else if (o is CommandItem)
            {
                QueueCommand((CommandItem)o);
            }
            else if (o is SkillItem)
            {
                QueueSkill((SkillItem)o);
            }
            else if (o is InterruptedObject)
            {
                QueueInterruptedObject((InterruptedObject)o);
            }
            else
            {
                Trace.Assert(false);
            }

            return true;
        }

        private void QueueInterruptedObject(InterruptedObject o)
        {
            formatter.Serialize(stream, o);
            stream.Flush();
        }
    }
}
