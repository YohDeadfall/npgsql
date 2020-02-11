namespace Npgsql.TypeHandlers.Base
{
    internal readonly struct IntegerToByteConverter : INpgsqlValueConverter<int, byte>
    {
        public int ToSource(byte value) => value;
        public byte ToTarget(int value) => checked((byte)value);
    }
}
