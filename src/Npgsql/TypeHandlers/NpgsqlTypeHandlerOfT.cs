using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Npgsql.BackendMessages;

namespace Npgsql.TypeHandlers
{
    internal abstract class NpgsqlTypeHandler<T> : NpgsqlTypeHandler
    {
        private readonly int _minRequiredSize;

        protected NpgsqlTypeHandler(int minRequiredSize = 0) =>
            _minRequiredSize = minRequiredSize >= 0 ? minRequiredSize : throw new ArgumentOutOfRangeException(nameof(minRequiredSize));

        public sealed override ValueTask<TAny> ReadAsync<TAny>(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription = null)
        {
            if (typeof(TAny) == typeof(T))
                return buffer.ReadBytesLeft < sizeof(int) + _minRequiredSize
                    ? ReadValueSlow<TAny>(buffer, fieldDescription)
                    : ReadValueFast<TAny>(buffer, fieldDescription);

            if (IsNullableOf<TAny, T>.Value)
                return buffer.ReadBytesLeft < sizeof(int) + _minRequiredSize
                    ? ReadNullableSlow<TAny>(buffer, fieldDescription)
                    : ReadNullableFast<TAny>(buffer, fieldDescription);

            throw new NotSupportedException();
        }

        private async ValueTask<TAny> ReadValueSlow<TAny>(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription)
        {
            Debug.Assert(typeof(TAny) == typeof(T));

            await buffer.Ensure(sizeof(int), true);

            var length = buffer.ReadInt32();
            if (length == -1)
                throw new InvalidCastException();

            var value = await ReadValueAsync(buffer, fieldDescription, length);
            return Unsafe.As<T, TAny>(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask<TAny> ReadValueFast<TAny>(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription)
        {
            Debug.Assert(typeof(ValueTask<TAny>) == typeof(ValueTask<T>));

            var length = buffer.ReadInt32();
            if (length == -1)
                throw new InvalidCastException();

            var taks = ReadValueAsync(buffer, fieldDescription, length);
            return Unsafe.As<ValueTask<T>, ValueTask<TAny>>(ref taks);
        }

        private async ValueTask<TAny> ReadNullableSlow<TAny>(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription)
        {
            Debug.Assert(typeof(ValueTask<TAny>) == typeof(ValueTask<T>));

            await buffer.Ensure(sizeof(int), true);

            var length = buffer.ReadInt32();
            if (length == -1)
                return default!;

            var value = await ReadValueAsync(buffer, fieldDescription, length);
            return PackNullable<TAny, T>(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask<TAny> ReadNullableFast<TAny>(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription)
        {
            Debug.Assert(typeof(TAny) == typeof(T));

            var length = buffer.ReadInt32();
            return length == -1 ? default : ReadAndPack();

            async ValueTask<TAny> ReadAndPack() =>
                PackNullable<TAny, T>(await ReadValueAsync(buffer, fieldDescription, length));
        }

        protected abstract ValueTask<T> ReadValueAsync(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription, int length);

        public sealed override ValueTask WriteAsync<TAny>(TAny value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            if (typeof(TAny) == typeof(T) || IsNullableOf<TAny, T>.Value)
                return buffer.WriteSpaceLeft < sizeof(int) + _minRequiredSize
                    ? WriteValueSlow(value, buffer, parameter, lengthCache)
                    : WriteValueFast(value, buffer, parameter, lengthCache);

            throw new NotSupportedException();
        }

        private async ValueTask WriteValueSlow<TAny>(TAny value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            await buffer.Flush(true);

            if (value == null || value is DBNull)
            {
                buffer.WriteInt32(-1);
            }
            else
            {
                Debug.Assert(typeof(TAny) == typeof(T) || IsNullableOf<TAny, T>.Value);

                var unpacked = IsNullableOf<TAny, T>.Value
                    ? UnpackNullable<TAny, T>(value)
                    : Unsafe.As<TAny, T>(ref value);

                buffer.WriteInt32(ValidateAndGetLength(unpacked, parameter, lengthCache));
                await WriteValueAsync(unpacked, buffer, parameter, lengthCache);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask WriteValueFast<TAny>(TAny value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            if (value == null || value is DBNull)
            {
                buffer.WriteInt32(-1);
                return default;
            }
            else
            {
                Debug.Assert(typeof(TAny) == typeof(T) || IsNullableOf<TAny, T>.Value);

                var unpacked = IsNullableOf<TAny, T>.Value
                    ? UnpackNullable<TAny, T>(value)
                    : Unsafe.As<TAny, T>(ref value);

                buffer.WriteInt32(ValidateAndGetLength(unpacked, parameter, lengthCache));
                return WriteValueAsync(unpacked, buffer, parameter, lengthCache);
            }
        }

        protected abstract ValueTask WriteValueAsync(T value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache);

        protected abstract int ValidateAndGetLength(T value, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache);

        internal sealed override NpgsqlTypeHandler CreateArrayHandler(NpgsqlArrayHandlerFactory factory) =>
            factory.CreateHandler<T>(this);

        internal sealed override NpgsqlTypeHandler CreateRangeHandler(NpgsqlRangeHandlerFactory factory) =>
            factory.CreateHandler<T>(this);
    }
}
