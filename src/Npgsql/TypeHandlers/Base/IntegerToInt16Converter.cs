namespace Npgsql.TypeHandlers.Base
{
    internal readonly struct IntegerToInt16Converter : INpgsqlValueConverter<int, short>
    {
        public int ToSource(short value) => value;
        public short ToTarget(int value) => checked((short)value);
    }
}
