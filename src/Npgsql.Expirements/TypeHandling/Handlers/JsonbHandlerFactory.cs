using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class JsonbHandlerFactory<T> : NpgsqlTypeHandlerFactory
    {
        private readonly JsonSerializerOptions? _options;

        public JsonbHandlerFactory(JsonSerializerOptions? options = null) => _options = options;

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection) =>
            new Handler(_options);

        private sealed class Handler : NpgsqlTypeHandler<T>
        {
            private readonly JsonSerializerOptions? _options;
            private const byte ProtocolVersion1 = 1;

            public Handler(JsonSerializerOptions? options) => _options = options;

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                var protocolVersion = buffer.ReadByte();
                if (protocolVersion != ProtocolVersion1)
                    throw new InvalidOperationException();

                return JsonSerializer.DeserializeAsync<T>(buffer.GetStream(), _options);
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value)
            {
                buffer.WriteByte(ProtocolVersion1);
                JsonSerializer.Serialize(new Utf8JsonWriter(buffer), _options);
            }
        }
    }
}
