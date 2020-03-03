namespace Npgsql.TypeHandlers.Base.BigInt
{
    internal sealed class BigIntAsInt16HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new BigIntHandler<short, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<long, short>
        {
            public long ToSource(short value) => value;
            public short ToTarget(long value) => checked((short)value);
        }
    }
}
