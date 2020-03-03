namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerAsDecimalHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<decimal, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<int, decimal>
        {
            public int ToSource(decimal value) => (int)value;
            public decimal ToTarget(int value) => value;
        }
    }
}
