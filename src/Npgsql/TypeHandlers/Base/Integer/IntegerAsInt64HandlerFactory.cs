namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerAsInt64HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<long, Converter>());

        internal readonly struct Converter : INpgsqlValueConverter<int, long>
        {
            public int ToSource(long value) => checked((int)value);
            public long ToTarget(int value) => value;
        }
    }
}
