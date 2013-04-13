using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using System.Net.Sockets;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Core.Utils;

namespace Sanguosha.Core.Network
{
    public delegate void GamePacketHandler(GameDataPacket request);
    public delegate void GamerDisconnectedHandler(ServerGamer gamer);

    public enum ConnectionStatus
    {
        Connected,
        Disconnected,
    }
    public class ServerGamer
    {
        public ServerGamer()
        {
            sema = new Semaphore(0, Int32.MaxValue);
            semPause = new Semaphore(0, 1);
            semLock = new Semaphore(1, 1);
        }

        public Game Game { get; set; }
        public bool Once { get; set; }
        public TcpClient TcpClient { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
        Stream dataStream;
        public Stream DataStream
        {
            get { return dataStream; }
            set
            {
                if (value != null)
                {
                    dataStream = value;
                    semPause.Release(1);
                }
                if (value == null)
                {
                    dataStream = value;
                    semPause.WaitOne(0);
                }
            }
        }

        Thread listener;

        public void StartListening()
        {
            listener = new Thread(ThreadMain) { IsBackground = true };
            listener.Start();
        }

        public void Stop()
        {
            listener.Abort();
        }
        Semaphore sema;
        Semaphore semPause;
        Semaphore semLock;

        private void ThreadMain()
        {
            Game.RegisterCurrentThread();
            while (true)
            {
                GameDataPacket packet;
                semPause.WaitOne();
                lock (this)
                {
                    packet = null;
                    try
                    {
                        packet = Serializer.DeserializeWithLengthPrefix<GameDataPacket>(DataStream, PrefixStyle.Base128);
                    }
                    catch (IOException)
                    {
                        ConnectionStatus = Network.ConnectionStatus.Disconnected;
                        var handler = OnDisconnected;
                        if (handler != null)
                        {
                            OnDisconnected(this);
                        }
                        return;
                    }
                    catch (Exception e)
                    {
                        Trace.Assert(e != null);
                    }
                    if (packet == null)
                    {
                        semPause.Release(1);
                        continue;
                    }
                }
                semPause.Release(1);
                try
                {
                    if (packet is GameResponse) sema.WaitOne();
                    var handler2 = OnGameDataPacketReceived;
                    if (handler2 != null)
                    {
                        handler2(packet);
                    }
                    if (Once)
                    {
                        Once = false;
                        semPause.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    while (e.InnerException != null)
                    {
                        e = e.InnerException;
                    }

                    Trace.TraceError(e.StackTrace);
                    Trace.Assert(false, e.StackTrace);

                    var crashReport = new StreamWriter(FileRotator.CreateFile("./Crash", "crash", ".dmp", 1000));
                    crashReport.WriteLine(e);
                    crashReport.Close();
                }

            }
        }

        public event GamerDisconnectedHandler OnDisconnected;
        public void Send(GameDataPacket packet)
        {
            semLock.WaitOne();
            Serializer.SerializeWithLengthPrefix<GameDataPacket>(DataStream, packet, PrefixStyle.Base128);
            DataStream.Flush();
            semLock.Release();
        }

        public void Receive()
        {
            sema.Release(1);
        }

        public event GamePacketHandler OnGameDataPacketReceived;

        public void Lock()
        {
            semLock.WaitOne();
        }

        public void Flush()
        {
            if (dataStream != null)
            {
                dataStream.Flush();
            }
        }

        public void Pause()
        {
            semPause.WaitOne();
        }

        public void Resume()
        {
            semPause.Release(1);
        }

        public void Unlock()
        {
            semLock.Release();
        }
    }
}
