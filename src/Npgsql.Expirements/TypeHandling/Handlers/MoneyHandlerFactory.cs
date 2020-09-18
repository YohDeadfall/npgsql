using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class MoneyHandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static Handler? s_handlerWithPrecision2;
        private static Handler? s_handlerWithPrecision4;
        private readonly int _precision;

        private const int NativeLength = sizeof(long);

        public MoneyHandlerFactory(int precision) =>
            _precision = precision >= 0 && precision <= 19 ? precision : throw new ArgumentOutOfRangeException();

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType != typeof(decimal))
                throw runtimeType is null
                    ? new ArgumentNullException(nameof(runtimeType))
                    : new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 2 bytes.", nameof(postgresType));

            return _precision switch
            {
                2 => s_handlerWithPrecision2 ??= new Handler(2),
                4 => s_handlerWithPrecision4 ??= new Handler(4),
                _ => new Handler(_precision)
            };
        }

        private sealed class Handler : NpgsqlTypeHandler<decimal>
        {
            private readonly int _precision;

            public Handler(int precision) : base(NativeLength) => _precision = precision;

            protected internal override ValueTask<decimal> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<decimal>(new DecimalRaw(buffer.ReadInt64()) { Scale = _precision }.Value);

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, decimal value)
            {
                var raw = new DecimalRaw(value);

                var scaleDifference = _precision - raw.Scale;
                if (scaleDifference > 0)
                    DecimalRaw.Multiply(ref raw, DecimalRaw.Powers10[scaleDifference]);
                else
                {
                    value = Math.Round(value, _precision, MidpointRounding.AwayFromZero);
                    raw = new DecimalRaw(value);
                }

                var native = (long)(raw.Mid << 32) | raw.Low;
                if (raw.Negative)
                {
                    if ((ulong)native > 9223372036854775808)
                        throw new OverflowException();

                    native = -native;
                }
                else
                {
                    if ((ulong)native > 9223372036854775807)
                        throw new OverflowException();
                }

                buffer.WriteInt64(native);
            }
        }
    }
}
