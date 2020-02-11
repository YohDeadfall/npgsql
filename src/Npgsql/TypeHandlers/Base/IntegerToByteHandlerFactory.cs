namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerToByteHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler() =>
            NpgsqlTypeHandlerCache.GetOrCreate(() => new IntegerHandler<byte, IntegerToByteConverter>());
    }
}
