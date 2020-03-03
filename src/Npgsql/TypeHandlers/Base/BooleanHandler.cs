using System.Threading.Tasks;
using Npgsql.BackendMessages;

namespace Npgsql.TypeHandlers.Base
{
    internal sealed class BooleanHandler : NpgsqlTypeHandler<bool>
    {
        private const int NativeSize = 1;

        public BooleanHandler() : base(NativeSize) { }

        protected override ValueTask<bool> ReadValue(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription, int length) =>
            new ValueTask<bool>(buffer.ReadByte() != 0);

        protected override ValueTask WriteValue(bool value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            buffer.WriteByte(value ? (byte)1 : (byte)0);
            return default;
        }

        protected override int ValidateAndGetLength(bool value, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache) => NativeSize;
    }
}
