namespace Npgsql.TypeHandlers
{
    internal abstract class NpgsqlTypeHandlerFactory
    {
        public abstract NpgsqlTypeHandler CreateHandler();
    }
}
