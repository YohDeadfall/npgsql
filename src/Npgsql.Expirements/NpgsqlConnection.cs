using Npgsql.Expirements.TypeHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.Expirements
{
    public sealed class NpgsqlConnection
    {
        public NpgsqlTypeMapper TypeMapper { get; } = new NpgsqlTypeMapper();
        public NpgsqlConnectionOptions Options { get; } = new NpgsqlConnectionOptions();
    }

    public sealed class NpgsqlConnectionOptions
    {
        public int ReadBufferSize { get; } = Environment.SystemPageSize;
        public int WriteBufferSize { get; } = Environment.SystemPageSize;
        public Encoding Encoding { get; } = Encoding.UTF8;
    }

    public sealed class NpgsqlTypeMapper
    {
        public NpgsqlTypeHandler GetHandler(Type type) => throw new NotImplementedException();

        public NpgsqlTypeHandler GetHandler(PostgresType type) => throw new NotImplementedException();

        public NpgsqlNamingPolicy ClassNamingPolicy => throw new NotImplementedException();
        public NpgsqlNamingPolicy AttributeNamingPolicy => throw new NotImplementedException();
    }
}
