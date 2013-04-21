using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sanguosha.Core.Utils
{
    public class RecordTakingInputStream : Stream
    {
        private Stream _inputStream;

        public Stream InputStream
        {
            get { return _inputStream; }
            set { _inputStream = value; }
        }
        private Stream _recordStream;

        public Stream RecordStream
        {
            get { return _recordStream; }
            set { _recordStream = value; }
        }

        public RecordTakingInputStream()
        {

        }

        public RecordTakingInputStream(Stream inputStream, Stream recordStream)
        {
            _inputStream = inputStream;
            _recordStream = recordStream;
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
            if (RecordStream != null)
            {
                RecordStream.Flush();
            }
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
            if (RecordStream != null)
            {
                RecordStream.Write(buffer, offset, bytesRead);
                RecordStream.Flush();
            }
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
            InputStream.Write(buffer, offset, count);
        }
    }
}
