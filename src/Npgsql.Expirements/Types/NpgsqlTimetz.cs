using System;

namespace Npgsql.Expirements.Types
{
    public readonly struct NpgsqlTimetz : IEquatable<NpgsqlTimetz>
    {
        private readonly NpgsqlTime _value;

        public static readonly NpgsqlTimetz NegativeInfinity = new NpgsqlTimetz(long.MinValue);
        public static readonly NpgsqlTimetz PositiveInfinity = new NpgsqlTimetz(long.MaxValue);

        public int Hour => _value.Hour;
        public int Minute => _value.Minute;
        public int Second => _value.Second;

        public int Milliseconds => _value.Milliseconds;
        public int Microseconds => _value.Microseconds;

        public NpgsqlTimetz(long value) =>
            _value = new NpgsqlTime(value);

        public NpgsqlTimetz(DateTimeOffset value) =>
            _value = new NpgsqlTime(value.UtcDateTime);

        public long ToNative() =>
            _value.ToNative();

        public DateTimeOffset ToDateTimeOffset() =>
            new DateTimeOffset(_value.ToDateTime().Ticks, TimeSpan.Zero);

        public override string ToString() =>
            ToDateTimeOffset().ToString();

        public int CompareTo(NpgsqlTimetz other) =>
            this == other ? 0 : this < other ? -1 : 1;

        public bool Equals(NpgsqlTimetz other) =>
            this == other;

        public override bool Equals(object? obj) =>
            obj is NpgsqlTimetz time && Equals(time);

        public override int GetHashCode() =>
            ToNative().GetHashCode();

        public static bool operator ==(NpgsqlTimetz left, NpgsqlTimetz right) => left.ToNative() == right.ToNative();
        public static bool operator !=(NpgsqlTimetz left, NpgsqlTimetz right) => left.ToNative() != right.ToNative();

        public static bool operator <(NpgsqlTimetz left, NpgsqlTimetz right) => left.ToNative() < right.ToNative();
        public static bool operator <=(NpgsqlTimetz left, NpgsqlTimetz right) => left.ToNative() <= right.ToNative();

        public static bool operator >(NpgsqlTimetz left, NpgsqlTimetz right) => left.ToNative() > right.ToNative();
        public static bool operator >=(NpgsqlTimetz left, NpgsqlTimetz right) => left.ToNative() >= right.ToNative();
    }
}
