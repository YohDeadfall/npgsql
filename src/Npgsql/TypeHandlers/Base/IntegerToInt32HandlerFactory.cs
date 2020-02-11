namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerToInt32HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<int, IntegerToInt32Converter>());
    }
}
