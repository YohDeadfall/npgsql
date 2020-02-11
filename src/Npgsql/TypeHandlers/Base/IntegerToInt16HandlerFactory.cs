namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerToInt16HandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<short, IntegerToInt16Converter>());
    }
}
