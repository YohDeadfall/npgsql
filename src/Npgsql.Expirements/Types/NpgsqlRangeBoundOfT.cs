using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Npgsql.Expirements.Types
{
    public readonly struct NpgsqlRangeBound<T> : IEquatable<NpgsqlRangeBound<T>>
        where T : IEquatable<T>, IComparable<T>
    {
        internal T ValueInternal { get; }
        internal NpgsqlRangeBoundFlags Flags { get; }

        public bool IsInclusive => Flags.HasFlag(NpgsqlRangeBoundFlags.Inclusive);
        public bool IsInfinite => Flags.HasFlag(NpgsqlRangeBoundFlags.Infinity);

        public T Value => IsInfinite
            ? throw new InvalidOperationException()
            : ValueInternal;

        public NpgsqlRangeBound(T value, bool isInclusive = false) =>
            (ValueInternal, Flags) = (value, isInclusive ? NpgsqlRangeBoundFlags.Inclusive : default);

        internal NpgsqlRangeBound([AllowNull] T value, NpgsqlRangeBoundFlags flags) =>
            (ValueInternal, Flags) = (value, flags);

        public bool Equals(NpgsqlRangeBound<T> other) => Flags == other.Flags && EqualityComparer<T>.Default.Equals(ValueInternal, other.ValueInternal);
        public override bool Equals(object? other) => other is NpgsqlRangeBound<T> bound && Equals(bound);
        public override int GetHashCode() => HashCode.Combine(ValueInternal, Flags);

        public static bool operator ==(NpgsqlRangeBound<T> left, NpgsqlRangeBound<T> right) => left.Equals(right);
        public static bool operator !=(NpgsqlRangeBound<T> left, NpgsqlRangeBound<T> right) => !left.Equals(right);

        public static implicit operator NpgsqlRangeBound<T>(T value) => new NpgsqlRangeBound<T>(value);
    }
}
