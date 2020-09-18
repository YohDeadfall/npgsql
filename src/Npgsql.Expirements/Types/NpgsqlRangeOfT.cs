using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Npgsql.Expirements.Types
{
    public readonly struct NpgsqlRange<T> : IEquatable<NpgsqlRange<T>>
        where T : IEquatable<T>, IComparable<T>
    {
        internal T LowerBoundInternal { get; }
        internal T UpperBoundInternal { get; }
        internal NpgsqlRangeFlags Flags { get; }

        public NpgsqlRange(NpgsqlRangeBound<T> lowerBound, NpgsqlRangeBound<T> upperBound)
        {
            LowerBoundInternal = lowerBound.ValueInternal;
            UpperBoundInternal = upperBound.ValueInternal;
            Flags =
                (NpgsqlRangeFlags)((int)lowerBound.Flags << 0) |
                (NpgsqlRangeFlags)((int)upperBound.Flags << 2);
        }

        internal NpgsqlRange([AllowNull] T lowerBound, [AllowNull] T upperBound, NpgsqlRangeFlags flags) =>
            (LowerBoundInternal, UpperBoundInternal, Flags) = (lowerBound, upperBound, flags);

        public NpgsqlRangeBound<T> LowerBound => new NpgsqlRangeBound<T>(LowerBoundInternal, (NpgsqlRangeBoundFlags)((int)Flags >> 0));

        public NpgsqlRangeBound<T> UpperBound => new NpgsqlRangeBound<T>(LowerBoundInternal, (NpgsqlRangeBoundFlags)((int)Flags >> 2));

        public bool IsEmpty => LowerBound == UpperBound;

        public bool Equals(NpgsqlRange<T> other) =>
            Flags == other.Flags &&
            EqualityComparer<T>.Default.Equals(LowerBoundInternal, other.LowerBoundInternal) &&
            EqualityComparer<T>.Default.Equals(UpperBoundInternal, other.UpperBoundInternal);

        public override bool Equals(object? obj) => obj is NpgsqlRange<T> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Flags, LowerBoundInternal, UpperBoundInternal);

        public static bool operator ==(NpgsqlRange<T> left, NpgsqlRange<T> right) => left.Equals(right);
        public static bool operator !=(NpgsqlRange<T> left, NpgsqlRange<T> right) => !left.Equals(right);
    }
}
