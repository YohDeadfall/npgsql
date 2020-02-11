using Npgsql.BackendMessages;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers
{
    internal abstract class NpgsqlTypeHandler
    {
        internal NpgsqlTypeHandler() { }

        public abstract ValueTask<TAny> ReadAsync<TAny>(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription = null);

        public abstract ValueTask WriteAsync<TAny>(TAny value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TNullable PackNullable<TNullable, TValue>(TValue value)
        {
            var nullable = new Container<TValue>?(new Container<TValue>(value));
            return Unsafe.As<Container<TValue>?, TNullable>(ref nullable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TValue UnpackNullable<TNullable, TValue>(TNullable nullable) =>
                Unsafe.As<TNullable, Container<TValue>?>(ref nullable).GetValueOrDefault().Value;

        internal static class IsNullableOf<TNullable, TValue>
        {
            public static readonly bool Value = Nullable.GetUnderlyingType(typeof(TNullable)) == typeof(TValue);
        }

        private readonly struct Container<T>
        {
            public T Value { get; }
            public Container(T value) => Value = value;
        }

        internal abstract NpgsqlTypeHandler CreateArrayHandler(NpgsqlArrayHandlerFactory factory);

        internal abstract NpgsqlTypeHandler CreateRangeHandler(NpgsqlRangeHandlerFactory factory);
    }
}
