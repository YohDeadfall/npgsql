using Npgsql.Expirements.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class TimestamptzHandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static Handler<NpgsqlTimestamptz, Converter>? s_nativeHandler;
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

            if (runtimeType == typeof(NpgsqlTimestamptz))
                return s_nativeHandler ??= new Handler<NpgsqlTimestamptz, Converter>();

            if (runtimeType == typeof(DateTimeOffset))
                return s_dateTimeOffsetHandler ??= new Handler<DateTimeOffset, Converter>();

            throw new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));
        }

        private sealed class Handler<T, TConverter> : NpgsqlTypeHandler<T>
            where TConverter : struct, INpgsqlValueConverter<NpgsqlTimestamptz, T>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<T>(new TConverter().ToRuntime(new NpgsqlTimestamptz(buffer.ReadInt64())));

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value) =>
                buffer.WriteInt64(new TConverter().ToNative(value).ToNative());
        }

        private readonly struct Converter :
            INpgsqlValueConverter<NpgsqlTimestamptz, NpgsqlTimestamptz>,
            INpgsqlValueConverter<NpgsqlTimestamptz, DateTimeOffset>
        {
            NpgsqlTimestamptz INpgsqlValueConverter<NpgsqlTimestamptz, NpgsqlTimestamptz>.ToNative(NpgsqlTimestamptz value) => value;
            NpgsqlTimestamptz INpgsqlValueConverter<NpgsqlTimestamptz, NpgsqlTimestamptz>.ToRuntime(NpgsqlTimestamptz value) => value;

            NpgsqlTimestamptz INpgsqlValueConverter<NpgsqlTimestamptz, DateTimeOffset>.ToNative(DateTimeOffset value) => new NpgsqlTimestamptz(value);
            DateTimeOffset INpgsqlValueConverter<NpgsqlTimestamptz, DateTimeOffset>.ToRuntime(NpgsqlTimestamptz value) => value.ToDateTimeOffset();
        }
    }
}
