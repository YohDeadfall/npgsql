using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class EnumHandlerFactory<T> : NpgsqlTypeHandlerFactory
        where T : Enum
    {
        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            throw new NotImplementedException();
        }

        private sealed class Handler : NpgsqlTypeHandler<T>
        {
            public Handler(Encoding encoding)
            {
                var values = (T[])Enum.GetValues(typeof(T));
                var keys = new ulong[values.Length];
                var names = new byte[values.Length][];

                for (var index = 0; index < values.Length; index++)
                {
                    var value = values[index];
                }
            }

            private static ulong GetKey(ReadOnlySpan<byte> name)
            {
                ref var reference = ref MemoryMarshal.GetReference(name);

                var length = name.Length;
                if (length > 7)
                    return Unsafe.ReadUnaligned<ulong>(ref reference);

                var key =
                    length > 5 ? Unsafe.ReadUnaligned<uint>(ref reference) | (ulong)Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref reference, 4)) << 32 :
                    length > 3 ? Unsafe.ReadUnaligned<uint>(ref reference) :
                    length > 1 ? Unsafe.ReadUnaligned<ushort>(ref reference) : 0UL;

                if ((length & 1) != 0)
                {
                    var offset = length - 1;
                    key |= (ulong)Unsafe.Add(ref reference, offset) << (offset * 8);
                }

                return key;
            }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                throw new NotImplementedException();
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
