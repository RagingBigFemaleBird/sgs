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
    public delegate void GamerDisconnectedHandler(NetworkGamer gamer);

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
            semPause = new Semaphore(1, 1);
        }

        public TcpClient TcpClient { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
        public Stream DataStream { get; set; }

        public void StartListening()
        {
            var listener = new Thread(ThreadMain) { IsBackground = true };
            listener.Start();
        }

        Semaphore sema;
        Semaphore semPause;

        private void ThreadMain()
        {
            while (true)
            {
                GameDataPacket packet;
                semPause.WaitOne();
                lock (this)
                {
                   packet = Serializer.DeserializeWithLengthPrefix<GameDataPacket>(DataStream, PrefixStyle.Base128);
                   if (packet == null)
                   {
                       var handler = OnDisconnected;
                       if (handler != null)
                       {
                           OnDisconnected(this);
                       }
                       return;
                   }
                }
                semPause.Release(1);
                if (packet is GameResponse) sema.WaitOne();
                var handler2 = OnGameDataPacketReceived;
                if (handler2 != null)
                {
                    handler2(packet);                    
                }
            }
        }

        public event GamerDisconnectedHandler OnDisconnected;
        public void Send(GameDataPacket packet)
        {
            Serializer.SerializeWithLengthPrefix<GameDataPacket>(DataStream, packet, PrefixStyle.Base128);
        }

        public void Receive()
        {
            sema.Release(1);
        }

        public event GamePacketHandler OnGameDataPacketReceived;

        internal void Lock()
        {
            throw new NotImplementedException();
        }

        internal void Flush()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            semPause.WaitOne();
        }

        public void Resume()
        {
            semPause.Release(1);
        }
    }
}
