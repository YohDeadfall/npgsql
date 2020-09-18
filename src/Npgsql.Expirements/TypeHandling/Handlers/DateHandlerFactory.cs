using Npgsql.Expirements.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class DateHandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static Handler<NpgsqlDate, Converter>? s_nativeHandler;
        private static Handler<DateTime, Converter>? s_dateTimeHandler;

        private const int NativeLength = sizeof(Int32);

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 8 bytes.", nameof(postgresType));

            if (runtimeType == typeof(NpgsqlDate))
                return s_nativeHandler ??= new Handler<NpgsqlDate, Converter>();

            if (runtimeType == typeof(DateTimeOffset))
                return s_dateTimeHandler ??= new Handler<DateTime, Converter>();

            throw new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));
        }

        private sealed class Handler<T, TConverter> : NpgsqlTypeHandler<T>
            where TConverter : struct, INpgsqlValueConverter<NpgsqlDate, T>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<T>(new TConverter().ToRuntime(new NpgsqlDate(buffer.ReadInt32())));

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value) =>
                buffer.WriteInt32(new TConverter().ToNative(value).ToNative());
        }

        private readonly struct Converter :
            INpgsqlValueConverter<NpgsqlDate, NpgsqlDate>,
            INpgsqlValueConverter<NpgsqlDate, DateTime>
        {
            NpgsqlDate INpgsqlValueConverter<NpgsqlDate, NpgsqlDate>.ToNative(NpgsqlDate value) => value;
            NpgsqlDate INpgsqlValueConverter<NpgsqlDate, NpgsqlDate>.ToRuntime(NpgsqlDate value) => value;

            NpgsqlDate INpgsqlValueConverter<NpgsqlDate, DateTime>.ToNative(DateTime value) => new NpgsqlDate(value);
            DateTime INpgsqlValueConverter<NpgsqlDate, DateTime>.ToRuntime(NpgsqlDate value) => value.ToDateTime();
        }
    }
}
