using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class Float8HandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static readonly Handler s_handler = new Handler();

        private const int NativeLength = sizeof(Int64);

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType != typeof(bool))
                throw runtimeType is null
                    ? new ArgumentNullException(nameof(runtimeType))
                    : new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 8 bytes.", nameof(postgresType));

            return s_handler;
        }

        private sealed class Handler : NpgsqlTypeHandler<Double>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<Double> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<Double>(BitConverter.Int64BitsToDouble(buffer.ReadInt64()));

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, Double value) =>
                buffer.WriteInt64(BitConverter.DoubleToInt64Bits(value));
        }
    }
}
