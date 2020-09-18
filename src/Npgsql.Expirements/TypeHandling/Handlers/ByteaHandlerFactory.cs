using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class ByteaHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            throw new NotImplementedException();
        }

        private sealed class Handler : NpgsqlTypeHandler<byte[]>
        {
            protected internal override ValueTask<byte[]> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                throw new NotImplementedException();
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, byte[] value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
