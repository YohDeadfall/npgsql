using System.Threading.Tasks;
using Npgsql.BackendMessages;

namespace Npgsql.TypeHandlers.Base
{
    internal sealed class IntegerHandler<T, TConverter> : NpgsqlTypeHandler<T>
        where TConverter : struct, INpgsqlValueConverter<int, T>
    {
        protected override ValueTask<T> ReadValue(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription, int length)
        {
            var value = new TConverter().ToTarget(buffer.ReadInt32());
            return new ValueTask<T>(value);
        }

        protected override ValueTask WriteValue(T value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            buffer.WriteInt32(new TConverter().ToSource(value));
            return new ValueTask();
        }

        public override ValueTask<T> ReadAsync(NpgsqlTypeReader reader, FieldDescription? fieldDescription = null)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(T value, NpgsqlStreamWriter writter, NpgsqlParameter? parameter = null) =>
            writter.WriteInt32(new TConverter().ToSource(value));
    }
}
