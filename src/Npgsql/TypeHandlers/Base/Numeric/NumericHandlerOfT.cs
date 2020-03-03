using System;
using System.Threading.Tasks;
using Npgsql.BackendMessages;

namespace Npgsql.TypeHandlers.Base.Numeric
{
    internal sealed class NumericHandler<T, TConverter> : NpgsqlTypeHandler<T>
        where TConverter : INpgsqlValueConverter<decimal, T>, new()
    {
        public NumericHandler() : base(NumericHandler.HeaderSize) { }

        protected override async ValueTask<T> ReadValue(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription, int length) =>
            new TConverter().ToTarget(await NumericHandler.ReadValue(buffer));

        protected override ValueTask WriteValue(T value, NpgsqlWriteBuffer buffer, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            throw new NotImplementedException();
        }

        protected override int ValidateAndGetLength(T value, NpgsqlParameter parameter, NpgsqlLengthCache lengthCache)
        {
            throw new NotImplementedException();
        }
    }
}
