using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class BooleanHandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static readonly Handler s_handler = new Handler();

        private const int NativeLength = sizeof(Byte);

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType != typeof(bool))
                throw runtimeType is null
                    ? new ArgumentNullException(nameof(runtimeType))
                    : new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 1 bytes.", nameof(postgresType));

            return s_handler;
        }

        private sealed class Handler : NpgsqlTypeHandler<bool>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<bool> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<bool>(buffer.ReadByte() != 0);

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, bool value) =>
                buffer.WriteByte((byte)(value ? 1 : 0));
        }
    }
}
