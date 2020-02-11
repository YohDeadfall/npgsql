namespace Npgsql.TypeHandlers.Base
{
    internal readonly struct IntegerToInt64Converter : INpgsqlValueConverter<int, long>
    {
        public int ToSource(long value) => checked((int)value);
        public long ToTarget(int value) => value;
    }
}
