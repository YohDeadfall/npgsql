using System;

namespace Npgsql.Expirements.Types
{
    public readonly struct NpgsqlDate : IEquatable<NpgsqlDate>, IComparable<NpgsqlDate>
    {
        private readonly int _value;

        internal const long DaysSinceEra = NpgsqlTimestamp.DaysSinceEra;
        internal const long TicksPerDay = NpgsqlTimestamp.TicksPerDay;

        public static readonly NpgsqlDate NegativeInfinity = new NpgsqlDate(int.MinValue);
        public static readonly NpgsqlDate PositiveInfinity = new NpgsqlDate(int.MaxValue);

        public int Year => ToDateTime().Year;
        public int Month => ToDateTime().Month;
        public int Day => ToDateTime().Day;

        public NpgsqlDate(int value) => _value = value;

        public NpgsqlDate(DateTime value)
        {
            var ticks = value.Ticks;
            var date = ticks / TimeSpan.TicksPerDay - DaysSinceEra;

            _value = (int)date;
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
            time *= 10;

            var ticks = Math.Abs(date * TimeSpan.TicksPerDay + time);
            return new DateTime(ticks);
        }

        public int ToNative() => _value;

        public override string ToString() => ToDateTime().ToString();

        public int CompareTo(NpgsqlDate other) =>
            this == other ? 0 : this < other ? -1 : 1;

        public bool Equals(NpgsqlDate other) =>
            this == other;

        public override bool Equals(object? obj) =>
            obj is NpgsqlDate timestamp && Equals(timestamp);

        public override int GetHashCode() =>
            ToNative().GetHashCode();

        public static bool operator ==(NpgsqlDate left, NpgsqlDate right) => left.ToNative() == right.ToNative();
        public static bool operator !=(NpgsqlDate left, NpgsqlDate right) => left.ToNative() != right.ToNative();

        public static bool operator <(NpgsqlDate left, NpgsqlDate right) => left.ToNative() < right.ToNative();
        public static bool operator <=(NpgsqlDate left, NpgsqlDate right) => left.ToNative() <= right.ToNative();

        public static bool operator >(NpgsqlDate left, NpgsqlDate right) => left.ToNative() > right.ToNative();
        public static bool operator >=(NpgsqlDate left, NpgsqlDate right) => left.ToNative() >= right.ToNative();
    }
}
