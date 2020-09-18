using System;

namespace Npgsql.Expirements.TypeHandling
{
    public abstract class NpgsqlTypeHandlerFactory
    {
        public abstract NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection);

        protected internal virtual NpgsqlTypeHandler CreateHandler<TElement>(Type runtimeType, PostgresType postgresType, NpgsqlTypeHandler<TElement> elementHandler) =>
            throw new NotImplementedException();
    }
}
