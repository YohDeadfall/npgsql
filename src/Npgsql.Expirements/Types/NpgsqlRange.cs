using System;

namespace Npgsql.Expirements.Types
{
    public static class NpgsqlRange
    {
        public static NpgsqlRange<T> Empty<T>()
            where T : IEquatable<T>, IComparable<T> =>
            default;
    }
}
