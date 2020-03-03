using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers.Range
{
    internal sealed class RangeHandlerFactory : NpgsqlRangeHandlerFactory
    {
        protected internal override NpgsqlTypeHandler CreateHandler<TElement>(NpgsqlTypeHandler<TElement> elementHandler) =>
            new RangeHandler<TElement>(elementHandler);
    }
}
