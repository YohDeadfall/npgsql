using Npgsql.Expirements.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class RangeHandlerFactory<T> : NpgsqlTypeHandlerFactory
        where T : IEquatable<T>, IComparable<T>
    {
        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            if (runtimeType == typeof(T))
                return new Handler((NpgsqlTypeHandler<T>)connection.TypeMapper.GetHandler(runtimeType));

            throw new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));
        }

        private sealed class Handler : NpgsqlTypeHandler<NpgsqlRange<T>>
        {
            public NpgsqlTypeHandler<T> ElementHandler { get; }

            public Handler(NpgsqlTypeHandler<T> elementHandler) => ElementHandler = elementHandler;

            protected internal override async ValueTask<NpgsqlRange<T>> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                var flags = (NpgsqlRangeFlags)buffer.ReadByte();
                if (flags == NpgsqlRangeFlags.Empty)
                    return default;

                var lowerBound = flags.HasFlag(NpgsqlRangeFlags.LowerBoundInfinite)
                    ? default
                    : await ElementHandler.ReadAsync(buffer, cancellationToken);

                var upperBound = flags.HasFlag(NpgsqlRangeFlags.UpperBoundInfinite)
                    ? default
                    : await ElementHandler.ReadAsync(buffer, cancellationToken);

                return new NpgsqlRange<T>(lowerBound, upperBound, flags);
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, NpgsqlRange<T> value)
            {
                if (value.IsEmpty)
                    buffer.WriteByte((byte)NpgsqlRangeFlags.Empty);
                else
                {
                    var flags = value.Flags;

                    buffer.WriteByte((byte)flags);

                    if (!flags.HasFlag(NpgsqlRangeFlags.LowerBoundInfinite))
                        ElementHandler.Write(buffer, value.LowerBoundInternal);

                    if (!flags.HasFlag(NpgsqlRangeFlags.UpperBoundInfinite))
                        ElementHandler.Write(buffer, value.UpperBoundInternal);
                }
            }
        }
    }
}
