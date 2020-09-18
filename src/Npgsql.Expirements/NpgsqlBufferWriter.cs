using Npgsql.Expirements.TypeHandling;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements
{
    public sealed class NpgsqlBufferWriter : IBufferWriter<byte>
    {
        private NpgsqlBufferSegment _head;
        private NpgsqlBufferSegment _tail;
        private Memory<byte> _tailMemory;
        private int _tailBytesBuffered;
        private int _length;

        public NpgsqlBufferWriter()
        {
            _head = _tail = new NpgsqlBufferSegment();
        }

        public Span<byte> GetSpan(int sizeHint = 0) => _tail.AvailableMemory.Span;

        public Memory<byte> GetMemory(int sizeHint = 0) => _tail.AvailableMemory;

        public Stream AsStream() => throw new NotImplementedException();

        public void Advance(int count)
        {
            _tail.Advance(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value) => Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value, bool littleEndian = false) =>
            Write(littleEndian == BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value, bool littleEndian = false) =>
            Write(littleEndian == BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value, bool littleEndian = false) =>
            Write(littleEndian == BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write<T>(T value)
            where T : unmanaged
        {
            var requiredSize = Unsafe.SizeOf<T>();

            EnsureMemory(requiredSize);
            MemoryMarshal.Write(_tailMemory.Span, ref value);

            _length += requiredSize;
            _tailBytesBuffered += requiredSize;
            _tailMemory = _tailMemory.Slice(requiredSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureMemory(int requiredSize)
        {
            if (requiredSize > _tailMemory.Length)
                Allocate();

            void Allocate()
            {
                if (_tailBytesBuffered > 0)
                {
                    _tail.Advance(_tailBytesBuffered);
                    _tailBytesBuffered = 0;
                }

                _tail = _tail.Next = new NpgsqlBufferSegment();
                _tailMemory = _tail.AvailableMemory;
            }
        }

        internal void Write<T>(NpgsqlTypeHandler<T> handler, T value)
        {
            if (value is null || value is DBNull)
                WriteInt32(-1);
            else
            {
                EnsureMemory(sizeof(int));

                var length = _length += sizeof(int);
                var lengthSpace = _tailMemory;

                _tail.Advance(sizeof(int));

                handler.WriteValue(this, value);
                BinaryPrimitives.WriteInt32BigEndian(lengthSpace.Span, _length - length);
            }
        }

        internal async ValueTask FlushAsync(Stream stream, CancellationToken cancellationToken)
        {
            for (var segment = _head; segment != null; segment = segment.Next)
            {
                if (segment.Length > 0)
                    await stream.WriteAsync(segment.Memory, cancellationToken).ConfigureAwait(false);

                segment.ResetMemory();
                // ReturnSegmentUnsynchronized(returnSegment);
            }

            // Mark bytes as written *after* flushing
            _head = _tail;
        }
    }
}
