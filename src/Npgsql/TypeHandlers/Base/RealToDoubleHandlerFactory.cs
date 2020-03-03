namespace Npgsql.TypeHandlers.Base
{
    internal sealed class RealToDoubleHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new RealHandler<double, RealToDoubleConverter>());
    }
}
