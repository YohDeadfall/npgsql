using System;

namespace Npgsql.Expirements.Types
{
    [Flags]
    internal enum NpgsqlRangeFlags : byte
    {
        None = 0,
        Empty = 1,
        LowerBoundInclusive = 2,
        UpperBoundInclusive = 4,
        LowerBoundInfinite = 8,
        UpperBoundInfinite = 16,
    }
}
