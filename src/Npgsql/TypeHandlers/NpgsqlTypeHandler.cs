using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers
{
    internal abstract class NpgsqlTypeHandler
    {
        internal NpgsqlTypeHandler()
        {
        }

        internal abstract NpgsqlTypeHandler CreateArrayHandler(NpgsqlArrayHandlerFactory factory);

        internal abstract NpgsqlTypeHandler CreateRangeHandler(NpgsqlRangeHandlerFactory factory);
    }

    internal sealed class NpgsqlStreamWriter
    {
        private Stream _underlyingStream;
        private NpgsqlMemoryChunk _head;
        private NpgsqlMemoryChunk _tail;
        private int _length;

        internal NpgsqlStreamWriter(Stream underlyingStream)
        {
            _underlyingStream = underlyingStream;
            _head = new NpgsqlMemoryChunk();
            _tail = _head;
        }

        internal void Write<T>(T value, NpgsqlTypeHandler<T> handler)
        {
            if (value is null || value is DBNull)
                WriteInt32(-1);
            else
            {
                EnsureWriteSpace(sizeof(int));

                var length = _length += sizeof(int);
                var lengthSpace = _head.Unused;

                _head.Advance(sizeof(int));

                handler.Write(value, this);
                BinaryPrimitives.WriteInt32BigEndian(lengthSpace, _length - length);
            }
        }

        public Span<byte> GetSpan() => _head.Unused;

        public Span<byte> GetSpan(int size) => _head.Unused;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value) =>
            Write(value);

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
            EnsureWriteSpace(Unsafe.SizeOf<T>());
            MemoryMarshal.Write(_head.Unused, ref value);

            _head.Advance(Unsafe.SizeOf<T>());
            _length += Unsafe.SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureWriteSpace(int requiredSize)
        {
            if (requiredSize > _head.UnusedLength)
                Allocate();

            void Allocate()
            {
                _head.Next = new NpgsqlMemoryChunk();
                _head = _head.Next;
            }
        }

        internal async ValueTask Flush(CancellationToken cancellationToken)
        {
            for (var current = _tail; current.Next != null; current = current.Next)
                await _underlyingStream.WriteAsync(current.UsedMemory, cancellationToken);
        }
    }

    internal sealed class NpgsqlMemoryChunk
    {
        public NpgsqlMemoryChunk? Next;
        private readonly byte[] _buffer;

        public NpgsqlMemoryChunk() =>
            _buffer = new byte[Environment.SystemPageSize];

        public Span<byte> Unused => new Span<byte>(_buffer, UsedLength, UnusedLength);
        public Memory<byte> UnusedMemory => new Memory<byte>(_buffer, UsedLength, UnusedLength);
        public int UnusedLength => _buffer.Length - UsedLength;

        public Span<byte> Used => new Span<byte>(_buffer, 0, UsedLength);
        public Memory<byte> UsedMemory => new Memory<byte>(_buffer, 0, UsedLength);
        public int UsedLength { get; private set; }

        public void Advance(int size) => UsedLength += size;
    }

    internal class NpgsqlStreamReader
    {
         
    }
}
