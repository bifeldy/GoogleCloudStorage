using System;
using System.IO;

namespace GoogleCloudStorage.Models {

    public class CForwardOnlySeekableStreamWrapper : Stream {

        private readonly Stream _baseStream;
        private long _position;
        private readonly long _length;

        public CForwardOnlySeekableStreamWrapper(Stream baseStream, long startPosition, long totalLength) {
            this._baseStream = baseStream;
            this._position = startPosition;
            this._length = totalLength;
        }

        public override bool CanRead => this._baseStream.CanRead;

        // MENGELABUI GOOGLE SDK
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        // MEMBERI TAHU GOOGLE TOTAL UKURAN FILE ASLI
        public override long Length => this._length;

        public override long Position {
            get => this._position;
            set {
                if (value < this._position) {
                    // Google meminta mundur (karena ada potongan yang gagal masuk server GCS).
                    // Selang S3 (NetworkStream) tidak bisa mundur. Kita lempar kode khusus ke UI!
                    throw new InvalidOperationException($"REWIND_REQUIRED:{value}");
                }

                this._position = value;
            }
        }

        public override void Flush() => this._baseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) {
            int bytesRead = this._baseStream.Read(buffer, offset, count);
            this._position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            long newPos = this._position;
            if (origin == SeekOrigin.Begin) {
                newPos = offset;
            }
            else if (origin == SeekOrigin.Current) {
                newPos += offset;
            }
            else if (origin == SeekOrigin.End) {
                newPos = this._length + offset;
            }

            this.Position = newPos; // Akan trigger exception jika mundur
            return this._position;
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing) {
            if (disposing) {
                this._baseStream.Dispose();
            }

            base.Dispose(disposing);
        }

    }
}