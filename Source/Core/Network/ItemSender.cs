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
using Sanguosha.Core.Utils;

namespace Sanguosha.Core.Network
{
    public class ItemSender
    {
        private Stream stream;
        private RawSerializer serializer;
        static IFormatter formatter = new BinaryFormatter();
        public ItemSender(Stream s)
        {
            stream = new BufferedStream(s);
            serializer = new RawSerializer(stream);
        }

        public void Flush()
        {
            try
            {
                stream.Flush();
            }
            catch (Exception)
            {
            }
        }

        private void QueueCard(CardItem card)
        {
            serializer.Serialize(ItemType.CardItem);
            serializer.Serialize(card.playerId);
            serializer.Serialize(card.place);
            serializer.Serialize(card.rank);
            serializer.Serialize(card.suit);
            serializer.Serialize(card.id);
            serializer.Serialize(card.deckName ?? string.Empty);
            serializer.Serialize(card.typeName ?? string.Empty);
            serializer.Serialize(card.typeHorseName ?? string.Empty);
        }

        private void QueuePlayer(Player player)
        {
            int? playerId = null;
            if (player != null)
            {
                playerId = Game.CurrentGame.Players.IndexOf(player);
                Trace.Assert(playerId >= 0);
            }
            serializer.Serialize(ItemType.Player);
            serializer.SerializeNullable(playerId);
        }

        private void QueueObject(object o)
        {
            serializer.Serialize(ItemType.Serializable);
            formatter.Serialize(stream, o);
        }

        private void QueueSkill(SkillItem skill)
        { 
            serializer.Serialize(ItemType.SkillItem);
            serializer.Serialize(skill.playerId);
            serializer.Serialize(skill.skillId);
            serializer.Serialize(skill.name ?? string.Empty);
            serializer.Serialize(skill.additionalTypeName ?? string.Empty);
            serializer.Serialize(skill.additionalTypeHorseName ?? string.Empty);
        }

        private void QueueInt(int i)
        {
            serializer.Serialize(ItemType.Int);
            serializer.Serialize(i);
        }

        private void QueueCommand(CommandItem c)
        {
            serializer.Serialize(ItemType.CommandItem);
            serializer.Serialize(c.command);
            serializer.Serialize(c.type);
            serializer.Serialize(c.data);
        }

        private void QueueValueType(object o)
        {
            serializer.Serialize(ItemType.ValueType);
            serializer.Serialize(o.GetType().AssemblyQualifiedName);
            serializer.Serialize(o);
        }

        public bool Send(object o)
        {
            if (o == null)
            {
                return false;
            }
            try
            {
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
                else if (o is CommandItem)
                {
                    QueueCommand((CommandItem)o);
                }
                else if (o is SkillItem)
                {
                    QueueSkill((SkillItem)o);
                }
                else if (o.GetType().IsValueType)
                {
                    QueueValueType(o);
                }
                else
                {
                    QueueObject(o);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

    }
}
