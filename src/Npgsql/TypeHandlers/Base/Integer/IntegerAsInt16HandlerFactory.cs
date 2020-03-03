namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerAsInt16HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<short, Converter>());

        internal readonly struct Converter : INpgsqlValueConverter<int, short>
        {
            public int ToSource(short value) => value;
            public short ToTarget(int value) => checked((short)value);
        }
    }
}
