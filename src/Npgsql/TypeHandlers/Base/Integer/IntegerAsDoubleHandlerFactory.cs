namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerAsDoubleHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<double, Converter>());

        internal readonly struct Converter : INpgsqlValueConverter<int, double>
        {
            public int ToSource(double value) => (int)value;
            public double ToTarget(int value) => value;
        }
    }
}
