using System;

namespace Npgsql.Expirements.Types
{
    public readonly struct NpgsqlTime : IEquatable<NpgsqlTime>, IComparable<NpgsqlTime>
    {
        private readonly long _value;

        internal const long DaysSinceEra = NpgsqlTimestamp.DaysSinceEra;
        internal const long TicksPerDay = NpgsqlTimestamp.TicksPerDay;
        internal const long TicksPerMicrosecond = NpgsqlTimestamp.TicksPerMicrosecond;

        public static readonly NpgsqlTimestamp NegativeInfinity = new NpgsqlTimestamp(long.MinValue);
        public static readonly NpgsqlTimestamp PositiveInfinity = new NpgsqlTimestamp(long.MaxValue);

        public int Hour => throw new NotImplementedException();
        public int Minute => throw new NotImplementedException();
        public int Second => throw new NotImplementedException();

        public int Milliseconds => throw new NotImplementedException();
        public int Microseconds => throw new NotImplementedException();

        public NpgsqlTime(long value) => _value = value;

        public NpgsqlTime(DateTime value)
        {
            var ticks = value.Ticks;
            var time = ticks % TimeSpan.TicksPerDay;

            time /= TicksPerMicrosecond;

            _value = time;
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

        public int CompareTo(NpgsqlTime other) =>
            this == other ? 0 : this < other ? -1 : 1;

        public bool Equals(NpgsqlTime other) =>
            this == other;

        public override bool Equals(object? obj) =>
            obj is NpgsqlTime timestamp && Equals(timestamp);

        public override int GetHashCode() =>
            ToNative().GetHashCode();

        public static bool operator ==(NpgsqlTime left, NpgsqlTime right) => left.ToNative() == right.ToNative();
        public static bool operator !=(NpgsqlTime left, NpgsqlTime right) => left.ToNative() != right.ToNative();

        public static bool operator <(NpgsqlTime left, NpgsqlTime right) => left.ToNative() < right.ToNative();
        public static bool operator <=(NpgsqlTime left, NpgsqlTime right) => left.ToNative() <= right.ToNative();

        public static bool operator >(NpgsqlTime left, NpgsqlTime right) => left.ToNative() > right.ToNative();
        public static bool operator >=(NpgsqlTime left, NpgsqlTime right) => left.ToNative() >= right.ToNative();
    }
}
