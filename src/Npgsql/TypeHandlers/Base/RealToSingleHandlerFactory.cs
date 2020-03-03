namespace Npgsql.TypeHandlers.Base
{
    internal sealed class RealToSingleHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new RealHandler<float, RealToSingleConverter>());
    }
}
