using System.Threading.Tasks;
using Npgsql.BackendMessages;

namespace Npgsql.TypeHandlers
{
    internal abstract class NpgsqlTypeHandler<T> : NpgsqlTypeHandler
    {
        public abstract ValueTask<T> ReadAsync(NpgsqlStreamReader stream, FieldDescription? fieldDescription = null);

        public void Write(T value, NpgsqlStreamWriter stream, NpgsqlParameter? parameter = null)
        {
            using var length = stream.WriteLength();
            WriteValue(value, stream, parameter);
        }

        protected abstract void WriteValue(T value, NpgsqlStreamWriter writter, NpgsqlParameter? parameter);

        internal sealed override NpgsqlTypeHandler CreateArrayHandler(NpgsqlArrayHandlerFactory factory) =>
            factory.CreateHandler<T>(this);

        internal sealed override NpgsqlTypeHandler CreateRangeHandler(NpgsqlRangeHandlerFactory factory) =>
            factory.CreateHandler<T>(this);
    }
}
