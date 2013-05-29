using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

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
                    foreach (var chunk in internalBuffer)
                    {
                        s.Write(chunk, 0, chunk.Length);
                    }
                }
                OutputStreams.Add(s);
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
            lock (OutputStreams)
            {
                if (count > 0)
                {
                    try
                    {
                        byte[] add = new byte[count];
                        Buffer.BlockCopy(buffer, offset, add, 0, count);
                        internalBuffer.Add(add);
                    }
                    catch (Exception)
                    {
                    }
                }

                List<Stream> streamsBroken = new List<Stream>();
                foreach (var stream in OutputStreams)
                {
                    try
                    {
                        stream.Write(buffer, offset, count);
                    }
                    catch (IOException)
                    {
                        streamsBroken.Add(stream);
                    }
                }
                foreach (var stream in streamsBroken)
                {
                    OutputStreams.Remove(stream);
                }
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
    }
}
