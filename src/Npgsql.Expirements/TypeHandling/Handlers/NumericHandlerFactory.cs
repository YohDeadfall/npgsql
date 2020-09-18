using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class NumericHandlerFactory : NpgsqlTypeHandlerFactory
    {
        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            throw new NotImplementedException();
        }

        private sealed class Handler : NpgsqlTypeHandler<decimal>
        {
            private const int NativeLength = sizeof(short) * 4;

            private const int MaxDecimalScale = 28;

            private const short SignPositive = 0x0000;
            private const short SignNegative = 0x4000;
            private const short SignNan = unchecked((short)0xC000);

            private const int MaxGroupCount = 8;
            private const int MaxGroupScale = 4;

            private static readonly uint MaxGroupSize = DecimalRaw.Powers10[MaxGroupScale];

            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<decimal> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                var readLength = buffer.GetSpan().Length;
                if (readLength < length)
                {
                    return ReadValueAsync();
                    async ValueTask<decimal> ReadValueAsync()
                    {
                        await buffer.EnsureAsync(readLength, cancellationToken);
                        return ReadValue();
                    }
                }

                return new ValueTask<decimal>(ReadValue());
                decimal ReadValue()
                {
                    var result = new DecimalRaw();
                    var groups = buffer.ReadInt16();
                    var weight = buffer.ReadInt16() - groups + 1;
                    var sign = buffer.ReadInt16();

                    if (sign == SignNan)
                        throw new InvalidCastException("Numeric NaN not supported by System.Decimal");

                    if (sign == SignNegative)
                        DecimalRaw.Negate(ref result);

                    var scale = buffer.ReadInt16();
                    if (scale > MaxDecimalScale)
                        throw new OverflowException("Numeric value does not fit in a System.Decimal");

                    result.Scale = scale;

                    var scaleDifference = scale + weight * MaxGroupScale;
                    if (groups == MaxGroupCount)
                    {
                        while (groups-- > 1)
                        {
                            DecimalRaw.Multiply(ref result, MaxGroupSize);
                            DecimalRaw.Add(ref result, (uint)buffer.ReadInt16());
                        }

                        var group = buffer.ReadInt16();
                        var groupSize = DecimalRaw.Powers10[-scaleDifference];
                        if (group % groupSize != 0)
                            throw new OverflowException("Numeric value does not fit in a System.Decimal");

                        DecimalRaw.Multiply(ref result, MaxGroupSize / groupSize);
                        DecimalRaw.Add(ref result, (uint)group / groupSize);
                    }
                    else
                    {
                        while (groups-- > 0)
                        {
                            DecimalRaw.Multiply(ref result, MaxGroupSize);
                            DecimalRaw.Add(ref result, (uint)buffer.ReadInt16());
                        }

                        if (scaleDifference < 0)
                            DecimalRaw.Divide(ref result, DecimalRaw.Powers10[-scaleDifference]);
                        else
                            while (scaleDifference > 0)
                            {
                                var scaleChunk = Math.Min(DecimalRaw.MaxUInt32Scale, scaleDifference);
                                DecimalRaw.Multiply(ref result, DecimalRaw.Powers10[scaleChunk]);
                                scaleDifference -= scaleChunk;
                            }
                    }

                    return result.Value;
                }
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, decimal value)
            {
                var weight = 0;
                var groupCount = 0;

                Span<short> groups = stackalloc short[MaxGroupCount];

                var raw = new DecimalRaw(value);
                if (raw.Low != 0 || raw.Mid != 0 || raw.High != 0)
                {
                    var scale = raw.Scale;
                    weight = -scale / MaxGroupScale - 1;

                    uint remainder;
                    var scaleChunk = scale % MaxGroupScale;
                    if (scaleChunk > 0)
                    {
                        var divisor = DecimalRaw.Powers10[scaleChunk];
                        var multiplier = DecimalRaw.Powers10[MaxGroupScale - scaleChunk];
                        remainder = DecimalRaw.Divide(ref raw, divisor) * multiplier;

                        if (remainder != 0)
                        {
                            weight--;
                            goto WriteGroups;
                        }
                    }

                    while ((remainder = DecimalRaw.Divide(ref raw, MaxGroupSize)) == 0)
                        weight++;

                    WriteGroups:
                    groups[groupCount++] = (short)remainder;

                    while (raw.Low != 0 || raw.Mid != 0 || raw.High != 0)
                        groups[groupCount++] = (short)DecimalRaw.Divide(ref raw, MaxGroupSize);
                }

                buffer.WriteInt16((short)(groupCount));
                buffer.WriteInt16((short)(groupCount + weight));
                buffer.WriteInt16((short)(raw.Negative ? SignNegative : SignPositive));
                buffer.WriteInt16((short)(raw.Scale));

                while (groupCount > 0)
                    buffer.WriteInt16(groups[--groupCount]);
            }
        }
    }
}
