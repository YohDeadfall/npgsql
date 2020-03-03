using Npgsql.BackendMessages;
using Npgsql.TypeHandling;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers.Range
{
    internal sealed class RangeHandler<TElement> : NpgsqlTypeHandler<NpgsqlRange<TElement>>
    {
        private readonly NpgsqlTypeHandler<TElement> _elementHandler;

        public RangeHandler(NpgsqlTypeHandler<TElement> elementHandler) =>
            _elementHandler = elementHandler;

        public override ValueTask<NpgsqlRange<TElement>> ReadAsync(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription = null)
        {
            throw new NotImplementedException();
        }

        public override void Write(NpgsqlRange<TElement> value, NpgsqlWriteBuffer buffer, NpgsqlParameter? parameter = null)
        {
            throw new NotImplementedException();
        }
    }
}
