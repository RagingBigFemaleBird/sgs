using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Sanguosha.Core.Utils
{
    public class ReplaySplitterStream : Stream
    {
        List<Stream> outputStreams;

        public void AddStream(Stream s)
        {
            lock (outputStreams)
            {
                outputStreams.Add(s);
            }
        }

        public ReplaySplitterStream()
        {
            outputStreams = new List<Stream>();
            internalBuffer = new List<byte[]>();
        }

        public override bool CanRead
        {
            get { return false; }
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
            foreach (var stream in outputStreams)
            {
                stream.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
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
            lock (outputStreams)
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
                foreach (var stream in outputStreams)
                {
                    try
                    {
                        stream.Write(buffer, offset, count);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void DumpTo(Stream s)
        {
            foreach (var chunk in internalBuffer)
            {
                s.Write(chunk, 0, chunk.Length);
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
