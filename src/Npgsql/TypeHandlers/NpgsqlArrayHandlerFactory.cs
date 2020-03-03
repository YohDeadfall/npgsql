using System;

namespace Npgsql.TypeHandlers
{
    internal abstract class NpgsqlArrayHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public sealed override NpgsqlTypeHandler CreateHandler() =>
            GetElementHandler().CreateArrayHandler(this);

        private static NpgsqlTypeHandler GetElementHandler() =>
            throw new NotImplementedException();

        protected internal abstract NpgsqlTypeHandler CreateHandler<TElement>(NpgsqlTypeHandler<TElement> elementHandler);
    }
}
