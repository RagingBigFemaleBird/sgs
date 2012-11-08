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
    public class ItemReceiver
    {
        private Stream stream, replayStream;
        static IFormatter formatter = new BinaryFormatter();

        public ItemReceiver(Stream s, Stream replay = null)
        {
            stream = s;
            replayStream = replay;
        }

        public object Receive()
        {
            object o = formatter.Deserialize(stream);
            if (replayStream != null)
            {
                formatter.Serialize(replayStream, o);
                replayStream.Flush();
            }
            if (!((o is int) || (o is PlayerItem) || (o is CardItem) || (o is CommandItem) || (o is SkillItem) || (o is InterruptedObject)))
            {
                return null;
            }
            if (o is PlayerItem)
            {
                PlayerItem i = (PlayerItem)o;
                o = Game.CurrentGame.Players[i.id];
            }
            return o;
        }
    }
}
