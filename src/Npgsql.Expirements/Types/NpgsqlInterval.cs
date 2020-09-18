using System;
using System.Diagnostics.CodeAnalysis;

namespace Npgsql.Expirements.Types
{
    public readonly struct NpgsqlInterval : IEquatable<NpgsqlInterval>, IComparable<NpgsqlInterval>
    {
        public long Ticks { get; }

        public int Months { get; }
        public int Days { get; }

        public int Hours => throw new NotImplementedException();
        public int Minutes => throw new NotImplementedException();
        public int Seconds => throw new NotImplementedException();

        public int Milliseconds => throw new NotImplementedException();
        public int Microseconds => throw new NotImplementedException();

        public NpgsqlInterval(long ticks, int days, int months) =>
            (Ticks, Days, Months) = (ticks, days, months);

        public NpgsqlInterval(TimeSpan value) => (Months, Days, Ticks) = (0, 0, 0);

        public TimeSpan ToTimeSpan() => throw new NotImplementedException();

        public bool Equals([AllowNull] NpgsqlInterval other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo([AllowNull] NpgsqlInterval other)
        {
            throw new NotImplementedException();
        }
    }
}
