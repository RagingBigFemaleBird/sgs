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
                    foreach (var chunk in internalBuffer)
                    {
                        Trace.TraceInformation("AddStream() : Writing chunk for {0}", RuntimeHelpers.GetHashCode(this));
                        s.Write(chunk, 0, chunk.Length);
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
            lock (OutputStreams)
            {
                foreach (var stream in OutputStreams)
                {
                    stream.Flush();
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Stream inputStream;
            lock (OutputStreams)
            {
                if (OutputStreams.Count == 0) return 0;
                inputStream = OutputStreams[0];
            }
            try
            {
                int bytesRead = inputStream.Read(buffer, offset, count);
                Trace.TraceInformation("Read() : {0} bytes read for {1}", count, RuntimeHelpers.GetHashCode(this));
                return bytesRead;
            }
            catch (Exception e)
            {
                lock (OutputStreams)
                {
                    OutputStreams.Remove(inputStream);
                }
                throw e;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        private bool _isLastWriteSuccessful;

        public bool IsLastWriteSuccessful
        {
            get
            {
                lock (OutputStreams)
                {
                    return _isLastWriteSuccessful;
                }
            }
            private set
            {
                _isLastWriteSuccessful = value;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (OutputStreams)
            {
                IsLastWriteSuccessful = true;
                if (count > 0 && IsRecordEnabled)
                {
                    byte[] add = new byte[count];
                    Buffer.BlockCopy(buffer, offset, add, 0, count);
                    Trace.TraceInformation("Write() : add chunk for {0}", RuntimeHelpers.GetHashCode(this));
                    internalBuffer.Add(add);
                }                
                List<Stream> streamsBroken = new List<Stream>();
                foreach (var stream in OutputStreams)
                {
                    try
                    {
                        Trace.TraceInformation("Write() : write data for {0}", RuntimeHelpers.GetHashCode(this));
                        stream.Write(buffer, offset, count);
                    }
                    catch (IOException)
                    {
                        IsLastWriteSuccessful = false;
                        streamsBroken.Add(stream);
                    }
                }
                foreach (var stream in streamsBroken)
                {
                    OutputStreams.Remove(stream);
                    Trace.TraceInformation("AddStream() : Remove stream for {0}", RuntimeHelpers.GetHashCode(this));
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

        public bool IsRecordEnabled { get; set; }
    }
}
