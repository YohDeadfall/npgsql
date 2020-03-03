namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerAsSingleHandler : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<float, Converter>());

        internal readonly struct Converter : INpgsqlValueConverter<int, float>
        {
            public int ToSource(float value) => (int)value;
            public float ToTarget(int value) => value;
        }
    }
}
