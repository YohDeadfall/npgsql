using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    internal sealed class NullableHandlerFactory<TNullable> : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            var underlyingType = Nullable.GetUnderlyingType(runtimeType);
            if (underlyingType is null)
                throw runtimeType is null
                    ? throw new ArgumentNullException(nameof(runtimeType))
                    : new ArgumentException("The runtime type must be a nullable type.", nameof(runtimeType));

            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            return connection.TypeMapper
                .GetHandler(runtimeType)
                .CreateHandler(runtimeType, postgresType, this);
        }

        protected internal override NpgsqlTypeHandler CreateHandler<TElement>(Type runtimeType, PostgresType postgresType, NpgsqlTypeHandler<TElement> elementHandler) =>
            new Handler<TElement>(elementHandler);

        private sealed class Handler<TUnderlying> : NpgsqlTypeHandler<TNullable>
        {
            public NpgsqlTypeHandler<TUnderlying> Underlying { get; }

            public Handler(NpgsqlTypeHandler<TUnderlying> underlying) : base(underlying.Length)
            {
                if (Unsafe.SizeOf<Container>() != Unsafe.SizeOf<TUnderlying>())
                    throw new NotSupportedException();

                Underlying = underlying;
            }

            protected internal override async ValueTask<TNullable> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                var nullable = new Container?(await Underlying.ReadValueAsync(buffer, cancellationToken, length));
                return Unsafe.As<Container?, TNullable>(ref nullable);
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, TNullable value)
            {
                var nullable = Unsafe.As<TNullable, Container?>(ref value);

                Debug.Assert(nullable.HasValue);
                Underlying.WriteValue(buffer, nullable.Value);
            }

            private readonly struct Container
            {
                public readonly TUnderlying Value;
                public Container(TUnderlying value) => Value = value;

                public static implicit operator Container(TUnderlying value) => new Container(value);
                public static implicit operator TUnderlying(Container value) => value.Value;
            }
        }
    }
}
