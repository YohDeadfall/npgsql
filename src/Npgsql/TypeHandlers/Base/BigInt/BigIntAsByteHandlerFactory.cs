namespace Npgsql.TypeHandlers.Base.BigInt
{
    internal sealed class BigIntAsByteHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new BigIntHandler<byte, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<long, byte>
        {
            public long ToSource(byte value) => value;
            public byte ToTarget(long value) => checked((byte)value);
        }
    }
}
