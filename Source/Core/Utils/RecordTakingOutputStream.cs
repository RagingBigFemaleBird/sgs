using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sanguosha.Core.Utils
{
    public class RecordTakingOutputStream : Stream
    {
        public List<Stream> OutputStreams
        {
            get;
            private set;
        }

        public void AddStream(Stream s, bool writeExisingData)
        {
            lock (OutputStreams)
            {
                if (writeExisingData)
                {
                    lock (internalBuffer)
                    {
                        foreach (var chunk in internalBuffer)
                        {
                            Trace.TraceInformation("AddStream() : Writing chunk for {0}", RuntimeHelpers.GetHashCode(this));
                            s.Write(chunk, 0, chunk.Length);
                        }
                    }
                }
                OutputStreams.Add(s);
                Trace.TraceInformation("AddStream() : Add stream for {0}", RuntimeHelpers.GetHashCode(this));
            }
        }

        public RecordTakingOutputStream()
        {
            OutputStreams = new List<Stream>();
            internalBuffer = new List<byte[]>();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            foreach (var stream in OutputStreams)
            {
                stream.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (OutputStreams.Count == 0) return 0;
            int bytesRead = OutputStreams[0].Read(buffer, offset, count);
            Trace.TraceInformation("Read() : {0} bytes read for {1}", count, RuntimeHelpers.GetHashCode(this));
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > 0 && IsRecordEnabled)
            {
                byte[] add = new byte[count];
                Buffer.BlockCopy(buffer, offset, add, 0, count);
                lock (internalBuffer)
                {
                    Trace.TraceInformation("Write() : add chunk for {0}", RuntimeHelpers.GetHashCode(this));
                    internalBuffer.Add(add);
                }
            }
            IOException ex = null;
            List<Stream> streamsBroken = new List<Stream>();
            lock (OutputStreams)
            {
                foreach (var stream in OutputStreams)
                {
                    try
                    {
                        Trace.TraceInformation("Write() : write data for {0}", RuntimeHelpers.GetHashCode(this));
                        stream.Write(buffer, offset, count);
                    }
                    catch (IOException e)
                    {
                        ex = e;
                        streamsBroken.Add(stream);
                    }
                }
                foreach (var stream in streamsBroken)
                {
                    OutputStreams.Remove(stream);
                    Trace.TraceInformation("AddStream() : Remove stream for {0}", RuntimeHelpers.GetHashCode(this));
                }                
            }
            if (ex != null)
            {
                Trace.TraceInformation("AddStream() : IOException, disconnected.");
                throw ex;
            }
        }
        
        List<byte[]> internalBuffer;

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsRecordEnabled { get; set; }
    }
}
