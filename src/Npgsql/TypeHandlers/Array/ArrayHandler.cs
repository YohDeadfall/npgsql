using Npgsql.BackendMessages;
using System;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers.Array
{
    internal sealed class ArrayHandler<TElement> : NpgsqlTypeHandler
    {
        private static readonly bool HandlesNulls = default(TElement) is null;
        private readonly NpgsqlTypeHandler<TElement> _elementHandler;

        public ArrayHandler(NpgsqlTypeHandler<TElement> elementHandler) =>
            _elementHandler = elementHandler;

        public override async ValueTask<TAny> Read<TAny>(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription = null)
        {
            await buffer.Ensure(12, true);
            var dimensionCount = buffer.ReadInt32();
            var containsNulls = buffer.ReadInt32() == 1;

            // Element OID
            buffer.ReadUInt32();

            if (containsNulls != HandlesNulls)
                throw new InvalidOperationException();

            if (dimensionCount == 0)
                return (TAny)(object)Array.Empty<TElement>();

            if (dimensionCount == 1)
            {
                await buffer.Ensure(8, true);
                var arrayLength = buffer.ReadInt32();

                // Lower bound
                buffer.ReadInt32();

                var oneDimensional = new TElement[arrayLength];
                for (var i = 0; i < oneDimensional.Length; i++)
                    oneDimensional[i] = await _elementHandler.Read<TElement>(buffer);

                return (TAny)(object)oneDimensional;
            }

            var dimensions = new int[dimensionCount];
            await buffer.Ensure(dimensionCount * 8, true);

            for (var i = 0; i < dimensionCount; i++)
            {
                dimensions[i] = buffer.ReadInt32();
                buffer.ReadInt32(); // Lower bound
            }

            var result = Array.CreateInstance(typeof(TElement), dimensions);

            // Multidimensional arrays
            // We can't avoid boxing here
            var indices = new int[dimensionCount];
            while (true)
            {
                var element = await _elementHandler.Read<TElement>(buffer);
                result.SetValue(element, indices);

                // TODO: Overly complicated/inefficient...
                indices[dimensionCount - 1]++;
                for (var dimension = dimensionCount - 1; dimension >= 0; dimension--)
                {
                    if (indices[dimension] <= result.GetUpperBound(dimension))
                        continue;

                    if (dimension == 0)
                        return (TAny)(object)result;

                    for (var j = dimension; j < dimensionCount; j++)
                        indices[j] = result.GetLowerBound(j);

                    indices[dimension - 1]++;
                }
            }
        }

        internal override NpgsqlTypeHandler CreateArrayHandler(NpgsqlArrayHandlerFactory factory) =>
            throw new NotSupportedException();

        internal override NpgsqlTypeHandler CreateRangeHandler(NpgsqlRangeHandlerFactory factory) =>
            throw new NotSupportedException();
    }
}
