namespace Npgsql.TypeHandlers.Base
{
    internal readonly struct IntegerToDecimalConverter : INpgsqlValueConverter<int, decimal>
    {
        public int ToSource(decimal value) => (int)value;
        public decimal ToTarget(int value) => value;
    }
}
