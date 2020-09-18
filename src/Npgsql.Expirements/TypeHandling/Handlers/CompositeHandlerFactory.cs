using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql.Expirements.TypeHandling.Handlers
{
    public sealed class CompositeHandlerFactory<T> : NpgsqlTypeHandlerFactory
    {
        private static readonly bool ValueTypeComposite = typeof(T).IsValueType;

        public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection)
        {
            if (runtimeType is null)
                throw new ArgumentNullException(nameof(runtimeType));

            var postgresCompositeType = postgresType as PostgresCompositeType;
            if (postgresCompositeType is null)
                throw postgresType is null
                    ? new ArgumentNullException(nameof(postgresType))
                    : new ArgumentException("The postgres types isn't a composite type.", nameof(postgresType));

            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            return new CompositeHandler(runtimeType, postgresCompositeType, connection.TypeMapper);
        }

        private sealed class CompositeHandler : NpgsqlTypeHandler<T>
        {
            //private readonly Func<T>? _constructor;
            private readonly NpgsqlTypeHandler[] _memberHandlers;

            public CompositeHandler(Type runtimeType, PostgresCompositeType postgresType, NpgsqlTypeMapper typeMapper)
            {
                var postgresFields = postgresType.Fields;
                var namingPolicy = typeMapper.AttributeNamingPolicy;

                var runtimeMemberHandlers = new NpgsqlTypeHandler[postgresFields.Count];
                var runtimeMemberHandlerCount = 0;

                foreach (var runtimeProperty in runtimeType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    CreateMemberHandler(runtimeProperty, runtimeProperty.PropertyType);

                foreach (var runtimeField in runtimeType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                    CreateMemberHandler(runtimeField, runtimeField.FieldType);

                if (runtimeMemberHandlerCount != postgresFields.Count)
                {
                    var notMappedFields = string.Join(", ", runtimeMemberHandlers
                        .Select((member, memberIndex) => member == null ? $"'{postgresFields[memberIndex].Name}'" : null)
                        .Where(member => member != null));
                    throw new InvalidOperationException($"PostgreSQL composite type {postgresType} contains fields {notMappedFields} which could not match any on CLR type {runtimeType}");
                }

                void CreateMemberHandler(MemberInfo runtimeMember, Type runtimeMemberType)
                {
                    var attribute = runtimeMember.GetCustomAttribute<PostgresNameAttribute>();
                    var name = attribute?.Name ?? namingPolicy.ConvertName(runtimeMember.Name);

                    for (var postgresFieldIndex = postgresFields.Count - 1; postgresFieldIndex >= 0; --postgresFieldIndex)
                    {
                        var postgresField = postgresFields[postgresFieldIndex];
                        if (postgresField.Name != name)
                            continue;

                        if (runtimeMemberHandlers[postgresFieldIndex] != null)
                            throw new AmbiguousMatchException($"Multiple class members are mapped to the '{postgresField.Name}' field.");

                        runtimeMemberHandlerCount++;
                        runtimeMemberHandlers[postgresFieldIndex] = typeMapper.GetHandler(postgresField.Type)
                            .CreateHandler(runtimeMemberType, postgresField.Type, null!);

                        break;
                    }
                }

                _memberHandlers = runtimeMemberHandlers;
            }

            protected internal override ValueTask<T> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length)
            {
                var fieldCount = buffer.ReadInt32();
                if (fieldCount != _memberHandlers.Length)
                    throw new InvalidOperationException();

                throw new NotImplementedException();
            }

            protected internal override void WriteValue(NpgsqlBufferWriter buffer, T value)
            {
            }
        }

        private sealed class CompositeMemberHandlerFactory : NpgsqlTypeHandlerFactory
        {
            public override NpgsqlTypeHandler CreateHandler(Type runtimeType, PostgresType postgresType, NpgsqlConnection connection) =>
                throw new NotSupportedException();

            protected internal override NpgsqlTypeHandler CreateHandler<TElement>(Type runtimeType, PostgresType postgresType, NpgsqlTypeHandler<TElement> elementHandler) =>
                ValueTypeComposite
                    ? (NpgsqlTypeHandler)new CompositeMemberHandlerOfStruct<TElement>(postgresType, elementHandler)
                    : (NpgsqlTypeHandler)new CompositeMemberHandlerOfClass<TElement>(postgresType, elementHandler);
        }

        private abstract class CompositeMemberHandler<TMember> : NpgsqlTypeHandler<TMember>
        {
            protected readonly PostgresType MemberType;
            protected readonly NpgsqlTypeHandler<TMember> MemberHandler;

            public CompositeMemberHandler(PostgresType memberType, NpgsqlTypeHandler<TMember> memberHandler)
            {
                MemberType = memberType;
                MemberHandler = memberHandler;
            }

            protected internal sealed override ValueTask<TMember> ReadValueAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, int length) =>
                throw new NotSupportedException();

            protected internal sealed override void WriteValue(NpgsqlBufferWriter buffer, TMember value) =>
                throw new NotSupportedException();
        }

        private interface CompositeMemberHandlerOfClass
        {
            ValueTask ReadMemberAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, T composite);
            void WriteMemberAsync(NpgsqlBufferWriter buffer, T composite);
        }

        private interface CompositeMemberHandlerOfStruct
        {
            ValueTask ReadMemberAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, ByReference composite);
            void WriteMemberAsync(NpgsqlBufferWriter buffer, in T composite);
        }

        private sealed class ByReference
        {
            public readonly T Value;
            public ByReference(T calue) => Value = calue;
        }

        private sealed class CompositeMemberHandlerOfClass<TMember> : CompositeMemberHandler<TMember>, CompositeMemberHandlerOfClass
        {
            private delegate TMember GetMember(T composite);
            private delegate void SetMember(T composite, TMember value);

            private readonly GetMember? _get;
            private readonly SetMember? _set;

            public CompositeMemberHandlerOfClass(PostgresType memberType, NpgsqlTypeHandler<TMember> memberHandler)
                : base(memberType, memberHandler)
            {
                _get = null;
                _set = null;
            }

            public async ValueTask ReadMemberAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, T composite)
            {
                if (_set == null)
                    throw new InvalidOperationException();

                var oid = buffer.ReadInt32();
                var value = await MemberHandler.ReadAsync(buffer, cancellationToken);

                _set(composite, value);
            }

            public void WriteMemberAsync(NpgsqlBufferWriter buffer, T composite)
            {
                if (_get == null)
                    throw new InvalidOperationException();

                buffer.WriteInt32(MemberType.Oid);
                MemberHandler.WriteValue(buffer, _get(composite));
            }
        }

        private sealed class CompositeMemberHandlerOfStruct<TMember> : CompositeMemberHandler<TMember>, CompositeMemberHandlerOfStruct
        {
            private delegate TMember GetMember(in T composite);
            private delegate void SetMember(in T composite, TMember value);

            private readonly GetMember? _get;
            private readonly SetMember? _set;

            public CompositeMemberHandlerOfStruct(PostgresType memberType, NpgsqlTypeHandler<TMember> memberHandler)
                : base(memberType, memberHandler)
            {
                _get = null;
                _set = null;
            }

            public async ValueTask ReadMemberAsync(NpgsqlBufferReader buffer, CancellationToken cancellationToken, ByReference composite)
            {
                if (_set == null)
                    throw new InvalidOperationException();

                var oid = buffer.ReadInt32();
                var value = await MemberHandler.ReadAsync(buffer, cancellationToken);

                _set(composite.Value, value);
            }

            public void WriteMemberAsync(NpgsqlBufferWriter buffer, in T composite)
            {
                if (_get == null)
                    throw new InvalidOperationException();

                buffer.WriteInt32(MemberType.Oid);
                MemberHandler.WriteValue(buffer, _get(composite));
            }
        }
    }
}
