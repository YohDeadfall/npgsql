namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerAsByteHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<byte, Converter>());

        private readonly struct Converter : INpgsqlValueConverter<int, byte>
        {
            public int ToSource(byte value) => value;
            public byte ToTarget(int value) => checked((byte)value);
        }
    }
}
