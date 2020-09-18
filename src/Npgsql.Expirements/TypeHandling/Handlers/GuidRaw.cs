using System;
using System.Runtime.InteropServices;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct GuidRaw
    {
        [FieldOffset(0)] public Guid Value;
        [FieldOffset(0)] public int Data1;
        [FieldOffset(4)] public short Data2;
        [FieldOffset(6)] public short Data3;
        [FieldOffset(8)] public long Data4;

        public GuidRaw(Guid value) : this() => Value = value;
    }
}
