namespace Npgsql.TypeHandlers.Array
{
    internal sealed class ArrayHandlerFactory : NpgsqlArrayHandlerFactory
    {
        protected internal override NpgsqlTypeHandler CreateHandler<TElement>(NpgsqlTypeHandler<TElement> elementHandler) =>
            new ArrayHandler<TElement>(elementHandler);
    }
}
