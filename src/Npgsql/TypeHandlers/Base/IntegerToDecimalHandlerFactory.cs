namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerToDecimalHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<decimal, IntegerToDecimalConverter>());
    }
}
