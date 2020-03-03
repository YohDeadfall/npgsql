namespace Npgsql.TypeHandlers.Base.BigInt
{
    internal sealed class BigIntAsInt32HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new BigIntHandler<int, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<long, int>
        {
            public long ToSource(int value) => value;
            public int ToTarget(long value) => checked((int)value);
        }
    }
}
