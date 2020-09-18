using Npgsql.Expirements.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class IntervalHandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static Handler<NpgsqlInterval, Converter>? s_nativeHandler;
        private static Handler<TimeSpan, Converter>? s_timeSpanHandler;

        private const int NativeLength = sizeof(Int64) + sizeof(Int32) * 2;

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 8 bytes.", nameof(postgresType));

            if (runtimeType == typeof(NpgsqlInterval))
                return s_nativeHandler ??= new Handler<NpgsqlInterval, Converter>();

            if (runtimeType == typeof(TimeSpan))
                return s_timeSpanHandler ??= new Handler<TimeSpan, Converter>();

            throw new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));
        }

        private sealed class Handler<T, TConverter> : NpgsqlTypeHandler<T>
            where TConverter : struct, INpgsqlValueConverter<NpgsqlInterval, T>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                var native = new NpgsqlInterval(
                    buffer.ReadInt64(),
                    buffer.ReadInt32(),
                    buffer.ReadInt32());
                return new ValueTask<T>(new TConverter().ToRuntime(native));
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value)
            {
                var native = new TConverter().ToNative(value);
                buffer.WriteInt64(native.Ticks);
                buffer.WriteInt32(native.Days);
                buffer.WriteInt32(native.Months);
            }
        }

        private readonly struct Converter :
            INpgsqlValueConverter<NpgsqlInterval, NpgsqlInterval>,
            INpgsqlValueConverter<NpgsqlInterval, TimeSpan>
        {
            NpgsqlInterval INpgsqlValueConverter<NpgsqlInterval, NpgsqlInterval>.ToNative(NpgsqlInterval value) => value;
            NpgsqlInterval INpgsqlValueConverter<NpgsqlInterval, NpgsqlInterval>.ToRuntime(NpgsqlInterval value) => value;

            NpgsqlInterval INpgsqlValueConverter<NpgsqlInterval, TimeSpan>.ToNative(TimeSpan value) => new NpgsqlInterval(value);
            TimeSpan INpgsqlValueConverter<NpgsqlInterval, TimeSpan>.ToRuntime(NpgsqlInterval value) => value.ToTimeSpan();
        }
    }
}
