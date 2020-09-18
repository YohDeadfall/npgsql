using Npgsql.Expirements.TypeHandling;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements
{
    public sealed class NpgsqlBufferReader
    {
        private readonly Stream _stream;
        private NpgsqlBufferSegment _head;
        private NpgsqlBufferSegment _tail;
        private ReadOnlyMemory<byte> _tailMemory;
        private int _length;

        public NpgsqlBufferReader(Stream stream)
        {
            _stream = stream;
            _head = _tail = new NpgsqlBufferSegment();
        }

        public ReadOnlySpan<byte> GetSpan() => _tailMemory.Span;

        public ReadOnlyMemory<byte> GetMemory() => _tailMemory;

        public Stream AsStream() => throw new NotImplementedException();

        public async ValueTask EnsureAsync(int requiredSize, CancellationToken cancellationToken)
        {
            if (requiredSize < _tailMemory.Length)
                return;

            var availableSize = 0;
            for (var segment = _tail; segment != null; segment = segment.Next)
                if (requiredSize < (availableSize += segment.Memory.Length)) return;

            while (requiredSize > 0)
            {
                if (_tail.AvailableMemory)
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte() => Read<byte>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16(bool littleEndian = false)
        {
            var value = Read<short>();
            return littleEndian == BitConverter.IsLittleEndian
                ? value : BinaryPrimitives.ReverseEndianness(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32(bool littleEndian = false)
        {
            var value = Read<int>();
            return littleEndian == BitConverter.IsLittleEndian
                ? value : BinaryPrimitives.ReverseEndianness(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64(bool littleEndian = false)
        {
            var value = Read<long>();
            return littleEndian == BitConverter.IsLittleEndian
                ? value : BinaryPrimitives.ReverseEndianness(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Read<T>()
            where T : unmanaged
        {
            var requiredSize = Unsafe.SizeOf<T>();
            if (requiredSize > _tailMemory.Length)
                return ReadSlow<T>();

            var result = MemoryMarshal.Read<T>(_tailMemory.Span);

            _length -= requiredSize;
            _tailMemory = _tailMemory.Slice(requiredSize);

            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T ReadSlow<T>()
            where T : unmanaged
        {
            var requiredSize = Unsafe.SizeOf<T>();
            if (requiredSize < _length)
                throw new InvalidOperationException();

            var value = default(T);
            var bytes = MemoryMarshal.AsBytes(
                MemoryMarshal.CreateSpan(ref value, 1));

            while (requiredSize > 0)
            {
                if (_tailMemory.Length == 0)
                {
                    var next = _tail.Next;
                    if (next is null)
                        throw new InvalidOperationException();

                    _tail = next;
                    _tailMemory = _tail.Memory;
                }

                var availableSize = Math.Min(_tailMemory.Length, requiredSize);

                requiredSize -= availableSize;

                var available = _tailMemory.Span[..availableSize];
                var destination = bytes[..requiredSize];

                available.CopyTo(destination);

            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> ReadAsync<T>(NpgsqlTypeHandler<T> handler, CancellationToken cancellationToken)
        {
            if (sizeof(int) + handler.Length > _tailMemory.Length)
            {
                _tail = _tail.Next = new NpgsqlBufferSegment();
                _tail.Advance(await _stream.ReadAsync(_tail.AvailableMemory, cancellationToken));
            }

            var previousLength = _length;
            var currentLength = _length = ReadInt32();

            try
            {
                return await handler.ReadValueAsync(this, cancellationToken, currentLength);
            }
            finally
            {
                _length = previousLength;
            }
        }
    }
}
