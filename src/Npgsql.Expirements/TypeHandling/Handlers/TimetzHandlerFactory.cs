using Npgsql.Expirements.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class TimetzHandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static Handler<NpgsqlTimetz, Converter>? s_nativeHandler;
        private static Handler<DateTimeOffset, Converter>? s_dateTimeOffsetHandler;

        private const int NativeLength = sizeof(Int64);

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 8 bytes.", nameof(postgresType));

            if (runtimeType == typeof(NpgsqlTimetz))
                return s_nativeHandler ??= new Handler<NpgsqlTimetz, Converter>();

            if (runtimeType == typeof(DateTimeOffset))
                return s_dateTimeOffsetHandler ??= new Handler<DateTimeOffset, Converter>();

            throw new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));
        }

        private sealed class Handler<T, TConverter> : NpgsqlTypeHandler<T>
            where TConverter : struct, INpgsqlValueConverter<Int64, T>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<T>(new TConverter().ToRuntime(buffer.ReadInt16()));

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value) =>
                buffer.WriteInt64(new TConverter().ToNative(value));
        }

        private readonly struct Converter :
            INpgsqlValueConverter<Int64, NpgsqlTimetz>,
            INpgsqlValueConverter<Int64, DateTimeOffset>
        {
            Int64 INpgsqlValueConverter<Int64, NpgsqlTimetz>.ToNative(NpgsqlTimetz value) => value.ToNative();
            NpgsqlTimetz INpgsqlValueConverter<Int64, NpgsqlTimetz>.ToRuntime(Int64 value) => new NpgsqlTimetz(value);

            Int64 INpgsqlValueConverter<Int64, DateTimeOffset>.ToNative(DateTimeOffset value) => new NpgsqlTimetz(value).ToNative();
            DateTimeOffset INpgsqlValueConverter<Int64, DateTimeOffset>.ToRuntime(Int64 value) => new NpgsqlTimetz(value).ToDateTimeOffset();
        }
    }
}
