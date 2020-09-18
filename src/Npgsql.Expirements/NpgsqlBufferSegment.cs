using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Npgsql.Expirements
{
    internal sealed class NpgsqlBufferSegment : ReadOnlySequenceSegment<byte>
    {
        private byte[]? _array;

        public NpgsqlBufferSegment()
        {
            _array = new byte[4096];

            Memory = _array;
            AvailableMemory = _array;
        }

        public int Length { get; private set; }

        public Memory<byte> AvailableMemory { get; private set; }

        public new NpgsqlBufferSegment? Next
        {
            get => Unsafe.As<NpgsqlBufferSegment>(base.Next);
            set => base.Next = value;
        }

        public void Advance(int size)
        {
            Length += size;
        }

        public void ResetMemory()
        {
        }
    }
}
