using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sanguosha.Core.Utils
{
    public class NullOutputStream : Stream
    {
        private Stream stream;

        public Stream InputStream
        {
            get { return stream; }
            set { stream = value; }
        }

        public NullOutputStream(Stream stream)
        {
            this.stream = stream;
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
            get
            {
                return true;
            }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { return InputStream.Length; }
        }

        public override long Position
        {
            get
            {
                return InputStream.Position;
            }
            set
            {
                InputStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = InputStream.Read(buffer, offset, count);
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
        }
    }
}
