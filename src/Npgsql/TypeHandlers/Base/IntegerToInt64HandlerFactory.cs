namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerToInt64HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<long, IntegerToInt64Converter>());
    }
}
