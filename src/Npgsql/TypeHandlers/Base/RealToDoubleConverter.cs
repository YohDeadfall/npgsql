namespace Npgsql.TypeHandlers.Base
{
    internal readonly struct RealToDoubleConverter : INpgsqlValueConverter<float, double>
    {
        public float ToSource(double value) => (float)value;
        public double ToTarget(float value) => value;
    }
}
