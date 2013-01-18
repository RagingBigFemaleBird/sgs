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
        private Stream outputStream;

        public Stream OutputStream
        {
            get { return outputStream; }
            set { outputStream = value; }
        }

        public RecordTakingOutputStream(Stream OutputStream)
        {
            outputStream = OutputStream;
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
            OutputStream.Flush();
        }

        public override long Length
        {
            get { return OutputStream.Length; }
        }

        public override long Position
        {
            get
            {
                return OutputStream.Position;
            }
            set
            {
                OutputStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = OutputStream.Read(buffer, offset, count);
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
            try
            {
                OutputStream.Write(buffer, offset, count);
            }
            catch (Exception)
            {
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
    }
}
