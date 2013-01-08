using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Sanguosha.Core.Utils
{
    public class TimeStampedFilteringStream : Stream
    {
        private Stream _inputStream;

        public Stream InputStream
        {
            get { return _inputStream; }
            set { _inputStream = value; }
        }

        public TimeStampedFilteringStream(Stream inputStream)
        {
            lastEpoch = 0;
            _inputStream = inputStream;
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
                return false;
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
        int lastEpoch;

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] ts = new byte[4];
            InputStream.Read(ts, 0, 4);
            if (lastEpoch != 0)
            {
                int toSleep = BitConverter.ToInt32(ts, 0) - lastEpoch;
                Thread.Sleep(toSleep * 1000);
            }
            lastEpoch = BitConverter.ToInt32(ts, 0);
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
            throw new NotImplementedException();
        }
    }
}
