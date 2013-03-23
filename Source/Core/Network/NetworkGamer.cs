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
    public delegate void GamePacketHandler(GameDataPacket request);

    public enum ConnectionStatus
    {
        Connected,
        Disconnected,
    }
    public class NetworkGamer
    {
        public NetworkGamer()
        {
            sema = new Semaphore(0, 1);
        }

        public TcpClient TcpClient { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
        public Stream DataStream { get; set; }

        public void StartListening()
        {
            
        }

        Semaphore sema;

        private void ThreadMain()
        {
            while (true)
            {
                GameDataPacket packet = Serializer.DeserializeWithLengthPrefix<GameDataPacket>(DataStream, PrefixStyle.Base128);
                if (packet is GameResponse) sema.WaitOne();
                var handler = OnGameDataPacketReceived;
                if (handler != null)
                {
                    handler(packet);
                }
            }
        }
                
        public void Send(GameDataPacket packet)
        {
            Serializer.SerializeWithLengthPrefix<GameDataPacket>(DataStream, packet, PrefixStyle.Base128);      
        }

        public void Receive()
        {
            sema.Release(1);
        }

        public event GamePacketHandler OnGameDataPacketReceived;
    }
}
