namespace Npgsql.TypeHandlers.Base.BigInt
{
    internal sealed class BigIntAsDoubleHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new BigIntHandler<double, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<long, double>
        {
            public long ToSource(double value) => (long)value;
            public double ToTarget(long value) => value;
        }
    }
}
