using System;

namespace Npgsql.Expirements.Types
{
    public readonly struct NpgsqlTimestamp : IEquatable<NpgsqlTimestamp>, IComparable<NpgsqlTimestamp>
    {
        private readonly long _value;

        internal const long DaysSinceEra = 730119; // days since era (0001-01-01) for 2000-01-01
        internal const long TicksPerDay = 86400000000;
        internal const long TicksPerMicrosecond = 10;

        public static readonly NpgsqlTimestamp NegativeInfinity = new NpgsqlTimestamp(long.MinValue);
        public static readonly NpgsqlTimestamp PositiveInfinity = new NpgsqlTimestamp(long.MaxValue);

        public NpgsqlDate Date => throw new NotImplementedException();
        public NpgsqlTime Time => throw new NotImplementedException();

        public int Year => Date.Year;
        public int Month => Date.Month;
        public int Day => Date.Day;

        public int Hour => Time.Hour;
        public int Minute => Time.Minute;
        public int Second => Time.Second;

        public int Milliseconds => Time.Milliseconds;
        public int Microseconds => Time.Microseconds;

        public NpgsqlTimestamp(long value) => _value = value;

        public NpgsqlTimestamp(DateTime value)
        {
            var ticks = value.Ticks;
            var date = Math.DivRem(ticks, TimeSpan.TicksPerDay, out var time);

            date -= DaysSinceEra;
            time /= TicksPerMicrosecond;

            _value = date * TicksPerDay + time;
        }

        public DateTime ToDateTime()
        {
            var native = ToNative();
            var date = Math.DivRem(native, TicksPerDay, out var time);

            if (native < 0 & time != 0)
            {
                date -= 1;
                time = TicksPerDay + time;
            }

            date += DaysSinceEra;
            time *= TicksPerMicrosecond;

            var ticks = Math.Abs(date * TimeSpan.TicksPerDay + time);
            return new DateTime(ticks);
        }

        public long ToNative() => _value;

        public override string ToString() => ToDateTime().ToString();

        public int CompareTo(NpgsqlTimestamp other) =>
            this == other ? 0 : this < other ? -1 : 1; 

        public bool Equals(NpgsqlTimestamp other) =>
            this == other;

        public override bool Equals(object? obj) =>
            obj is NpgsqlTimestamp timestamp && Equals(timestamp);

        public override int GetHashCode() =>
            ToNative().GetHashCode();

        public static bool operator ==(NpgsqlTimestamp left, NpgsqlTimestamp right) => left.ToNative() == right.ToNative();
        public static bool operator !=(NpgsqlTimestamp left, NpgsqlTimestamp right) => left.ToNative() != right.ToNative();

        public static bool operator <(NpgsqlTimestamp left, NpgsqlTimestamp right) => left.ToNative() < right.ToNative();
        public static bool operator <=(NpgsqlTimestamp left, NpgsqlTimestamp right) => left.ToNative() <= right.ToNative();

        public static bool operator >(NpgsqlTimestamp left, NpgsqlTimestamp right) => left.ToNative() > right.ToNative();
        public static bool operator >=(NpgsqlTimestamp left, NpgsqlTimestamp right) => left.ToNative() >= right.ToNative();
    }
}
