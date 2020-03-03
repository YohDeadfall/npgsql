namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerAsInt32HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<int, Converter>());

        internal readonly struct Converter : INpgsqlValueConverter<int, int>
        {
            public int ToSource(int value) => value;
            public int ToTarget(int value) => value;
        }
    }
}
