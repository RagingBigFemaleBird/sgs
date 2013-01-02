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
        static IFormatter formatter = new BinaryFormatter();

        public ItemReceiver()
        {

        }

        public ItemReceiver(Stream inputStream, Stream replayStream = null)
        {
            InputStream = inputStream;
            RecordStream = replayStream;
        }

        public Stream InputStream
        {
            get;
            set;
        }

        public Stream RecordStream
        {
            get;
            set;
        }

        public object Receive()
        {
            object o;
            try
            {
                o = formatter.Deserialize(InputStream);
            }
            catch (Exception)
            {
                return null;
            }
            if (RecordStream != null)
            {
                formatter.Serialize(RecordStream, o);
                RecordStream.Flush();
            }
            return o;
        }
    }
}
