using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedVariants.Module {
    /**
     * Taken from https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Helpers/UndisposableStream.cs
     */
    public sealed class UndisposableStream : Stream {
        private readonly Stream inner;
        public UndisposableStream(Stream inner) {
            this.inner = inner;
        }

        public override void Close() {
        }

        protected override void Dispose(bool disposing) {
        }

        #region Redirections to inner

        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => inner.Length;

        public override long Position {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush() {
            inner.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count) {
            return inner.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin) {
            return inner.Seek(offset, origin);
        }
        public override void SetLength(long value) {
            inner.SetLength(value);
        }
        public override void Write(byte[] buffer, int offset, int count) {
            inner.Write(buffer, offset, count);
        }
        public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback callback, object state) {
            return inner.BeginRead(array, offset, count, callback, state);
        }
        public override int EndRead(IAsyncResult asyncResult) {
            return inner.EndRead(asyncResult);
        }
        public override int ReadByte() {
            return inner.ReadByte();
        }
        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback callback, object state) {
            return inner.BeginWrite(array, offset, count, callback, state);
        }
        public override void EndWrite(IAsyncResult asyncResult) {
            inner.EndWrite(asyncResult);
        }
        public override void WriteByte(byte value) {
            inner.WriteByte(value);
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return inner.ReadAsync(buffer, offset, count, cancellationToken);
        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return inner.WriteAsync(buffer, offset, count, cancellationToken);
        }
        public override Task FlushAsync(CancellationToken cancellationToken) {
            return inner.FlushAsync(cancellationToken);
        }

        #endregion
    }
}
