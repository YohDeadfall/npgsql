using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling
{
    public abstract class NpgsqlTypeHandler<T> : NpgsqlTypeHandler
    {
        internal int Length { get; }

        protected NpgsqlTypeHandler() => Length = 0;

        protected NpgsqlTypeHandler(int length) => Length = length > 0 ? length : throw new ArgumentOutOfRangeException();

        public ValueTask<T> ReadAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken) => buffer.ReadAsync<T>(this, cancellationToken);

        public void Write(NpgsqlBufferWriter buffer, T value) => buffer.Write(this, value);

        protected internal abstract ValueTask<T> ReadValueAsync(NpgsqlBufferReader reader, CancellationToken cancellationToken, int length);

        protected internal abstract void WriteValue(NpgsqlBufferWriter writer, T value);

        internal sealed override async ValueTask<object?> ReadObjectAsync(NpgsqlBufferReader reader, CancellationToken cancellationToken) =>
            await ReadAsync(reader, cancellationToken);

        internal sealed override void WriteParameter(NpgsqlBufferWriter writer, NpgsqlParameter parameter)
        {
            var value = parameter is NpgsqlParameter<T> typedParameter
                ? typedParameter.Value
                : (T)parameter.Value!;

            Write(writer, value);
        }

        internal override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlTypeHandlerFactory factory) =>
            factory.CreateHandler(runtimeType, postgresType, this);
    }
}
