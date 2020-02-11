using System;

namespace Npgsql.TypeHandlers
{
    internal abstract class NpgsqlRangeHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public sealed override NpgsqlTypeHandler CreateHandler() =>
            GetElementHandler().CreateRangeHandler(this);

        private static NpgsqlTypeHandler GetElementHandler() =>
            throw new NotImplementedException();

        protected internal abstract NpgsqlTypeHandler CreateHandler<TElement>(NpgsqlTypeHandler elementHandler);
    }
}
