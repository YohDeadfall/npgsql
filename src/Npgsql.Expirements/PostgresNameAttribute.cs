using System;

namespace Npgsql.Expirements
{
    public sealed class PostgresNameAttribute : Attribute
    {
        public string Name { get; }

        public PostgresNameAttribute(string name) =>
            Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
