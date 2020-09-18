using System;

namespace Npgsql.Expirements.Types
{
    public readonly struct NpgsqlTimestamptz : IEquatable<NpgsqlTimestamptz>
    {
        private readonly NpgsqlTimestamp _value;

        public static readonly NpgsqlTimestamptz NegativeInfinity = new NpgsqlTimestamptz(long.MinValue);
        public static readonly NpgsqlTimestamptz PositiveInfinity = new NpgsqlTimestamptz(long.MaxValue);

        public NpgsqlDate Date => _value.Date;
        public NpgsqlTime Time => _value.Time;

        public int Year => _value.Year;
        public int Month => _value.Month;
        public int Day => _value.Day;

        public int Hour => _value.Hour;
        public int Minute => _value.Minute;
        public int Second => _value.Second;

        public int Milliseconds => _value.Milliseconds;
        public int Microseconds => _value.Microseconds;

        public NpgsqlTimestamptz(long value) =>
            _value = new NpgsqlTimestamp(value);

        public NpgsqlTimestamptz(DateTimeOffset value) =>
            _value = new NpgsqlTimestamp(value.UtcDateTime);

        public long ToNative() =>
            _value.ToNative();

        public DateTimeOffset ToDateTimeOffset() =>
            new DateTimeOffset(_value.ToDateTime().Ticks, TimeSpan.Zero);

        public override string ToString() =>
            ToDateTimeOffset().ToString();

        public int CompareTo(NpgsqlTimestamptz other) =>
            this == other ? 0 : this < other ? -1 : 1;

        public bool Equals(NpgsqlTimestamptz other) =>
            this == other;

        public override bool Equals(object? obj) =>
            obj is NpgsqlTimestamptz timestamp && Equals(timestamp);

        public override int GetHashCode() =>
            ToNative().GetHashCode();

        public static bool operator ==(NpgsqlTimestamptz left, NpgsqlTimestamptz right) => left.ToNative() == right.ToNative();
        public static bool operator !=(NpgsqlTimestamptz left, NpgsqlTimestamptz right) => left.ToNative() != right.ToNative();

        public static bool operator <(NpgsqlTimestamptz left, NpgsqlTimestamptz right) => left.ToNative() < right.ToNative();
        public static bool operator <=(NpgsqlTimestamptz left, NpgsqlTimestamptz right) => left.ToNative() <= right.ToNative();

        public static bool operator >(NpgsqlTimestamptz left, NpgsqlTimestamptz right) => left.ToNative() > right.ToNative();
        public static bool operator >=(NpgsqlTimestamptz left, NpgsqlTimestamptz right) => left.ToNative() >= right.ToNative();
    }
}
