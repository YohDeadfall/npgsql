using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling
{
    public abstract class NpgsqlTypeHandler
    {
        internal NpgsqlTypeHandler()
        {
        }

        internal abstract ValueTask<object?> ReadObjectAsync(NpgsqlBufferReader reader, CancellationToken cancellationToken);

        internal abstract void WriteParameter(NpgsqlBufferWriter writer, NpgsqlParameter parameter);

        internal abstract NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlTypeHandlerFactory factory);
    }
}
