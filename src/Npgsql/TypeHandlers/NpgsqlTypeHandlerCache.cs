using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers
{
    internal static class NpgsqlTypeHandlerCache
    {
        public static T GetOrCreate<T>(Func<T> factory)
            where T : NpgsqlTypeHandler
        {
            return LazyInitializer.EnsureInitialized(ref Cache<T>.Value, factory)!;
        }

        private static class Cache<T>
            where T : NpgsqlTypeHandler
        {
            public static T? Value;
        }
    }
}
