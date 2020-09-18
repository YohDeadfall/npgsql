using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class JsonHandlerFactory<T> : NpgsqlTypeHandlerFactory
    {
        private readonly JsonSerializerOptions? _options;

        public JsonHandlerFactory(JsonSerializerOptions? options = null) => _options = options;

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection) =>
            new Handler(_options);

        private sealed class Handler : NpgsqlTypeHandler<T>
        {
            private readonly JsonSerializerOptions? _options;

            public Handler(JsonSerializerOptions? options) => _options = options;

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                JsonSerializer.DeserializeAsync<T>(buffer.GetStream(), _options);

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value) =>
                JsonSerializer.Serialize(new Utf8JsonWriter(buffer), _options);
        }
    }
}
