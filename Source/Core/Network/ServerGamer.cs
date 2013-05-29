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
            semaPakArrival = new Semaphore(0, Int32.MaxValue);
            receiverLock = new object();
            senderLock = new object();
            DataStream = new RecordTakingOutputStream();
        }
        
        Thread listener;
        Semaphore semaPakArrival;
        object receiverLock;
        object senderLock;

        public Game Game { get; set; }
        public TcpClient TcpClient { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }

        public RecordTakingOutputStream DataStream
        {
            get;
            private set;
        }

        public void StartListening()
        {
            if (listener != null) return;
            listener = new Thread(ReceiveLoop) { IsBackground = true };
            listener.Start();
        }

        public void StopListening()
        {
            if (listener == null) return;
            listener.Abort();
            listener = null;
        }

        public GameDataPacket Receive()
        {
            GameDataPacket packet;            
            lock (this)
            {
                lock (receiverLock)
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
                        return null;
                    }
                    catch (Exception e)
                    {
                        Trace.Assert(e != null);
                    }
                }
            }            
            return packet;
        }
                

        private void ReceiveLoop()
        {
            Game.RegisterCurrentThread();
            while (true)
            {
                GameDataPacket packet = Receive();
                if (packet == null) break;
                try
                {
                    if (packet is GameResponse) semaPakArrival.WaitOne();
                    var handler2 = OnGameDataPacketReceived;
                    if (handler2 != null)
                    {
                        handler2(packet);
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
                    break;
                }
            }
            listener = null;
        }

        public event GamerDisconnectedHandler OnDisconnected;
        public void Send(GameDataPacket packet)
        {
            try
            {
                lock (senderLock)
                {
                    Serializer.SerializeWithLengthPrefix<GameDataPacket>(DataStream, packet, PrefixStyle.Base128);
                    DataStream.Flush();
                }
            }
            catch (Exception)
            {
                ConnectionStatus = Network.ConnectionStatus.Disconnected;
                var handler = OnDisconnected;
                if (handler != null)
                {
                    try
                    {
                        OnDisconnected(this);
                    }
                    catch (Exception) { }
                }
            }
        }

        public void ReceiveAsync()
        {
            semaPakArrival.Release();
        }

        public event GamePacketHandler OnGameDataPacketReceived;

        public void Flush()
        {
            if (DataStream != null)
            {
                DataStream.Flush();
            }
        }
        
        public void AddStream(Stream newStream)
        {
            if (DataStream == null)
            {
                DataStream = new RecordTakingOutputStream();
                DataStream.AddStream(newStream, false);
                return;
            }
            lock (receiverLock)
            {
                lock (senderLock)
                {                    
                    try
                    {
                        var uiDetach = new UIStatusHint() { IsDetached = true };
                        var uiAttach = new UIStatusHint() { IsDetached = false };
                        Serializer.SerializeWithLengthPrefix<GameDataPacket>(newStream, uiDetach, PrefixStyle.Base128);
                        DataStream.Flush();
                        DataStream.AddStream(newStream, true);
                        Serializer.SerializeWithLengthPrefix<GameDataPacket>(newStream, uiAttach, PrefixStyle.Base128);
                        DataStream.Flush();

                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }        
    }
}
