using System;
using System.IO;

namespace PhotoViewer.Model
{
    /// <summary>
    /// MemoryStreamのメモリリークに対応したWrappingクラス
    /// </summary>
    public class WrappingStream : Stream
    {
        Stream mStreamBase;

        public WrappingStream(Stream _streamBase)
        {
            if (_streamBase == null)
            {
                throw new ArgumentNullException("StreamBase");
            }

            mStreamBase = _streamBase;
        }

        public override bool CanRead
        {
            get { return mStreamBase == null ? false : mStreamBase.CanRead; }
        }

        public override bool CanSeek
        {
            get { return mStreamBase == null ? false : mStreamBase.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return mStreamBase == null ? false : mStreamBase.CanWrite; }
        }

        public override long Length
        {
            get { ThrowIfDisposed(); return mStreamBase.Length; }
        }

        public override long Position
        {
            get { ThrowIfDisposed(); return mStreamBase.Position; }
            set { ThrowIfDisposed(); mStreamBase.Position = value; }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return mStreamBase.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return mStreamBase.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            return mStreamBase.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            mStreamBase.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            ThrowIfDisposed();
            mStreamBase.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return mStreamBase.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            ThrowIfDisposed();
            return mStreamBase.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            return mStreamBase.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ThrowIfDisposed();
            mStreamBase.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            mStreamBase.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfDisposed();
            mStreamBase.WriteByte(value);
        }

        protected Stream WrappedStream
        {
            get { return mStreamBase; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) mStreamBase = null;
            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (mStreamBase == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}