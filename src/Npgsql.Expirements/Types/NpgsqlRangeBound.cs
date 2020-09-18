using System;

namespace Npgsql.Expirements.Types
{
    public static class NpgsqlRangeBound
    {
        public static NpgsqlRangeBound<T> Exclusive<T>(T value)
            where T : IEquatable<T>, IComparable<T> =>
            new NpgsqlRangeBound<T>(value, false);

        public static NpgsqlRangeBound<T> Inclusive<T>(T value)
            where T : IEquatable<T>, IComparable<T> =>
            new NpgsqlRangeBound<T>(value, true);

        public static NpgsqlRangeBound<T> Infinite<T>()
            where T : IEquatable<T>, IComparable<T> =>
            new NpgsqlRangeBound<T>(default, NpgsqlRangeBoundFlags.Infinity);
    }
}
