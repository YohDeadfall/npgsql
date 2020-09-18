using System;

namespace Npgsql.Expirements.Types
{
    [Flags]
    internal enum NpgsqlRangeBoundFlags : byte
    {
        Inclusive = NpgsqlRangeFlags.LowerBoundInclusive,
        Infinity = NpgsqlRangeFlags.LowerBoundInfinite
    }
}
