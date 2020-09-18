using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class TextHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            if (runtimeType == typeof(ReadOnlyMemory<char>))
                return new Handler<ReadOnlyMemory<char>, Converter, MemoryEnumerator>(connection.Options.Encoding, connection.Options.ReadBufferSize);

            if (runtimeType == typeof(ReadOnlySequence<char>))
                return new Handler<ReadOnlySequence<char>, Converter, SequenceEnumerator>(connection.Options.Encoding, connection.Options.ReadBufferSize);

            if (runtimeType == typeof(String))
                return new Handler<String, Converter, MemoryEnumerator>(connection.Options.Encoding, connection.Options.ReadBufferSize);

            throw new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));
        }

        private sealed class Handler<T, TConverter, TEnumerator> : NpgsqlTypeHandler<T>
            where TConverter : struct, IConverter<T, TEnumerator>
            where TEnumerator : IEnumerator
        {
            private readonly int _bufferSize;
            private readonly Encoding _encoding;
            private Decoder? _decoder;
            private Encoder? _encoder;

            public Handler(Encoding encoding, int bufferSize)
            {
                _encoding = encoding;
                _bufferSize = bufferSize;
            }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader reader, CancellationToken cancellationToken, int length)
            {
                var valueSpan = reader.GetSpan();
                if (valueSpan.Length < length)
                    return ReadValueAsyncLong();

                var value = new TConverter().ToRuntime(_encoding.GetString(valueSpan));
                return new ValueTask<T>(value);

                async ValueTask<T> ReadValueAsyncLong()
                {
                    if (length < _bufferSize)
                    {
                        var bytesLeft = length;
                        var bytes = ArrayPool<byte>.Shared.Rent(length);

                        var bytesAvailable = reader.GetMemory();

                        bytesAvailable.CopyTo(bytes.AsMemory());
                        bytesLeft -= bytesAvailable.Length;

                        await reader.EnsureAsync(bytesLeft, cancellationToken);

                        bytesAvailable.CopyTo(bytes.AsMemory());
                        bytesLeft -= bytesAvailable.Length;

                        Debug.Assert(bytesLeft == 0);

                        return new TConverter().ToRuntime(_encoding.GetString(bytes));
                    }

                    var decoder = _decoder ??= _encoding.GetDecoder();
                    var builder = new StringBuilder();

                    var charsMax = _encoding.GetMaxCharCount(4 * 1024);
                    var chars = ArrayPool<char>.Shared.Rent(charsMax);

                    var byteIndex = 0;
                    while (Decode())
                        await reader.EnsureAsync(0, cancellationToken);

                    ArrayPool<char>.Shared.Return(chars);
                    return new TConverter().ToRuntime(builder);

                    bool Decode()
                    {
                        var charsSpan = chars.AsSpan();
                        var bytesSpan = reader.GetSpan();
                        var flush = length == byteIndex + bytesSpan.Length;

                        decoder.Convert(
                            bytesSpan,
                            charsSpan,
                            flush,
                            out var bytesUsed,
                            out var charsUsed,
                            out var completed);

                        builder.Append(chars, 0, charsUsed);
                        byteIndex += bytesUsed;

                        return byteIndex < length;
                    }
                }
            }

            protected internal override void WriteValue(NpgsqlBufferWriter writer, T value)
            {
                var enumerator = new TConverter().ToNative(value);
                if (enumerator.MoveNext())
                {
                    var encoder = _encoder ??= _encoding.GetEncoder();
                    var bytesMin = _encoding.GetMaxByteCount(1);

                    do
                    {
                        var chars = enumerator.Current.Span;
                        var moveNext = enumerator.MoveNext();

                        while (chars.Length != 0)
                        {
                            var bytes = writer.GetSpan(bytesMin);

                            encoder.Convert(
                                chars,
                                bytes,
                                !moveNext,
                                out var charsUsed,
                                out var bytesUsed,
                                out var completed);

                            writer.Advance(bytesUsed);
                            chars = chars[charsUsed..];
                        }
                    }
                    while (enumerator.MoveNext());
                }
            }
        }

        private interface IConverter<T, TEnumerator>
            where TEnumerator : IEnumerator
        {
            T ToRuntime(String value);
            T ToRuntime(StringBuilder value);

            TEnumerator ToNative(T value);
        }

        private struct Converter :
            IConverter<ReadOnlyMemory<char>, MemoryEnumerator>,
            IConverter<ReadOnlySequence<char>, SequenceEnumerator>,
            IConverter<String, MemoryEnumerator>
        {
            ReadOnlyMemory<char> IConverter<ReadOnlyMemory<char>, MemoryEnumerator>.ToRuntime(string value) => value.AsMemory();
            ReadOnlyMemory<char> IConverter<ReadOnlyMemory<char>, MemoryEnumerator>.ToRuntime(StringBuilder value) => value.ToString().AsMemory();
            MemoryEnumerator IConverter<ReadOnlyMemory<char>, MemoryEnumerator>.ToNative(ReadOnlyMemory<char> value) => new MemoryEnumerator(value);

            ReadOnlySequence<char> IConverter<ReadOnlySequence<char>, SequenceEnumerator>.ToRuntime(string value) => new ReadOnlySequence<char>(value.AsMemory());
            ReadOnlySequence<char> IConverter<ReadOnlySequence<char>, SequenceEnumerator>.ToRuntime(StringBuilder value) => SequenceSegment.CreateSequence(value);
            SequenceEnumerator IConverter<ReadOnlySequence<char>, SequenceEnumerator>.ToNative(ReadOnlySequence<char> value) => new SequenceEnumerator(value);

            string IConverter<String, MemoryEnumerator>.ToRuntime(String value) => value;
            string IConverter<String, MemoryEnumerator>.ToRuntime(StringBuilder value) => value.ToString();
            MemoryEnumerator IConverter<string, MemoryEnumerator>.ToNative(string value) => new MemoryEnumerator(value.AsMemory());
        }

        private interface IEnumerator
        {
            ReadOnlyMemory<char> Current { get; }
            bool MoveNext();
        }

        private struct MemoryEnumerator : IEnumerator
        {
            private bool _moveNext;

            public MemoryEnumerator(ReadOnlyMemory<char> memory) =>
                (Current, _moveNext) = (memory, true);

            public ReadOnlyMemory<char> Current { get; }

            public bool MoveNext()
            {
                var moveNext = _moveNext;
                if (moveNext) _moveNext = false;
                return moveNext;
            }
        }

        private struct SequenceEnumerator : IEnumerator
        {
            private readonly ReadOnlySequence<char>.Enumerator _enumerator;

            public SequenceEnumerator(ReadOnlySequence<char> sequence) =>
                _enumerator = sequence.GetEnumerator();

            public ReadOnlyMemory<char> Current => _enumerator.Current;
            public bool MoveNext() => _enumerator.MoveNext();
        }

        private sealed class SequenceSegment : ReadOnlySequenceSegment<char>
        {
            public static ReadOnlySequence<char> CreateSequence(StringBuilder value)
            {
                var first = default(SequenceSegment);
                var last = default(SequenceSegment);

                foreach (var chunk in value.GetChunks())
                    first ??= last = new SequenceSegment(chunk, last);

                Debug.Assert(first != null);
                Debug.Assert(last != null);

                return new ReadOnlySequence<char>(first, 0, last, last.Memory.Length);
            }

            private SequenceSegment(ReadOnlyMemory<char> memory, SequenceSegment? previous)
            {
                Memory = memory;

                if (previous != null)
                {
                    previous.Next = this;
                    RunningIndex = previous.RunningIndex + previous.Memory.Length;
                }
            }
        }
    }
}
