using System.Threading.Tasks;
using Npgsql.BackendMessages;

namespace Npgsql.TypeHandlers.Base
{
    internal sealed class RealHandler<T, TConverter> : NpgsqlTypeHandler<T>
        where TConverter : INpgsqlValueConverter<float, T>, new()
    {
        private const int NativeSize = sizeof(float);

        public RealHandler() : base(NativeSize) { }

        protected override ValueTask<T> ReadValue(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription, int length)
        {
            var value = new TConverter().ToTarget(buffer.ReadSingle());
            return new ValueTask<T>(value);
        }

        protected override ValueTask WriteValue(T value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            buffer.WriteSingle(new TConverter().ToSource(value));
            return new ValueTask();
        }

        protected override int ValidateAndGetLength(T value, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache) => NativeSize;
    }
}
