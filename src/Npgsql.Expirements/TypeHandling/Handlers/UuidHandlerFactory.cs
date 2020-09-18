using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class UuidHandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static readonly Handler s_guidHandler = new Handler();

        private const int NativeLength = 16;

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType != typeof(Guid))
                throw runtimeType is null
                    ? new ArgumentNullException(nameof(runtimeType))
                    : new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 16 bytes.", nameof(postgresType));

            return s_guidHandler;
        }

        private sealed class Handler : NpgsqlTypeHandler<Guid>
        {
            // The following table shows .NET GUID vs Postgres UUID (RFC 4122) layouts.
            //
            // Note that the first fields are converted from/to native endianness (handled by the Read*
            // and Write* methods), while the last field is always read/written in big-endian format.
            //
            // We're passing BitConverter.IsLittleEndian to prevent reversing endianness on little-endian systems.
            //
            // | Bits | Bytes | Name  | Endianness (GUID) | Endianness (RFC 4122) |
            // | ---- | ----- | ----- | ----------------- | --------------------- |
            // | 32   | 4     | Data1 | Native            | Big                   |
            // | 16   | 2     | Data2 | Native            | Big                   |
            // | 16   | 2     | Data3 | Native            | Big                   |
            // | 64   | 8     | Data4 | Big               | Big                   |

            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<Guid> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                var raw = new GuidRaw
                {
                    Data1 = buffer.ReadInt32(),
                    Data2 = buffer.ReadInt16(),
                    Data3 = buffer.ReadInt16(),
                    Data4 = buffer.ReadInt64(BitConverter.IsLittleEndian)
                };

                return new ValueTask<Guid>(raw.Value);
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, Guid value)
            {
                var raw = new GuidRaw(value);

                buffer.WriteInt32(raw.Data1);
                buffer.WriteInt16(raw.Data2);
                buffer.WriteInt16(raw.Data3);
                buffer.WriteInt64(raw.Data4, BitConverter.IsLittleEndian);
            }
        }
    }
}
