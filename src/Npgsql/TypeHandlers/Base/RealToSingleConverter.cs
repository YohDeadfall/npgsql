namespace Npgsql.TypeHandlers.Base
{
    internal readonly struct RealToSingleConverter : INpgsqlValueConverter<float, float>
    {
        public float ToSource(float value) => value;
        public float ToTarget(float value) => value;
    }
}
