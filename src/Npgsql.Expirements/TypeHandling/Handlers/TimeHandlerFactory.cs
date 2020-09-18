﻿using Npgsql.Expirements.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class TimeHandlerFactory : NpgsqlTypeHandlerFactory
    {
        private static Handler<NpgsqlTime, Converter>? s_nativeHandler;
        private static Handler<DateTime, Converter>? s_dateTimeHandler;

        private const int NativeLength = sizeof(Int64);

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            if (postgresType is null)
                throw new ArgumentNullException(nameof(postgresType));

            if (postgresType.Length != NativeLength)
                throw new ArgumentException("Type length must be equal to 8 bytes.", nameof(postgresType));

            if (runtimeType == typeof(NpgsqlTime))
                return s_nativeHandler ??= new Handler<NpgsqlTime, Converter>();

            if (runtimeType == typeof(DateTime))
                return s_dateTimeHandler ??= new Handler<DateTime, Converter>();

            throw new ArgumentException("The runtime type isn't supported.", nameof(runtimeType));
        }

        private sealed class Handler<T, TConverter> : NpgsqlTypeHandler<T>
            where TConverter : struct, INpgsqlValueConverter<NpgsqlTime, T>
        {
            public Handler() : base(NativeLength) { }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                new ValueTask<T>(new TConverter().ToRuntime(new NpgsqlTime(buffer.ReadInt64())));

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value) =>
                buffer.WriteInt64(new TConverter().ToNative(value).ToNative());
        }

        private readonly struct Converter :
            INpgsqlValueConverter<NpgsqlTime, NpgsqlTime>,
            INpgsqlValueConverter<NpgsqlTime, DateTime>
        {
            NpgsqlTime INpgsqlValueConverter<NpgsqlTime, NpgsqlTime>.ToNative(NpgsqlTime value) => value;
            NpgsqlTime INpgsqlValueConverter<NpgsqlTime, NpgsqlTime>.ToRuntime(NpgsqlTime value) => value;

            NpgsqlTime INpgsqlValueConverter<NpgsqlTime, DateTime>.ToNative(DateTime value) => new NpgsqlTime(value);
            DateTime INpgsqlValueConverter<NpgsqlTime, DateTime>.ToRuntime(NpgsqlTime value) => value.ToDateTime();
        }
    }
}
