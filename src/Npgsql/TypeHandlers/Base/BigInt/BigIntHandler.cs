using System.Threading.Tasks;
using Npgsql.BackendMessages;

namespace Npgsql.TypeHandlers.Base
{
    internal sealed class BigIntHandler<T, TConverter> : NpgsqlTypeHandler<T>
        where TConverter : struct, INpgsqlValueConverter<long, T>
    {
        private const int NativeSize = sizeof(long);

        public BigIntHandler() : base(NativeSize) { }

        protected override ValueTask<T> ReadValue(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription, int length)
        {
            var value = new TConverter().ToTarget(buffer.ReadInt64());
            return new ValueTask<T>(value);
        }

        protected override ValueTask WriteValue(T value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            buffer.WriteInt64(new TConverter().ToSource(value));
            return new ValueTask();
        }

        protected override int ValidateAndGetLength(T value, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache) => NativeSize;
    }
}
