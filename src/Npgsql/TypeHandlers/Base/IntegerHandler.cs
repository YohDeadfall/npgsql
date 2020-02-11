using Npgsql.BackendMessages;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerHandler<T, TConverter> : NpgsqlTypeHandler<T>
        where TConverter : INpgsqlValueConverter<int, T>, new()
    {
        private const int NativeSize = sizeof(int);

        public IntegerHandler() : base(NativeSize) { }

        protected override ValueTask<T> ReadValueAsync(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription, int length)
        {
            var value = new TConverter().ToTarget(buffer.ReadInt32());
            return new ValueTask<T>(value);
        }

        protected override ValueTask WriteValueAsync(T value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            buffer.WriteInt32(new TConverter().ToSource(value));
            return new ValueTask();
        }

        protected override int ValidateAndGetLength(T value, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache) => NativeSize;
    }
}
