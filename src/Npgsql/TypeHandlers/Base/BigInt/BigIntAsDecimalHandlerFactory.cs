namespace Npgsql.TypeHandlers.Base.BigInt
{
    internal sealed class BigIntAsDecimalHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new BigIntHandler<decimal, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<long, decimal>
        {
            public long ToSource(decimal value) => (long)value;
            public decimal ToTarget(long value) => value;
        }
    }
}
