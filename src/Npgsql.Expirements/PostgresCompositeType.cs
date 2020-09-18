using System;
using System.Collections.Generic;

namespace Npgsql.Expirements
{
    public sealed class PostgresCompositeType : PostgresType
    {
        public IReadOnlyList<PostgresCompositeField> Fields { get; } = null!;

        public PostgresCompositeType(int oid)
            : base(oid, null) { }
    }

    public sealed class PostgresCompositeField
    {
        public string Name { get; }
        public PostgresType Type { get; }

        public PostgresCompositeField(string name, PostgresType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
