namespace Npgsql.TypeHandlers.Base.BigInt
{
    internal sealed class BigIntAsInt64HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new BigIntHandler<long, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<long, long>
        {
            public long ToSource(long value) => value;
            public long ToTarget(long value) => value;
        }
    }
}
