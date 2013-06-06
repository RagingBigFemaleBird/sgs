using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using System.Net.Sockets;
using System.Diagnostics;

namespace Sanguosha.Core.Network
{
    public class ClientGamer
    {
        public ClientGamer()
        {
            sema = new Semaphore(0, 1);
            semPause = new Semaphore(0, 1);
            isStopped = false;
        }

        public TcpClient TcpClient { get; set; }
        public Stream DataStream { get; set; }

        Semaphore sema;
        Semaphore semPause;
        bool isStopped;
        public GameDataPacket Receive()
        {
            Trace.Assert(!isStopped);
            if (isStopped) return null;
            var packet = Serializer.DeserializeWithLengthPrefix<GameDataPacket>(DataStream, PrefixStyle.Base128);
            if (packet is EndOfGameNotification)
            {
                isStopped = true;
                DataStream.Close();
                TcpClient.Close();
                return null;
            }
            Trace.TraceInformation("Packet type {0} received", packet.GetType().Name);
            return packet;
        }

        public bool Send(GameDataPacket packet)
        {
            try
            {
                Serializer.SerializeWithLengthPrefix<GameDataPacket>(DataStream, packet, PrefixStyle.Base128);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
