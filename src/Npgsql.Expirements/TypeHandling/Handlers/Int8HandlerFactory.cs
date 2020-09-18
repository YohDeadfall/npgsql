using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class Int8HandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static Handler<Byte, Converter>? s_byteHandler;
        private static Handler<Int16, Converter>? s_int16Handler;
        private static Handler<Int32, Converter>? s_int32Handler;
        private static Handler<Int64, Converter>? s_int64Handler;
        private static Handler<Single, Converter>? s_singleHandler;
        private static Handler<Double, Converter>? s_doubleHandler;
        private static Handler<Decimal, Converter>? s_decimalHandler;

        private const int NativeLength = sizeof(Int64);

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 8 bytes.", nameof(postgresType));

            if (runtimeType == typeof(Byte))
                return s_byteHandler ??= new Handler<Byte, Converter>();

            if (runtimeType == typeof(Int16))
                return s_int16Handler ??= new Handler<Int16, Converter>();

            if (runtimeType == typeof(Int32))
                return s_int32Handler ??= new Handler<Int32, Converter>();

            if (runtimeType == typeof(Int64))
                return s_int64Handler ??= new Handler<Int64, Converter>();

            if (runtimeType == typeof(Single))
                return s_singleHandler ??= new Handler<Single, Converter>();

            if (runtimeType == typeof(Double))
                return s_doubleHandler ??= new Handler<Double, Converter>();

            if (runtimeType == typeof(Decimal))
                return s_decimalHandler ??= new Handler<Decimal, Converter>();

            throw new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));
        }

        private sealed class Handler<T, TConverter> : NpgsqlTypeHandler<T>
            where TConverter : struct, INpgsqlValueConverter<Int64, T>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<T>(new TConverter().ToRuntime(buffer.ReadInt64()));

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value) =>
                buffer.WriteInt64(new TConverter().ToNative(value));
        }

        private readonly struct Converter :
            INpgsqlValueConverter<Int64, Byte>,
            INpgsqlValueConverter<Int64, Int16>,
            INpgsqlValueConverter<Int64, Int32>,
            INpgsqlValueConverter<Int64, Int64>,
            INpgsqlValueConverter<Int64, Single>,
            INpgsqlValueConverter<Int64, Double>,
            INpgsqlValueConverter<Int64, Decimal>
        {
            Int64 INpgsqlValueConverter<Int64, Byte>.ToNative(Byte value) => value;
            Byte INpgsqlValueConverter<Int64, Byte>.ToRuntime(Int64 value) => checked((Byte)value);

            Int64 INpgsqlValueConverter<Int64, Int16>.ToNative(Int16 value) => value;
            Int16 INpgsqlValueConverter<Int64, Int16>.ToRuntime(Int64 value) => checked((Int16)value);

            Int64 INpgsqlValueConverter<Int64, Int32>.ToNative(Int32 value) => value;
            Int32 INpgsqlValueConverter<Int64, Int32>.ToRuntime(Int64 value) => checked((Int32)value);

            Int64 INpgsqlValueConverter<Int64, Int64>.ToNative(Int64 value) => value;
            Int64 INpgsqlValueConverter<Int64, Int64>.ToRuntime(Int64 value) => value;

            Int64 INpgsqlValueConverter<Int64, Single>.ToNative(Single value) => (Int64)value;
            Single INpgsqlValueConverter<Int64, Single>.ToRuntime(Int64 value) => value;

            Int64 INpgsqlValueConverter<Int64, Double>.ToNative(Double value) => (Int64)value;
            Double INpgsqlValueConverter<Int64, Double>.ToRuntime(Int64 value) => value;

            Int64 INpgsqlValueConverter<Int64, Decimal>.ToNative(Decimal value) => (Int64)value;
            Decimal INpgsqlValueConverter<Int64, Decimal>.ToRuntime(Int64 value) => value;
        }
    }
}
