namespace Npgsql.TypeHandlers.Base.BigInt
{
    internal sealed class BigIntAsSingleHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new BigIntHandler<float, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<long, float>
        {
            public long ToSource(float value) => (long)value;
            public float ToTarget(long value) => value;
        }
    }
}
