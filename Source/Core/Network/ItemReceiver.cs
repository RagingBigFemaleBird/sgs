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
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Network
{
    public class ItemReceiver
    {
        static IFormatter formatter = new BinaryFormatter();
        RawDeserializer deserializer;
        RecordTakingInputStream stream;

        public ItemReceiver()
        {
            stream = new RecordTakingInputStream();
            deserializer = new RawDeserializer(stream);
        }

        public ItemReceiver(Stream inputStream, Stream recordStream = null) : this()
        {
            InputStream = inputStream;
            RecordStream = recordStream;
        }

        public Stream InputStream
        {
            get
            {
                return stream.InputStream;
            }
            set
            {
                if (InputStream == value) return;
                stream.InputStream = value;                
            }
        }

        public Stream RecordStream
        {
            get
            {
                return stream.RecordStream;
            }
            set
            {
                stream.RecordStream = value;
            }
        }

        public object Receive()
        {
            object o = null;
            ItemType type = ItemType.Serializable;
            try
            {
                type = (ItemType)deserializer.DeserializeInt();
                Trace.TraceInformation("Trying to parse a {0}", type);
                switch (type)
                {
                    case ItemType.CardItem:
                        var cardItem = new CardItem();
                        cardItem.playerId = deserializer.DeserializeInt();
                        cardItem.place = deserializer.DeserializeInt();
                        cardItem.rank = deserializer.DeserializeInt();
                        cardItem.suit = deserializer.DeserializeInt();
                        cardItem.id = deserializer.DeserializeInt();
                        cardItem.deckName = deserializer.DeserializeString();
                        cardItem.typeName = deserializer.DeserializeString();
                        cardItem.typeHorseName = deserializer.DeserializeString();
                        o = cardItem;
                        break;
                    case ItemType.Player:
                        int? id = deserializer.DeserializeNInt();
                        o = (id == null ? null : Game.CurrentGame.Players[(int)id]);
                        break;
                    case ItemType.Int:
                        o = deserializer.DeserializeInt();
                        break;
                    case ItemType.SkillItem:
                        var skillItem = new SkillItem();
                        skillItem.playerId = deserializer.DeserializeInt();
                        skillItem.skillId = deserializer.DeserializeInt();
                        skillItem.name = deserializer.DeserializeString();
                        skillItem.additionalTypeName = deserializer.DeserializeString();
                        skillItem.additionalTypeHorseName = deserializer.DeserializeString();
                        o = skillItem;
                        break;
                    case ItemType.CommandItem:
                        CommandItem item = new CommandItem();
                        item.command = (Command)deserializer.DeserializeInt();
                        item.type = (ItemType)deserializer.DeserializeInt();
                        if (item.type == ItemType.Int)
                            item.data = deserializer.DeserializeInt();
                        else if (item.type == ItemType.HandCardMovement)
                            item.data = deserializer.Deserialize(typeof(HandCardMovement));
                        else if (item.type == ItemType.CardRearrangement)
                            item.data = deserializer.Deserialize(typeof(CardRearrangement));
                        else if (item.type == ItemType.CardUsageResponded)
                            item.data = deserializer.Deserialize(typeof(CardUsageResponded));
                        o = item;
                        break;                        
                    case ItemType.ValueType:
                        Type objectType = Type.GetType(deserializer.DeserializeString());
                        o = deserializer.Deserialize(objectType);
                        break;                         
                    case ItemType.Serializable:
                        o = formatter.Deserialize(stream);
                        break;
                    default:
                        o = null;
                        Trace.TraceError("Unknown item type: {0}", type);
                        break;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error occured when trying to deserialize an {0}, {1}. ", type, e.StackTrace);
                return null;
            }
            return o;
        }
    }
}
