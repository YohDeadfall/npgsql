using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    internal sealed class ArrayHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            var postgresArrayType = postgresType as PostgresArrayType;
            if (postgresArrayType is null)
                throw postgresType is null
                    ? new ArgumentNullException(nameof(postgresType))
                    : new ArgumentException("The postgres types isn't an array type.", nameof(postgresType));

            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            if (runtimeType.IsArray)
            {
                var elementType = runtimeType.GetElementType();
                Debug.Assert(elementType != null);

                return connection.TypeMapper
                    .GetHandler(elementType)
                    .CreateHandler(runtimeType, postgresArrayType, this);
            }

            throw new ArgumentException("The runtime type isn't an array type.", nameof(runtimeType));
        }

        protected internal override NpgsqlTypeHandler CreateHandler<TElement>(Type runtimeType, PostgresType postgresType, NpgsqlTypeHandler<TElement> elementHandler)
        {
            if (runtimeType.GetArrayRank() == 1)
                return new Handler<TElement[], TElement>(postgresType, elementHandler);

            var handlerType = typeof(Handler<,>).MakeGenericType(typeof(TElement), runtimeType);
            var handler = Activator.CreateInstance(handlerType, elementHandler);

            Debug.Assert(handler != null);

            return Unsafe.As<NpgsqlTypeHandler>(handler);
        }

        private sealed class Handler<TArray, TElement> : NpgsqlTypeHandler<TArray>
            where TArray : class
        {
            private static readonly int Dimensions = typeof(TArray).GetArrayRank();
            private static readonly int HandlesNulls = default(TElement) is null ? 1 : 0;

            public PostgresType ElementType { get; }
            public NpgsqlTypeHandler<TElement> ElementHandler { get; }

            public Handler(PostgresType elementType, NpgsqlTypeHandler<TElement> elementHandler) =>
                (ElementType, ElementHandler) = (elementType, elementHandler);

            protected internal override async ValueTask<TArray> ReadValueAsync(NpgsqlBufferReader reader, CancellationToken cancellationToken, int length)
            {
                await reader.EnsureAsync(sizeof(int) * 3, cancellationToken);

                var dimensionCount = reader.ReadInt32();
                if (dimensionCount != Dimensions)
                {
                    if (dimensionCount == 0)
                        return Dimensions == 0
                            ? Unsafe.As<TArray>(Array.Empty<TElement>())
                            : Unsafe.As<TArray>(Array.CreateInstance(typeof(TElement), new int[Dimensions]));

                    throw new InvalidOperationException();
                }

                var handlersNulls = reader.ReadInt32();
                if (handlersNulls != HandlesNulls)
                    throw new InvalidOperationException();

                var elementOid = reader.ReadInt32();
                if (elementOid != ElementType.Oid)
                    throw new InvalidOperationException();

                if (Dimensions == 1)
                {
                    await reader.EnsureAsync(8, cancellationToken);

                    var arrayLength = reader.ReadInt32();
                    var lowerBound = reader.ReadInt32();

                    var array = new TElement[arrayLength];
                    for (var i = 0; i < array.Length; i++)
                        array[i] = await ElementHandler.ReadAsync(reader, cancellationToken);

                    return Unsafe.As<TArray>(array);
                }
                else
                {
                    var dimensions = new int[Dimensions];
                    await reader.EnsureAsync(Dimensions * sizeof(int) * 2, cancellationToken);

                    for (var dimension = 0; dimension < Dimensions; dimension++)
                    {
                        dimensions[dimension] = reader.ReadInt32();
                        var lowerBound = reader.ReadInt32();
                    }

                    var array = Array.CreateInstance(typeof(TElement), dimensions);
                    var indices = new int[Dimensions];

                    while (true)
                    {
                        var element = await ElementHandler.ReadAsync(reader, cancellationToken);

                        array.SetValue(element, indices);
                        indices[Dimensions - 1]++;

                        for (var dimension = Dimensions - 1; dimension >= 0; dimension--)
                        {
                            if (indices[dimension] <= array.GetUpperBound(dimension))
                                continue;

                            if (dimension == 0)
                                return Unsafe.As<TArray>(array);

                            for (var j = dimension; j < Dimensions; j++)
                                indices[j] = array.GetLowerBound(j);

                            indices[dimension - 1]++;
                        }
                    }
                }
            }

            protected internal override void WriteValue(NpgsqlBufferWriter writer, TArray value)
            {
                writer.WriteInt32(Dimensions);
                writer.WriteInt32(HandlesNulls);
                writer.WriteInt32(ElementType.Oid);

                if (Dimensions == 0)
                {
                    var array = Unsafe.As<TElement[]>(value);

                    writer.WriteInt32(array.Length);
                    writer.WriteInt32(1);

                    foreach (var element in array)
                        ElementHandler.Write(writer, element);
                }
                else
                {
                    var array = Unsafe.As<Array>(value);
                    for (var dimension = 0; dimension < Dimensions; dimension++)
                    {
                        writer.WriteInt32(array.GetLength(dimension));
                        writer.WriteInt32(1);
                    }

                    for (var dimension = 0; dimension < Dimensions; dimension++)
                        for (int index = 0, length = array.GetLength(dimension); index < length; index++)
                            ElementHandler.Write(writer, (TElement)array.GetValue(index)!);
                }
            }
        }
    }
}
