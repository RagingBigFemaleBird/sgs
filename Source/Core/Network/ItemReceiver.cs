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
            object o;
            try
            {
                o = formatter.Deserialize(stream);
            }
            catch (Exception)
            {
                return null;
            }
            if (replayStream != null)
            {
                formatter.Serialize(replayStream, o);
                replayStream.Flush();
            }
            return o;
        }
    }
}
