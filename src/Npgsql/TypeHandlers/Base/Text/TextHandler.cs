using Npgsql.BackendMessages;
using Npgsql.PostgresTypes;
using Npgsql.TypeHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers.Base
{
    internal sealed class TextHandler<T, TReader, TWriter> : NpgsqlTypeHandler<T>
        where TReader : struct
        where TWriter : struct
    {
        private readonly Encoding _encoding;
        private readonly Encoder _encoder;

        public override ValueTask<T> ReadAsync(NpgsqlStreamReader stream, FieldDescription? fieldDescription = null)
        {
            throw new NotImplementedException();
        }

        protected override void WriteValue(T x, NpgsqlStreamWriter stream, NpgsqlParameter? parameter)
        {
            var value = string.Empty;
            var bytesMin = _encoding.GetMaxByteCount(1);
            var charIndex = 0;

            bool completed;
            do
            {
                _encoder.Convert(
                    value.AsSpan(charIndex),
                    stream.GetSpan(bytesMin),
                    charIndex == value.Length,
                    out var charsUsed,
                    out var bytesUsed,
                    out completed);

                charIndex += charsUsed;
            }
            while (!completed);
        }
    }
    internal sealed class TextAsStringHandlerFactory : NpgsqlTypeHandlerFactory<string>
    {
        public override TypeHandling.NpgsqlTypeHandler<string> Create(PostgresType pgType, NpgsqlConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}
