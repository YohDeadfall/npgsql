namespace Npgsql.TypeHandlers.Base
{
    internal readonly struct IntegerToInt32Converter : INpgsqlValueConverter<int, int>
    {
        public int ToSource(int value) => value;
        public int ToTarget(int value) => value;
    }
}
