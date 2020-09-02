using System;
using System.IO;

namespace PhotoViewer.Model
{
    /// <summary>
    /// Wrapping class corresponding to memory leak of MemoryStream
    /// </summary>
    public class WrappingStream : Stream
    {
        public WrappingStream(Stream _streamBase)
        {
            WrappedStream = _streamBase ?? throw new ArgumentNullException("StreamBase");
        }

        public override bool CanRead
        {
            get { return WrappedStream != null && WrappedStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return WrappedStream != null && WrappedStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return WrappedStream != null && WrappedStream.CanWrite; }
        }

        public override long Length
        {
            get { ThrowIfDisposed(); return WrappedStream.Length; }
        }

        public override long Position
        {
            get { ThrowIfDisposed(); return WrappedStream.Position; }
            set { ThrowIfDisposed(); WrappedStream.Position = value; }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return WrappedStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return WrappedStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            return WrappedStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            WrappedStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            ThrowIfDisposed();
            WrappedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return WrappedStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            ThrowIfDisposed();
            return WrappedStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            return WrappedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ThrowIfDisposed();
            WrappedStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            WrappedStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfDisposed();
            WrappedStream.WriteByte(value);
        }

        protected Stream WrappedStream { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing) WrappedStream = null;
            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (WrappedStream == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}