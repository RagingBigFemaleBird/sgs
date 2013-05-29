using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using System.Net.Sockets;

namespace Sanguosha.Core.Network
{
    public class ClientGamer
    {
        public ClientGamer()
        {
            sema = new Semaphore(0, 1);
            semPause = new Semaphore(0, 1);
        }

        public TcpClient TcpClient { get; set; }
        public Stream DataStream { get; set; }

        Semaphore sema;
        Semaphore semPause;

        public GameDataPacket Receive()
        {
            var packet = Serializer.DeserializeWithLengthPrefix<GameDataPacket>(DataStream, PrefixStyle.Base128);
            return packet;
        }

        public void Send(GameDataPacket packet)
        {
            Serializer.SerializeWithLengthPrefix<GameDataPacket>(DataStream, packet, PrefixStyle.Base128);
        }
    }
}
