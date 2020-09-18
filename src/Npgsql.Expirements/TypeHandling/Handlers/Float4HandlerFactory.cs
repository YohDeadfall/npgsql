using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class Float4HandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static readonly Handler s_handler = new Handler();

        private const int NativeLength = sizeof(Int32);

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType != typeof(bool))
                throw runtimeType is null
                    ? new ArgumentNullException(nameof(runtimeType))
                    : new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 4 bytes.", nameof(postgresType));

            return s_handler;
        }

        private sealed class Handler : NpgsqlTypeHandler<Single>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<Single> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<Single>(BitConverter.Int32BitsToSingle(buffer.ReadInt32()));

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, Single value) =>
                buffer.WriteInt32(BitConverter.SingleToInt32Bits(value));
        }
    }
}
